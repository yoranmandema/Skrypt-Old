using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Skrypt.Tokenization {
    /// <summary>
    /// The token processor class.
    /// Contains all methods to process existing tokens
    /// </summary>
    static class TokenProcessor {
        /// <summary>
        /// Unescape string and remove outer " characters
        /// </summary>
        static void ProcessStringToken (Token token) {
            token.Value = token.Value.Substring(1, token.Value.Length - 2);    
            token.Value = Regex.Unescape(token.Value);
        }

        /// <summary>
        /// Process all tokens in a list
        /// </summary>      
        static public void ProcessTokens (List<Token> Tokens) {
            foreach (Token token in Tokens) {
                switch (token.Type) {
                    case TokenTypes.StringLiteral:
                        ProcessStringToken(token);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
