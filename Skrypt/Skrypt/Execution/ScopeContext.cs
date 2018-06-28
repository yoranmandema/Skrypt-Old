using System;
using System.Collections.Generic;
using System.Text;
using Skrypt.Library;
using Newtonsoft.Json;

namespace Skrypt.Execution {
    public class ScopeContext {
        public string Type = "";
        public Dictionary<string, SkryptObject> Variables { get; set; } = new Dictionary<string, SkryptObject>();
        public ScopeContext ParentScope = null;

        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }
    }
}

