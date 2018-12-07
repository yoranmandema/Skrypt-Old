using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class BlockNode : Node {
        public override TokenTypes Type => TokenTypes.Block;
        public override string Body => "Block";
    }
}
