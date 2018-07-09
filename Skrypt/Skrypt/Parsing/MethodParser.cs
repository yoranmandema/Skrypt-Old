using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Tokenization;

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


            //Node node = new Node();
            //node.Add(ParameterNode);
            //node.Add(BlockNode);
            //node.TokenType = Tokens[1].Value;

            //string currentSignature = Tokens[2].Value;

            //foreach (Node par in node.SubNodes[0].SubNodes) {
            //    currentSignature += "_" + par.TokenType;
            //}

            //node.Body = currentSignature;

            //// Check if method with the same signature already exists        
            //foreach (Node method in engine.MethodNodes) {
            //    if (method.Body == currentSignature) {
            //        engine.throwError("Method with this signature already exists!", Tokens[0]);
            //    }
            //}

            //engine.MethodNodes.Add(node);
            //engine.Methods.Add(new UserMethod {
            //    Name = currentSignature,
            //    ReturnType = Tokens[1].Value,
            //    BlockNode = BlockNode,
            //});

            var returnNode = new Node
            {
                Body = tokens[1].Value,
                TokenType = "MethodDeclaration"
            };
            returnNode.SubNodes.Add(blockNode);
            returnNode.SubNodes.Add(parameterNode);

            return new ParseResult {Node = returnNode, Delta = index};
        }
    }
}