using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class IdentifierNode : Node {
        public override TokenTypes Type => TokenTypes.Identifier;
    }
}
