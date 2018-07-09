using System;
using System.Collections.Generic;
using System.Text;
using Skrypt.Library;
using Newtonsoft.Json;

namespace Skrypt.Execution {
    public class SubContext {
        public bool InLoop = false;
        public bool InMethod = false;
        public UserMethod Method = null;
        public SkryptObject ReturnObject = null;
        public bool GettingCaller = false;
        public SkryptObject Caller;
    }

    public class ScopeContext {
        public string Type = "";
        public SubContext subContext = new SubContext();
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, SkryptObject> Types { get; set; } = new Dictionary<string, SkryptObject>();
        public ScopeContext ParentScope = null;

        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }

        public void AddVariable(string Name, SkryptObject Value, bool IsConstant = false) {
            Variables[Name] = new Variable {
                Name = Name,
                Value = Value,
                IsConstant = IsConstant,
                Scope = this
            };
        }

        public void AddType(string Name, SkryptObject Value, bool IsConstant = false) {
            Types[Name] = Value;
        }
    }
}

