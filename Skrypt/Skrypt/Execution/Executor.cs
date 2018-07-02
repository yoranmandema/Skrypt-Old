using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library.SkryptClasses;
using System.Reflection;
using Skrypt.Library;
using Skrypt.Engine;
using Skrypt.Parsing;

namespace Skrypt.Execution {
    public class Executor {
        SkryptEngine engine;

        public Executor(SkryptEngine e) {
            engine = e;
        }

        Variable getVariable (string name, ScopeContext scopeContext) {
            Variable FoundVar = null;

            if (scopeContext.Variables.ContainsKey(name)) {
                FoundVar = scopeContext.Variables[name];
            } else if (scopeContext.ParentScope != null) {
                FoundVar = getVariable(name, scopeContext.ParentScope);
            }

            return FoundVar;
        }

        bool CheckCondition (Node node, ScopeContext scopeContext) {
            bool ConditionResult = false;

            try {
                ConditionResult = engine.executor.ExecuteExpression(node, scopeContext).ToBoolean();
            } catch (Exception e) {
                engine.throwError(e.Message);
            }

            return ConditionResult;
        }

        public void ExecuteWhileStatement (Node node, ScopeContext scopeContext) {
            while (true) {
                bool ConditionResult = CheckCondition(node.SubNodes[0].SubNodes[0], scopeContext);

                if (!ConditionResult) {
                    break;
                }

                ExecuteBlock(node.SubNodes[1], scopeContext);
            }
        }

        public void ExecuteIfStatement(Node node, ScopeContext scopeContext) {
            bool ConditionResult = CheckCondition(node.SubNodes[0].SubNodes[0], scopeContext);

            if (ConditionResult) {
                ExecuteBlock(node.SubNodes[1], scopeContext);
                return;
            }

            if (node.SubNodes.Count > 2) {
                for (int i = 2; i < node.SubNodes.Count; i++) {
                    Node elseNode = node.SubNodes[i];

                    if (elseNode.Body == "elseif") {
                        ConditionResult = engine.executor.ExecuteExpression(elseNode.SubNodes[0].SubNodes[0], scopeContext).ToBoolean();

                        if (ConditionResult) {
                            ExecuteBlock(elseNode.SubNodes[1], scopeContext);
                            return;
                        }
                    }
                    else {
                        ExecuteBlock(elseNode, scopeContext);
                    }
                }
            }
        }

        public ScopeContext ExecuteBlock (Node node, ScopeContext scopeContext, SubContext subContext = null) {
            ScopeContext scope = new ScopeContext();

            if (scopeContext != null) {
                scope.subContext = scopeContext.subContext;
                scope.ParentScope = scopeContext;
            }

            if (subContext != null) {
                scope.subContext = subContext;
            }

            foreach (Node subNode in node.SubNodes) {
                if (subNode.TokenType == "Statement") {
                    switch (subNode.Body) {
                        case "while":
                            ExecuteWhileStatement(subNode, scope);
                        break;
                        case "if":
                            ExecuteIfStatement(subNode, scope);
                        break;
                    }
                }
                else if (subNode.TokenType == "MethodDeclaration") {
                    foreach (KeyValuePair<string, Variable> pair in scope.Variables.Where((p) => p.Value.Value.Name == "method")) {
                        if (pair.Value.Name == subNode.Body) {
                            engine.throwError("A method with this signature already exists in this context!", node.Token);
                        }
                    }

                    UserMethod result = new UserMethod();
                    result.Name = "method";
                    result.Signature = subNode.Body;
                    result.BlockNode = subNode.SubNodes[0];
                    result.CallName = subNode.Body.Split('_')[0];

                    foreach (Node snode in subNode.SubNodes[1].SubNodes) {
                        result.Parameters.Add(snode.Body);
                    }

                    scope.Variables[subNode.Body] = new Variable {
                        Name = result.CallName,
                        Value = result,
                        Scope = scope
                    };
                }
                else {
                    SkryptObject result = engine.executor.ExecuteExpression(subNode, scope);

                    if (scope.subContext.ReturnObject != null) {
                        return scope;
                    }
                }
            }

            return scope; 
        }

        public SkryptObject ExecuteExpression(Node node, ScopeContext scopeContext) {
            Operator op = Operator.AllOperators.Find(o => o.OperationName == node.Body || o.Operation == node.Body);

            if (op != null) {
                //int Members = node.SubNodes.Count;

                //if (Members < op.Members) {
                //    engine.throwError("Missing member of operation!", node.Token);
                //}

                if (op.OperationName == "return") {
                    if (!scopeContext.subContext.InMethod) {
                        engine.throwError("Can't use return operator outside method!", node.SubNodes[0].Token);
                    }

                    SkryptObject result = null;

                    if (node.SubNodes.Count == 1) {
                        result = ExecuteExpression(node.SubNodes[0], scopeContext);
                    } else {
                        result = new SkryptVoid();
                    }

                    if (result.Name != scopeContext.subContext.Method.ReturnType) {
                        engine.throwError("Can't return '" + result.Name + "' in a method that returns '" + scopeContext.subContext.Method.ReturnType + "'!", node.Token);
                    }

                    scopeContext.subContext.ReturnObject = result;
                    return result;
                }

                if (op.OperationName == "assign") {
                    if (node.SubNodes[0].TokenType != "Identifier") {
                        engine.throwError("Can't assign non-variable", node.SubNodes[0].Token);
                    }

                    SkryptObject result = ExecuteExpression(node.SubNodes[1], scopeContext);

                    if (result.Name == "void") {
                        engine.throwError("Can't assign to void", node.SubNodes[1].Token);
                    }

                    if (engine.Constants.ContainsKey(node.SubNodes[0].Body)) {
                        engine.throwError("Can't assign constant " + node.SubNodes[0].Body, node.SubNodes[0].Token);
                    }

                    Variable foundVariable = getVariable(node.SubNodes[0].Body, scopeContext);

                    if (foundVariable != null) {
                        //if (foundVariable.Value.Name != result.Name) {
                        //    engine.throwError("Can't assign " + foundVariable.Name + " to " + result.Name, node.SubNodes[1].Token);
                        //}

                        foundVariable.Value = result;
                    }
                    else {
                        scopeContext.Variables[node.SubNodes[0].Body] = new Variable {
                            Name = node.SubNodes[0].Body,
                            Value = result,
                            Scope = scopeContext
                        };
                    }

                    return result;
                }

                if (op.Members == 2) {
                    SkryptObject Left = ExecuteExpression(node.SubNodes[0], scopeContext);
                    SkryptObject Right = ExecuteExpression(node.SubNodes[1], scopeContext);

                    try {
                        switch (op.OperationName) {
                            case "add":
                                return Left._Add(Right);
                            case "subtract":
                                return Left._Subtract(Right);
                            case "multiply":
                                return Left._Multiply(Right);
                            case "divide":
                                return Left._Divide(Right);
                            case "modulo":
                                return Left._Modulo(Right);
                            case "lesser":
                                return Left._Lesser(Right);
                            case "greater":
                                return Left._Greater(Right);
                            case "equal":
                                return Left._Equal(Right);
                            default:
                                throw new Exception();
                        }
                    } catch (Exception e) {
                        engine.throwError("No such operation as " + Left.Name + " " + op.Operation + " " + Right.Name, node.SubNodes[0].Token);
                    }

                    //dynamic Casted = Activator.CreateInstance(Left.GetType());

                    //if (Casted.Operations.ContainsKey(op.OperationName)) {
                    //    var Operation = Casted.Operations[op.OperationName];

                    //    return Operation(Left, Right);
                    //} else {
                    //    engine.throwError("No such operation as " + Left.Name + " " + op.Operation + " " + Right.Name, node.SubNodes[0].Token);
                    //}

                    //if (Left.Name == "void" || Right.Name == "void") {
                    //    engine.throwError("No such operation as " + Left.Name + " " + op.Operation + " " + Right.Name, node.SubNodes[0].Token);
                    //}

                    //if (Left != null && Right != null) {
                    //    Type t1 = Left.GetType();
                    //    Type t2 = Right.GetType();

                    //    MethodInfo methodInfo1 = t1.GetMethod("_" + op.OperationName, new Type[] { t1, t2 });
                    //    MethodInfo methodInfo2 = t2.GetMethod("_" + op.OperationName, new Type[] { t1, t2 });

                    //    if (methodInfo1 != null) {
                    //        try {
                    //            object result = methodInfo1.Invoke(null, new object[] { Left, Right });

                    //            return (SkryptObject)result;
                    //        }
                    //        catch (Exception e) { }
                    //    }
                    //    else if (methodInfo2 != null) {
                    //        try {
                    //            object result = methodInfo2.Invoke(null, new object[] { Left, Right });
                    //            return (SkryptObject)result;
                    //        }
                    //        catch (Exception e) { }
                    //    }

                    //    engine.throwError("No such operation as " + Left.Name + " " + op.Operation + " " + Right.Name, node.SubNodes[0].Token);
                    //}
                }
                else if (op.Members == 1) {
                    SkryptObject Left = ExecuteExpression(node.SubNodes[0], scopeContext);

                    try {
                        switch (op.OperationName) {
                            case "postincrement":
                                return Left._PostIncrement();
                            default:
                                throw new Exception();
                        }
                    } catch (Exception e) {
                        engine.throwError("No such operation as " + op.Operation + " " + Left.Name, node.SubNodes[0].Token);
                    }

                //if (Left.Name == "void") {
                //    engine.throwError("No such operation as " + op.Operation + " " + Left.Name, node.SubNodes[0].Token);
                //}

                //if (Left != null) {
                //    Type t1 = Left.GetType();

                //    MethodInfo methodInfo1 = null;
                //    var Methods1 = t1.GetMethodsBySig("_" + op.OperationName, t1);

                //    if (Methods1.Count() > 0) methodInfo1 = Methods1.ElementAt(0);

                //    if (methodInfo1 != null) {
                //        try {
                //            object result = methodInfo1.Invoke(null, new object[] { Left });
                //            return (SkryptObject)result;
                //        }
                //        catch (Exception e) { }
                //    }

                //    engine.throwError("No such operation as " + op.Operation + " " + Left.Name, node.SubNodes[0].Token);
                //}
            }
            }
            else if (node.SubNodes.Count == 0) {
                switch (node.TokenType) {
                    case "NumericLiteral":
                        return new Numeric (Double.Parse(node.Body));
                    case "StringLiteral":
                        return new SkryptString {
                            value = node.Body,
                        };
                    case "BooleanLiteral":
                        return new SkryptBoolean {
                            value = node.Body == "true" ? true : false,
                        };
                    case "NullLiteral":
                        return new SkryptNull ();
                }
            }
            else if (node.TokenType == "ArrayLiteral") {
                SkryptArray array = new SkryptArray ();

                foreach (Node subNode in node.SubNodes) {
                    SkryptObject Result = ExecuteExpression(subNode, scopeContext);

                    if (Result.Name == "void") {
                        engine.throwError("Can't add void to array!", node.SubNodes[0].Token);
                    }

                    array.value.Add(Result);
                }

                return array;
            }
            else if (node.TokenType == "FunctionLiteral") { 
                UserMethod result = new UserMethod();
                result.Name = "method";
                result.Signature = node.Body;
                result.BlockNode = node.SubNodes[0];
                result.CallName = node.Body.Split('_')[0];

                foreach (Node snode in node.SubNodes[1].SubNodes) {
                    result.Parameters.Add(snode.Body);
                }

                return result;
            }

            if (node.TokenType == "Identifier") {
                if (engine.Constants.ContainsKey(node.Body)) {
                    return engine.Constants[node.Body];
                }

                Variable foundVariable = getVariable(node.Body, scopeContext);

                if (foundVariable != null) {
                    return foundVariable.Value;
                }
                else {
                    engine.throwError("Variable '" + node.Body + "' does not exist in the current context!", node.Token);
                }
            }

            if (node.TokenType == "Call") {
                List<SkryptObject> Arguments = new List<SkryptObject>();
                string signature = node.Body;
                string searchString = node.Body;
                ScopeContext methodContext = new ScopeContext {
                    ParentScope = scopeContext
                };

                foreach (Node subNode in node.SubNodes) {
                    SkryptObject Result = ExecuteExpression(subNode, scopeContext);

                    if (Result.Name == "void") {
                        engine.throwError("Can't pass void into arguments!", node.SubNodes[0].Token);
                    }

                    Arguments.Add(Result);

                    signature += "_" + Result.Name;
                }

                Variable found = getVariable(node.Body, scopeContext);

                if (found != null) {
                    SkryptObject functionValue = found.Value;

                    if (functionValue.GetType() == typeof(UserMethod)) {
                        UserMethod method = (UserMethod)functionValue;

                        for (int i = 0; i < method.Parameters.Count; i++) {
                            string parName = method.Parameters[i];
                            SkryptObject input;

                            if (i < Arguments.Count) {
                                input = Arguments[i];
                            } else {
                                input = new SkryptNull ();
                            }

                            methodContext.Variables[parName] = new Variable {
                                Name = parName,
                                Value = input,
                                Scope = methodContext
                            };
                        }

                        SkryptObject MethodResult = method.Execute(engine, Arguments.ToArray(), methodContext);

                        return MethodResult;
                    }
                }
                else if (engine.Methods.Exists((m) => m.Name == searchString)) {
                    SkryptObject MethodResult = engine.Methods.Find((m) => m.Name == searchString).Execute(engine, Arguments.ToArray(), methodContext);

                    return MethodResult;
                }
                else {
                    engine.throwError("Method '" + node.Body + "(" + String.Join(",", signature.Split('_').Skip(1).ToArray()) + ")' does not exist in the current context!", node.Token);
                }
            }

            return null;
        }

        public SkryptObject Invoke (string Name, params object[] arguments) {
            string signature = Name;
            string searchString = Name;
            ScopeContext methodContext = new ScopeContext {
                ParentScope = engine.GlobalScope
            };

            SkryptObject[] parameters = new SkryptObject[arguments.Length];

            for (int i = 0; i < arguments.Length; i++) {
                object arg = arguments[i];

                if (arg.GetType() == typeof(int) || arg.GetType() == typeof(float) || arg.GetType() == typeof(double)) {
                    parameters[i] = new Numeric(Convert.ToDouble(arg));
                } else if (arg.GetType() == typeof(string)) {
                    parameters[i] = new SkryptString { value = (string)arg };
                } else if (arg.GetType() == typeof(bool)) {
                    parameters[i] = new SkryptBoolean { value = (bool)arg };
                }

                i++;
            }

            foreach (SkryptObject parameter in parameters) {
                if (parameter.Name == "void") {
                    throw new SkryptException("Can't pass void into arguments!");
                }

                signature += "_" + parameter.Name;
            }

            foreach (Node method in engine.MethodNodes) {
                if (method.Body == signature) {
                    searchString = signature;

                    for (int i = 0; i < method.SubNodes[0].SubNodes.Count; i++) {
                        Node par = method.SubNodes[0].SubNodes[i];
                        methodContext.Variables[par.Body].Value = parameters[i];
                    }
                }
            }

            if (engine.Methods.Exists((m) => m.Name == searchString)) {
                SkryptObject MethodResult = engine.Methods.Find((m) => m.Name == searchString).Execute(engine, parameters, methodContext);

                return MethodResult;
            }
            else {
                throw new SkryptException("Method '" + Name + "(" + String.Join(",", signature.Split('_').Skip(1).ToArray()) + ")' does not exist!");
            }
        }
    }
}
