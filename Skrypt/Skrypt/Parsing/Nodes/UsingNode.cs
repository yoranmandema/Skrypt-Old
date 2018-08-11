using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class UsingNode : Node {
        public override TokenTypes Type => TokenTypes.Using;
        public override string Body => "Using";
        public Node Getter { get; set; }
    }
}
