using System;
using System.Collections.Generic;
using Skrypt.Library;

namespace Skrypt.Execution {
    public class ScopeContext {
        public string Type = "";
        public Dictionary<string, SkryptObject> Variables { get; set; } = new Dictionary<string, SkryptObject>();

        public ScopeContext(ScopeContext Copy = null) {
            if (Copy != null) {
                Variables = new Dictionary<string, SkryptObject>(Copy.Variables);
                Type = Copy.Type;
            }
        }

        public ScopeContext WithType (string Type) {
            ScopeContext newScope = new ScopeContext {
                Type = Type,
                Variables = new Dictionary<string, SkryptObject>(this.Variables)
            };

            return newScope;
        }

        public ScopeContext Copy () {
            ScopeContext newScope = new ScopeContext {
                Type = this.Type,
                Variables = new Dictionary<string, SkryptObject>(this.Variables)
            };

            return newScope;
        }
    }
}
