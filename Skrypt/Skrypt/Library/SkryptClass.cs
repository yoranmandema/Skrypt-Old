using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Skrypt.Engine;
using Skrypt.Execution;
using Skrypt.Library.Native;
using S = Skrypt.Library.Native.System;

namespace Skrypt.Library
{
    public delegate SkryptObject OperationDelegate(SkryptObject[] input, SkryptEngine engine);

    public class Operation
    {
        public Operators Operator;
        public OperationDelegate OperationDelegate;
        public Type TypeLeft;
        public Type TypeRight;

        public Operation(Operators o, Type tl, Type tr, OperationDelegate del)
        {
            Operator = o;
            TypeLeft = tl;
            TypeRight = tr;
            OperationDelegate = del;
        }

        public Operation(Operators o, Type tl, OperationDelegate del)
        {
            Operator = o;
            TypeLeft = tl;
            TypeRight = null;
            OperationDelegate = del;
        }
    }

    public class SkryptObject {
        [JsonIgnore]
        public List<Operation> Operations = new List<Operation>
           {
                new Operation(Operators.Not, typeof(SkryptObject),
                    (input, engine) =>
                    {
                        return engine.Create<S.Boolean>(input[0].GetType() != typeof(S.Null));
                    }),
                new Operation(Operators.Equal, typeof(SkryptObject), typeof(SkryptObject),
                    (input, engine) =>
                    {
                        return engine.Create<S.Boolean>(ReferenceEquals(input[0],input[1]));
                    }),
                new Operation(Operators.NotEqual, typeof(SkryptObject), typeof(SkryptObject),
                    (input, engine) =>
                    {
                        return engine.Create<S.Boolean>(!ReferenceEquals(input[0],input[1]));
                    }),
            };

        public List<SkryptProperty> Properties = new List<SkryptProperty>();
        [JsonIgnore] public ScopeContext ScopeContext { get; set; }
        [JsonIgnore] public SkryptEngine Engine { get; set; }
        public virtual string Name { get; set; }

        public static Operation GetOperation(Operators o, Type tl, Type tr, List<Operation> ops)
        {
            for (var i = 0; i < ops.Count; i++)
            {
                var op = ops[i];

                if (op.Operator != o) continue;

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
            Properties = new List<SkryptProperty>(Object.Properties.Copy());
            return this;
        }

        public SkryptObject GetProperty(string name) {
            return Properties.Find(x => x.Name == name)?.Value;
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

        public string StringTree (string indent = "") {
            string str = Name + " ";

            if (Properties.Count > 0) {
                str += "{\n";

                foreach (var p in Properties) {
                    if (p.Value != this)
                        str += indent + "\t" + $"{p.Name}: {(p.Value.GetType() == typeof(SkryptObject) ? p.Value.StringTree(indent + "\t") : p.Value.ToString())}\n";
                }

                str += indent + "}";
            }

            return str;
        }

        public static string GetString (SkryptObject o, SkryptEngine engine) {
            var s = o.ToString();
            var ts = o.GetProperty("ToString");

            if (ts != null && typeof(UserMethod).IsAssignableFrom(ts.GetType())) {
                s = ((UserMethod)ts).Execute(engine, o, null, engine.CurrentScope).ReturnObject.ToString();
            }

            return s;
        }

        public override string ToString() {
            return StringTree();
        }
    }
}