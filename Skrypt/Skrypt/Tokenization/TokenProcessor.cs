using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Skrypt.Tokenization {
    static class TokenProcessor {
        static void ProcessStringToken (Token token) {
            token.Value = token.Value.Substring(1, token.Value.Length - 2);    
            token.Value = Regex.Unescape(token.Value);
        }

        static public void ProcessTokens (List<Token> Tokens) {
            foreach (Token token in Tokens) {
                switch (token.Type) {
                    case "StringLiteral":
                        ProcessStringToken(token);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
