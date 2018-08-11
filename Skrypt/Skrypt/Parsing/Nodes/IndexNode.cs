using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class IndexNode : Node {
        public override TokenTypes Type => TokenTypes.Index;
        public override string Body => "Index";
        public Node Getter { get; set; }
        public List<Node> Arguments { get; set; }
    }
}
