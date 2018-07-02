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
        [JsonIgnore]
        public ScopeContext Scope { get; set; }
        public List<SkryptProperty> Properties = new List<SkryptProperty>();

        public virtual bool ToBoolean () {
            return true;
        }

        //public virtual SkryptObject Add(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject Subtract(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject Multiply(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject Divide(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject Modulo(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject Power(SkryptObject A, SkryptObject B) { throw new Exception(); }

        //public virtual SkryptObject Lesser(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject Greater(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject LesserOrEqual(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject GreaterOrEqual(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject Equal(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject NotEqual(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject And(SkryptObject A, SkryptObject B) { throw new Exception(); }
        //public virtual SkryptObject Or(SkryptObject A, SkryptObject B) { throw new Exception(); }

        //public virtual SkryptObject Negate(SkryptObject A) { throw new Exception(); }
        //public virtual SkryptObject Not(SkryptObject A) { throw new Exception(); }
        //public virtual SkryptObject PostIncremenet(SkryptObject A) { throw new Exception(); }
        //public virtual SkryptObject PreIncrement(SkryptObject A) { throw new Exception(); }

    }
}
