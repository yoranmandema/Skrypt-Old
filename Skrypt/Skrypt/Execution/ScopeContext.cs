using System.Collections.Generic;
using Newtonsoft.Json;
using Skrypt.Library;
using System;
using Skrypt.Parsing;
using Skrypt.Engine;

namespace Skrypt.Execution
{
    public enum ScopeProperties {
        GettingCaller = 1,
        InLoop = 2,
        BrokeLoop = 4,
        SkippedLoop = 8,
        InMethod = 16,
        InClassDeclaration = 32,
    } 

    public class ScopeContext
    {
        [JsonIgnore]public ScopeContext ParentScope = null;
        [JsonIgnore]public List<ScopeContext> SubScopes = new List<ScopeContext>();
        public int Start;
        public int End;
        public bool StrictlyLocal;
        public ScopeProperties Properties = 0;
        public CallStack CallStack { get; set; }
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, SkryptObject> Types { get; set; } = new Dictionary<string, SkryptObject>();
        public SkryptObject Caller { get; set; }
        public SkryptObject ReturnObject { get; set; }
        public SkryptObject ParentClass { get; set; }

        public void SetVariable(string name, SkryptObject value, Modifier modifiers = Modifier.None)
        {
            Variables[name] = new Variable
            {
                Name = name,
                Value = value,
                Modifiers = modifiers,
                Scope = this
            };
        }

        public void AddType(string Name, SkryptObject Value, bool IsConstant = false) {
            Types[Name] = Value;
        }
    }
}