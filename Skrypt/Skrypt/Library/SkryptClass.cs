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

        public virtual bool ToBoolean () {
            return true;
        }

        public string toJSON () {
            return JsonConvert.SerializeObject(this, Formatting.Indented).Replace("\"", "");
        }
    }
}
