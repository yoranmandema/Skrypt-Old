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
        private readonly SkryptEngine engine;

        public ClassParser(SkryptEngine e)
        {
            engine = e;
        }

        private ParseResult TryParse(List<Token> Tokens)
        {
            if (Tokens[0].Value == "func")
                return engine.methodParser.Parse(Tokens);
            if (Tokens[0].Value == "class")
                return engine.classParser.Parse(Tokens);
            return engine.expressionParser.Parse(Tokens);
        }

        private Node ParseContents(List<Token> Tokens)
        {
            var Node = new Node {Body = "Block", TokenType = "ClassDeclaration"};
            var i = 0;

            Console.WriteLine(ExpressionParser.TokenString(Tokens));

            while (i < Tokens.Count - 1)
            {
                var Test = TryParse(Tokens.GetRange(i, Tokens.Count - i));
                i += Test.delta;

                if (Test.node.TokenType == "MethodDeclaration")
                {
                    Node.AddAsFirst(Test.node);
                    continue;
                }

                Node.Add(Test.node);
            }

            return Node;
        }

        public ParseResult Parse(List<Token> Tokens)
        {
            var i = 0;

            var skip = engine.expectType(TokenTypes.Identifier, Tokens, i);
            i += skip.delta;

            skip = engine.expectValue("{", Tokens, i);
            i += skip.delta;

            var result = engine.generalParser.parseSurrounded("{", "}", i, Tokens, ParseContents);
            var Node = result.node;
            Node.Body = Tokens[1].Value;
            i += result.delta + 1;

            return new ParseResult {node = Node, delta = i};
        }
    }
}