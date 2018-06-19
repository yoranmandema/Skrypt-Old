using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    /// <summary>
    /// The statement parser class.
    /// Contains all methods to parse if, elseif, else and while statements
    /// </summary>
    static class StatementParser {

        /// <summary>
        /// Parses a statement ([if,while] (expression) {block}) into a node
        /// </summary>
        static public Node ParseStatement (List<Token> Tokens, ref int Index) {
            // Create main statement node
            Node node = new Node { Body = Tokens[Index].Value, TokenType = "Statement" };

            // Skip to condition, and parse condition as expression
            Index += 2;
            int i = Index;
            int endCondition = i;
            ExpressionParser.SkipFromTo(ref Index, ref endCondition, "(", ")", Tokens);

            Node conditionNode = new Node { Body = "Condition", TokenType = "Expression" };
            conditionNode.Add(ExpressionParser.ParseExpression(conditionNode, Tokens.GetRange(i, endCondition - i)));

            // Skip to content block, and parse as block
            Index++;
            i = Index;
            int endBlock = i;
            ExpressionParser.SkipFromTo(ref Index, ref endBlock, "{", "}", Tokens);

            Node blockNode = GeneralParser.Parse(Tokens.GetRange(i, endBlock - i));

            // Add condition and block nodes to main node
            node.Add(conditionNode);
            node.Add(blockNode);

            // Go back so we don't skip any token after the block
            Index--;

            return node;
        }

        /// <summary>
        /// Parses an else statement (else {block})
        /// </summary>
        static public Node ParseElseStatement(List<Token> Tokens, ref int Index) {
            // Skip to content block, and parse as block
            Index += 3;
            int i = Index;
            int endBlock = i;
            ExpressionParser.SkipFromTo(ref Index, ref endBlock, "{", "}", Tokens);

            // Parse block and rename to 'else'
            Node node = GeneralParser.Parse(Tokens.GetRange(i, endBlock - i));
            node.Body = "else";

            return node;
        }

        /// <summary>
        /// Parses an if statement, including elseif and else statements
        /// </summary>
        static public Node ParseIfStatement(List<Token> Tokens, ref int Index) {
            // Create main statement node
            Node node = ParseStatement(Tokens, ref Index);

            // Only parse statements elseif/else if there's any tokens after if statement
            if (Index < Tokens.Count) {
                // Look for, and parse elseif statements
                while (Tokens[Index + 1].Value == "elseif") {
                    Index += 1;
                    Node elseIfNode = ParseStatement(Tokens, ref Index);
                    node.Add(elseIfNode);
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
        static public Node Parse(List<Token> Tokens, ref int Index) {
            switch (Tokens[Index].Value) {
                case "while":
                    return ParseStatement(Tokens, ref Index);
                case "if":
                    return ParseIfStatement(Tokens, ref Index);
            }

            // No statement found
            return null;
        }
    }
}
