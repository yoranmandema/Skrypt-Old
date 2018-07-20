using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Execution;
using Skrypt.Parsing;
using System;

namespace Skrypt.Library
{
    public delegate SkryptObject SkryptDelegate(SkryptEngine engine, SkryptObject self, SkryptObject[] input);
    public delegate void SkryptSetDelegate(SkryptObject self, SkryptObject value);
    public delegate SkryptObject SkryptGetDelegate(SkryptObject self, SkryptObject[] input);

    public class SkryptMethod : SkryptObject
    {
        public string ReturnType { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();

        public virtual ScopeContext Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters,
            ScopeContext scope)
        {
            return null;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class UserMethod : SkryptMethod
    {
        public Node BlockNode { get; set; }
        public string CallName { get; set; }
        public string Signature { get; set; }

        public override ScopeContext Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters,
            ScopeContext scope)
        {
            var resultingScope =
                engine.Executor.ExecuteBlock(BlockNode, scope, new SubContext {InMethod = true, Method = this});

            resultingScope.SubContext.ReturnObject = resultingScope.SubContext.ReturnObject ?? new Native.System.Null();
            resultingScope.Variables = new Dictionary<string, Variable>(scope.Variables);

            return resultingScope;
        }
    }

    public class SharpMethod : SkryptMethod
    {
        public SkryptDelegate Method { get; set; }

        public override ScopeContext Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters,
            ScopeContext scope)
        {
            var returnValue = Method(engine, self, parameters);

            if (typeof(SkryptType).IsAssignableFrom(returnValue.GetType())) {
                returnValue.SetPropertiesTo(engine.Executor.GetType(((SkryptType)returnValue).TypeName, scope));
            }

            var newScope = new ScopeContext {
                ParentScope = scope,
                SubContext = scope.SubContext
            };

            newScope.SubContext.ReturnObject = returnValue;
            newScope.SubContext.ReturnObject.Engine = engine;
            newScope.Variables = new Dictionary<string, Variable>(scope.Variables);

            return newScope;
        }
    }

    public class GetMethod : SkryptMethod {
        public SkryptGetDelegate Method { get; set; }

        public override ScopeContext Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters,
            ScopeContext scope) {

            var returnValue = Method(self, parameters);

            if (typeof(SkryptType).IsAssignableFrom(returnValue.GetType())) {
                returnValue.SetPropertiesTo(engine.Executor.GetType(((SkryptType)returnValue).TypeName, scope));
            }

            var newScope = new ScopeContext {
                ParentScope = scope,
                SubContext = scope.SubContext
            };

            newScope.SubContext.ReturnObject = returnValue;
            newScope.SubContext.ReturnObject.Engine = engine;
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