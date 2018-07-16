using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library;
using Newtonsoft.Json;
using Skrypt.Parsing;

namespace Skrypt.Execution {
    public class Variable {
        public string Name;
        public Modifier Modifiers;
        [JsonIgnore]
        public ScopeContext Scope;
        public SkryptObject Value;
    }
}
