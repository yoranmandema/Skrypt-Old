using System;
using System.Collections.Generic;
using Skrypt.Library;

namespace Skrypt.Execution {
    public class ScopeContext {
        public Dictionary<string, SkryptObject> Variables { get; set; } = new Dictionary<string, SkryptObject>();

        public ScopeContext(ScopeContext Copy = null) {
            if (Copy != null) {
                Variables = new Dictionary<string, SkryptObject>(Copy.Variables);
            }
        }
    }
}
