using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {

    /// <summary>
    /// The expression parser class.
    /// Contains all methods to parse expressions, and helper methods
    /// </summary>
    static class ExpressionParser {
        /// <summary>
        /// Class representing an operator group
        /// </summary>
        class OperatorGroup {
            public List<string> Operators = new List<string>();
            public int Members;
            public bool LeftAssociate = true;
            public bool IsPostfix = false;

            public OperatorGroup(string[] ops, bool LA = true, int mems = 2, bool PF = false) {
                Operators = ops.ToList<string>();
                Members = mems;
                LeftAssociate = LA;
                IsPostfix = PF;
            }
        }

        // Create list of operator groups with the right precedence order
        static List<OperatorGroup> OperatorPrecedence = new List<OperatorGroup> {
            new OperatorGroup(new[] {"."}, true, 2, true),
            new OperatorGroup(new[] {"="}, false),
            new OperatorGroup(new[] {"||"}),
            new OperatorGroup(new[] {"&&"}),
            new OperatorGroup(new[] {"!=","=="}),
            new OperatorGroup(new[] {"<",">","<=",">="}),
            new OperatorGroup(new[] {"+","-"}),
            new OperatorGroup(new[] {"*","/","%"}),
            new OperatorGroup(new[] {"^"}),
            new OperatorGroup(new[] { "-", "!" }, false, 1),
            new OperatorGroup(new[] { "++", "--" }, false, 1, true),
        };

        // (debug) Serializes a list of tokens into a string
        static string TokenString (List<Token> Tokens) {
            string sb = "";

            foreach (Token token in Tokens) {
                sb += token.Value;
            }

            return sb;
        }

        /// <summary>
        /// Parses a list of tokens into an expression recursively
        /// </summary>
        static public Node ParseExpression (Node branch, List<Token> Tokens) {

            // Create left and right token buffers
            List<Token> leftBuffer = new List<Token>();
            List<Token> rightBuffer = new List<Token>();

            bool isInPars = false; 

            // Do logic in delegate so we can easily exit out of it when we need to
            Action loop = () => {
                foreach (OperatorGroup OP in OperatorPrecedence) {
                    foreach (string Operator in OP.Operators) {
                        int i = 0;
                        bool CanLoop = Tokens.Count > 0;
                        int parDepth = 0;
                        int firstPar = -1;

                        while (CanLoop) {                      
                            Token token = Tokens[i];
                            Token previousToken = i >= 1 ? Tokens[i-1] : null;

                            if (token.Value == "(") {
                                if (previousToken != null) {
                                    // Previous token was identifier; possible method call
                                    if (previousToken.Type == "Identifier") {
                                        Node node = ParseCall(Tokens, ref i);

                                        // Only add method call node if all tokens were consumed in it
                                        if (i == Tokens.Count) {
                                            branch.Add(node);
                                            return;
                                        }
                                    }
                                }

                                if (firstPar == -1)
                                    firstPar = i;
                            
                                parDepth++;
                            } else if (token.Value == ")") {
                                parDepth--;

                                // Whole expression is surrouned in parenthesis 
                                if (i == Tokens.Count - 1 && firstPar == 0) {
                                    isInPars = true;
                                    return;
                                }
                            } else if (token.Value == "[") {
                                if (previousToken != null) {
                                    // Previous token was identifier or string; possible indexing
                                    if (previousToken.Type == "Identifier" || previousToken.Type == "StringLiteral") {
                                        Node node = ParseIndexing(Tokens, ref i);

                                        // Only add indexing node if all tokens were consumed in it
                                        if (i == Tokens.Count) {
                                            branch.Add(node);
                                            return;
                                        }
                                    }
                                } else {
                                    Node node = ParseArrayLiteral(Tokens, ref i);

                                    // Only add indexing node if all tokens were consumed in it
                                    if (i == Tokens.Count) {
                                        branch.Add(node);
                                        return;
                                    }
                                }
                            } else if (token.Value == Operator && parDepth == 0) {

                                // Fill left and right buffers
                                leftBuffer = Tokens.GetRange(0, i);
                                rightBuffer = Tokens.GetRange(i + 1, Tokens.Count - i - 1);

                                // Create operation node with type and body
                                Node NewNode = new Node();
                                NewNode.Body = token.Value;
                                NewNode.TokenType = token.Type;

                                if (OP.Members == 1) {
                                    // Parse unary and do postfix logic

                                    Node LeftNode = !OP.IsPostfix ? null : ParseExpression(NewNode, leftBuffer);
                                    NewNode.Add(LeftNode);

                                    Node RightNode = OP.IsPostfix ? null : ParseExpression(NewNode, rightBuffer);
                                    NewNode.Add(RightNode);
                                } else {
                                    // Parse operators that need 2 sides

                                    Node LeftNode = ParseExpression(NewNode, leftBuffer);
                                    NewNode.Add(LeftNode);

                                    Node RightNode = ParseExpression(NewNode, rightBuffer);
                                    NewNode.Add(RightNode);
                                }

                                branch.Add(NewNode);
                                return;
                            }

                            // Check if we're still in bounds
                            CanLoop = OP.LeftAssociate ? i < Tokens.Count - 1 : ((Tokens.Count - 1) - i) > 0 ;
                            i++;
                        }
                    }
                }
            };
            loop();

            // Parse expression within parenthesis if it's completely surrounded
            if (isInPars) {
                return ParseExpression(branch, Tokens.GetRange(1, Tokens.Count - 2));
            }

            if (Tokens.Count != 1) {
                return null;
            }

            // return resulting end node
            return new Node {
                Body = Tokens[0].Value,
                //Value = null,
                TokenType = Tokens[0].Type
            };
        }

        /// <summary>
        /// Sets parses individual arguments as expressions
        /// </summary>
        static public void SetArguments(List<List<Token>> Arguments, List<Token> Tokens) {
            int depth = 0;
            int indexDepth = 0;
            int i = 0;
            int startArg = 0;
            List<Token> Buffer = new List<Token>();

            for (i = 0; i < Tokens.Count; i++) {
                Token token = Tokens[i];

                if (token.Value == "(") {
                    depth++;
                }
                else if (token.Value == ")") {
                    depth--;
                }

                if (token.Value == "[") {
                    indexDepth++;
                }
                else if (token.Value == "]") {
                    indexDepth--;
                }

                if ((token.Value == "," || i == Tokens.Count - 1) && depth == 0 && indexDepth == 0) {
                    Buffer = Tokens.GetRange(startArg, (i == Tokens.Count - 1 ? i + 1 : i) - startArg);
                    startArg = i + 1;
                    Arguments.Add(Buffer);
                }
            }
        }

        /// <summary>
        /// Skip tokens that are surrounded by 'upScope' and 'downScope'
        /// </summary>
        static public void SkipFromTo(ref int Index, ref int endSkip, string upScope, string downScope, List<Token> Tokens) {
            int depth = 1;

            while (depth != 0) {
                if (Tokens[Index].Value == upScope) {
                    depth++;
                }
                else if (Tokens[Index].Value == downScope) {
                    depth--;

                    if (depth == 0) {
                        endSkip = Index;
                    }
                }

                Index++;
            }
        }

        /// <summary>
        /// Parses a method call with arguments
        /// </summary>
        static public Node ParseCall(List<Token> Tokens, ref int Index) {
            // Create method call node with identifier as body
            Node node = new Node { Body = Tokens[Index-1].Value, TokenType = "Call" };

            // Skip to arguments, and parse arguments
            Index++;
            int i = Index;
            int endArguments = i;
            SkipFromTo(ref Index, ref endArguments, "(", ")", Tokens);

            List<List<Token>> Arguments = new List<List<Token>>();
            SetArguments(Arguments, Tokens.GetRange(i, endArguments - i));

            foreach (List<Token> Argument in Arguments) {
                Node argNode = ParseExpression(node, Argument);
                node.Add(argNode);
            }

            return node;
        }

        /// <summary>
        /// Parses an index operation
        /// </summary>
        static public Node ParseIndexing(List<Token> Tokens, ref int Index) {
            Node node = new Node { Body = Tokens[Index - 1].Value, TokenType = "Index" };

            // Skip to index expression, and as expression
            Index++;
            int i = Index;
            int endArguments = i;
            SkipFromTo(ref Index, ref endArguments, "[", "]", Tokens);

            Node argNode = ParseExpression(node, Tokens.GetRange(i, endArguments - i));
            node.Add(argNode);

            return node;
        }

        /// <summary>
        /// Parses an array literal
        /// </summary>
        static public Node ParseArrayLiteral (List<Token> Tokens, ref int Index) {
            // Create array literal node
            Node node = new Node { Body = "Array", TokenType = "ArrayLiteral" };

            // Skip to arguments, and parse arguments
            Index++;
            int i = Index;
            int endArguments = i;
            SkipFromTo(ref Index, ref endArguments, "[", "]", Tokens);

            List<List<Token>> Arguments = new List<List<Token>>();
            SetArguments(Arguments, Tokens.GetRange(i, endArguments - i));

            foreach (List<Token> Argument in Arguments) {
                Node argNode = ParseExpression(node, Argument);
                node.Add(argNode);
            }

            return node;
        }

        /// <summary>
        /// Parses a list of tokens into an expression node
        /// </summary>
        static public Node Parse(List<Token> Tokens, ref int Index) {
            Node node = new Node();
            int i = Index;

            // Skip until we hit the end of an expression, or a keyword
            while (Tokens[Index].Value != ";" && Tokens[Index].Type != "Keyword" && Index < Tokens.Count - 1) {
                Index++;
            }

            // Parse tokens in expression
            node.Add(ParseExpression(node, Tokens.GetRange(i, Index - i)));

            // Only return the first subnode, so we don't create a messy AST
            Node returnNode = null;
            if (node.SubNodes.Count > 0) {
                returnNode = node.SubNodes[0];
            }

            return returnNode;
        }
    }
}
