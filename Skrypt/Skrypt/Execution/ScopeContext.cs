using System.Collections.Generic;
using Newtonsoft.Json;
using Skrypt.Library;

namespace Skrypt.Execution
{
    public class SubContext
    {
        public SkryptObject Caller;
        public bool GettingCaller = false;
        public bool InLoop = false;
        public bool InMethod = false;
        public UserMethod Method = null;
        public SkryptObject ReturnObject = null;
    }

    public class ScopeContext
    {
        public ScopeContext ParentScope = null;
        public SubContext subContext = new SubContext();
        public string Type = "";
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }

        public void AddVariable(string Name, SkryptObject Value, bool IsConstant = false)
        {
            Variables[Name] = new Variable
            {
                Name = Name,
                Value = Value,
                IsConstant = IsConstant,
                Scope = this
            };
        }
    }
}