﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Skrypt.Engine;
using Skrypt.Execution;
using S = Skrypt.Library.Native.System;

namespace Skrypt.Library
{
    public delegate SkryptObject OperationDelegate(SkryptObject[] input);

    public class Operation
    {
        public string Name;
        public OperationDelegate OperationDelegate;
        public Type TypeLeft;
        public Type TypeRight;

        public Operation(string n, Type tl, Type tr, OperationDelegate del)
        {
            Name = n;
            TypeLeft = tl;
            TypeRight = tr;
            OperationDelegate = del;
        }

        public Operation(string n, Type tl, OperationDelegate del)
        {
            Name = n;
            TypeLeft = tl;
            TypeRight = null;
            OperationDelegate = del;
        }

        public override string ToString() {
            return $"{TypeLeft.Name} {Name} {TypeRight.Name}";
        }
    }

    public class SkryptObject {
        [JsonIgnore]
        public List<Operation> Operations = new List<Operation>
           {
                new Operation("not", typeof(SkryptObject),
                    input =>
                    {
                        return new S.Boolean(input[0].GetType() != typeof(S.Null));
                    }),
                new Operation("equal", typeof(SkryptObject), typeof(SkryptObject),
                    input =>
                    {
                        return new S.Boolean(ReferenceEquals(input[0],input[1]));
                    }),
                new Operation("notequal", typeof(SkryptObject), typeof(SkryptObject),
                    input =>
                    {
                        return new S.Boolean(ReferenceEquals(input[0],input[1]));
                    }),
            };

        [JsonIgnore] public List<SkryptProperty> Properties = new List<SkryptProperty>();
        [JsonIgnore] public ScopeContext ScopeContext { get; set; }
        [JsonIgnore] public SkryptEngine Engine { get; set; }
        public virtual string Name { get; set; }

        public Operation GetOperation(string n, Type tl, Type tr, List<Operation> ops)
        {
            for (var i = 0; i < ops.Count; i++)
            {
                var op = ops[i];

                if (op.Name != n) continue;

                if (!op.TypeLeft.IsAssignableFrom(tl)) continue;

                if (op.TypeRight != null)
                    if (!op.TypeRight.IsAssignableFrom(tr))
                        continue;

                return op;
            }

            return null;
        }

        public SkryptObject Clone () {
            return (SkryptObject)MemberwiseClone();
        }

        public SkryptObject SetPropertiesTo (SkryptObject Object) {
            Properties = new List<SkryptProperty>(Object.Properties);
            return this;
        }

        public SkryptObject GetProperty(string name) {
            return Properties.Find(x => x.Name == name).Value;
        }

        public void SetProperty(string name, SkryptObject value) {
            Properties.Find(x => x.Name == name).Value = value;
        }

        public virtual Native.System.Boolean ToBoolean()
        {
            return true;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }

        public override string ToString() {
            string str = Name;

            if (Properties.Count > 0) {
                str += " {\n";

                foreach (var p in Properties) {
                    str += $"\t{p.Name}: {p.Value}\n";
                }

                str += "}";
            }

            return str;
        }
    }
}