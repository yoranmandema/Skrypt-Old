using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Skrypt.Engine;
using Skrypt.Parsing;
using Skrypt.Library;

namespace Skrypt.Tokenization
{
    /// <summary>
    ///     The token processor class.
    ///     Contains all methods to process existing tokens
    /// </summary>
    public class TokenProcessor
    {
        private readonly SkryptEngine _engine;

        public TokenProcessor(SkryptEngine e) {
            _engine = e;
        }

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
                str = "" + Decimal.Parse(str, NumberStyles.Any);

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

        private Operator FindOperator (Token token) {
            var group = ExpressionParser.OperatorPrecedence.Find(x => x.Operators.Find(y => y.Operation == token.Value) != null);

            return group.Operators.Find(y => y.Operation == token.Value);
        }
        
        private void InsertEnd (List<Token> tokens, int index, int start = 0) {
            Console.WriteLine("expression: " + ExpressionParser.TokenString(tokens.GetRange(start, tokens.Count - start)));

            tokens.Insert(index, new Token { Value = "EndOfExpression", Type = TokenTypes.EndOfExpression });
        }

        /// <summary>
        ///     Process all tokens in a list
        /// </summary>
        public void ProcessTokens(List<Token> tokens)
        {
            for(int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                
                switch (token.Type) {
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

            var expression = new List<Token>();
            var expressionStart = 0;
            Token previousToken = null;

            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                var unmodifiedI = i;

                if (i > 0) {
                    previousToken = tokens[i-1];
                }

                if (token.Value == "(" && token.Type == TokenTypes.Punctuator) {
                    i = _engine.ExpressionParser.SkipFromTo("(", ")", tokens, i).End;
                }
                else if (token.Value == "{" && token.Type == TokenTypes.Punctuator) {
                    i = _engine.ExpressionParser.SkipFromTo("{", "}", tokens, i).End;
                }
                else if (token.Value == "[" && token.Type == TokenTypes.Punctuator) {
                    i = _engine.ExpressionParser.SkipFromTo("[", "]", tokens, i).End; 
                }

                if (unmodifiedI != i) {
                    //i++;
                    Console.WriteLine(ExpressionParser.TokenString(tokens.GetRange(unmodifiedI, i - unmodifiedI)));

                    //continue;
                }

                var needsValueAfter = false;
                Action loop = () => {
                    foreach (var op in ExpressionParser.OperatorPrecedence) {
                        foreach (var Operator in op.Operators) {
                            //if (Operator.Operation == "(") {
                            //    continue;
                            //}

                            if (token.Value == Operator.Operation && token.Type == TokenTypes.Punctuator) {
                                if (op.Members == 1) {
                                    if (!op.IsPostfix) {
                                        needsValueAfter = true;
                                    }
                                }
                                else if (op.Members == 2) {
                                    needsValueAfter = true;
                                }

                                return;
                            }
                        }
                    }
                };
                loop();

                if (!needsValueAfter && previousToken != null) {
                    if (previousToken.IsValuable() && token.IsValuable()) {
                        InsertEnd(tokens, i, expressionStart);
                        expressionStart = i;
                        i--;
                    }

                    if (previousToken.IsGroup() && token.IsValuable()) {
                        InsertEnd(tokens, i, expressionStart);
                        expressionStart = i;
                        i--;
                    }
                }
            }
        }
    }
}