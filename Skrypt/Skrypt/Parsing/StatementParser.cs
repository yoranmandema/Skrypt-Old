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
        public Node ParseStatement (List<Token> Tokens, ref int Index) {
            // Create main statement node
            Node node = new Node { Body = Tokens[Index].Value, TokenType = "Statement" };

            engine.expectValue("(", Tokens, ref Index);

            int i = Index + 1;
            int endCondition = i;
            engine.expressionParser.SkipFromTo(ref Index, ref endCondition, "(", ")", Tokens);

            Node conditionNode = new Node { Body = "Condition", TokenType = "Expression" };
            conditionNode.Add(engine.expressionParser.ParseExpression(conditionNode, Tokens.GetRange(i, endCondition - i)));

            engine.expectValue("{", Tokens, ref Index);

            i = Index + 1;
            int endBlock = i;
            engine.expressionParser.SkipFromTo(ref Index, ref endBlock, "{", "}", Tokens);

            Node blockNode = engine.generalParser.Parse(Tokens.GetRange(i, endBlock - i));

            // Add condition and block nodes to main node
            node.Add(conditionNode);
            node.Add(blockNode);

            return node;
        }

        /// <summary>
        /// Parses an else statement (else {block})
        /// </summary>
        public Node ParseElseStatement(List<Token> Tokens, ref int Index) {
            engine.expectValue("else", Tokens, ref Index);
            engine.expectValue("{", Tokens, ref Index);

            // Skip to content block, and parse as block
            engine.expressionParser.SkipUntil(ref Index, new Token {Value="{"}, Tokens);
            int i = Index;
            int endBlock = i;
            engine.expressionParser.SkipFromTo(ref Index, ref endBlock, "{", "}", Tokens);

            // Parse block and rename to 'else'
            Node node = engine.generalParser.Parse(Tokens.GetRange(i, endBlock - i));
            node.Body = "else";

            return node;
        }

        /// <summary>
        /// Parses an if statement, including elseif and else statements
        /// </summary>
        public Node ParseIfStatement(List<Token> Tokens, ref int Index) {
            // Create main statement node
            Node node = ParseStatement(Tokens, ref Index);

            // Only parse statements elseif/else if there's any tokens after if statement
            if (Index < Tokens.Count - 1) {
                // Look for, and parse elseif statements
                while (Tokens[Index + 1].Value == "elseif") {
                    Index++;

                    Node elseIfNode = ParseStatement(Tokens, ref Index);
                    node.Add(elseIfNode);

                    if (Index == Tokens.Count - 1) {
                        break;
                    }
                }

                // No more elseif statements left; check and parse else statement
                if (Tokens[Index + 1].Value == "else") {
                    Node elseNode = ParseElseStatement(Tokens, ref Index);
                    node.Add(elseNode);
                }

            }

            return node;
        }

        /// <summary>
        /// Parses any statement and returns node 
        /// </summary>
        public Node Parse(List<Token> Tokens, ref int Index) {
            switch (Tokens[Index].Value) {
                case "while":
                    return ParseStatement(Tokens, ref Index);
                case "if":
                    return ParseIfStatement(Tokens, ref Index);
                case "else":
                    engine.throwError("else statement must come directly after if or elseif statement", Tokens[Index]);
                    break;
                case "elseif":
                    engine.throwError("elseif statement must come directly after if or elseif statement", Tokens[Index]);
                    break;
            }

            // No statement found
            return null;
        }
    }
}
