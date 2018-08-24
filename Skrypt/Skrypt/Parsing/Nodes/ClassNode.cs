using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    public class ClassNode : Node {
        public override TokenTypes Type => TokenTypes.ClassDeclaration;
        public override string Body => "ClassDeclaration";
        public Node BodyNode { get; set; }
        public Node InheritNode { get; set; }
        public string Name { get; set; }
    }
}
