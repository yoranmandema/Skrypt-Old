using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class ArrayNode : Node {
        public List<Node> Values { get; set; }
        public override TokenTypes Type => TokenTypes.ArrayLiteral;
        public override string Body => "Array";
    }
}