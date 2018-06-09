using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    class ParserState {
        public Parser parser;
        public List<Token> UnparsedTokens;

        public void SetState(ParserState State) {
            parser.State = State;
        }
    }
}
