using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
    }

    public class SkryptObject
    {
        public List<Operation> Operations = new List<Operation>();
        public List<SkryptProperty> Properties = new List<SkryptProperty>();
        public string Name { get; set; }


        public Operation GetOperation(string n, Type tl, Type tr, List<Operation> ops)
        {
            for (var i = 0; i < ops.Count; i++)
            {
                var op = ops[i];

                if (op.Name != n) continue;

                if (!(tl.IsSubclassOf(op.TypeLeft) || tl == op.TypeLeft)) continue;

                if (op.TypeRight != null)
                    if (!(tr.IsSubclassOf(op.TypeRight) || tr == op.TypeRight))
                        continue;

                return op;
            }

            return null;
        }

        public SkryptObject SetPropertiesTo(SkryptObject Object)
        {
            Properties = new List<SkryptProperty>(Object.Properties);
            return this;
        }

        public virtual Native.System.Boolean ToBoolean()
        {
            return true;
        }

        public virtual SkryptObject Clone()
        {
            return (SkryptObject) MemberwiseClone();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }
    }
}