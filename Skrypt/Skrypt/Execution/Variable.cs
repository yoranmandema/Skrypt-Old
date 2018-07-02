using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library;
using Newtonsoft.Json;

namespace Skrypt.Execution {
    public class Variable {
        public string Name;
        [JsonIgnore]
        public ScopeContext Scope;
        public SkryptObject Value;
    }
}
