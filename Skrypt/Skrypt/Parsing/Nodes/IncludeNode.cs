using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class IncludeNode : Node {
        public override TokenTypes Type => TokenTypes.Include;
        public override string Body => "Include";
        public string Path { get; set; }
    }
}
