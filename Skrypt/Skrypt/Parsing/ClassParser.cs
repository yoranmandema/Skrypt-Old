using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Parsing;
using Skrypt.Library;
using Skrypt.Engine;

namespace Skrypt.Parsing {
    /// <summary>
    /// The class parser class.
    /// Contains all methods to parse class definition code
    /// </summary>
    public class ClassParser {
        public ParseResult Parse(List<Token> Tokens) {
            // Create main node
            Node Node = new Node ();
            int index = 0;

            return new ParseResult { node = Node, delta = index };
        }
    }
}
