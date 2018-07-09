﻿using System.Collections.Generic;
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
        public SubContext SubContext = new SubContext();
        public string Type = "";
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, SkryptObject> Types { get; set; } = new Dictionary<string, SkryptObject>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }

        public void AddVariable(string name, SkryptObject value, bool isConstant = false)
        {
            Variables[name] = new Variable
            {
                Name = name,
                Value = value,
                IsConstant = isConstant,
                Scope = this
            };
        }

        public void AddType(string Name, SkryptObject Value, bool IsConstant = false) {
            Types[Name] = Value;
        }
    }
}