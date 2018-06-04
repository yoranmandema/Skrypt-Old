using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Interpreter {
    class Token {
        public string Value { get; set; } = null;
        public string Type { get; set; } = null;

        public int Start { get; set; }
        public int End { get; set; }
    }
}
