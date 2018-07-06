using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Skrypt.Library;
using Skrypt.Engine;
using Skrypt.Parsing;
using Skrypt.Library.Native;

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

        public SkryptProperty GetProperty (SkryptObject Object, string ToFind) {
            var Find = Object.Properties.Find((x) => x.Name == ToFind);

            if (Find == null) {
                engine.throwError("Object does not contain property '" + ToFind + "'!");
            }

            return Find;
        }

        public SkryptProperty ExecuteAccess (SkryptObject Object, Node node, ScopeContext scopeContext) {
            if (node.SubNodes.Count == 0) {
                return GetProperty(Object, node.Body);
            }

            SkryptProperty Property = GetProperty(Object, node.SubNodes[0].Body);

            if (node.SubNodes[1].Body == "access") {
                return ExecuteAccess(Property.Value, node.SubNodes[1], scopeContext);
            } else {
                return Property;
            }
        }

        public SkryptObject ExecuteExpression(Node node, ScopeContext scopeContext) {
            Operator op = Operator.AllOperators.Find(o => o.OperationName == node.Body || o.Operation == node.Body);

            if (op != null) {
                if (op.OperationName == "return") {
                    if (!scopeContext.subContext.InMethod) {
                        engine.throwError("Can't use return operator outside method!", node.SubNodes[0].Token);
                    }

                    SkryptObject result = null;

                    if (node.SubNodes.Count == 1) {
                        result = ExecuteExpression(node.SubNodes[0], scopeContext);
                    } else {
                        result = new Library.Native.System.Void();
                    }

                    scopeContext.subContext.ReturnObject = result;
                    return result;
                }

                if (op.OperationName == "access") {
                    SkryptObject Target = ExecuteExpression(node.SubNodes[1], scopeContext);
                    SkryptObject Result = ExecuteAccess(Target, node.SubNodes[0], scopeContext).Value;
                    return Result;
                }

                if (op.OperationName == "assign") {
                    SkryptObject result = ExecuteExpression(node.SubNodes[1], scopeContext);

                    if (node.SubNodes[0].SubNodes.Count == 0 && node.SubNodes[0].TokenType == "Identifier") {
                        Variable Variable = getVariable(node.SubNodes[0].Body, scopeContext);

                        if (Variable != null) {
                            Variable.Value = result;
                        }
                        else {
                            scopeContext.Variables[node.SubNodes[0].Body] = new Variable {
                                Name = node.SubNodes[0].Body,
                                Value = result,
                                Scope = scopeContext
                            };
                        }
                    }
                    else if (node.SubNodes[0].Body == "access") {
                        SkryptObject Target = ExecuteExpression(node.SubNodes[0].SubNodes[1], scopeContext);
                        SkryptProperty AccessResult = ExecuteAccess(Target, node.SubNodes[0].SubNodes[0], scopeContext);

                        AccessResult.Value = result;
                    }
                    else if (node.SubNodes[0].Body == "Index") {
                        ExecuteIndexSet(result, node.SubNodes[0], scopeContext);
                    }
                    else {
                        engine.throwError("Left hand side needs to be a variable or property!", node.SubNodes[0].Token);
                    }

                    return result;
                }

                if (op.Members == 2) {
                    SkryptObject LeftResult = ExecuteExpression(node.SubNodes[0], scopeContext);
                    SkryptObject RightResult = ExecuteExpression(node.SubNodes[1], scopeContext);

                    dynamic Left = Convert.ChangeType(LeftResult, LeftResult.GetType());
                    dynamic Right = Convert.ChangeType(RightResult, RightResult.GetType());

                    Operation OpLeft = Left.GetOperation(op.OperationName, LeftResult.GetType(), RightResult.GetType(), Left.Operations);
                    Operation OpRight = Right.GetOperation(op.OperationName, LeftResult.GetType(), RightResult.GetType(), Right.Operations);

                    OperationDelegate Operation = null;

                    if (OpLeft != null) {
                        Operation = OpLeft.operation;
                    }
                    else if (OpRight != null) {
                        Operation = OpRight.operation;
                    }
                    else {
                        engine.throwError("No such operation as " + Left.Name + " " + op.Operation + " " + Right.Name, node.SubNodes[0].Token);
                    }

                    return Operation(new SkryptObject[] { LeftResult, RightResult });                    
                }
                else if (op.Members == 1) {
                    SkryptObject LeftResult = ExecuteExpression(node.SubNodes[0], scopeContext);

                    dynamic Left = Convert.ChangeType(LeftResult, LeftResult.GetType());

                    Operation OpLeft = Left.GetOperation(op.OperationName, LeftResult.GetType(), null, Left.Operations);

                    OperationDelegate Operation = null;

                    if (OpLeft != null) {
                        Operation = OpLeft.operation;
                    }
                    else {
                        engine.throwError("No such operation as " + Left.Name + " " + op.Operation, node.SubNodes[0].Token);
                    }

                    return Operation(new SkryptObject[] { LeftResult });              
                }
            }
            else if (node.TokenType == "ArrayLiteral") {
                Library.Native.System.Array array = new Library.Native.System.Array();

                for (int i = 0; i < node.SubNodes.Count; i++) {
                    Node subNode = node.SubNodes[i];

                    SkryptObject Result = ExecuteExpression(subNode, scopeContext);

                    if (Result.Name == "void") {
                        engine.throwError("Can't add void to array!", node.SubNodes[0].Token);
                    }

                    array.value["" + i] = Result;
                }

                return array;
            }
            else if (node.SubNodes.Count == 0) {
                switch (node.TokenType) {
                    case "NumericLiteral":
                        return new Library.Native.System.Numeric(Double.Parse(node.Body));
                    case "StringLiteral":
                        return new Library.Native.System.String(node.Body);
                    case "BooleanLiteral":
                        return new Library.Native.System.Boolean(node.Body == "true" ? true : false);
                    case "NullLiteral":
                        return new Library.Native.System.Null();
                }
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

            if (node.TokenType == "Index") {
                return ExecuteIndex(node, scopeContext);
            }

            if (node.TokenType == "Call") {
                List<SkryptObject> Arguments = new List<SkryptObject>();
                ScopeContext methodContext = new ScopeContext {
                    ParentScope = scopeContext
                };

                foreach (Node subNode in node.SubNodes[1].SubNodes) {
                    SkryptObject Result = ExecuteExpression(subNode, scopeContext);

                    if (Result.Name == "void") {
                        engine.throwError("Can't pass void into arguments!", node.SubNodes[0].Token);
                    }

                    Arguments.Add(Result);
                }

                SkryptObject Method = ExecuteExpression(node.SubNodes[0].SubNodes[0], scopeContext);

                if (Method.GetType() != typeof(SharpMethod)) {
                    var Find = Method.Properties.Find((x) => x.Name == "Constructor");

                    if (Find != null) {
                        Method = Find.Value;
                    }
                }

                if (Method.GetType() == typeof(UserMethod)) {
                    UserMethod method = (UserMethod)Method;

                    for (int i = 0; i < method.Parameters.Count; i++) {
                        string parName = method.Parameters[i];
                        SkryptObject input;

                        if (i < Arguments.Count) {
                            input = Arguments[i];
                        }
                        else {
                            input = new Library.Native.System.Null();
                        }

                        methodContext.Variables[parName] = new Variable {
                            Name = parName,
                            Value = input,
                            Scope = methodContext
                        };
                    }

                    SkryptObject MethodResult = method.Execute(engine, Arguments.ToArray(), methodContext);

                    return MethodResult;
                } else if (Method.GetType() == typeof(SharpMethod)) {
                    SkryptObject MethodResult = ((SharpMethod)Method).Execute(engine, Arguments.ToArray(), methodContext);

                    return MethodResult;
                } else {
                    engine.throwError("Cannot call value, as it is not a function!", node.SubNodes[0].SubNodes[0].Token);
                }
            }

            return null;
        }

        public SkryptObject ExecuteIndexSet (SkryptObject Value, Node node, ScopeContext scopeContext) {
            List<SkryptObject> Arguments = new List<SkryptObject>();

            foreach (Node subNode in node.SubNodes[1].SubNodes) {
                SkryptObject Result = ExecuteExpression(subNode, scopeContext);

                if (Result.Name == "void") {
                    engine.throwError("Can't pass void into arguments!", node.SubNodes[0].Token);
                }

                Arguments.Add(Result);
            }

            SkryptObject Object = ExecuteExpression(node.SubNodes[0].SubNodes[0], scopeContext);

            dynamic Left = Convert.ChangeType(Object, Object.GetType());

            Operation OpLeft = Left.GetOperation("indexset", Object.GetType(), Arguments[0].GetType(), Left.Operations);

            OperationDelegate Operation = null;

            if (OpLeft != null) {
                Operation = OpLeft.operation;
            }
            else {
                engine.throwError("No such operation as index set " + Left.Name + "!", node.SubNodes[0].Token);
            }

            var inputArray = new List<SkryptObject>(Arguments);

            inputArray.Insert(0, Value);
            inputArray.Insert(0, Object);

            return Operation(inputArray.ToArray());
        }

        public SkryptObject ExecuteIndex (Node node, ScopeContext scopeContext) {
            List<SkryptObject> Arguments = new List<SkryptObject>();

            foreach (Node subNode in node.SubNodes[1].SubNodes) {
                SkryptObject Result = ExecuteExpression(subNode, scopeContext);

                if (Result.Name == "void") {
                    engine.throwError("Can't pass void into arguments!", node.SubNodes[0].Token);
                }

                Arguments.Add(Result);
            }

            SkryptObject Object = ExecuteExpression(node.SubNodes[0].SubNodes[0], scopeContext);

            dynamic Left = Convert.ChangeType(Object, Object.GetType());

            Operation OpLeft = Left.GetOperation("index", Object.GetType(), Arguments[0].GetType(), Left.Operations);

            OperationDelegate Operation = null;

            if (OpLeft != null) {
                Operation = OpLeft.operation;
            }
            else {
                engine.throwError("No such operation as index " + Left.Name + "!", node.SubNodes[0].Token);
            }

            var inputArray = new List<SkryptObject>(Arguments);

            inputArray.Insert(0, Object);

            //SkryptProperty property = new SkryptProperty {
            //    Value = Operation(inputArray.ToArray())
            //};

            return Operation(inputArray.ToArray());
        }

        //public SkryptObject Invoke (string Name, params object[] arguments) {
        //    string signature = Name;
        //    string searchString = Name;
        //    ScopeContext methodContext = new ScopeContext {
        //        ParentScope = engine.GlobalScope
        //    };

        //    SkryptObject[] parameters = new SkryptObject[arguments.Length];

        //    for (int i = 0; i < arguments.Length; i++) {
        //        object arg = arguments[i];

        //        if (arg.GetType() == typeof(int) || arg.GetType() == typeof(float) || arg.GetType() == typeof(double)) {
        //            parameters[i] = new Numeric(Convert.ToDouble(arg));
        //        } else if (arg.GetType() == typeof(string)) {
        //            parameters[i] = new SkryptString { value = (string)arg };
        //        } else if (arg.GetType() == typeof(bool)) {
        //            parameters[i] = new SkryptBoolean { value = (bool)arg };
        //        }

        //        i++;
        //    }

        //    foreach (SkryptObject parameter in parameters) {
        //        if (parameter.Name == "void") {
        //            throw new SkryptException("Can't pass void into arguments!");
        //        }

        //        signature += "_" + parameter.Name;
        //    }

        //    foreach (Node method in engine.MethodNodes) {
        //        if (method.Body == signature) {
        //            searchString = signature;

        //            for (int i = 0; i < method.SubNodes[0].SubNodes.Count; i++) {
        //                Node par = method.SubNodes[0].SubNodes[i];
        //                methodContext.Variables[par.Body].Value = parameters[i];
        //            }
        //        }
        //    }

        //    if (engine.Methods.Exists((m) => m.Name == searchString)) {
        //        SkryptObject MethodResult = engine.Methods.Find((m) => m.Name == searchString).Execute(engine, parameters, methodContext);

        //        return MethodResult;
        //    }
        //    else {
        //        throw new SkryptException("Method '" + Name + "(" + String.Join(",", signature.Split('_').Skip(1).ToArray()) + ")' does not exist!");
        //    }
        //}
    }
}
