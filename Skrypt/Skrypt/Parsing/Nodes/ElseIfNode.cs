using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class ElseIfNode : Node, IBranchNode {
        public Node Condition { get; set; }
        public Node Block { get; set; }

        public override TokenTypes Type => TokenTypes.Statement;
    }
}