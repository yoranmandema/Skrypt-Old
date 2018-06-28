using System;
using System.Collections.Generic;
using System.Text;
using Skrypt.Library;

namespace Skrypt.Execution {
    public class ScopeContext {
        public string Type = "";
        public Dictionary<string, SkryptObject> Variables { get; set; } = new Dictionary<string, SkryptObject>();

        public string DictToString<T, V>(IEnumerable<KeyValuePair<T, V>> items, string format) {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format;

            StringBuilder itemString = new StringBuilder();
            foreach (var item in items)
                itemString.AppendFormat(format, item.Key, item.Value);

            return itemString.ToString();
        }

        public override string ToString() {

            string output = "Type:\n" + Type;
            output = "Variables:\n";

            output += DictToString<string, SkryptObject>(Variables, null);

            return output;
        }

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
