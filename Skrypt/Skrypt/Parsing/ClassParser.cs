using System;
using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Tokenization;

namespace Skrypt.Parsing
{
    /// <summary>
    ///     The class parser class.
    ///     Contains all methods to parse class definition code
    /// </summary>
    public class ClassParser
    {
        private readonly SkryptEngine _engine;

        public ClassParser(SkryptEngine e)
        {
            _engine = e;
        }

        Node ParseContents (List<Token> tokens, string name) {
            var constructorStart = -1;
            var clone = new List<Token>(tokens);

            for (int i = 0; i < clone.Count; i++) {
                var t = clone[i];

                if (t.Value == name) {
                    constructorStart = i;
                    continue;
                }

                if (constructorStart != -1) {
                    if ((t.Value == "(") && (t.Type == TokenTypes.Punctuator) && (i == (constructorStart + 1))) {
                        SkipInfo skip = _engine.ExpressionParser.SkipFromTo("(", ")", clone, i);

                        if (skip.End + 1 < clone.Count) {
                            var t2 = clone[skip.End + 1];

                            if ((t2.Value == "{") && (t2.Type == TokenTypes.Punctuator)) {
                                skip = _engine.ExpressionParser.SkipFromTo("{", "}", clone, i);

                                clone.RemoveAt(constructorStart);

                                clone.Insert(constructorStart, new Token {
                                    Value = "Constructor",
                                    Type = TokenTypes.Identifier,
                                    Start = clone[constructorStart].Start,
                                    End = clone[constructorStart].Start,
                                });

                                clone.Insert(constructorStart, new Token {
                                    Value = "fn",
                                    Type = TokenTypes.Keyword,
                                    Start = clone[constructorStart].Start,
                                    End = clone[constructorStart].Start,
                                });

                                clone.Insert(constructorStart, new Token {
                                    Value = "public",
                                    Type = TokenTypes.Keyword,
                                    Start = clone[constructorStart].Start,
                                    End = clone[constructorStart].Start,
                                });

                                break;
                            }
                        }
                    }
                }
            }

            var result = _engine.GeneralParser.Parse(clone);

            return result;
        }

        public ParseResult Parse(List<Token> tokens)
        {
            var i = 0;

            var skip = _engine.ExpectType(TokenTypes.Identifier, tokens, i);
            i += skip.Delta;

            skip = _engine.ExpectValue(new string[] {":","{"}, tokens, i);
            i += skip.Delta;

            var inheritNode = new Node {Body = "Inherit", Type = TokenTypes.Inherit };

            if (tokens[i].Equals(":", TokenTypes.Punctuator)) {
                bool isIdentifier = true;
                Token nextToken = null;
                var s = _engine.ExpectType(TokenTypes.Identifier, tokens);
                i++;

                while (i < tokens.Count) {

                    var token = tokens[i];

                    if (i < tokens.Count - 2) nextToken = tokens[i + 1];

                    if (token.Type == TokenTypes.EndOfExpression) break;

                    if (isIdentifier) {
                        inheritNode.Add(_engine.ExpressionParser.Parse(new List<Token> { tokens[i] }).Node);

                        if (!(nextToken?.Equals("{", TokenTypes.Punctuator) ?? false) && i < tokens.Count - 1) {
                            var sk = _engine.ExpectValue(",", tokens, i);
                            isIdentifier = false;
                        } else {
                            break;
                        }
                    }
                    else {
                        var sk = _engine.ExpectType(TokenTypes.Identifier, tokens, i);
                        isIdentifier = true;
                    }

                    i++;
                }

                i++;
            }

            List<Token> SurroundedTokens = _engine.GeneralParser.GetSurroundedTokens("{", "}", i, tokens);
            Node node = ParseContents(SurroundedTokens, tokens[1].Value);

            var result = new ParseResult { Node = node, Delta = SurroundedTokens.Count + 1 };

            var classNode = new ClassNode {
                InheritNode = inheritNode,
                BodyNode = result.Node,
                Name = tokens[1].Value,
                Token = tokens[0]
            };

            i += result.Delta + 1;
             
            return new ParseResult { Node = classNode, Delta = i + 1};
        }
    }
}