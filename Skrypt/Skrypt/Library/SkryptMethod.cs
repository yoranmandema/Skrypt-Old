using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Execution;
using Skrypt.Parsing;

namespace Skrypt.Library
{
    public delegate SkryptObject SkryptDelegate(SkryptObject self, SkryptObject[] input);

    public class SkryptMethod : SkryptObject
    {
        public string ReturnType;

        public virtual SkryptObject Execute(SkryptEngine engine, SkryptObject Self, SkryptObject[] parameters,
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

        public override SkryptObject Execute(SkryptEngine engine, SkryptObject Self, SkryptObject[] parameters,
            ScopeContext scope)
        {
            var ResultingScope =
                engine.executor.ExecuteBlock(BlockNode, scope, new SubContext {InMethod = true, Method = this});

            var ReturnVariable = ResultingScope.subContext.ReturnObject ?? new Native.System.Void();

            return ReturnVariable;
        }
    }

    public class SharpMethod : SkryptMethod
    {
        public SkryptDelegate method;

        public override SkryptObject Execute(SkryptEngine engine, SkryptObject Self, SkryptObject[] parameters,
            ScopeContext scope)
        {
            var returnValue = method(Self, parameters);

            if (returnValue.GetType().IsSubclassOf(typeof(SkryptType)))
                returnValue.SetPropertiesTo(engine.Types[((SkryptType) returnValue).TypeName]);

            return returnValue;
        }
    }
}