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
    class StatementParser {

        SkryptEngine engine;

        public StatementParser (SkryptEngine e) {
            engine = e;
        }

        /// <summary>
        /// Parses a statement ([if,while] (expression) {block}) into a node
        /// </summary>
        public ParseResult ParseStatement (List<Token> Tokens) {
            int index = 0;
            skipInfo skip;

            // Create main statement node
            Node node = new Node { Body = Tokens[index].Value, TokenType = "Statement" };

            skip = engine.expectValue("(", Tokens);
            index = skip.end;

            int i = index + 1;
            skip = engine.expressionParser.SkipFromTo("(", ")", Tokens, index);
            int endCondition = skip.end;
            index = skip.end;

            Node conditionNode = new Node { Body = "Condition", TokenType = "Expression" };
            conditionNode.Add(engine.expressionParser.ParseExpression(conditionNode, Tokens.GetRange(i, endCondition - i)));

            skip = engine.expectValue("{", Tokens, index);
            index = skip.end;

            i = index + 1;
            skip = engine.expressionParser.SkipFromTo("{", "}", Tokens, index);
            int endBlock = skip.end;
            index = skip.end;

            Node blockNode = engine.generalParser.Parse(Tokens.GetRange(i, endBlock - i));

            // Add condition and block nodes to main node
            node.Add(conditionNode);
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
            index = skip.end;

            int i = 1;
            skip = engine.expressionParser.SkipFromTo("{", "}", Tokens, index);
            int endBlock = skip.end;
            index = skip.end;

            // Parse block and rename to 'else'
            Node node = engine.generalParser.Parse(Tokens.GetRange(i, endBlock - i));
            node.Body = "else";

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
                while (Tokens[index + 1].Value == "elseif") {
                    Console.WriteLine("found elseif");
                    index++;

                    ParseResult elseIfResult = ParseStatement(Tokens.GetRange(index,Tokens.Count - index));
                    result.node.Add(elseIfResult.node);
                    index += elseIfResult.delta;

                    if (index == Tokens.Count - 1) {
                        break;
                    }
                }
            }

            if (index < Tokens.Count - 1) {
                // No more elseif statements left; check and parse else statement
                if (Tokens[index + 1].Value == "else") {
                    index++;

                    ParseResult elseResult = ParseElseStatement(Tokens.GetRange(index, Tokens.Count - index));
                    Console.WriteLine(elseResult);
                    result.node.Add(elseResult.node);
                    index += elseResult.delta;
                }
            }

            Console.WriteLine(index);

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
