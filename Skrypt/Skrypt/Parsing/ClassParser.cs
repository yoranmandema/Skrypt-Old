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

        ParseResult TryParse(List<Token> Tokens) {

            if (Tokens[0].Value == "func") {
                return engine.methodParser.Parse(Tokens);
            }
            else if (Tokens[0].Value == "class") {
                return engine.classParser.Parse(Tokens);
            }
            else {
                return engine.expressionParser.Parse(Tokens);
            }
        }

        Node ParseContents (List<Token> Tokens) {
            Node Node = new Node { Body = "Block", TokenType = "ClassDeclaration" };
            int i = 0;

            Console.WriteLine(ExpressionParser.TokenString(Tokens));

            while (i < Tokens.Count - 1) {
                var Test = TryParse(Tokens.GetRange(i, Tokens.Count - i));
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

            var result = engine.generalParser.parseSurrounded("{", "}", i, Tokens, ParseContents);
            var Node = result.node;
            Node.Body = Tokens[1].Value;
            i += result.delta + 1;
             
            return new ParseResult { node = Node, delta = i };
        }
    }
}
