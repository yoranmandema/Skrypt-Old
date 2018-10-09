using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Execution;
using Skrypt.Parsing;
using static Skrypt.Library.Native.System;
using System;
using System.Reflection;
using System.Linq;

namespace Skrypt.Library
{
    public delegate SkryptObject SkryptDelegate(SkryptEngine engine, SkryptObject self, SkryptObject[] input);
    public delegate void SkryptSetDelegate(SkryptObject self, SkryptObject value);
    public delegate SkryptObject SkryptGetDelegate(SkryptObject self, SkryptObject[] input);

    public class SkryptMethod : SkryptObject {
        public string ReturnType { get; set; }
        public string CallName { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();

        public virtual ScopeContext Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters, ScopeContext scope) {
            return null;
        }

        public static bool IsSkryptMethod (MethodInfo m) {
            if (m.ReturnType != typeof(SkryptObject)) return false;

            if (m.GetParameters().Count() != 3) return false;

            if (m.GetParameters()[0].ParameterType != typeof(SkryptEngine)) return false;

            if (m.GetParameters()[1].ParameterType != typeof(SkryptObject)) return false;

            if (m.GetParameters()[2].ParameterType != typeof(SkryptObject[])) return false;

            return true;
        }

        public static ScopeContext GetPopulatedScope (SkryptMethod m, SkryptObject[] a = null) {
            var s = new ScopeContext();

            if (m.GetType() == typeof(UserFunction)) {
                for (int i = 0; i < m.Parameters.Count; i++) {
                    s.SetVariable(m.Parameters[i],a[i]);
                }
            } else {
                for (int i = 0; i < a.Length; i++) {
                    s.SetVariable(a[i].GetHashCode() + "", a[i]);
                }
            }

            return s;
        }

        public static ScopeContext GetPopulatedScope(SkryptMethod m) {
            var s = new ScopeContext();

            if (m.GetType() == typeof(UserFunction)) {
                for (int i = 0; i < m.Parameters.Count; i++) {
                    s.SetVariable(m.Parameters[i], new Null());
                }
            }  

            return s;
        }
    }

    public class UserFunction : SkryptMethod {
        public string Path;
        public Node BlockNode { get; set; }
        public string Signature { get; set; }

        public override ScopeContext Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters, ScopeContext scope)  {
            var inputScope = new ScopeContext {
                ParentScope = scope,
                Properties = scope.Properties | ScopeProperties.InMethod,
                CallStack = scope.CallStack
            };

            if (self != null) {
                inputScope.SetVariable("self", self, Modifier.Const);
            }

            var resultingScope = engine.Executor.ExecuteBlock(BlockNode, inputScope);

            resultingScope.ReturnObject = resultingScope.ReturnObject ?? new Native.System.Null();
            resultingScope.Variables = new Dictionary<string, Variable>(scope.Variables);

            resultingScope.ReturnObject.Engine = Engine;     

            return resultingScope;
        }
    }

    public class SharpMethod : SkryptMethod {
        public Delegate Method { get; set; }
        public new bool IsSkryptMethod { get; set; }

        public SharpMethod (Delegate d) {
            Method = d;

            IsSkryptMethod = IsSkryptMethod(Method.GetMethodInfo());
        }

        public override ScopeContext Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters, ScopeContext scope) {
            SkryptObject returnValue = null;

            if (IsSkryptMethod) {
                returnValue = ((SkryptDelegate)Method)(engine, self, parameters);
            } else {
                var parCount = Method.Method.GetParameters().Count();

                var input = new object[parCount];

                for (int i = 0; i < input.Length; i++) {
                    if (i < parameters.Length) {
                        input[i] = parameters[i];
                    } else {
                        input[i] = null;
                    }
                }

                returnValue = (SkryptObject)Method.DynamicInvoke(input);

                if (returnValue == null) returnValue = engine.Create<Null>();
            }

            if (typeof(SkryptType).IsAssignableFrom(returnValue.GetType())) {
                returnValue.GetPropertiesFrom(engine.Executor.GetType(((SkryptType)returnValue).TypeName, scope));
            }

            var newScope = new ScopeContext {
                ParentScope = scope,
                Properties = scope.Properties | ScopeProperties.InMethod
            };

            if (self != null) {
                newScope.SetVariable("self", self, Modifier.Const);
            }

            newScope.ReturnObject = returnValue;
            newScope.ReturnObject.Engine = engine;
            newScope.Variables = new Dictionary<string, Variable>(scope.Variables);

            return newScope;
        }
    }

    public class GetMethod : SkryptMethod {
        public SkryptGetDelegate Method { get; set; }

        public override ScopeContext Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters, ScopeContext scope) {        
            var returnValue = Method(self, parameters);

            if (typeof(SkryptType).IsAssignableFrom(returnValue.GetType())) {
                returnValue.GetPropertiesFrom(engine.Executor.GetType(((SkryptType)returnValue).TypeName, scope));
            }

            var newScope = new ScopeContext {
                ParentScope = scope,
                Properties = scope.Properties
            };

            newScope.ReturnObject = returnValue;
            newScope.ReturnObject.Engine = engine;
            newScope.Variables = new Dictionary<string, Variable>(scope.Variables);

            return newScope;
        }
    }

    public class SetMethod : SkryptMethod {
        public SkryptSetDelegate Method { get; set; }

        public void Execute(SkryptEngine engine, SkryptObject self, SkryptObject value,
            ScopeContext scope) {
            Method(self, value);
        }
    }
}