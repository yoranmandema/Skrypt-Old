using System.Collections.Generic;
using Newtonsoft.Json;
using Skrypt.Library;
using System;
using Skrypt.Parsing;
using Skrypt.Engine;

namespace Skrypt.Execution
{
    public class SubContext
    {
        public SkryptObject Caller {get; set;}
        public bool GettingCaller = false;
        public bool InLoop = false;
        public bool BrokeLoop = false;
        public bool SkippedLoop = false;
        public bool InMethod = false;
        public bool StrictlyLocal = false;
        public bool InClassDeclaration = true;
        public UserMethod Method = null;
        public SkryptObject ReturnObject = null;
        public SkryptObject ParentClass = null;

        public void Merge(SubContext other) {
            GettingCaller = GettingCaller || other.GettingCaller;
            InLoop = InLoop || other.InLoop;
            BrokeLoop = BrokeLoop || other.BrokeLoop;
            SkippedLoop = SkippedLoop || other.SkippedLoop;
            InMethod = InMethod || other.InMethod;
            StrictlyLocal = StrictlyLocal || other.StrictlyLocal;
            InClassDeclaration = InClassDeclaration || other.InClassDeclaration;
            ParentClass = other.ParentClass;
        }
    }

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
        public CallStack CallStack { get; set; }
        public ScopeProperties Properties = 0;
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, SkryptObject> Types { get; set; } = new Dictionary<string, SkryptObject>();
        public string Type { get; set; } = "";
        public bool StrictlyLocal;
        public SkryptObject Caller { get; set; }
        public UserMethod Method { get; set; }
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