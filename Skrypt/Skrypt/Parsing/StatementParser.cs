using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Engine;

namespace Skrypt.Parsing {
    /// <summary>
    /// The statement parser class.
    /// Contains all methods to parse if, elseif, else and while statements
    /// </summary>
    public class StatementParser {
        readonly SkryptEngine engine;

        public StatementParser (SkryptEngine e) {
            engine = e;
        }

        /// <summary>
        /// Parses a statement ([if,while] (expression) {block}) into a node
        /// </summary>
        public ParseResult ParseStatement (List<Token> Tokens) {
            int index = 0;
            skipInfo skip;
            ParseResult result;

            // Create main statement node
            Node node = new Node { Body = Tokens[index].Value, TokenType = "Statement" };

            skip = engine.expectValue("(", Tokens);
            index += skip.delta;

            result = engine.generalParser.parseSurrounded("(",")",index,Tokens,engine.expressionParser.ParseClean);
            Node conditionNode = result.node;
            index += result.delta;

            Node conditionParentNode = new Node
            {
                Body = "Condition",
                TokenType = "Condition"
            };

            if (conditionNode == null) {
                engine.throwError("Condition can't be empty!", Tokens[index-1], 1);
            }

            conditionParentNode.Add(conditionNode);

            skip = engine.expectValue("{", Tokens, index);
            index += skip.delta;

            result = engine.generalParser.parseSurrounded("{", "}", index, Tokens, engine.generalParser.Parse);
            Node blockNode = result.node;
            index += result.delta + 1;

            // Add condition and block nodes to main node
            node.Add(conditionParentNode);
            node.Add(blockNode);

            return new ParseResult { node=node, delta=index};
        }

        /// <summary>
        /// Parses an else statement (else {block})
        /// </summary>
        public ParseResult ParseElseStatement(List<Token> Tokens) {
            int index = 0;
            skipInfo skip;

            skip = engine.expectValue("{", Tokens);
            index += skip.delta;

            ParseResult result = engine.generalParser.parseSurrounded("{", "}", index, Tokens, engine.generalParser.Parse);
            Node node = result.node;
            node.Body = "else";
            index += result.delta + 1;

            return new ParseResult { node = node, delta = index };
        }

        /// <summary>
        /// Parses an if statement, including elseif and else statements
        /// </summary>
        public ParseResult ParseIfStatement(List<Token> Tokens) {
            int index = 0;

            // Create main statement node
            ParseResult result = ParseStatement(Tokens);
            index += result.delta;

            // Only parse statements elseif/else if there's any tokens after if statement
            if (index < Tokens.Count - 1) {
                // Look for, and parse elseif statements
                while (Tokens[index].Value == "elseif") {
                    ParseResult elseIfResult = ParseStatement(Tokens.GetRange(index,Tokens.Count - index));
                    result.node.Add(elseIfResult.node);
                    index += elseIfResult.delta;

                    if (index == Tokens.Count) {
                        break;
                    }
                }
            }

            if (index < Tokens.Count - 1) {
                // No more elseif statements left; check and parse else statement
                if (Tokens[index].Value == "else") {
                    ParseResult elseResult = ParseElseStatement(Tokens.GetRange(index, Tokens.Count - index));
                    result.node.Add(elseResult.node);
                    index += elseResult.delta;
                }
            }

            return new ParseResult { node=result.node, delta=index};
        }

        /// <summary>
        /// Parses any statement and returns node 
        /// </summary>
        public ParseResult Parse(List<Token> Tokens) {
            switch (Tokens[0].Value) {
                case "while":
                    return ParseStatement(Tokens);
                case "if":
                    return ParseIfStatement(Tokens);
                case "else":
                    engine.throwError("else statement must come directly after if or elseif statement", Tokens[0]);
                    break;
                case "elseif":
                    engine.throwError("elseif statement must come directly after if or elseif statement", Tokens[0]);
                    break;
            }

            // No statement found
            return null;
        }
    }
}
