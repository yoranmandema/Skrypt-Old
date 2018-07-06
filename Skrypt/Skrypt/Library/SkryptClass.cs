using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Execution;
using Newtonsoft.Json;

namespace Skrypt.Library {
    public delegate SkryptObject OperationDelegate(SkryptObject[] Input);

    public class Operation {
        public string Name;
        public Type TypeLeft;
        public Type TypeRight;
        public OperationDelegate operation;

        public Operation(string N, Type TL, Type TR, OperationDelegate DEL) {
            Name = N;
            TypeLeft = TL;
            TypeRight = TR;
            operation = DEL;
        }

        public Operation(string N, Type TL, OperationDelegate DEL) {
            Name = N;
            TypeLeft = TL;
            TypeRight = null;
            operation = DEL;
        }
    }

    public class SkryptObject {
        public string Name { get; set; }
        public List<SkryptProperty> Properties = new List<SkryptProperty>();

        public List<Operation> Operations = new List<Operation>();

        public Operation GetOperation (string N, Type TL, Type TR, List<Operation> Ops) {
            for (int i = 0; i <Ops.Count; i++) {
                Operation Op = Ops[i];

                if (Op.Name != N) {
                    continue;
                }

                if (!(TL.IsSubclassOf(Op.TypeLeft) || TL == Op.TypeLeft)) {
                    continue;
                }

                if (Op.TypeRight != null) {
                    if (!(TR.IsSubclassOf(Op.TypeRight) || TR == Op.TypeRight)) {
                        continue;
                    }
                }

                return Op;
            }

            return null;
        }

        public SkryptObject SetPropertiesTo (SkryptObject Object) {
            Properties = new List<SkryptProperty>(Object.Properties);
            return this;
        }

        public virtual Native.System.Boolean ToBoolean () {
            return true;
        }

        public virtual SkryptObject Clone () {
            return (SkryptObject)MemberwiseClone();
        }

        public string toJSON () {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }
    }
}
