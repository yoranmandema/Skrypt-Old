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

        /// <summary>
        ///     Parses everything inside the body of a class.
        /// </summary>
        Node ParseContents (List<Token> tokens, string name) {
            var constructorStart = -1;
            var clone = new List<Token>(tokens);
            var previousToken = default(Token);

            for (int i = 0; i < clone.Count; i++) {
                var t = clone[i];

                if (i > 0) {
                    previousToken = clone[i - 1];
                }

                // Look for constructor shortcut
                if (t.Equals(name, TokenTypes.Identifier)) {
                    // Should not be preceeded the fn keyword.
                    if (!(previousToken != null && previousToken.Equals("fn", TokenTypes.Keyword))) {
                        constructorStart = i;
                        continue;
                    }
                }

                // Syntactic sugar to more easily write constructor functions.
                // This basically looks wether we have the pattern '[class name] () {...}' 
                // and replaces that with 'public fn Constructor (...) {...}'
                if (constructorStart != -1) {
                    if ((t.Value == "(") && (t.Type == TokenTypes.Punctuator) && (i == (constructorStart + 1))) {
                        var skip = _engine.ExpressionParser.SkipFromTo("(", ")", clone, i);

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

        /// <summary>
        ///     Parses a class definition.
        /// </summary>
        public ParseResult Parse(List<Token> tokens) {
            var i = 0;

            var skip = _engine.ExpectType(TokenTypes.Identifier, tokens, i);
            i += skip.Delta;

            skip = _engine.ExpectValue(new string[] {":","{"}, tokens, i);
            i += skip.Delta;

            var inheritNode = new Node {Body = "Inherit", Type = TokenTypes.Inherit };

            // Class inherits from another class or classes.
            if (tokens[i].Equals(":", TokenTypes.Punctuator)) {
                var isIdentifier    = true;
                var nextToken       = default(Token);
                var s               = _engine.ExpectType(TokenTypes.Identifier, tokens);
                i++;

                while (i < tokens.Count) {
                    var token = tokens[i];

                    if (i < tokens.Count - 2) {
                        nextToken = tokens[i + 1];
                    }

                    if (token.Type == TokenTypes.EndOfExpression) {
                        break;
                    }

                    if (isIdentifier) {
                        inheritNode.Add(_engine.ExpressionParser.Parse(new List<Token> { tokens[i] }).Node);

                        // No curly bracket means there could be another class to inherit from.
                        // Before that a seperator ',' token has to be present.
                        if (!(nextToken?.Equals("{", TokenTypes.Punctuator) ?? false) && i < tokens.Count - 1) {
                            var sk = _engine.ExpectValue(",", tokens, i);
                            isIdentifier = false;
                        } else {
                            break;
                        }
                    }
                    else {
                        // An object (identifier token) has to be present after a seperator ',' token.
                        var sk = _engine.ExpectType(TokenTypes.Identifier, tokens, i);
                        isIdentifier = true;
                    }

                    i++;
                }

                i++;
            }

            // Parse class body.
            var SurroundedTokens = _engine.GeneralParser.GetSurroundedTokens("{", "}", i, tokens);
            var node = ParseContents(SurroundedTokens, tokens[1].Value);

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