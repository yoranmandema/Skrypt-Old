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
        private void ProcessStringToken(Token token)
        {
            token.Value = token.Value.Substring(1, token.Value.Length - 2);
            token.Value = Regex.Unescape(token.Value);
        }

        private void ProcessNumericToken(Token token) {
            string str = "";

            if (token.Type == TokenTypes.NumericLiteral) {
                str = token.Value.Replace("e", "E");
                str = "" + Decimal.Parse(str, NumberStyles.Any);

                token.Value = str;
            }
            else if (token.Type == TokenTypes.BinaryLiteral) {
                if (str.Length < 3) {
                    _engine.ThrowError("Syntax error, invalid binary literal found.", token);
                }

                str = token.Value.Substring(2);
                str = "" + Convert.ToInt32(str, 2);

                token.Value = str;
            }
            else if (token.Type == TokenTypes.HexadecimalLiteral) {
                if (str.Length < 3) {
                    _engine.ThrowError("Syntax error, invalid hexadecimal literal found.", token);
                }

                str = token.Value.Substring(2);
                str = "" + int.Parse(str, NumberStyles.HexNumber);
            }

            token.Value = str;
            token.Type = TokenTypes.NumericLiteral;
        }

        private Operator FindOperator (Token token) {
            var group = ExpressionParser.OperatorPrecedence.Find(x => {
                var f = x.Operators.Find(y => y.Operation == token.Value);
                Console.WriteLine(f);

                return f != null;
            });

            return group?.Operators.Find(y => y.Operation == token.Value);
        }
        
        private void InsertEnd (List<Token> tokens, int index) {
            tokens.Insert(index, new Token { Value = "EndOfExpression", Type = TokenTypes.EndOfExpression});
        }

        private void ProcessSurrounded (string open, string close, List<Token> tokens, ref int index) {
            var skip = _engine.ExpressionParser.SkipFromTo(open, close, tokens, index);

            var beforeCount = skip.Delta;
            var blockTokens = tokens.GetRange(skip.Start + 1, skip.Delta - 1);

            tokens.RemoveRange(skip.Start + 1, skip.Delta - 1);

            SetEndOfExpressions(blockTokens);

            tokens.InsertRange(skip.Start + 1, blockTokens);

            index = _engine.ExpressionParser.SkipFromTo(open, close, tokens, index).End;
        }

        public void SetEndOfExpressions (List<Token> tokens) {
            var expression = new List<Token>();
            Token previousToken = null;
            Token nextToken = null;

            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                var unmodifiedI = i;

                if (i > 0) {
                    previousToken = tokens[i - 1];
                }

                if (i < tokens.Count - 1) {
                    nextToken = tokens[i + 1];
                }

                if (token.Type == TokenTypes.Punctuator) {
                    switch (token.Value) {
                        case "{":
                            ProcessSurrounded("{", "}", tokens, ref i);
                            break;
                        case "(":
                            ProcessSurrounded("(", ")", tokens, ref i);
                            break;
                        case "[":
                            ProcessSurrounded("[", "]", tokens, ref i);
                            break;
                    }

                    switch (token.Value) {
                        case "}":
                        case ")":
                        case "]":
                            _engine.ThrowError($"Opening token for '{token.Value}' not found.", token);
                            break;
                    }
                }

                if (unmodifiedI != i) {
                    continue;
                }

                var needsValueAfter = false;
                var isOperator = false;
                var isPostfix = false;
                var unaryOperator = false;
                Action loop = () => {
                    foreach (var op in ExpressionParser.OperatorPrecedence) {
                        foreach (var Operator in op.Operators) {
                            if (token.Value == Operator.Operation && token.Type == TokenTypes.Punctuator) {
                                if (op.Members == 1) {
                                    if (!op.IsPostfix) {
                                        needsValueAfter = true;
                                    }
                                    else {
                                        isPostfix = true;
                                    }

                                    unaryOperator = true;
                                }
                                else if (op.Members == 2) {
                                    needsValueAfter = true;
                                }

                                if (Operator.Operation == "break" || Operator.Operation == "return" || Operator.Operation == "continue") {
                                    needsValueAfter = false;
                                }

                                isOperator = true;

                                return;
                            }
                        }
                    }
                };
                loop();

                if (unaryOperator) {
                    if (isPostfix) {
                        if (nextToken != null) {
                            if (nextToken.IsValuable()) {
                                InsertEnd(tokens, isPostfix ? i + 1 : i);
                                i++;
                            }

                            var nextOp = FindOperator(nextToken);

                            if (nextOp != null && FindOperator(nextToken).Members == 1) {
                                InsertEnd(tokens, isPostfix ? i + 1 : i);
                                i++;
                            }
                        }
                    } else {
                        if (previousToken != null) {
                            if (previousToken.IsValuable()) {
                                InsertEnd(tokens, isPostfix ? i + 1 : i);
                                i++;
                            }

                            var prevOp = FindOperator(previousToken);

                            if (prevOp != null && prevOp.Members == 1) {
                                InsertEnd(tokens, isPostfix ? i + 1 : i);
                                i++;
                            }
                        }
                    }
                }

                if (!needsValueAfter && previousToken != null) {
                    if (previousToken.IsValuable() && token.IsValuable()) {
                        InsertEnd(tokens, i);
                        i--;
                    }

                    if (previousToken.IsGroup() && token.IsValuable()) {
                        InsertEnd(tokens, i);
                        i--;
                    }

                    if (previousToken.IsValuable() && token.IsKeyword()) {
                        InsertEnd(tokens, i);
                        i--;
                    }

                    if (previousToken.IsGroup() && token.IsKeyword() && (token.Value != "else" && token.Value != "elseif")) {
                        InsertEnd(tokens, i);
                        i--;
                    }

                    if ((token.Value == "using" || token.Value == "return" || token.Value == "continue" || token.Value == "break") && token.Type == TokenTypes.Punctuator) {
                        InsertEnd(tokens, i);
                        i++;
                    }

                    if ((previousToken.Value == "continue" || previousToken.Value == "break") && previousToken.Type == TokenTypes.Punctuator) {
                        InsertEnd(tokens, i);
                        i++;
                    }
                }
            }
        }

        /// <summary>
        ///     Process all tokens in a list
        /// </summary>
        public void ProcessTokens(List<Token> tokens)
        {
            for (int i = 0; i < tokens.Count; i++) {
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

            SetEndOfExpressions(tokens);      
        }
    }
}