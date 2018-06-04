using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Tokenization {
    class Token {
        public string Value { get; set; }
        public string Type { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        public override string ToString() {
            return "Type: " + Type + ", Value: " + Value + " (" + Start + "," + End + ")";
        }
    }
}
