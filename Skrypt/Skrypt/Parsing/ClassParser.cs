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
        readonly SkryptEngine engine;

        public ClassParser(SkryptEngine e) {
            engine = e;
        }

        static string[] PropertyWords = new[] { "static", "private", "public", "constant" };

        ParseResult TryParse(List<Token> Tokens, string Name) {
            var Properties = new List<string>();
            Node propertyNode = new Node {
                Body = "PropertyDeclaration",
                TokenType = "PropertyDeclaration",
            };

            Node markNode = propertyNode.Add(new Node {
                Body = "Properties",
                TokenType = "Properties",
            });

            Console.WriteLine(ExpressionParser.TokenString(Tokens));

            List<Token> FurtherTokens = null;

            for (int i = 0; i < Tokens.Count; i++) {
                var t = Tokens[i];

                string Prop = t.Value;

                if (PropertyWords.Contains(Prop)) {
                    if (Prop == "public" && Properties.Contains("private")) {
                        engine.throwError("Property can't be marked as both private and public!");
                    } else if (Prop == "private" && Properties.Contains("public")) {
                        engine.throwError("Property can't be marked as both private and public!");
                    }

                    if (Properties.Contains(Prop)) {
                        engine.throwError("Property is already marked as " + Prop);
                    } else {
                        markNode.Add(new Node {
                            Body = Prop,
                            TokenType = "Property",
                        });

                        Properties.Add(Prop);
                    }
                } else {
                    FurtherTokens = Tokens.GetRange(i,Tokens.Count - i);
                    break;
                }
            }

            ParseResult FurtherResult = null;

            if (FurtherTokens[0].Value == Name) {
                FurtherTokens[0].Value = "Constructor";
                FurtherTokens.Insert(0, new Token {Value = "func"});
                FurtherResult = engine.methodParser.Parse(FurtherTokens);
                FurtherResult.delta--;

                if (Properties.Contains("static") || Properties.Contains("constant")) {
                    engine.throwError("Constructor method cannot be marked as static or constant!", FurtherTokens[1]);
                }
            } else if (FurtherTokens[0].Value == "func") {
                FurtherResult = engine.methodParser.Parse(FurtherTokens);
            }
            else if (FurtherTokens[0].Value == "class") {
                FurtherResult = engine.classParser.Parse(FurtherTokens);
            }
            else {
                FurtherResult = engine.expressionParser.Parse(FurtherTokens);
            }

            propertyNode.Add(FurtherResult.node);

            return new ParseResult { node = propertyNode, delta = FurtherResult.delta + Properties.Count};
        }

        Node ParseContents (List<Token> Tokens, string Name) {
            Node Node = new Node { Body = "Block", TokenType = "ClassDeclaration" };
            int i = 0;

            while (i < Tokens.Count - 1) {
                var Test = TryParse(Tokens.GetRange(i, Tokens.Count - i), Name);
                i += Test.delta;

                if (Test.node.TokenType == "MethodDeclaration") {
                    Node.AddAsFirst(Test.node);
                    continue;
                }

                Node.Add(Test.node);
            }

            return Node;
        }

        public ParseResult Parse(List<Token> Tokens) {
            int i = 0;

            var skip = engine.expectType(TokenTypes.Identifier, Tokens, i);
            i += skip.delta;

            skip = engine.expectValue("{", Tokens, i);
            i += skip.delta;

            List<Token> SurroundedTokens = engine.generalParser.GetSurroundedTokens("{", "}", i, Tokens);
            Node node = ParseContents(SurroundedTokens, Tokens[1].Value);

            var result = new ParseResult { node = node, delta = SurroundedTokens.Count + 1 };

            var Node = result.node;
            Node.Body = Tokens[1].Value;
            i += result.delta + 1;
             
            return new ParseResult { node = Node, delta = i };
        }
    }
}
