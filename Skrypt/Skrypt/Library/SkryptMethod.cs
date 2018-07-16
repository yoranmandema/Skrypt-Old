using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Execution;
using Skrypt.Parsing;
using System;

namespace Skrypt.Library
{
    public delegate SkryptObject SkryptDelegate(SkryptObject self, SkryptObject[] input);
    public delegate void SkryptSetDelegate(SkryptObject self, SkryptObject value);

    public class SkryptMethod : SkryptObject
    {
        public string ReturnType;

        public virtual SkryptObject Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters,
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
        public Node BlockNode;
        public string CallName;
        public List<string> Parameters = new List<string>();
        public string Signature;

        public override SkryptObject Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters,
            ScopeContext scope)
        {
            var resultingScope =
                engine.Executor.ExecuteBlock(BlockNode, scope, new SubContext {InMethod = true, Method = this});

            var returnVariable = resultingScope.SubContext.ReturnObject ?? new Native.System.Null();

            return returnVariable;
        }
    }

    public class SharpMethod : SkryptMethod
    {
        public SkryptDelegate Method;

        public override SkryptObject Execute(SkryptEngine engine, SkryptObject self, SkryptObject[] parameters,
            ScopeContext scope)
        {
            var returnValue = Method(self, parameters);

            if (typeof(SkryptType).IsAssignableFrom(returnValue.GetType())) {
                returnValue.SetPropertiesTo(engine.Executor.GetType(((SkryptType)returnValue).TypeName, scope));
            }

            return returnValue;
        }
    }

    public class SetMethod : SkryptMethod {
        public SkryptSetDelegate Method;

        public void Execute(SkryptEngine engine, SkryptObject self, SkryptObject value,
            ScopeContext scope) {
            Method(self, value);
        }
    }
}