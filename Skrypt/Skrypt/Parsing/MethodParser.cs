using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Tokenization;
using System;

namespace Skrypt.Parsing
{
    /// <summary>
    ///     The method parser class.
    ///     Contains all methods to parse user defined methods
    /// </summary>
    public class MethodParser
    {
        private readonly SkryptEngine _engine;

        public MethodParser(SkryptEngine e)
        {
            _engine = e;
        }

        public Node ParseSingleParameter(List<Token> tokens)
        {
            var index = 0;
            var name = tokens[index].Value;

            var node = new Node
            {
                Body = name,
                TokenType = "Parameter"
            };

            return node;
        }

        public Node ParseParameters(List<Token> tokens)
        {
            var node = new Node();

            var parameterLists = new List<List<Token>>();
            ExpressionParser.SetArguments(parameterLists, tokens);

            foreach (var parameter in parameterLists)
            {
                var parameterNode = ParseSingleParameter(parameter);
                node.Add(parameterNode);
            }

            node.Body = "Parameters";
            node.TokenType = "Parameters";

            return node;
        }

        public ParseResult ParseFunctionLiteral(List<Token> tokens)
        {
            var index = 0;
            var node = new Node();
            SkipInfo skip;
            ParseResult result;

            result = _engine.GeneralParser.ParseSurrounded("(", ")", index, tokens, ParseParameters);
            var parameterNode = result.Node;
            index += result.Delta;

            skip = _engine.ExpectValue("{", tokens, index);
            index += skip.Delta;

            result = _engine.GeneralParser.ParseSurrounded("{", "}", index, tokens, _engine.GeneralParser.Parse);
            var blockNode = result.Node;
            index += result.Delta + 1;

            var returnNode = new Node
            {
                Body = "Function",
                TokenType = "FunctionLiteral"
            };
            returnNode.SubNodes.Add(blockNode);
            returnNode.SubNodes.Add(parameterNode);

            return new ParseResult {Node = returnNode, Delta = index};
        }

        public Node ParseLambdaParameters (List<Token> tokens) {
            var node = new Node();

            if (tokens[0].Value == "(" && tokens[0].Type == TokenTypes.Punctuator) {
                node = _engine.GeneralParser.ParseSurrounded("(", ")", 0, tokens, ParseParameters).Node;
            } else {
                node = ParseSingleParameter(tokens);
            }

            return node;
        }

        public Node ParseLambdaBlock (List<Token> tokens) {
            var node = new Node();

            if (tokens[0].Value == "{" && tokens[0].Type == TokenTypes.Punctuator) {
                node = _engine.GeneralParser.ParseSurrounded("{", "}", 0, tokens, _engine.GeneralParser.Parse).Node;
            }
            else {
                node = new Node { Body = "Block", TokenType = "Block" };
                var returnNode = new Node { Body = "return", TokenType = "Punctuator" };
                var expressionNode = _engine.ExpressionParser.Parse(tokens).Node;

                returnNode.Add(expressionNode);
                node.Add(returnNode);
            }

            return node;
        }

        public ParseResult ParseLambda(List<Token> tokens) {
            var node = new Node();
            var parameterBuffer = new List<Token>();
            var blockBuffer = new List<Token>();

            for (int i = 0; i < tokens.Count - 1; i++) {
                if (tokens[i].Value == "=>" && tokens[i].Type == TokenTypes.Punctuator) {
                    blockBuffer = tokens.GetRange(i + 1, tokens.Count - i -1);
                    break;
                }

                parameterBuffer.Add(tokens[i]);
            }

            var parameterNode = ParseLambdaParameters(parameterBuffer);
            var blockNode = ParseLambdaBlock(blockBuffer);

            if (parameterNode.SubNodes.Count == 0) {
                var parNode = new Node();
                parNode.Body = "Parameters";
                parNode.TokenType = "Parameters";

                parNode.Add(parameterNode);
                parameterNode = parNode;
            }

            var returnNode = new Node {
                Body = "Function",
                TokenType = "FunctionLiteral"
            };
            returnNode.SubNodes.Add(blockNode);
            returnNode.SubNodes.Add(parameterNode);

            return new ParseResult { Node = returnNode, Delta = tokens.Count };
        }

        /// <summary>
        ///     Parses a list of tokens into a method node
        /// </summary>
        public ParseResult Parse(List<Token> tokens)
        {
            var index = 0;
            SkipInfo skip;
            ParseResult result;

            skip = _engine.ExpectType(TokenTypes.Identifier, tokens);
            index += skip.Delta;

            //skip = engine.expectType("Identifier", Tokens,index);
            //index += skip.delta;

            skip = _engine.ExpectValue("(", tokens, index);
            index += skip.Delta;

            result = _engine.GeneralParser.ParseSurrounded("(", ")", index, tokens, ParseParameters);
            var parameterNode = result.Node;
            index += result.Delta;

            skip = _engine.ExpectValue("{", tokens, index);
            index += skip.Delta;

            result = _engine.GeneralParser.ParseSurrounded("{", "}", index, tokens, _engine.GeneralParser.Parse);
            var blockNode = result.Node;
            index += result.Delta + 1;

            var returnNode = new Node
            {
                Body = tokens[1].Value,
                TokenType = "MethodDeclaration"
            };
            returnNode.SubNodes.Add(blockNode);
            returnNode.SubNodes.Add(parameterNode);

            return new ParseResult {Node = returnNode, Delta = index + 1};
        }
    }
}