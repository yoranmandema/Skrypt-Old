﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Engine;

namespace Skrypt.Parsing {
    public class skipInfo {
        public int start;
        public int end;
        public int delta;
    }

    /// <summary>
    /// The expression parser class.
    /// Contains all methods to parse expressions, and helper methods
    /// </summary>
    class ExpressionParser {
        SkryptEngine engine;

        public ExpressionParser(SkryptEngine e) {
            engine = e;
        }

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
        public static string TokenString (List<Token> Tokens) {
            string sb = "";

            foreach (Token token in Tokens) {
                sb += token.Value;
            }

            return sb;
        }

        /// <summary>
        /// Parses a list of tokens into an expression recursively
        /// </summary>
        public Node ParseExpression (Node branch, List<Token> Tokens) {
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

                            if (Tokens[i].Type == "Keyword") {
                                engine.throwError("Unexpected keyword '" + Tokens[i].Value + "' found", Tokens[i]);
                            }

                            if (token.Value == "(") {
                                if (previousToken != null) {
                                    // Previous token was identifier; possible method call
                                    if (previousToken.Type == "Identifier") {
                                        int start = i;
                                        ParseResult result = ParseCall(Tokens, i);
                                        i += result.delta;

                                        // Only add method call node if all tokens were consumed in it
                                        if (i == Tokens.Count - 1 && start == 1) {
                                            branch.Add(result.node);                                      
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
                                        ParseResult result = ParseIndexing(Tokens, i);
                                        i += result.delta;

                                        // Only add indexing node if all tokens were consumed in it
                                        if (i == Tokens.Count - 1) {
                                            branch.Add(result.node);
                                            return;
                                        }
                                    }
                                } else {
                                    ParseResult result = ParseArrayLiteral(Tokens, i);
                                    i += result.delta;

                                    // Only add indexing node if all tokens were consumed in it
                                    if (i == Tokens.Count - 1) {
                                        branch.Add(result.node);
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
        public skipInfo SkipFromTo(string upScope, string downScope, List<Token> Tokens, int startingPoint = 0) {
            int start = startingPoint;
            int depth = 0;
            int index = 0;
            int end = 0;
            Token firstToken = null;

            while (true) {
                if (Tokens[index].Value == upScope) {
                    depth++;

                    if (firstToken == null) {
                        firstToken = Tokens[index];
                    }
                }
                else if (Tokens[index].Value == downScope) {
                    depth--;

                    if (depth == 0) {
                        end = index;
                        int delta = index - start;

                        return new skipInfo {start=start, end=end, delta=delta};
                    }
                }

                index++;

                if (index == Tokens.Count) {
                    if (depth > 0) {
                        engine.throwError("Closing token '" + downScope + "' not found", firstToken);
                    } else if (depth < 0) {
                        engine.throwError("Opening token '" + upScope + "' not found", Tokens[index]);
                    }
                }
            }
        }

        /// <summary>
        /// Skip tokens until we hit a token with the given value
        /// </summary>
        public void SkipUntil (ref int Index, Token Comparator, List<Token> Tokens) {
            Token startToken = Tokens[Index];

            while (!Tokens[Index].LazyEqual(Comparator)) {
                Index++;

                if (Index == Tokens.Count - 1) {
                    engine.throwError("Token '" + Comparator + "' expected", startToken);
                }
            }
        }

        /// <summary>
        /// Parses a method call with arguments
        /// </summary>
        public ParseResult ParseCall(List<Token> Tokens, int startingPoint = 0) {
            int index = startingPoint;
            skipInfo skip;

            // Create method call node with identifier as body
            Node node = new Node { Body = Tokens[index - 1].Value, TokenType = "Call" };

            // Skip to arguments, and parse arguments
            int i = index + 1;
            skip = SkipFromTo("(", ")", Tokens, index);
            int endArguments = skip.end;
            index = skip.end;

            List<List<Token>> Arguments = new List<List<Token>>();
            SetArguments(Arguments, Tokens.GetRange(i, endArguments - i));

            foreach (List<Token> Argument in Arguments) {
                Node argNode = ParseExpression(node, Argument);
                node.Add(argNode);
            }

            int delta = index - startingPoint;

            return new ParseResult {node=node,delta=delta};
        }

        /// <summary>
        /// Parses an index operation
        /// </summary>
        public ParseResult ParseIndexing(List<Token> Tokens, int startingPoint) {
            int index = startingPoint;
            skipInfo skip;
            Node node = new Node { Body = Tokens[index - 1].Value, TokenType = "Index" };

            // Skip to index expression, and as expression
            int i = index + 1;
            skip = SkipFromTo("[", "]", Tokens, index);
            int endArguments = skip.end;
            index = skip.end;

            Node argNode = ParseExpression(node, Tokens.GetRange(i, endArguments - i));
            node.Add(argNode);

            int delta = index - startingPoint;

            return new ParseResult { node = node, delta = delta };
        }

        /// <summary>
        /// Parses an array literal
        /// </summary>
        public ParseResult ParseArrayLiteral (List<Token> Tokens, int startingPoint) {
            int index = startingPoint;
            skipInfo skip;
            Node node = new Node { Body = "Array", TokenType = "ArrayLiteral" };

            // Skip to arguments, and parse arguments
            int i = index + 1;
            skip = SkipFromTo("[", "]", Tokens, index);
            int endArguments = skip.end;
            index = skip.end;

            List<List<Token>> Arguments = new List<List<Token>>();
            SetArguments(Arguments, Tokens.GetRange(i, endArguments - i));

            foreach (List<Token> Argument in Arguments) {
                Node argNode = ParseExpression(node, Argument);
                node.Add(argNode);
            }

            int delta = index - startingPoint;

            return new ParseResult { node = node, delta = delta };
        }

        /// <summary>
        /// Parses a list of tokens into an expression node
        /// </summary>
        public ParseResult Parse(List<Token> Tokens) {
            Node node = new Node();
            int i = 0;
            int delta = 0;

            // Skip until we hit the end of an expression, or a keyword
            while (Tokens[delta].Value != ";"/* && Tokens[Index].Type != "Keyword"*/ && delta < Tokens.Count - 1) {
                delta++;
            }

            // Parse tokens in expression
            node.Add(ParseExpression(node, Tokens.GetRange(i, delta - i)));

            // Only return the first subnode, so we don't create a messy AST
            Node returnNode = null;
            if (node.SubNodes.Count > 0) {
                returnNode = node.SubNodes[0];
            }

            return new ParseResult {node = returnNode, delta = delta};
        }
    }
}
