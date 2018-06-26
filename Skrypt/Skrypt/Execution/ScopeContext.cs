using System;
using System.Collections.Generic;
using Skrypt.Library;

namespace Skrypt.Execution {
    public class ScopeContext {
        public Dictionary<string, SkryptObject> Variables { get; set; }

        public ScopeContext(Dictionary<string, SkryptObject> vars = null) {
            if (vars == null) {
                Variables = new Dictionary<string, SkryptObject>();
            }
            else {
                Variables = vars;
            }
        }
    }
}
