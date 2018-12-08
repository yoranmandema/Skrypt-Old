using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class ImportNode : Node {
        public override TokenTypes Type => TokenTypes.Import;
        public override string Body => "Import";
        public Node Getter { get; set; }
    }
}
