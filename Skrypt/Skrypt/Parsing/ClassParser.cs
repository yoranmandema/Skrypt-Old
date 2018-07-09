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

        private ParseResult TryParse(List<Token> tokens)
        {
            if (tokens[0].Value == "func")
                return _engine.MethodParser.Parse(tokens);
            if (tokens[0].Value == "class")
                return _engine.ClassParser.Parse(tokens);
            return _engine.ExpressionParser.Parse(tokens);
        }

        private Node ParseContents(List<Token> tokens)
        {
            var node = new Node {Body = "Block", TokenType = "ClassDeclaration"};
            var i = 0;

            Console.WriteLine(ExpressionParser.TokenString(tokens));

            while (i < tokens.Count - 1)
            {
                var test = TryParse(tokens.GetRange(i, tokens.Count - i));
                i += test.Delta;

                if (test.Node.TokenType == "MethodDeclaration")
                {
                    node.AddAsFirst(test.Node);
                    continue;
                }

                node.Add(test.Node);
            }

            return node;
        }

        public ParseResult Parse(List<Token> tokens)
        {
            var i = 0;

            var skip = _engine.ExpectType(TokenTypes.Identifier, tokens, i);
            i += skip.Delta;

            skip = _engine.ExpectValue("{", tokens, i);
            i += skip.Delta;

            var result = _engine.GeneralParser.ParseSurrounded("{", "}", i, tokens, ParseContents);
            var node = result.Node;
            node.Body = tokens[1].Value;
            i += result.Delta + 1;

            return new ParseResult {Node = node, Delta = i};
        }
    }
}