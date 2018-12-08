using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Interpreter.Compiler {
    class CompileScope {
        Dictionary<string, int> identifiers = new Dictionary<string, int>();

        public int GetIndexFromIdentifier(string identifier) {
            if (identifiers.ContainsKey(identifier)) {
                return identifiers[identifier];
            } else {
                return identifiers[identifier] = identifiers.Count;
            }
        }
    }
}
