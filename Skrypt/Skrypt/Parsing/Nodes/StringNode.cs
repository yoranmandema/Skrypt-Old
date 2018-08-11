using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class StringNode : Node {
        public string Value { get; set; }
        public override TokenTypes Type => TokenTypes.StringLiteral;
    }
}
