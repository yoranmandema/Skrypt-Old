using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Library;

namespace Skrypt.Engine {
    public class CallStack {
        public string Descriptor { get; set; }
        public Token Token { get; set; }
        public CallStack Parent { get; set; }

        public CallStack (string descriptor, Token token, CallStack callStack) {
            Descriptor = descriptor;
            Token = token;
            Parent = callStack;
        }

        public override string ToString() {
            var str = $"\tat {Descriptor}";

            if (Token != null) str += $"\t(line: {Token.Line}, colom: {Token.Colom})";

            if (Parent != null) {
                str += "\n" + Parent;
            }

            return str;
        }
    }
}
