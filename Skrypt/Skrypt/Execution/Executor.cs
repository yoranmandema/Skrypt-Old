using System;
using System.Collections.Generic;
using System.Linq;
using Skrypt.Engine;
using Skrypt.Library;
using Skrypt.Parsing;

namespace Skrypt.Execution
{
    public class Executor
    {
        private readonly SkryptEngine _engine;

        public Executor(SkryptEngine e)
        {
            _engine = e;
        }

        private Variable GetVariable(string name, ScopeContext scopeContext)
        {
            Variable foundVar = null;

            if (scopeContext.Variables.ContainsKey(name))
                foundVar = scopeContext.Variables[name];
            else if (scopeContext.ParentScope != null) foundVar = GetVariable(name, scopeContext.ParentScope);

            return foundVar;
        }

        private bool CheckCondition(Node node, ScopeContext scopeContext)
        {
            var conditionResult = false;

            //try {
            conditionResult = _engine.Executor.ExecuteExpression(node, scopeContext).ToBoolean();
            //} catch (Exception e) {
            //    engine.throwError(e.Message);
            //}

            return conditionResult;
        }

        public void ExecuteWhileStatement(Node node, ScopeContext scopeContext)
        {
            while (true)
            {
                var conditionResult = CheckCondition(node.SubNodes[0].SubNodes[0], scopeContext);

                if (!conditionResult) break;

                ExecuteBlock(node.SubNodes[1], scopeContext);
            }
        }

        public void ExecuteIfStatement(Node node, ScopeContext scopeContext)
        {
            var conditionResult = CheckCondition(node.SubNodes[0].SubNodes[0], scopeContext);

            if (conditionResult)
            {
                ExecuteBlock(node.SubNodes[1], scopeContext);
                return;
            }

            if (node.SubNodes.Count > 2)
                for (var i = 2; i < node.SubNodes.Count; i++)
                {
                    var elseNode = node.SubNodes[i];

                    if (elseNode.Body == "elseif")
                    {
                        conditionResult = _engine.Executor
                            .ExecuteExpression(elseNode.SubNodes[0].SubNodes[0], scopeContext).ToBoolean();

                        if (conditionResult)
                        {
                            ExecuteBlock(elseNode.SubNodes[1], scopeContext);
                            return;
                        }
                    }
                    else
                    {
                        ExecuteBlock(elseNode, scopeContext);
                    }
                }
        }

        public ScopeContext ExecuteBlock(Node node, ScopeContext scopeContext, SubContext subContext = null)
        {
            var scope = new ScopeContext();

            if (scopeContext != null)
            {
                scope.SubContext = scopeContext.SubContext;
                scope.ParentScope = scopeContext;
            }

            if (subContext != null) scope.SubContext = subContext;

            foreach (var subNode in node.SubNodes)
                if (subNode.TokenType == "Statement")
                {
                    switch (subNode.Body)
                    {
                        case "while":
                            ExecuteWhileStatement(subNode, scope);
                            break;
                        case "if":
                            ExecuteIfStatement(subNode, scope);
                            break;
                    }
                }
                else if (subNode.TokenType == "MethodDeclaration")
                {
                    foreach (var pair in scope.Variables.Where(p =>
                        p.Value.Value.Name == "method"))
                        if (pair.Value.Name == subNode.Body)
                            _engine.ThrowError("A method with this signature already exists in this context!",
                                node.Token);

                    var result = new UserMethod
                    {
                        Name = "method",
                        Signature = subNode.Body,
                        BlockNode = subNode.SubNodes[0],
                        CallName = subNode.Body.Split('_')[0]
                    };

                    foreach (var snode in subNode.SubNodes[1].SubNodes) result.Parameters.Add(snode.Body);

                    scope.Variables[subNode.Body] = new Variable
                    {
                        Name = result.CallName,
                        Value = result,
                        Scope = scope
                    };
                }
                else if (subNode.TokenType == "ClassDeclaration")
                {
                    var contentScope = ExecuteBlock(subNode, scope);

                    var properties = contentScope.Variables;
                    var Object = new SkryptObject();
                    Object.Name = subNode.Body;

                    foreach (var p in properties)
                        Object.Properties.Add(new SkryptProperty
                        {
                            Name = p.Key,
                            Value = p.Value.Value
                        });

                    scope.AddVariable(Object.Name, Object);
                }
                else
                {
                    var result = _engine.Executor.ExecuteExpression(subNode, scope);

                    if (scope.SubContext.ReturnObject != null) return scope;
                }

            return scope;
        }

        public SkryptProperty GetProperty(SkryptObject Object, string toFind)
        {
            var find = Object.Properties.Find(x => x.Name == toFind);

            if (find == null) _engine.ThrowError("Object does not contain property '" + toFind + "'!");

            return find;
        }

        public SkryptProperty ExecuteAccess(SkryptObject Object, Node node, ScopeContext scopeContext)
        {
            if (node.SubNodes.Count == 0) return GetProperty(Object, node.Body);

            var property = GetProperty(Object, node.SubNodes[0].Body);

            if (node.SubNodes[1].Body == "access")
                return ExecuteAccess(property.Value, node.SubNodes[1], scopeContext);
            return property;
        }

        public SkryptObject ExecuteExpression(Node node, ScopeContext scopeContext)
        {
            var op = Operator.AllOperators.Find(o => o.OperationName == node.Body);

            if (op != null)
            {
                if (op.OperationName == "return")
                {
                    if (!scopeContext.SubContext.InMethod)
                        _engine.ThrowError("Can't use return operator outside method!", node.SubNodes[0].Token);

                    SkryptObject result = null;

                    result = node.SubNodes.Count == 1
                        ? ExecuteExpression(node.SubNodes[0], scopeContext)
                        : new Library.Native.System.Void();

                    scopeContext.SubContext.ReturnObject = result;
                    return result;
                }

                if (op.OperationName == "access")
                {
                    var target = ExecuteExpression(node.SubNodes[1], scopeContext);
                    var result = ExecuteAccess(target, node.SubNodes[0], scopeContext).Value;

                    if (scopeContext.SubContext.GettingCaller) scopeContext.SubContext.Caller = target;

                    return result;
                }

                if (op.OperationName == "assign")
                {
                    var result = ExecuteExpression(node.SubNodes[1], scopeContext);

                    if (result.GetType().IsSubclassOf(typeof(SkryptType)))
                        if (((SkryptType) result).CreateCopyOnAssignment)
                            result = result.Clone();

                    if (node.SubNodes[0].SubNodes.Count == 0 && node.SubNodes[0].TokenType == "Identifier")
                    {
                        var variable = GetVariable(node.SubNodes[0].Body, scopeContext);

                        if (variable != null)
                            variable.Value = result;
                        else
                            scopeContext.Variables[node.SubNodes[0].Body] = new Variable
                            {
                                Name = node.SubNodes[0].Body,
                                Value = result,
                                Scope = scopeContext
                            };
                    }
                    else if (node.SubNodes[0].Body == "access")
                    {
                        var target = ExecuteExpression(node.SubNodes[0].SubNodes[1], scopeContext);
                        var accessResult = ExecuteAccess(target, node.SubNodes[0].SubNodes[0], scopeContext);

                        accessResult.Value = result;
                    }
                    else if (node.SubNodes[0].Body == "Index")
                    {
                        ExecuteIndexSet(result, node.SubNodes[0], scopeContext);
                    }
                    else
                    {
                        _engine.ThrowError("Left hand side needs to be a variable or property!", node.SubNodes[0].Token);
                    }

                    return result;
                }

                if (op.Members == 2)
                {
                    var leftResult = ExecuteExpression(node.SubNodes[0], scopeContext);
                    var rightResult = ExecuteExpression(node.SubNodes[1], scopeContext);

                    dynamic left = Convert.ChangeType(leftResult, leftResult.GetType());
                    dynamic right = Convert.ChangeType(rightResult, rightResult.GetType());

                    Operation opLeft = left.GetOperation(op.OperationName, leftResult.GetType(), rightResult.GetType(),
                        left.Operations);
                    Operation opRight = right.GetOperation(op.OperationName, leftResult.GetType(),
                        rightResult.GetType(), right.Operations);

                    OperationDelegate operation = null;

                    if (opLeft != null)
                        operation = opLeft.OperationDelegate;
                    else if (opRight != null)
                        operation = opRight.OperationDelegate;
                    else
                        _engine.ThrowError("No such operation as " + left.Name + " " + op.Operation + " " + right.Name,
                            node.SubNodes[0].Token);

                    var result = (SkryptType) operation(new[] {leftResult, rightResult});

                    result.SetPropertiesTo(_engine.Types[result.TypeName]);

                    return result;
                }

                if (op.Members == 1)
                {
                    var leftResult = ExecuteExpression(node.SubNodes[0], scopeContext);

                    dynamic left = Convert.ChangeType(leftResult, leftResult.GetType());

                    Operation opLeft = left.GetOperation(op.OperationName, leftResult.GetType(), null, left.Operations);

                    OperationDelegate operation = null;

                    if (opLeft != null)
                        operation = opLeft.OperationDelegate;
                    else
                        _engine.ThrowError("No such operation as " + left.Name + " " + op.Operation,
                            node.SubNodes[0].Token);

                    var result = (SkryptType) operation(new[] {leftResult});

                    result.SetPropertiesTo(_engine.Types[result.TypeName]);

                    return result;
                }
            }
            else if (node.TokenType == "ArrayLiteral")
            {
                var array = new Library.Native.System.Array();

                for (var i = 0; i < node.SubNodes.Count; i++)
                {
                    var subNode = node.SubNodes[i];

                    var result = ExecuteExpression(subNode, scopeContext);

                    if (result.Name == "void") _engine.ThrowError("Can't add void to array!", node.SubNodes[0].Token);

                    array.Value.Add(result);
                }

                array.SetPropertiesTo(_engine.Types[array.TypeName]);

                return array;
            }
            else if (node.SubNodes.Count == 0)
            {
                switch (node.TokenType)
                {
                    case "NumericLiteral":
                        var newNumeric = new Library.Native.System.Numeric(double.Parse(node.Body));
                        newNumeric.SetPropertiesTo(_engine.Types[newNumeric.TypeName]);

                        return newNumeric;
                    case "StringLiteral":
                        var newString = new Library.Native.System.String(node.Body);
                        newString.SetPropertiesTo(_engine.Types[newString.TypeName]);

                        return newString;
                    case "BooleanLiteral":
                        var newBool = new Library.Native.System.Boolean(node.Body == "true" ? true : false);
                        newBool.SetPropertiesTo(_engine.Types[newBool.TypeName]);

                        return newBool;
                    case "NullLiteral":
                        return new Library.Native.System.Null();
                }
            }
            else if (node.TokenType == "FunctionLiteral")
            {
                var result = new UserMethod
                {
                    Name = "method",
                    Signature = node.Body,
                    BlockNode = node.SubNodes[0],
                    CallName = node.Body.Split('_')[0]
                };

                foreach (var snode in node.SubNodes[1].SubNodes) result.Parameters.Add(snode.Body);

                return result;
            }

            if (node.TokenType == "Identifier")
            {
                var foundVariable = GetVariable(node.Body, scopeContext);

                if (foundVariable != null)
                    return foundVariable.Value;
                _engine.ThrowError("Variable '" + node.Body + "' does not exist in the current context!",
                    node.Token);
            }

            if (node.TokenType == "Index") return ExecuteIndex(node, scopeContext);

            if (node.TokenType == "Call")
            {
                var arguments = new List<SkryptObject>();
                var methodContext = new ScopeContext
                {
                    ParentScope = scopeContext
                };

                foreach (var subNode in node.SubNodes[1].SubNodes)
                {
                    var result = ExecuteExpression(subNode, scopeContext);

                    if (result.Name == "void")
                        _engine.ThrowError("Can't pass void into arguments!", node.SubNodes[0].Token);

                    arguments.Add(result);
                }

                var findCallerContext = new ScopeContext
                {
                    ParentScope = scopeContext
                };

                findCallerContext.SubContext.GettingCaller = true;
                var method = ExecuteExpression(node.SubNodes[0].SubNodes[0], findCallerContext);
                var Object = findCallerContext.SubContext.Caller;

                if (method.GetType() != typeof(SharpMethod))
                {
                    var type = method.Name;
                    var find = method.Properties.Find(x => x.Name == "Constructor");

                    if (find != null)
                    {
                        method = find.Value;
                        Object = new SkryptObject();
                        Object.SetPropertiesTo(scopeContext.Variables[type].Value);
                        return Object;
                    }
                }

                if (method.GetType() == typeof(UserMethod))
                {
                    var userMethod = (UserMethod) method;

                    for (var i = 0; i < userMethod.Parameters.Count; i++)
                    {
                        var parName = userMethod.Parameters[i];
                        SkryptObject input;

                        input = i < arguments.Count ? arguments[i] : new Library.Native.System.Null();

                        methodContext.Variables[parName] = new Variable
                        {
                            Name = parName,
                            Value = input,
                            Scope = methodContext
                        };
                    }

                    var methodResult = userMethod.Execute(_engine, Object, arguments.ToArray(), methodContext);

                    return methodResult;
                }

                if (method.GetType() == typeof(SharpMethod))
                {
                    var methodResult =
                        ((SharpMethod) method).Execute(_engine, Object, arguments.ToArray(), methodContext);

                    return methodResult;
                }

                _engine.ThrowError("Cannot call value, as it is not a function!",
                    node.SubNodes[0].SubNodes[0].Token);
            }

            return null;
        }

        public SkryptObject ExecuteIndexSet(SkryptObject value, Node node, ScopeContext scopeContext)
        {
            var arguments = new List<SkryptObject>();

            foreach (var subNode in node.SubNodes[1].SubNodes)
            {
                var result = ExecuteExpression(subNode, scopeContext);

                if (result.Name == "void") _engine.ThrowError("Can't pass void into arguments!", node.SubNodes[0].Token);

                arguments.Add(result);
            }

            var Object = ExecuteExpression(node.SubNodes[0].SubNodes[0], scopeContext);

            dynamic left = Convert.ChangeType(Object, Object.GetType());

            Operation opLeft = left.GetOperation("indexset", Object.GetType(), arguments[0].GetType(), left.Operations);

            OperationDelegate operation = null;

            if (opLeft != null)
                operation = opLeft.OperationDelegate;
            else
                _engine.ThrowError("No such operation as index set " + left.Name + "!", node.SubNodes[0].Token);

            var inputArray = new List<SkryptObject>(arguments);

            inputArray.Insert(0, value);
            inputArray.Insert(0, Object);

            return operation(inputArray.ToArray());
        }

        public SkryptObject ExecuteIndex(Node node, ScopeContext scopeContext)
        {
            var arguments = new List<SkryptObject>();

            foreach (var subNode in node.SubNodes[1].SubNodes)
            {
                var result = ExecuteExpression(subNode, scopeContext);

                if (result.Name == "void") _engine.ThrowError("Can't pass void into arguments!", node.SubNodes[0].Token);

                arguments.Add(result);
            }

            var Object = ExecuteExpression(node.SubNodes[0].SubNodes[0], scopeContext);

            dynamic left = Convert.ChangeType(Object, Object.GetType());

            Operation opLeft = left.GetOperation("index", Object.GetType(), arguments[0].GetType(), left.Operations);

            OperationDelegate operation = null;

            if (opLeft != null)
                operation = opLeft.OperationDelegate;
            else
                _engine.ThrowError("No such operation as index " + left.Name + "!", node.SubNodes[0].Token);

            var inputArray = new List<SkryptObject>(arguments);

            inputArray.Insert(0, Object);

            //SkryptProperty property = new SkryptProperty {
            //    Value = Operation(inputArray.ToArray())
            //};

            return operation(inputArray.ToArray());
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