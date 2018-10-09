using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Skrypt.Engine;
using Skrypt.Library;
using Skrypt.Parsing;
using Skrypt.Tokenization;

namespace Skrypt.Execution
{
    public class Executor {
        private readonly SkryptEngine _engine;

        public Executor(SkryptEngine e) {
            _engine = e;
        }

        /// <summary>
        ///     Recursively fetches a variable from the given scope.
        /// </summary>
        public Variable GetVariable (string name, ScopeContext scopeContext) {
            Variable foundVar = null;

            if (scopeContext.Variables.ContainsKey(name)) {
                foundVar = scopeContext.Variables[name];
            } else if (scopeContext.ParentScope != null) {
                foundVar = GetVariable(name, scopeContext.ParentScope);
            }

            return foundVar;
        }

        /// <summary>
        ///     Recursively fetches a type from the given scope.
        /// </summary>
        public SkryptObject GetType(string name, ScopeContext scopeContext) {
            SkryptObject foundVar = null;

            if (scopeContext.Types.ContainsKey(name)) {
                foundVar = scopeContext.Types[name];
            }
            else if (scopeContext.ParentScope != null) {
                foundVar = GetType(name, scopeContext.ParentScope);
            }

            if (foundVar == null) {
                _engine.ThrowError("Could not find type: " + name);
            }

            return foundVar;
        }

        /// <summary>
        ///     Fetches a property from an object.
        /// </summary>
        public SkryptProperty GetProperty(SkryptObject Object, string toFind, ScopeContext context, bool setter = false) {
            var find = Object.Properties.Find((x) => {
                if (x.Name == toFind) {
                    if (!setter) {
                        if (x.IsSetter) return false;

                        return true;
                    }
                    else {
                        if (x.IsGetter) return false;

                        return true;
                    }
                }

                return false;
            });

            if (find == null) _engine.ThrowError("Object does not contain property '" + toFind + "'!");

            var canFailPrivate = true;

            // Allow getting private properties from 'self' variable.
            if (GetVariable("self", context) != null) {
                canFailPrivate = !GetVariable("self", context).Value.Equals(Object);
            }

            if (canFailPrivate) {
                if ((find.Modifiers & Modifier.Private) != 0) {
                    _engine.ThrowError("Property '" + toFind + "' is inaccessable due to its protection level.");
                }
            }

            return find;
        }

        /// <summary>
        ///     Solves an expression and returns its boolean result.
        /// </summary>
        private bool CheckCondition(Node node, ScopeContext scopeContext) {
            var conditionResult = false;

            conditionResult = _engine.Executor.ExecuteExpression(node, scopeContext).ToBoolean();

            return conditionResult;
        }

        /// <summary>
        ///     Executes a while statement node.
        /// </summary>
        public void ExecuteWhileStatement(Node node, ScopeContext scopeContext) {
            // Loop until an exit condition is reached.
            while (true) {
                var conditionResult = CheckCondition(node.Nodes[0].Nodes[0], scopeContext);

                // Exit when the condition is false.
                if (!conditionResult) break;

                var scope = ExecuteBlock(node.Nodes[1], scopeContext, ScopeProperties.InLoop);

                // Exit when a break expression was reached.
                if ((scope.Properties & ScopeProperties.BrokeLoop) != 0) break;

                if (scope.ReturnObject != null) {
                    scopeContext.ReturnObject = scope.ReturnObject;
                    break;
                }
            }
        }

        /// <summary>
        ///     Executes a for statement node.
        /// </summary>
        public void ExecuteForStatement(Node node, ScopeContext scopeContext) {
            var initNode = node.Nodes[0];
            var condNode = node.Nodes[1];
            var modiNode = node.Nodes[2];
            var block = node.Nodes[3];

            ScopeContext loopScope = new ScopeContext();
            loopScope.StrictlyLocal = true;

            if (scopeContext != null) {
                loopScope.ParentScope = scopeContext;
                scopeContext.SubScopes.Add(loopScope);
            }

            var initResult = ExecuteExpression(initNode, loopScope);

            if (scopeContext != null) {
                loopScope.Properties = scopeContext.Properties;
            }

            loopScope.Properties |= ScopeProperties.InLoop;

            loopScope.StrictlyLocal = false;

            // Loop until an exit condition is reached.
            while (true) {
                var conditionResult = CheckCondition(condNode, loopScope);

                // Exit when the condition is false.
                if (!conditionResult) break;

                var scope = ExecuteBlock(block, loopScope);

                // Exit when a break expression was reached.
                if ((scope.Properties & ScopeProperties.BrokeLoop) != 0) break;

                if (scope.ReturnObject != null) {
                    scopeContext.ReturnObject = scope.ReturnObject;
                    break;
                }

                var modiResult = ExecuteExpression(modiNode, loopScope);
            }
        }

        /// <summary>
        ///     Executes an if statement node.
        /// </summary>
        public void ExecuteIfStatement(Node node, ScopeContext scopeContext)  {
            var conditionResult = CheckCondition(node.Nodes[0].Nodes[0], scopeContext);

            // Execute body of statement if its evaluated to true.
            if (conditionResult) {
                var scope = ExecuteBlock(node.Nodes[1], scopeContext);

                if (scope.ReturnObject != null) {
                    scopeContext.ReturnObject = scope.ReturnObject;
                }

                return;
            }

            // Execute elseif/else statements proceeding the current statement.
            if (node.Nodes.Count > 2) {
                for (var i = 2; i < node.Nodes.Count; i++) {
                    var elseNode = node.Nodes[i];

                    // Execute elseif statement as if statement.
                    if (elseNode.Body == "elseif") {
                        conditionResult = _engine.Executor.ExecuteExpression(elseNode.Nodes[0].Nodes[0], scopeContext).ToBoolean();

                        if (conditionResult) {
                            var scope = ExecuteBlock(elseNode.Nodes[1], scopeContext);

                            if (scope.ReturnObject != null) {
                                scopeContext.ReturnObject = scope.ReturnObject;
                            }

                            return;
                        }
                    }
                    // Execute remaining else statement.
                    else {
                        var scope = ExecuteBlock(elseNode, scopeContext);

                        if (scope.ReturnObject != null) {
                            scopeContext.ReturnObject = scope.ReturnObject;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Executes a class definition node.
        /// </summary>
        public SkryptObject ExecuteClassDeclaration(ClassNode node, ScopeContext scopeContext) {
            string ClassName = node.Name;
            var ParentClass = scopeContext.ParentClass;

            if (ParentClass != null) {
                ClassName = ParentClass.Name + "." + ClassName;
            }

            // The static object.
            SkryptObject Object = new SkryptObject {
                Name = ClassName,
            };

            // The object base that gets instantiated.
            SkryptType TypeObject = new SkryptType {
                Name = ClassName,
            };

            scopeContext.AddType(ClassName, TypeObject);

            // Give objects a value displaying their type.
            Object.Properties.Add(new SkryptProperty {
                Name = "TypeName",
                Value = new Library.Native.System.String(ClassName),
                Modifiers = Parsing.Modifier.Const
            });

            TypeObject.Properties.Add(new SkryptProperty {
                Name = "TypeName",
                Value = new Library.Native.System.String(ClassName),
                Modifiers = Parsing.Modifier.Const
            });

            // Give instance object a value pointing to its base type.
            TypeObject.Properties.Add(new SkryptProperty {
                Name = "Type",
                Value = Object,
                Modifiers = Parsing.Modifier.Const
            });

            // Execute class body.
            var scope = ExecuteBlock(node.BodyNode, scopeContext, ScopeProperties.InClassDeclaration);

            // Add variables with modifiers to instance and base objects.
            foreach (var v in scope.Variables) {
                if (v.Value.Modifiers != Modifier.None) {
                    var property = new SkryptProperty {
                        Name = v.Key,
                        Value = v.Value.Value,
                        Modifiers = v.Value.Modifiers
                    };

                    if ((v.Value.Modifiers & Modifier.Instance) == 0) {
                        var find = TypeObject.Properties.Find(x => x.Name == v.Key);

                        if (find != null) {
                            find = property;
                        } else {
                            Object.Properties.Add(property);
                        }
                    } else {
                        var find = TypeObject.Properties.Find(x => x.Name == v.Key);

                        if (find != null) {
                            find = property;
                        }
                        else {
                            TypeObject.Properties.Add(property);
                        }
                    }
                }
            }

            // Add variables from inherited classes to objects.
            foreach (var inheritNode in node.InheritNode.Nodes) {
                var value = ExecuteExpression(inheritNode, scopeContext);

                var BaseType = GetType(((Library.Native.System.String)value.GetProperty("TypeName")).Value, scopeContext);
                var instance = (SkryptType)Activator.CreateInstance(BaseType.GetType());
                instance.ScopeContext = _engine.CurrentScope;
                instance.Engine = _engine;
                instance.GetPropertiesFrom(BaseType);

                if (value.GetType() != typeof(SkryptObject) || BaseType.GetType() != typeof(SkryptType)) {
                    _engine.ThrowError("Can only inherit from Skrypt-based objects");
                }

                foreach (var p in value.Properties) {
                    var find = Object.Properties.Find(x => {
                        if (x.IsGetter != p.IsGetter) return false;
                        if (x.IsSetter != p.IsSetter) return false;
                        if (x.Name != p.Name) return false;

                        return true;
                    });

                    if (find == null) {
                        Object.Properties.Add(new SkryptProperty {
                            Name = p.Name,
                            Value = p.Value,
                            Modifiers = p.Modifiers,
                            IsGetter = p.IsGetter,
                            IsSetter = p.IsSetter
                        });
                    }
                }

                foreach (var p in instance.Properties) {
                    var find = TypeObject.Properties.Find(x => {
                        if (x.IsGetter != p.IsGetter) return false;
                        if (x.IsSetter != p.IsSetter) return false;
                        if (x.Name != p.Name) return false;

                        return true;
                    });

                    if (find == null) {
                        TypeObject.Properties.Add(new SkryptProperty {
                            Name = p.Name,
                            Value = p.Value,
                            Modifiers = p.Modifiers,
                            IsGetter = p.IsGetter,
                            IsSetter = p.IsSetter
                        });
                    }
                }
            }

            Object.Name = node.Name;

            return Object;
        }

        /// <summary>
        ///     Executes a function definition node.
        /// </summary>
        public UserFunction ExecuteFunctionDeclaration (Node node, ScopeContext scopeContext) {
            UserFunction result = new UserFunction {
                Name = node.Body,
                Signature = node.Body,
                BlockNode = node.Nodes[0],
                CallName = node.Body,
                Path = _engine.CurrentExecutingFile
            };

            // Create parameters.
            foreach (Node snode in node.Nodes[1].Nodes) {
                result.Parameters.Add(snode.Body);
            }

            return result;
        }

        /// <summary>
        ///     Executes a using statement.
        /// </summary>
        public ScopeContext ExecuteUsing(UsingNode node, ScopeContext scopeContext) {
            var Object = ExecuteExpression(node.Getter, scopeContext);

            // Add all public variables from object as variables to scope.
            foreach (var property in Object.Properties) {
                if ((property.Modifiers & Modifier.Public) != 0) {
                    scopeContext.SetVariable(property.Name, property.Value, Modifier.Const);
                }
            }

            return scopeContext;
        }

        /// <summary>
        ///     Executes a block.
        /// </summary>
        public ScopeContext ExecuteBlock (Node node, ScopeContext scopeContext, ScopeProperties properties = 0) {
            _engine.State = EngineState.Executing;

            ScopeContext scope = new ScopeContext ();

            // Copy settings from previous scope.
            if (scopeContext != null) {
                scope.Properties = scopeContext.Properties;
                scope.ParentScope = scopeContext;
                scope.Types = scopeContext.Types;
                scope.CallStack = scopeContext.CallStack;
                scope.ParentClass = scopeContext.ParentClass;

                scopeContext.SubScopes.Add(scope);
            }

            if (properties != 0) {
                scope.Properties |= scopeContext.Properties;
            }

            if ((scope.Properties & ScopeProperties.InClassDeclaration) == 0) {
                if ((node.Modifiers & Modifier.Instance) != 0 || (node.Modifiers & Modifier.Public) != 0 || (node.Modifiers & Modifier.Private) != 0) {
                    _engine.ThrowError("Property modifiers cannot be used outside class", node.Token);
                }
            }

            _engine.CurrentScope = scope;
            var oldStack = _engine.CurrentStack;
            var oldPath = _engine.CurrentExecutingFile;

            foreach (var subNode in node.Nodes) {
                if (subNode.Type == TokenTypes.Statement) {
                    switch (subNode.Body) {
                        case "while":
                            ExecuteWhileStatement(subNode, scope);
                            break;
                        case "if":
                            ExecuteIfStatement(subNode, scope);
                            break;
                        case "for":
                            ExecuteForStatement(subNode, scope);
                            break;
                    }

                    if ((scope.Properties & ScopeProperties.SkippedLoop) != 0) return scope;
                    if ((scope.Properties & ScopeProperties.BrokeLoop) != 0) return scope;
                    if (scope.ReturnObject != null) return scope;
                }
                else if (subNode.Type == TokenTypes.Include) {
                    _engine.CurrentExecutingFile = ((IncludeNode)subNode).Path;
                    scope = ExecuteBlock(subNode, scope);
                }
                else if (subNode.Type == TokenTypes.MethodDeclaration) {
                    var result = ExecuteFunctionDeclaration(subNode, scope);

                    scope.SetVariable(result.CallName, result, subNode.Modifiers);
                }
                else if (subNode.Type == TokenTypes.ClassDeclaration) {
                    var createdClass = ExecuteClassDeclaration((ClassNode)subNode, scope);

                    scope.SetVariable(createdClass.Name, createdClass, subNode.Modifiers);
                }
                else if (subNode.Type == TokenTypes.Using) {
                    var _scope = ExecuteUsing((UsingNode)subNode, scope);
                }
                else {
                    var result = _engine.Executor.ExecuteExpression(subNode, scope);

                    if ((scope.Properties & ScopeProperties.SkippedLoop) != 0) return scope;
                    if ((scope.Properties & ScopeProperties.BrokeLoop) != 0) return scope;
                    if (scope.ReturnObject != null) return scope;
                }

                // Reset current stack and scope variables.
                _engine.CurrentScope = scope;
                _engine.CurrentStack = oldStack;
                _engine.CurrentExecutingFile = oldPath;
            }

            return scope;
        }

        public class AccessResult {
            public SkryptObject Owner;
            public SkryptProperty Property;
        }

        /// <summary>
        ///     Executes an access expression.
        /// </summary>
        public AccessResult ExecuteAccess(SkryptObject Object, Node node, ScopeContext scopeContext, bool setter = false) {
            var localScope = new ScopeContext {
                Properties = scopeContext.Properties,
                ParentScope = scopeContext,
                Types = scopeContext.Types,
                CallStack = scopeContext.CallStack
            };

            // Add properties as variables so they can be obtained using existing methods.
            foreach (var p in Object.Properties) {
                localScope.SetVariable(p.Name, p.Value, p.Modifiers);
            }
            
            scopeContext.Caller = Object;
            localScope.Caller = Object;

            if (node.Body == "access") {
                var target = ExecuteExpression(node.Nodes[0], localScope);
                localScope.Caller = target;
                scopeContext.Caller = target;

                // Execute target as method if its a getter property.
                if (target.GetType() == typeof(GetMethod)) {
                    var ex = ((GetMethod)target).Execute(_engine, Object, new SkryptObject[0], new ScopeContext { ParentScope = scopeContext });
                    target = ex.ReturnObject;
                }

                var result = ExecuteAccess(target, node.Nodes[1], localScope, setter); 
                return result;
            }
            else {
                localScope = null;

                return new AccessResult {
                    Property = GetProperty(Object, node.Body, scopeContext, setter),
                    Owner = Object
                };
            }
        }

        /// <summary>
        ///     Executes an expression.
        /// </summary>
        public SkryptObject ExecuteExpression(Node node, ScopeContext scopeContext) {
            _engine.CurrentNode = node;

            var op = Operator.AllOperators.Find(o => o.OperationName == node.Body);

            if (op != null) {
                // Do return operation.
                if (op.OperationName == "return") {
                    if ((scopeContext.Properties & ScopeProperties.InMethod) == 0) {
                        _engine.ThrowError("Can't use return operator outside method", node.Token);
                    }

                    // Return null if no value was given.
                    SkryptObject result = node.Nodes.Count == 1
                        ? ExecuteExpression(node.Nodes[0], scopeContext)
                        : new Library.Native.System.Null();

                    scopeContext.ReturnObject = result;

                    return null;
                }

                // Do break operation.
                if (op.OperationName == "break") {
                    if ((scopeContext.Properties & ScopeProperties.InLoop) == 0)
                        _engine.ThrowError("Can't use break operator outside loop", node.Token);

                    scopeContext.Properties |= ScopeProperties.BrokeLoop;
                    return null;
                }

                // Do continue operation.
                if (op.OperationName == "continue") {
                    if ((scopeContext.Properties & ScopeProperties.InLoop) == 0)
                        _engine.ThrowError("Can't use continue operator outside loop", node.Token);

                    scopeContext.Properties |= ScopeProperties.SkippedLoop;
                    return null;
                }

                // Do access operation.
                if (op.OperationName == "access") {
                    var target = ExecuteExpression(node.Nodes[0], scopeContext);
                    var result = ExecuteAccess(target, node.Nodes[1], scopeContext);

                    scopeContext.Caller = result.Owner;

                    // Execute result as method if its a getter property.
                    if (result.Property.Value.GetType() == typeof(GetMethod)) {
                        var ex = ((GetMethod)result.Property.Value).Execute(_engine, result.Owner, new SkryptObject[0], new ScopeContext { ParentScope = scopeContext});

                        return ex.ReturnObject;
                    }
                    else {
                        return result.Property.Value;
                    }
                }

                // Do assignment operation.
                if (op.OperationName == "assign") {
                    // Solve right side.
                    var result = ExecuteExpression(node.Nodes[1], scopeContext);

                    // Copy value of results like numbers, string etc, so variables that 
                    // are assigned to eachother don't edit eachothers values.
                    if (typeof(SkryptType).IsAssignableFrom(result.GetType())) {
                        if (((SkryptType)result).CreateCopyOnAssignment) {
                            result = result.Clone();
                        }
                    }

                    // Left side is an identifier.
                    if (node.Nodes[0].Nodes.Count == 0 && node.Nodes[0].Type == TokenTypes.Identifier) {
                        if (GeneralParser.Keywords.Contains(node.Nodes[0].Body)) {
                            _engine.ThrowError("Setting variable names to keywords is disallowed");
                        }

                        var variable = GetVariable(node.Nodes[0].Body, scopeContext);

                        if (variable != null && !scopeContext.StrictlyLocal) {
                            if ((variable.Modifiers & Modifier.Const) != 0)
                                _engine.ThrowError("Variable is marked as constant and can thus not be modified.", node.Nodes[0].Token);

                            if ((variable.Modifiers & Modifier.Strong) != 0) {
                                if (variable.Value.Name != result.Name) 
                                    _engine.ThrowError($"Can't set strong variable of type {variable.Value.Name} to {result.Name}", node.Nodes[0].Token);
                            }

                            variable.Value = result;
                        }
                        else {
                            scopeContext.SetVariable(node.Nodes[0].Body, result, node.Modifiers);
                        }
                    }
                    // Left side is an expression.
                    else if (node.Nodes[0].Body == "access") {
                        var target = ExecuteExpression(node.Nodes[0].Nodes[0], scopeContext);
                        var accessResult = ExecuteAccess(target, node.Nodes[0].Nodes[1], scopeContext, true);

                        if ((accessResult.Property.Modifiers & Modifier.Const) != 0)
                            _engine.ThrowError("Property is marked as constant and can thus not be modified.", node.Nodes[0].Nodes[1].Token);

                        if ((accessResult.Property.Modifiers & Modifier.Strong) != 0)
                            if (accessResult.Property.Value.Name != result.Name)
                                _engine.ThrowError($"Can't set strong property of type {accessResult.Property.Value.Name} to {result.Name}", node.Nodes[0].Nodes[1].Token);

                        if (accessResult.Property.IsSetter) {
                            ((SetMethod)accessResult.Property.Value).Execute(_engine, target, result, new ScopeContext { ParentScope = scopeContext });
                        }
                        else {
                            accessResult.Property.Value = result;
                        }
                    }
                    // Left side is an expression.
                    else if (node.Nodes[0].Body == "Index") {
                        ExecuteIndexSet(result, (IndexNode)node.Nodes[0], scopeContext);
                    }
                    else {
                        _engine.ThrowError("Left hand side needs to be a variable or property!", node.Nodes[0].Token);
                    }

                    return result;
                }

                // Evaluate node as binary expression.
                if (op.Members == 2) {
                    var leftResult = ExecuteExpression(node.Nodes[0], scopeContext);
                    var rightResult = ExecuteExpression(node.Nodes[1], scopeContext);

                    return _engine.Eval(op, leftResult, rightResult, node);
                }

                // Evaluate node as unary expression.
                if (op.Members == 1) {
                    var leftResult = ExecuteExpression(node.Nodes[0], scopeContext);

                    return _engine.Eval(op, leftResult, node); ;
                }
            }
            // Create array literal instance.
            else if (node.Type == TokenTypes.ArrayLiteral) {
                var arrayNode = (ArrayNode)node;
                var array = _engine.Create<Library.Native.System.Array>();

                // Solve all expressions within the array and add the resulting values.
                for (var i = 0; i < arrayNode.Values.Count; i++) {
                    var subNode = arrayNode.Values[i];

                    var result = ExecuteExpression(subNode, scopeContext);

                    array.List.Add(result);
                }

                return array;
            }
            // Create function literal instance.
            else if (node.Type == TokenTypes.FunctionLiteral) {
                var result = new UserFunction {
                    Name = "method",
                    Signature = node.Body,
                    BlockNode = node.Nodes[0],
                    CallName = node.Body.Split('_')[0],
                    Path = _engine.CurrentExecutingFile
                };

                foreach (var snode in node.Nodes[1].Nodes) {
                    result.Parameters.Add(snode.Body);
                }

                return result;
            }
            else if (node.Type == TokenTypes.Conditional) {
                var conditionNode = (ConditionalNode)node;
                var conditionBool = ExecuteExpression(conditionNode.Condition, scopeContext);

                if (conditionBool.ToBoolean()) {
                    return ExecuteExpression(conditionNode.Pass, scopeContext);
                } else {
                    return ExecuteExpression(conditionNode.Fail, scopeContext);
                }
            }
            else {
                switch (node.Type) {
                    case TokenTypes.NumericLiteral:
                        return _engine.Create<Library.Native.System.Numeric>(((NumericNode)node).Value);
                    case TokenTypes.StringLiteral:
                        return _engine.Create<Library.Native.System.String>(((StringNode)node).Value);
                    case TokenTypes.BooleanLiteral:
                        return _engine.Create<Library.Native.System.Boolean>(((BooleanNode)node).Value);
                    case TokenTypes.NullLiteral:
                        return _engine.Create<Library.Native.System.Null>();
                }
            }

            // Get variable
            if (node.Type == TokenTypes.Identifier) {
                var foundVariable = GetVariable(node.Body, scopeContext);

                if (foundVariable != null) {
                    return foundVariable.Value;
                }

                _engine.ThrowError("Variable '" + node.Body + "' does not exist in the current context!", node.Token);
            }

            // Do index operation
            if (node.Type == TokenTypes.Index) return ExecuteIndex((IndexNode)node, scopeContext);

            // Do function call operation.
            if (node.Type == TokenTypes.Call) {
                var callNode = (CallNode)node;
                var arguments = new List<SkryptObject>();

                // Solve argument expressions.
                foreach (var subNode in callNode.Arguments)  {
                    var result = ExecuteExpression(subNode, scopeContext);

                    arguments.Add(result);
                }

                // Get the value that's being called.
                var foundFunction = ExecuteExpression(callNode.Getter, scopeContext);

                var caller = scopeContext.Caller;

                SkryptObject BaseType = null;

                bool isConstructor = false;
                bool validConstructor = true;

                // Check if we're creating a new instance of a type.
                if (!typeof(SkryptMethod).IsAssignableFrom(foundFunction.GetType())) {
                    var find = foundFunction.GetProperty("Constructor");
                    var typeName = foundFunction.GetProperty("TypeName").ToString();

                    validConstructor = foundFunction.GetType() == typeof(SkryptObject);

                    foundFunction = find ?? new SkryptMethod {Name = "Constructor"};

                    BaseType = GetType(typeName, scopeContext);

                    // Create new instance of type and set that as the caller.
                    caller = (SkryptType)Activator.CreateInstance(BaseType.GetType());
                    caller.ScopeContext = _engine.CurrentScope;
                    caller.Engine = _engine;
                    caller.Name = typeName;
                    caller.GetPropertiesFrom(BaseType);

                    isConstructor = true;
                }

                // Create new scope to use within the function.
                var methodContext = new ScopeContext {
                    ParentScope = scopeContext,
                    // Create new call stack for method.
                    CallStack = new CallStack(((SkryptMethod)foundFunction).Name, callNode.Getter.Token, scopeContext.CallStack, ""),
                    Properties = scopeContext.Properties | ScopeProperties.InMethod
                };

                _engine.CurrentStack = methodContext.CallStack;

                ScopeContext methodScopeResult = null;

                // Function is defined in Skrypt.
                if (foundFunction.GetType() == typeof(UserFunction)) {
                    UserFunction function = (UserFunction)foundFunction;

                    // Create variables for each parameter based on the given arguments.
                    for (var i = 0; i < function.Parameters.Count; i++) {
                        var parName = function.Parameters[i];
                        SkryptObject input;

                        // Set to null if there's no argument.
                        input = i < arguments.Count ? arguments[i] : new Library.Native.System.Null();

                        methodContext.Variables[parName] = new Variable {
                            Name = parName,
                            Value = input,
                            Scope = methodContext
                        };
                    }

                    methodContext.CallStack.Path = _engine.CurrentExecutingFile;

                    _engine.CurrentExecutingFile = function.Path;

                    methodScopeResult = function.Execute(_engine, caller, arguments.ToArray(), methodContext);
                }
                // Function is defined in C#.
                else if (foundFunction.GetType() == typeof(SharpMethod)) {
                    methodScopeResult = ((SharpMethod)foundFunction).Execute(_engine, caller, arguments.ToArray(), methodContext);
                }
                else if (!validConstructor) {
                    _engine.ThrowError("Cannot call value, as it is not a function!", callNode.Getter.Token);
                }

                if (isConstructor) {
                    caller.ScopeContext = _engine.CurrentScope;
                    caller.Engine = _engine;

                    return caller;
                } else {
                    return methodScopeResult.ReturnObject;
                }
            }

            return null;
        }

        /// <summary>
        ///     Executes an index-set opeation.
        /// </summary>
        public SkryptObject ExecuteIndexSet(SkryptObject value, IndexNode node, ScopeContext scopeContext) {
            var arguments = new List<SkryptObject>();

            foreach (var subNode in node.Arguments)  {
                var result = ExecuteExpression(subNode, scopeContext);

                arguments.Add(result);
            }

            var Object = ExecuteExpression(node.Getter, scopeContext);

            // Dynamically change type so we can get it's actual operations.
            dynamic left = Convert.ChangeType(Object, Object.GetType());

            Operation opLeft = SkryptObject.GetOperation(Operators.IndexSet, Object.GetType(), arguments[0].GetType(), left.Operations);

            OperationDelegate operation = null;

            if (opLeft != null) {
                operation = opLeft.OperationDelegate;
            }
            else {
                _engine.ThrowError("No such operation as index set " + left.Name + "!", node.Getter.Token);
            }

            var inputArray = new List<SkryptObject>(arguments);

            inputArray.Insert(0, value);
            inputArray.Insert(0, Object);

            return operation(inputArray.ToArray(), _engine);
        }

        /// <summary>
        ///     Executes an index opeation.
        /// </summary>
        public SkryptObject ExecuteIndex(IndexNode node, ScopeContext scopeContext) {
            var arguments = new List<SkryptObject>();

            foreach (var subNode in node.Arguments) {
                var result = ExecuteExpression(subNode, scopeContext);

                arguments.Add(result);
            }

            var Object = ExecuteExpression(node.Getter, scopeContext);

            // Dynamically change type so we can get it's actual operations.
            dynamic left = Convert.ChangeType(Object, Object.GetType());

            Operation opLeft = SkryptObject.GetOperation(Operators.Index, Object.GetType(), arguments[0].GetType(), left.Operations);

            OperationDelegate operation = null;

            if (opLeft != null) {
                operation = opLeft.OperationDelegate;
            }
            else {
                _engine.ThrowError("No such operation as index " + left.Name + "!", node.Getter.Token);
            }

            var inputArray = new List<SkryptObject>(arguments);

            inputArray.Insert(0, Object);

            return operation(inputArray.ToArray(), _engine);
        }

        /// <summary>
        ///     Allows you to invoke a function from the engine's global scope.
        /// </summary>
        public SkryptObject Invoke (string name, params object[] arguments) {
            var parameters = new SkryptObject[arguments.Length];
            var foundMethod = GetVariable(name, _engine.GlobalScope).Value;
            var input = new List<SkryptObject>();
            var methodContext = new ScopeContext {
                ParentScope = _engine.GlobalScope
            };

            for (int i = 0; i < arguments.Length; i++) {
                object arg = arguments[i];

                if (arg.GetType() == typeof(int) || arg.GetType() == typeof(float) || arg.GetType() == typeof(double)) {
                    parameters[i] = new Library.Native.System.Numeric(Convert.ToDouble(arg));
                }
                else if (arg.GetType() == typeof(string)) {
                    parameters[i] = new Library.Native.System.String((string)arg);                  
                }
                else if (arg.GetType() == typeof(bool)) {
                    parameters[i] = new Library.Native.System.Boolean((bool)arg);
                }
                else if (arg == null) {
                    parameters[i] = new Library.Native.System.Null();
                }

                parameters[i].GetPropertiesFrom(GetType(((SkryptType)parameters[i]).TypeName, _engine.GlobalScope));

                input.Add(parameters[i]);
            }

            ScopeContext methodScopeResult = null;

            if (foundMethod.GetType() == typeof(UserFunction)) {
                UserFunction method = (UserFunction)foundMethod;

                for (var i = 0; i < method.Parameters.Count; i++) {
                    var parName = method.Parameters[i];
                    SkryptObject inp;

                    inp = i < input.Count ? input[i] : new Library.Native.System.Null();

                    methodContext.Variables[parName] = new Variable {
                        Name = parName,
                        Value = inp,
                        Scope = methodContext
                    };
                }

                methodScopeResult = method.Execute(_engine, null, input.ToArray(), methodContext);
            }
            else if (foundMethod.GetType() == typeof(SharpMethod)) {
                methodScopeResult = ((SharpMethod)foundMethod).Execute(_engine, null, input.ToArray(), methodContext);
            }
            else {
                _engine.ThrowError("Cannot call value, as it is not a function!");
            }

            return methodScopeResult.ReturnObject;
        }        
    }
}