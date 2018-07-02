using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Execution;
using Newtonsoft.Json;

namespace Skrypt.Library {
    public class SkryptObject {
        public string Name { get; set; }
        //[JsonIgnore]
        //public ScopeContext Scope { get; set; }
        public List<SkryptProperty> Properties = new List<SkryptProperty>();

        public delegate SkryptObject operation(SkryptObject A, SkryptObject B);
        public Dictionary<string, operation> Operations;// = new Dictionary<string, operation>();

        public virtual SkryptObject _Add(SkryptObject X) {throw new Exception();}
        public virtual SkryptObject _Subtract(SkryptObject X) { throw new Exception(); }
        public virtual SkryptObject _Multiply(SkryptObject X) { throw new Exception(); }
        public virtual SkryptObject _Divide(SkryptObject X) { throw new Exception(); }
        public virtual SkryptObject _Modulo(SkryptObject X) { throw new Exception(); }

        public virtual SkryptObject _Lesser(SkryptObject X) { throw new Exception(); }
        public virtual SkryptObject _Greater(SkryptObject X) { throw new Exception(); }
        public virtual SkryptObject _Equal(SkryptObject X) { throw new Exception(); }

        public virtual SkryptObject _PostIncrement() { throw new Exception(); }

        public virtual bool ToBoolean () {
            return true;
        }

        public string toJSON () {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }
    }
}
