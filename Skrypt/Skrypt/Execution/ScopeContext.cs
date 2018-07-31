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
        private SkryptObject _caller;
        public SkryptObject Caller {
            get {
                return _caller;
            }
            set {
                //Console.WriteLine("Caller set to: " + value);
                _caller = value;
            }
        }
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
        //public SkryptObject CreatedClass = null;

        public void Merge(SubContext other) {
            //Console.WriteLine("bef: " + JsonConvert.SerializeObject(this, Formatting.None).Replace("\"", ""));

            GettingCaller = GettingCaller || other.GettingCaller;
            InLoop = InLoop || other.InLoop;
            BrokeLoop = BrokeLoop || other.BrokeLoop;
            SkippedLoop = SkippedLoop || other.SkippedLoop;
            InMethod = InMethod || other.InMethod;
            StrictlyLocal = StrictlyLocal || other.StrictlyLocal;
            InClassDeclaration = InClassDeclaration || other.InClassDeclaration;
            ParentClass = other.ParentClass;
            //Console.WriteLine("aft: " + JsonConvert.SerializeObject(this, Formatting.None).Replace("\"", ""));
        }
    }

    public class ScopeContext
    {
        [JsonIgnore]public ScopeContext ParentScope = null;
        public CallStack CallStack { get; set; }
        public SubContext SubContext = new SubContext();
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, SkryptObject> Types { get; set; } = new Dictionary<string, SkryptObject>();
        public string Type { get; set; } = "";

        public void AddVariable(string name, SkryptObject value, Modifier modifiers = Modifier.None)
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