using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Tokenization;
using Skrypt.Library;

namespace Skrypt.Parsing {
    /// <summary>
    /// The method parser class.
    /// Contains all methods to parse user defined methods
    /// </summary>
    public class MethodParser {
        readonly SkryptEngine engine;

        public MethodParser(SkryptEngine e) {
            engine = e;
        }

        public Node ParseSingleParameter (List<Token> Tokens) {
            int index = 0;
            skipInfo skip;

            string Name = Tokens[index].Value;

            Node node = new Node();
            node.Body = Name;
            node.TokenType = "Parameter";

            return node;
        }

        public Node ParseParameters (List<Token> Tokens) {           
            Node node = new Node();

            List<List<Token>> ParameterLists = new List<List<Token>>();
            ExpressionParser.SetArguments(ParameterLists, Tokens);

            foreach (List<Token> Parameter in ParameterLists) {
                Node ParameterNode = ParseSingleParameter(Parameter);
                node.Add(ParameterNode);
            }

            node.Body = "Parameters";
            node.TokenType = "Parameters";

            return node;
        }

        public ParseResult ParseFunctionLiteral (List<Token> Tokens) {
            int index = 0;
            Node node = new Node();
            skipInfo skip;
            ParseResult result;

            result = engine.generalParser.parseSurrounded("(", ")", index, Tokens, ParseParameters);
            Node ParameterNode = result.node;
            index += result.delta;

            skip = engine.expectValue("{", Tokens, index);
            index += skip.delta;

            result = engine.generalParser.parseSurrounded("{", "}", index, Tokens, engine.generalParser.Parse);
            Node BlockNode = result.node;
            index += result.delta + 1;

            Node returnNode = new Node();
            returnNode.Body = "Function";
            returnNode.TokenType = "FunctionLiteral";
            returnNode.SubNodes.Add(BlockNode);
            returnNode.SubNodes.Add(ParameterNode);

            return new ParseResult { node = returnNode, delta = index };
        }

        /// <summary>
        /// Parses a list of tokens into a method node
        /// </summary>
        public ParseResult Parse(List<Token> Tokens) {
            int index = 0;
            skipInfo skip;
            ParseResult result;

            skip = engine.expectType(TokenTypes.Identifier, Tokens);
            index += skip.delta;

            //skip = engine.expectType("Identifier", Tokens,index);
            //index += skip.delta;

            skip = engine.expectValue("(", Tokens, index);
            index += skip.delta;

            result = engine.generalParser.parseSurrounded("(", ")", index, Tokens, ParseParameters);
            Node ParameterNode = result.node;
            index += result.delta;

            skip = engine.expectValue("{", Tokens, index);
            index += skip.delta;

            result = engine.generalParser.parseSurrounded("{", "}", index, Tokens, engine.generalParser.Parse);
            Node BlockNode = result.node;
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

            Node returnNode = new Node();
            returnNode.Body = Tokens[1].Value;
            returnNode.TokenType = "MethodDeclaration";
            returnNode.SubNodes.Add(BlockNode);
            returnNode.SubNodes.Add(ParameterNode);

            return new ParseResult { node = returnNode, delta = index };
        }
    }
}
