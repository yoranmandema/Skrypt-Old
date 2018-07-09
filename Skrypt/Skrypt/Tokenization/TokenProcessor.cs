using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Skrypt.Tokenization
{
    /// <summary>
    ///     The token processor class.
    ///     Contains all methods to process existing tokens
    /// </summary>
    internal static class TokenProcessor
    {
        /// <summary>
        ///     Unescape string and remove outer " characters
        /// </summary>
        private static void ProcessStringToken(Token token)
        {
            token.Value = token.Value.Substring(1, token.Value.Length - 2);
            token.Value = Regex.Unescape(token.Value);
        }

        /// <summary>
        ///     Process all tokens in a list
        /// </summary>
        public static void ProcessTokens(List<Token> tokens)
        {
            foreach (var token in tokens)
                switch (token.Type)
                {
                    case TokenTypes.StringLiteral:
                        ProcessStringToken(token);
                        break;
                    default:
                        break;
                }
        }
    }
}