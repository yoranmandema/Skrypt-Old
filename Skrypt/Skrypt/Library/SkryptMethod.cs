using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Parsing;
using Skrypt.Execution;
using Skrypt.Engine;

namespace Skrypt.Library {
    public delegate SkryptObject SkryptDelegate(params SkryptObject[] input);

    public class SkryptMethod {
        public string Name;
        public string ReturnType;

        public virtual SkryptObject Execute (SkryptEngine engine, SkryptObject[] parameters, ScopeContext scope) {
            return null;
        }
    }

    public class UserMethod : SkryptMethod {
        public Node BlockNode;

        public override SkryptObject Execute(SkryptEngine engine, SkryptObject[] parameters, ScopeContext scope) {
            SkryptObject ReturnVariable = null;
            
            engine.executor.ExecuteBlock(BlockNode, scope, ref ReturnVariable);

            return ReturnVariable;
        }
    }

    public class SharpMethod : SkryptMethod {
        public SkryptDelegate method;

        public override SkryptObject Execute(SkryptEngine engine, SkryptObject[] parameters, ScopeContext scope) {
            return method(parameters);
        }
    }
}
