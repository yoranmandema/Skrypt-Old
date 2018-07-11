using System;
using System.Collections.Generic;
using System.Globalization;
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

        private static void ProcessNumericToken(Token token) {
            string str = "";

            if (token.Type == TokenTypes.NumericLiteral) {
                str = token.Value.Replace("e", "E");
                str = "" + Decimal.Parse(str, System.Globalization.NumberStyles.Any);

                token.Value = str;
            }
            else if (token.Type == TokenTypes.BinaryLiteral) {
                str = token.Value.Substring(2);
                str = "" + Convert.ToInt32(str, 2);

                token.Value = str;
            }
            else if (token.Type == TokenTypes.HexadecimalLiteral) {
                str = token.Value.Substring(2);
                str = "" + int.Parse(str, NumberStyles.HexNumber);
            }

            token.Value = str;
            token.Type = TokenTypes.NumericLiteral;
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
                    case TokenTypes.BinaryLiteral:
                    case TokenTypes.HexadecimalLiteral:
                    case TokenTypes.NumericLiteral:
                        ProcessNumericToken(token);
                        break;
                    default:
                        break;
                }
        }
    }
}