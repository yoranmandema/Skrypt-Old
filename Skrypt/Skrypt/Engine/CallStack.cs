using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Library;

namespace Skrypt.Engine {
    public class CallStack {
        public string Path { get; set; }
        public string Descriptor { get; set; }
        public Token Token { get; set; }
        public CallStack Parent { get; set; }

        public CallStack (string descriptor, Token token, CallStack callStack, string path) {
            Descriptor = descriptor;
            Token = token;
            Parent = callStack;
            Path = path;
        }

        public override string ToString() {
            var str = $"\tat {Descriptor}";

            if (!string.IsNullOrEmpty(Path)) str += $" in: {Path} ";

            if (Token != null) str += $"({Token.Line}, {Token.Column}, {Token.LineEnd}, {Token.ColumnEnd})";

            if (Parent != null) {
                var parentMessage = Parent.ToString();

                if (parentMessage.StartsWith(str)) {
                    return parentMessage; 
                } else {
                    str += "\n" + parentMessage;
                }
            }

            return str;
        }
    }
}
