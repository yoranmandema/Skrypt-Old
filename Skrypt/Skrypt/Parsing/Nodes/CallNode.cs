using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class CallNode : Node {
        public override TokenTypes Type => TokenTypes.Call;
        public override string Body => "Call";
        public Node Getter { get; set; }
        public List<Node> Arguments { get; set; }
    }
}
