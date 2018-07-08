using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Parsing;
using Skrypt.Execution;
using Skrypt.Engine;

namespace Skrypt.Library {
    public delegate SkryptObject SkryptDelegate(SkryptObject self, SkryptObject[] input);

    public class SkryptMethod : SkryptObject {
        public string ReturnType;

        public virtual SkryptObject Execute (SkryptEngine engine, SkryptObject Self, SkryptObject[] parameters, ScopeContext scope) {
            return null;
        }

        public override string ToString() {
            return base.ToString();
        }
    }

    public class UserMethod : SkryptMethod {
        public Node BlockNode;
        public string Signature;
        public string CallName;
        public List<string> Parameters = new List<string>();

        public override SkryptObject Execute(SkryptEngine engine, SkryptObject Self, SkryptObject[] parameters, ScopeContext scope) {
            ScopeContext ResultingScope = engine.executor.ExecuteBlock(BlockNode, scope, new SubContext {InMethod = true, Method = this});

            SkryptObject ReturnVariable = ResultingScope.subContext.ReturnObject ?? new Native.System.Void();

            return ReturnVariable;
        }
    }

    public class SharpMethod : SkryptMethod {
        public SkryptDelegate method;

        public override SkryptObject Execute(SkryptEngine engine, SkryptObject Self, SkryptObject[] parameters, ScopeContext scope) {
            var returnValue = method(Self, parameters);

            if (returnValue.GetType().IsSubclassOf(typeof(SkryptType))) {
                returnValue.SetPropertiesTo(engine.Types[((SkryptType)returnValue).TypeName]);
            }

            return returnValue;
        }
    }
}
