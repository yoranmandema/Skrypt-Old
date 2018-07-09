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
        private readonly SkryptEngine engine;

        public MethodParser(SkryptEngine e)
        {
            engine = e;
        }

        public Node ParseSingleParameter(List<Token> Tokens)
        {
            var index = 0;
            skipInfo skip;

            var Name = Tokens[index].Value;

            var node = new Node
            {
                Body = Name,
                TokenType = "Parameter"
            };

            return node;
        }

        public Node ParseParameters(List<Token> Tokens)
        {
            var node = new Node();

            var ParameterLists = new List<List<Token>>();
            ExpressionParser.SetArguments(ParameterLists, Tokens);

            foreach (var Parameter in ParameterLists)
            {
                var ParameterNode = ParseSingleParameter(Parameter);
                node.Add(ParameterNode);
            }

            node.Body = "Parameters";
            node.TokenType = "Parameters";

            return node;
        }

        public ParseResult ParseFunctionLiteral(List<Token> Tokens)
        {
            var index = 0;
            var node = new Node();
            skipInfo skip;
            ParseResult result;

            result = engine.generalParser.parseSurrounded("(", ")", index, Tokens, ParseParameters);
            var ParameterNode = result.node;
            index += result.delta;

            skip = engine.expectValue("{", Tokens, index);
            index += skip.delta;

            result = engine.generalParser.parseSurrounded("{", "}", index, Tokens, engine.generalParser.Parse);
            var BlockNode = result.node;
            index += result.delta + 1;

            var returnNode = new Node
            {
                Body = "Function",
                TokenType = "FunctionLiteral"
            };
            returnNode.SubNodes.Add(BlockNode);
            returnNode.SubNodes.Add(ParameterNode);

            return new ParseResult {node = returnNode, delta = index};
        }

        /// <summary>
        ///     Parses a list of tokens into a method node
        /// </summary>
        public ParseResult Parse(List<Token> Tokens)
        {
            var index = 0;
            skipInfo skip;
            ParseResult result;

            skip = engine.expectType(TokenTypes.Identifier, Tokens);
            index += skip.delta;

            //skip = engine.expectType("Identifier", Tokens,index);
            //index += skip.delta;

            skip = engine.expectValue("(", Tokens, index);
            index += skip.delta;

            result = engine.generalParser.parseSurrounded("(", ")", index, Tokens, ParseParameters);
            var ParameterNode = result.node;
            index += result.delta;

            skip = engine.expectValue("{", Tokens, index);
            index += skip.delta;

            result = engine.generalParser.parseSurrounded("{", "}", index, Tokens, engine.generalParser.Parse);
            var BlockNode = result.node;
            index += result.delta + 1;


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
                Body = Tokens[1].Value,
                TokenType = "MethodDeclaration"
            };
            returnNode.SubNodes.Add(BlockNode);
            returnNode.SubNodes.Add(ParameterNode);

            return new ParseResult {node = returnNode, delta = index};
        }
    }
}