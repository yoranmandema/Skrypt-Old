using System;
using System.Collections.Generic;
using System.Linq;
using Skrypt.Engine;
using Skrypt.Library;
using Skrypt.Tokenization;

namespace Skrypt.Parsing
{
    public class skipInfo
    {
        public int delta;
        public int end;
        public int start;
    }

    /// <summary>
    ///     The expression parser class.
    ///     Contains all methods to parse expressions, and helper methods
    /// </summary>
    public class ExpressionParser
    {
        // Create list of operator groups with the right precedence order
        private static readonly List<OperatorGroup> OperatorPrecedence = new List<OperatorGroup>
        {
            new OperatorGroup(new[] {new Op_Return()}, false, 1),
            new OperatorGroup(new[] {new Op_Assign()}, false),
            new OperatorGroup(new[] {new Op_Access()}, false, 2, false),
            new OperatorGroup(new[] {new Op_Or()}),
            new OperatorGroup(new[] {new Op_And()}),
            new OperatorGroup(new Operator[] {new Op_NotEqual(), new Op_Equal()}),
            new OperatorGroup(new Operator[]
                {new Op_Lesser(), new Op_Greater(), new Op_LesserEqual(), new Op_GreaterEqual()}),
            new OperatorGroup(new Operator[] {new Op_Add(), new Op_Subtract()}),
            new OperatorGroup(new Operator[] {new Op_Multiply(), new Op_Divide(), new Op_Modulo()}),
            new OperatorGroup(new[] {new Op_Power()}),
            new OperatorGroup(new Operator[] {new Op_Negate(), new Op_Not()}, false, 1),
            new OperatorGroup(new Operator[] {new Op_PostInc(), new Op_PostDec()}, false, 1, true)
        };

        private readonly SkryptEngine engine;

        public ExpressionParser(SkryptEngine e)
        {
            engine = e;
        }

        // (debug) Serializes a list of tokens into a string
        public static string TokenString(List<Token> Tokens)
        {
            var sb = "";

            foreach (var token in Tokens) sb += token.Value;

            return sb;
        }

        /// <summary>
        ///     Parses a list of tokens into an expression recursively
        /// </summary>
        public Node ParseExpression(Node branch, List<Token> Tokens)
        {
            if (Tokens.Count == 1)
                return new Node
                {
                    Body = Tokens[0].Value,
                    //Value = null,
                    TokenType = "" + Tokens[0].Type,
                    Token = Tokens[0]
                };

            // Create left and right token buffers
            var leftBuffer = new List<Token>();
            var rightBuffer = new List<Token>();

            var isInPars = false;
            var isMethodCall = false;
            var isIndexing = false;
            var isArrayLiteral = false;
            var isFunctionLiteral = false;
            var isChain = false;

            var accessEnd = 0;

            // Do logic in delegate so we can easily exit out of it when we need to
            Action loop = () =>
            {
                foreach (var OP in OperatorPrecedence)
                foreach (var Operator in OP.Operators)
                {
                    var i = 0;
                    var CanLoop = Tokens.Count > 0;

                    while (CanLoop)
                    {
                        var s = skipChain(Tokens, i);

                        if (s.delta > 0)
                        {
                            i = s.end - 1;
                            if (s.start == 0 && i == Tokens.Count - 1 && s.delta != 0)
                            {
                                isChain = true;
                                return;
                            }
                        }

                        var token = Tokens[i];
                        var previousToken = i >= 1 ? Tokens[i - 1] : null;

                        if (token.Value == "func")
                        {
                            var skip = engine.expectValue("(", Tokens, i);
                            i += skip.delta;

                            var start = i;
                            skip = engine.expressionParser.SkipFromTo("(", ")", Tokens, i);
                            i += skip.delta;

                            skip = engine.expressionParser.SkipFromTo("{", "}", Tokens, i);
                            i += skip.delta;

                            if (start == 1 && skip.end == Tokens.Count - 1)
                            {
                                isFunctionLiteral = true;
                                return;
                            }
                        }

                        if (GeneralParser.Keywords.Contains(Tokens[i].Value))
                            engine.throwError("Unexpected keyword '" + Tokens[i].Value + "' found", Tokens[i], 2);

                        //if (token.Value == "(") {
                        //    if (previousToken != null) {
                        //        // Previous token was identifier; possible method call
                        //        if (previousToken.Type == TokenTypes.Identifier) {
                        //            skipInfo skip = engine.expressionParser.SkipFromTo("(", ")", Tokens, i);
                        //            i += skip.delta;

                        //            if (skip.start == 1 && skip.end == Tokens.Count - 1) {
                        //                isMethodCall = true;
                        //                return;
                        //            }
                        //        }
                        //    }
                        //}
                        if (token.Value == "(" && token.Type == TokenTypes.Punctuator)
                        {
                            var skip = engine.expressionParser.SkipFromTo("(", ")", Tokens, i);
                            i += skip.delta;

                            if (skip.start == 0 && skip.end == Tokens.Count - 1)
                            {
                                isInPars = true;
                                return;
                            }
                        }
                        //if (token.Value == "[") {
                        //    if (previousToken != null) {
                        //        // Previous token was identifier or string; possible indexing
                        //        if (previousToken.Type == TokenTypes.Identifier || previousToken.Type == TokenTypes.StringLiteral) {
                        //            skipInfo skip = engine.expressionParser.SkipFromTo("[", "]", Tokens, i);
                        //            i += skip.delta;

                        //            if (skip.start == 1 && skip.end == Tokens.Count - 1) {
                        //                isIndexing = true;
                        //                return;
                        //            }
                        //        }
                        //    }
                        //}
                        if (token.Value == "[" && token.Type == TokenTypes.Punctuator)
                        {
                            var skip = engine.expressionParser.SkipFromTo("[", "]", Tokens, i);
                            i += skip.delta;

                            if (skip.start == 0 && skip.end == Tokens.Count - 1)
                            {
                                isArrayLiteral = true;
                                return;
                            }
                        }

                        if (token.Value == Operator.Operation && token.Type == TokenTypes.Punctuator)
                        {
                            // Fill left and right buffers
                            leftBuffer = Tokens.GetRange(0, i);
                            rightBuffer = Tokens.GetRange(i + 1, Tokens.Count - i - 1);

                            var HasRequiredLeftTokens = leftBuffer.Count > 0;
                            var HasRequiredRightTokens = rightBuffer.Count > 0;

                            if (OP.Members == 1)
                            {
                                if (OP.IsPostfix)
                                    HasRequiredRightTokens = true;
                                else
                                    HasRequiredLeftTokens = true;
                            }

                            if (HasRequiredLeftTokens && HasRequiredRightTokens)
                            {
                                // Create operation node with type and body
                                var NewNode = new Node
                                {
                                    Body = Operator.OperationName,
                                    TokenType = "" + token.Type,
                                    Token = token
                                };

                                if (OP.Members == 1)
                                {
                                    // Parse unary and do postfix logic

                                    var LeftNode = !OP.IsPostfix ? null : ParseExpression(NewNode, leftBuffer);
                                    NewNode.Add(LeftNode);

                                    var RightNode = OP.IsPostfix ? null : ParseExpression(NewNode, rightBuffer);
                                    NewNode.Add(RightNode);
                                }
                                else
                                {
                                    // Parse operators that need 2 sides

                                    var LeftNode = ParseExpression(NewNode, leftBuffer);
                                    NewNode.Add(LeftNode);

                                    var RightNode = ParseExpression(NewNode, rightBuffer);
                                    NewNode.Add(RightNode);
                                }

                                branch.Add(NewNode);
                                return;
                            }

                            if (Operator.FailOnMissingMembers)
                                engine.throwError("Missing member of operation!", token, 10);
                        }

                        // Check if we're still in bounds
                        CanLoop = OP.LeftAssociate ? i < Tokens.Count - 1 : Tokens.Count - 1 - i > 0;
                        i++;
                    }
                }
            };
            loop();

            if (isChain) return ParseChain(Tokens);

            // Parse expression within parenthesis if it's completely surrounded
            if (isInPars) return ParseExpression(branch, Tokens.GetRange(1, Tokens.Count - 2));

            // Parse method call
            if (isMethodCall)
            {
                var result = ParseCall(Tokens, accessEnd);
                return result.node;
            }

            // Parse indexing
            if (isIndexing)
            {
                var result = ParseIndexing(Tokens);
                return result.node;
            }

            // Parse indexing
            if (isArrayLiteral)
            {
                var result = ParseArrayLiteral(Tokens);
                return result.node;
            }

            // Parse function literal
            if (isFunctionLiteral)
            {
                var result = engine.methodParser.ParseFunctionLiteral(Tokens.GetRange(1, Tokens.Count - 1));
                return result.node;
            }

            return null;
        }

        /// <summary>
        ///     Sets parses individual arguments as expressions
        /// </summary>
        public static void SetArguments(List<List<Token>> Arguments, List<Token> Tokens)
        {
            var depth = 0;
            var indexDepth = 0;
            var i = 0;
            var startArg = 0;
            var Buffer = new List<Token>();

            for (i = 0; i < Tokens.Count; i++)
            {
                var token = Tokens[i];

                if (token.Value == "(" && token.Type == TokenTypes.Punctuator)
                    depth++;
                else if (token.Value == ")" && token.Type == TokenTypes.Punctuator) depth--;

                if (token.Value == "[" && token.Type == TokenTypes.Punctuator)
                    indexDepth++;
                else if (token.Value == "]" && token.Type == TokenTypes.Punctuator) indexDepth--;

                if ((token.Value == "," && token.Type == TokenTypes.Punctuator || i == Tokens.Count - 1) &&
                    depth == 0 && indexDepth == 0)
                {
                    Buffer = Tokens.GetRange(startArg, (i == Tokens.Count - 1 ? i + 1 : i) - startArg);
                    startArg = i + 1;
                    Arguments.Add(Buffer);
                }
            }
        }

        /// <summary>
        ///     Skip tokens that are surrounded by 'upScope' and 'downScope'
        /// </summary>
        public skipInfo SkipFromTo(string upScope, string downScope, List<Token> Tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var depth = 0;
            var index = startingPoint;
            var end = 0;
            Token firstToken = null;

            while (true)
            {
                if (Tokens[index].Value == upScope && Tokens[index].Type == TokenTypes.Punctuator)
                {
                    depth++;

                    if (firstToken == null) firstToken = Tokens[index];
                }
                else if (Tokens[index].Value == downScope && Tokens[index].Type == TokenTypes.Punctuator)
                {
                    depth--;

                    if (depth == 0)
                    {
                        end = index;
                        var delta = index - start;

                        return new skipInfo {start = start, end = end, delta = delta};
                    }
                }

                index++;

                if (index == Tokens.Count)
                {
                    if (depth > 0)
                        engine.throwError("Closing token '" + downScope + "' not found", firstToken);
                    else if (depth < 0) engine.throwError("Opening token '" + upScope + "' not found", Tokens[index]);
                }
            }
        }

        /// <summary>
        ///     Skip tokens until we hit a token with the given value
        /// </summary>
        public void SkipUntil(ref int Index, Token Comparator, List<Token> Tokens)
        {
            var startToken = Tokens[Index];

            while (!Tokens[Index].LazyEqual(Comparator))
            {
                Index++;

                if (Index == Tokens.Count - 1) engine.throwError("Token '" + Comparator + "' expected", startToken);
            }
        }

        //public skipInfo SkipAccessing (List<Token> Tokens, int startingPoint = 0) {
        //    int start = startingPoint;
        //    int index = startingPoint;
        //    int end = 0;
        //    int state = 1;

        //    Token nextToken = Tokens[index];

        //    if (nextToken.Type == TokenTypes.Identifier) {
        //        state = 1;
        //    } else if (nextToken.Value == ".") {
        //        state = 0;
        //    }

        //    while (true) {
        //        if (state == 1) {
        //            if (nextToken.Type == TokenTypes.Identifier) {
        //                state = 0;
        //            } else {
        //                engine.throwError("Identifier expected!", nextToken);
        //            }
        //        } else if (state == 0) {
        //            if (nextToken.Value == ".") {
        //                state = 1;
        //            } else {
        //                break;
        //            }
        //        }

        //        index++;
        //        nextToken = Tokens[index];
        //    }

        //    end = index;
        //    int delta = index - start;

        //    return new skipInfo { start = start, end = end, delta = delta };
        //}

        public skipInfo SkipAccess(List<Token> Tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var index = startingPoint;
            var end = 0;
            var state = 0;
            var token = Tokens[index];

            if (token.IsValuable()) state = 0;

            if (token.Value == ".") state = 1;

            while (true)
            {
                if (token.IsValuable() && state == 0)
                    state = 1;
                else if (token.Value == "." && state == 1)
                    state = 0;
                else
                    break;

                index++;

                if (index == Tokens.Count)
                    break;

                token = Tokens[index];
            }

            if (index > 0)
                if (Tokens[index - 1].Value == "." && Tokens[index - 1].Type == TokenTypes.Punctuator)
                    engine.throwError("Identifier expected!", Tokens[index - 1]);

            end = index;
            var delta = index - start;

            return new skipInfo {start = start, end = end, delta = delta};
        }

        public skipInfo skipChain(List<Token> Tokens, int startingPoint)
        {
            var start = startingPoint;
            var index = startingPoint;
            var end = 0;

            var token = Tokens[index];

            if (!token.IsValuable()) return new skipInfo {start = start, end = end, delta = 0};

            while (true)
            {
                //Console.WriteLine(token);

                if (token.Value == "(" && token.Type == TokenTypes.Punctuator)
                {
                    var skip = engine.expressionParser.SkipFromTo("(", ")", Tokens, index);
                    index = skip.end + 1;
                }
                else if (token.Value == "[" && token.Type == TokenTypes.Punctuator)
                {
                    var skip = engine.expressionParser.SkipFromTo("[", "]", Tokens, index);
                    index = skip.end + 1;
                }
                else if (token.Value == "." && token.Type == TokenTypes.Punctuator || token.IsValuable())
                {
                    var skip = engine.expressionParser.SkipAccess(Tokens, index);
                    index = skip.end;
                }
                else
                {
                    break;
                }

                if (index == Tokens.Count)
                    break;

                token = Tokens[index];
            }

            end = index;
            var delta = index - start;

            return new skipInfo {start = start, end = end, delta = delta};
        }

        public Node ParseChain(List<Token> Tokens)
        {
            var node = new Node();

            if (Tokens.Count == 2) engine.throwError("Access operator can only be used after a value!", Tokens[0]);

            if (Tokens.Count == 1) return ParseExpression(node, Tokens);

            var Reverse = Tokens.GetRange(0, Tokens.Count);
            Reverse.Reverse();

            if (Reverse[0].Value == "]" && Reverse[0].Type == TokenTypes.Punctuator)
            {
                var skip = SkipFromTo("]", "[", Reverse, 0);

                if (skip.end + 1 >= Reverse.Count)
                    engine.throwError("Indexing operator needs left hand value!", Reverse[skip.end]);
                else if (Reverse[skip.end + 1].Value == "." && Reverse[skip.end + 1].Type == TokenTypes.Punctuator)
                    engine.throwError("Indexing operator needs left hand value!", Reverse[skip.end]);

                var getterNode = new Node();
                getterNode.Add(ParseChain(Tokens.GetRange(0, Tokens.Count - (skip.end + 1))));
                getterNode.Body = "Getter";
                getterNode.TokenType = "Getter";
                node.Add(getterNode);

                //List<Token> ExpressionTokens = Reverse.GetRange(1, skip.end - 1);
                //ExpressionTokens.Reverse();
                //node.Add(ParseExpression(node, ExpressionTokens));

                var ArgumentTokens = Reverse.GetRange(0, skip.end + 1);
                ArgumentTokens.Reverse();

                var result = engine.generalParser.parseSurroundedExpressions("[", "]", 0, ArgumentTokens);
                var argumentsNode = result.node;
                argumentsNode.Body = "Arguments";
                argumentsNode.TokenType = "Arguments";
                node.Add(argumentsNode);

                node.Body = "Index";
                node.TokenType = "Index";
            }
            else if (Reverse[0].Value == ")")
            {
                var skip = SkipFromTo(")", "(", Reverse, 0);

                if (skip.end + 1 >= Reverse.Count)
                    engine.throwError("Call operator needs left hand value!", Reverse[skip.end]);
                else if (Reverse[skip.end + 1].Value == ".")
                    engine.throwError("Call operator needs left hand value!", Reverse[skip.end]);

                var getterNode = new Node();
                getterNode.Add(ParseChain(Tokens.GetRange(0, Tokens.Count - (skip.end + 1))));
                getterNode.Body = "Getter";
                getterNode.TokenType = "Getter";
                node.Add(getterNode);

                var ArgumentTokens = Reverse.GetRange(0, skip.end + 1);
                ArgumentTokens.Reverse();

                var result = engine.generalParser.parseSurroundedExpressions("(", ")", 0, ArgumentTokens);
                var argumentsNode = result.node;
                argumentsNode.Body = "Arguments";
                argumentsNode.TokenType = "Arguments";
                node.Add(argumentsNode);

                node.Body = "Call";
                node.TokenType = "Call";
            }
            else
            {
                node.Body = "access";
                node.TokenType = "" + TokenTypes.Punctuator;

                node.Add(ParseExpression(node, new List<Token> {Reverse[0]}));
                node.Add(ParseChain(Tokens.GetRange(0, Tokens.Count - 2)));
            }

            return node;
        }

        /// <summary>
        ///     Parses a method call with arguments
        /// </summary>
        public ParseResult ParseCall(List<Token> Tokens, int accessEnd)
        {
            var index = 0;
            var name = Tokens[index].Value;
            var node = new Node
            {
                Body = "Call",
                TokenType = "Call"
            };

            var AccessTokens = Tokens.GetRange(0, accessEnd);
            //Console.WriteLine("Call: " + TokenString(AccessTokens));
            var getterNode = new Node();
            getterNode.Add(ParseExpression(getterNode, AccessTokens));
            getterNode.Body = "Getter";
            getterNode.TokenType = "Getter";
            node.Add(getterNode);

            var result = engine.generalParser.parseSurroundedExpressions("(", ")", accessEnd, Tokens);
            var argumentsNode = result.node;
            argumentsNode.Body = "Arguments";
            argumentsNode.TokenType = "Arguments";
            node.Add(argumentsNode);

            index = Tokens.Count - 1;

            //Console.WriteLine(node);

            //skipInfo skip = engine.expectValue("(", Tokens);
            //index += skip.delta;

            //ParseResult result = engine.generalParser.parseSurroundedExpressions("(", ")", accessEnd, Tokens);
            //Node node = result.node;
            //node.TokenType = "Call";
            //node.Body = name;
            //node.Token = Tokens[0];
            //index += result.delta;

            return new ParseResult {node = node, delta = index};
        }

        /// <summary>
        ///     Parses an index operation
        /// </summary>
        public ParseResult ParseIndexing(List<Token> Tokens)
        {
            var node = new Node {Body = Tokens[0].Value, TokenType = "Index"};
            var index = 1;

            var result =
                engine.generalParser.parseSurrounded("[", "]", index, Tokens, engine.expressionParser.ParseClean);
            var argNode = result.node;
            index += result.delta;
            node.Add(argNode);
            node.Token = Tokens[0];

            return new ParseResult {node = node, delta = index};
        }

        /// <summary>
        ///     Parses an array literal
        /// </summary>
        public ParseResult ParseArrayLiteral(List<Token> Tokens)
        {
            var index = 0;

            var result = engine.generalParser.parseSurroundedExpressions("[", "]", 0, Tokens);
            var node = result.node;
            node.TokenType = "ArrayLiteral";
            node.Body = "Array";
            index += result.delta;

            return new ParseResult {node = node, delta = index};
        }

        /// <summary>
        ///     Parses an expression node without any parenting node
        /// </summary>
        public Node ParseClean(List<Token> Tokens)
        {
            var node = new Node();
            node.Add(ParseExpression(node, Tokens));

            // Only return the first subnode, so we don't create a messy AST
            Node returnNode = null;
            if (node.SubNodes.Count > 0) returnNode = node.SubNodes[0];

            return returnNode;
        }

        /// <summary>
        ///     Parses a list of tokens into an expression node
        /// </summary>
        public ParseResult Parse(List<Token> Tokens)
        {
            var node = new Node();
            var delta = 0;
            var pScope = 0;
            var bScope = 0;
            var cScope = 0;
            var addDelta = 0;

            Token previousToken = null;

            // Skip until we hit the end of an expression
            while (true)
            {
                if (Tokens[delta].Type == TokenTypes.Punctuator)
                    switch (Tokens[delta].Value)
                    {
                        case "(":
                            pScope++;
                            break;
                        case ")":
                            pScope--;
                            break;
                        case "[":
                            bScope++;
                            break;
                        case "]":
                            bScope--;
                            break;
                        case "{":
                            cScope++;
                            break;
                        case "}":
                            cScope--;
                            break;
                    }

                if (pScope == 0 && bScope == 0 && cScope == 0)
                    if (Tokens[delta].Value == ";")
                    {
                        addDelta = 1;
                        break;
                    }

                previousToken = Tokens[delta];

                delta++;

                if (delta == Tokens.Count) break;
            }

            var returnNode = ParseClean(Tokens.GetRange(0, delta));

            delta += addDelta;

            return new ParseResult {node = returnNode, delta = delta};
        }

        /// <summary>
        ///     Class representing an operator group
        /// </summary>
        private class OperatorGroup
        {
            public readonly bool IsPostfix;
            public readonly bool LeftAssociate = true;
            public readonly int Members;
            public readonly List<Operator> Operators = new List<Operator>();

            public OperatorGroup(Operator[] ops, bool LA = true, int mems = 2, bool PF = false)
            {
                Operators = ops.ToList();
                Members = mems;
                LeftAssociate = LA;
                IsPostfix = PF;
            }
        }
    }
}