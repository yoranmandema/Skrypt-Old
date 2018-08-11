using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class ConditionalNode : Node {
        public override TokenTypes Type => TokenTypes.Conditional;
        public override string Body => "Conditional";
        public Node Condition { get; set; }
        public Node Pass { get; set; }
        public Node Fail { get; set; }
    }
}
