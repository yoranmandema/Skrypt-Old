using System;
using System.Collections.Generic;
using System.Linq;
using Skrypt.Engine;
using Skrypt.Library;
using Skrypt.Tokenization;

namespace Skrypt.Parsing
{
    public class SkipInfo
    {
        public int Delta;
        public int End;
        public int Start;
    }

    public class ScopeCheck {
        int roundScope = 0;
        int squareScope = 0;
        int curlyScope = 0;

        public bool IsInScope => roundScope == 0 && squareScope == 0 && curlyScope == 0;
        public bool IsInRoundScope => roundScope == 0;
        public bool IsInSquareScope => squareScope == 0;
        public bool IsInCurlyScope => curlyScope == 0;

        public ScopeCheck Check(Token token) {
            if (token.Type == TokenTypes.Punctuator) {
                switch (token.Value) {
                    case "(":
                        roundScope++;
                        break;
                    case ")":
                        roundScope--;
                        break;
                    case "[":
                        squareScope++;
                        break;
                    case "]":
                        squareScope--;
                        break;
                    case "{":
                        curlyScope++;
                        break;
                    case "}":
                        curlyScope--;
                        break;
                }
            }

            return this;
        }
    }

    /// <summary>
    ///     The expression parser class.
    ///     Contains all methods to parse expressions, and helper methods.
    /// </summary>
    public class ExpressionParser {
        // Create list of operator groups with the right precedence order.
        public static readonly List<OperatorGroup> OperatorPrecedence = new List<OperatorGroup> {
            new OperatorGroup(new Operator[] {
                new OpAssign(),
                new OpAssignAdd(),
                new OpAssignSubtract(),
                new OpAssignDivide(),
                new OpAssignMultiply(),
                new OpAssignModulo(),
                new OpAssignPower(),
                new OpAssignBitAnd(),
                new OpAssignBitOr(),
                new OpAssignBitXor(),
                new OpAssignBitShiftL(),
                new OpAssignBitShiftR(),
                new OpAssignBitShiftRZ()
            },false),
            new OperatorGroup(new Operator[] {new OpLambda()}, true),
            new OperatorGroup(new Operator[] {new OpBreak(),new OpContinue() }, false, 0),
            new OperatorGroup(new Operator[] {new OpReturn()}, false, 1),
            new OperatorGroup(new Operator[] {new OpConditional(),new OpConditionalElse() }, true, 0),
            new OperatorGroup(new Operator[] {new OpOr()}),
            new OperatorGroup(new Operator[] {new OpAnd()}),
            new OperatorGroup(new Operator[] {new OpBitOr()}),
            new OperatorGroup(new Operator[] {new OpBitXor()}),
            new OperatorGroup(new Operator[] {new OpBitAnd()}),
            new OperatorGroup(new Operator[] {new OpNotEqual(), new OpEqual()}),
            new OperatorGroup(new Operator[] {new OpLesser(), new OpGreater(), new OpLesserEqual(), new OpGreaterEqual()}),
            new OperatorGroup(new Operator[] {new OpBitShiftL(), new OpBitShiftR(), new OpBitShiftRZ()}),
            new OperatorGroup(new Operator[] {new OpAdd(), new OpSubtract()}),
            new OperatorGroup(new Operator[] {new OpMultiply(), new OpDivide(), new OpModulo()}),
            new OperatorGroup(new Operator[] {new OpPower()}),
            new OperatorGroup(new Operator[] {new OpBitNot()}, false, 1),
            new OperatorGroup(new Operator[] {new OpNegate(), new OpNot()}, false, 1),
            new OperatorGroup(new Operator[] {new OpPostInc(), new OpPostDec()}, false, 1, true),
            new OperatorGroup(new Operator[] {new OpCall()}, false),
            new OperatorGroup(new Operator[] {new OpIndex()}, false),
            new OperatorGroup(new Operator[] {new OpAccess()}, false),
        };

        private readonly SkryptEngine _engine;

        public ExpressionParser(SkryptEngine e) {
            _engine = e;
        }

        // (debug) Serializes a list of tokens into a string
        public static string TokenString(List<Token> tokens) {
            var sb = "";

            foreach (var token in tokens) sb += token.Value;

            return sb;
        }

        // Checks whether a list of tokens is a valid conditional statement.
        private bool IsConditional(List<Token> tokens) {
            var isConditional = false;
            var scopeCheck = new ScopeCheck();

            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];

                scopeCheck.Check(token);

                if (token.Equals("?", TokenTypes.Punctuator) && scopeCheck.IsInScope) {
                    isConditional = true;
                    SkipInfo skip = SkipFromTo("?", ":", tokens, i);
                }
            }

            return isConditional;
        }

        enum ExpressionContextType {
            None,
            Parentheses,
            FunctionCall,
            Indexing,
            ArrayLiteral,
            FunctionLiteral,
            Conditional,
            Lambda
        }

        /// <summary>
        ///     Recursively parses a list of tokens into an expression.
        /// </summary>
        public Node ParseExpression(Node branch, List<Token> tokens) {
            if (tokens.Count == 1 && tokens[0].Type != TokenTypes.Punctuator) {
                if (GeneralParser.NotPermittedInExpression.Contains(tokens[0].Value)) {
                    _engine.ThrowError("Syntax error, unexpected keyword '" + tokens[0].Value + "' found.", tokens[0]);
                }

                switch (tokens[0].Type) {
                    case TokenTypes.NullLiteral:
                        return new NullNode {
                            Token = tokens[0]
                        };
                    case TokenTypes.BooleanLiteral:
                        return new BooleanNode {
                            Value = tokens[0].Value == "true" ? true : false,
                            Token = tokens[0]
                        };
                    case TokenTypes.StringLiteral:
                        return new StringNode {
                            Value = tokens[0].Value,
                            Token = tokens[0]
                        };
                    case TokenTypes.NumericLiteral:
                        return new NumericNode {
                            Value = double.Parse(tokens[0].Value),
                            Token = tokens[0]
                        };
                    default:
                        return new Node {
                            Body = tokens[0].Value,
                            Type = tokens[0].Type,
                            Token = tokens[0]
                        };
                }
            }

            var leftBuffer = new List<Token>();
            var rightBuffer = new List<Token>();       
            var contextType = ExpressionContextType.None;
            var CallArgsStart = 0;

            void loop () {
                foreach (var op in OperatorPrecedence) {
                    foreach (var Operator in op.Operators) {
                        var i = 0;
                        var canLoop = tokens.Count > 0;

                        while (canLoop) {
                            var token = tokens[i];
                            var previousToken = i >= 1 ? tokens[i - 1] : null;

                            // Expression contains a comma without it being part of a function or array.
                            if (token.Type == TokenTypes.Punctuator && token.Value == ",") {
                                _engine.ThrowError("Syntax error, unexpected token '" + token.Value + "' found.", token);
                            }

                            // Expression contains a keyword.
                            if (GeneralParser.NotPermittedInExpression.Contains(token.Value)) {
                                _engine.ThrowError("Unexpected keyword '" + token.Value + "' found.", token);
                            }

                            // Check for function literal.
                            if (token.Equals("fn", TokenTypes.Keyword)) {
                                var skip = _engine.ExpectValue("(", tokens, i);
                                i += skip.Delta;

                                // Skip over parameters and body.
                                var start = i;
                                skip = _engine.ExpressionParser.SkipFromTo("(", ")", tokens, i);
                                i += skip.Delta;

                                _engine.ExpectValue("{", tokens, i);

                                skip = _engine.ExpressionParser.SkipFromTo("{", "}", tokens, i);
                                i += skip.Delta;

                                // Whole expression is a function literal - exit and parse it as such.
                                if (start == 1 && skip.End == tokens.Count - 1) {
                                    contextType = ExpressionContextType.FunctionLiteral;
                                    return;
                                }
                            }

                            // At this point, an opening bracket has to be proceeded by a lambda literal.
                            if (token.Equals("{", TokenTypes.Punctuator) && !previousToken.Equals("=>", TokenTypes.Punctuator)) {
                                _engine.ThrowError("Statement expected.", token);
                            }

                            // Skip parsing of assignment calls within function bodies
                            if (Operator.Operation == "=" && token.Equals("{", TokenTypes.Punctuator)) {
                                var skip = _engine.ExpressionParser.SkipFromTo("{", "}", tokens, i);
                                i += skip.Delta;
                            }

                            // Check for function call or expressions surrounded in parentheses.
                            if (token.Equals("(", TokenTypes.Punctuator)) {
                                // Skip over contents.
                                var skip = _engine.ExpressionParser.SkipFromTo("(", ")", tokens, i);
                                i += skip.Delta;

                                if (skip.Start == 0 && skip.End == tokens.Count - 1) {
                                    // Expression is fully surrounded by parentheses.
                                    contextType = ExpressionContextType.Parentheses;
                                    return;
                                } else if (skip.End == tokens.Count - 1 && skip.Start > 0 && Operator.Operation == "(") {
                                    var before = tokens[skip.Start - 1];
                                    var isFunctionCall = true;

                                    // Check whether the token preceding the opening parenthesis is an operator.
                                    // If it is, it means its not a function call.
                                    foreach (var _op in OperatorPrecedence) {
                                        foreach (var _Operator in _op.Operators) {
                                            if (before.Value == _Operator.Operation && before.Type == TokenTypes.Punctuator) {
                                                isFunctionCall = false;
                                            }
                                        }
                                    }

                                    if (isFunctionCall) {
                                        contextType = ExpressionContextType.FunctionCall;
                                        CallArgsStart = skip.Start;
                                        return;
                                    }
                                }

                                token = tokens[i];
                            }
                            // Check for array literals or index operations.
                            else if (token.Equals("[", TokenTypes.Punctuator)) {
                                var skip = _engine.ExpressionParser.SkipFromTo("[", "]", tokens, i);
                                i += skip.Delta;

                                if (skip.Start == 0 && skip.End == tokens.Count - 1) {
                                    // Expression is fully surrounded by brackets, this means its an array literal.
                                    contextType = ExpressionContextType.ArrayLiteral;
                                    return;
                                }
                                else if (skip.End == tokens.Count - 1 && skip.Start > 0 && Operator.Operation == "[") {
                                    var before = tokens[skip.Start - 1];
                                    var isIndexing = true;

                                    // Check whether the token preceding the opening bracket is an operator.
                                    // If it is, it means its not a index operation.
                                    foreach (var _op in OperatorPrecedence) {
                                        foreach (var _Operator in _op.Operators) {
                                            if (before.Value == _Operator.Operation && before.Type == TokenTypes.Punctuator) {
                                                isIndexing = false;
                                            }
                                        }
                                    }

                                    if (isIndexing) {
                                        contextType = ExpressionContextType.Indexing;
                                        CallArgsStart = skip.Start;
                                        return;
                                    }
                                }
                            }
                            // Check for operations.
                            else if (token.Value == Operator.Operation && token.Type == TokenTypes.Punctuator) {
                                if (token.Equals(":", TokenTypes.Punctuator)) {
                                    _engine.ThrowError("Incomplete conditional statement.", token);
                                } else if (token.Equals("?", TokenTypes.Punctuator)) {
                                    if (IsConditional(tokens)) {
                                        contextType = ExpressionContextType.Conditional;
                                        return;
                                    }
                                    else {
                                        _engine.ThrowError("Incomplete conditional statement.", token);
                                    }
                                }

                                // Fill left and right buffers.
                                leftBuffer = tokens.GetRange(0, i);
                                rightBuffer = tokens.GetRange(i + 1, tokens.Count - i - 1);

                                var hasRequiredLeftTokens = leftBuffer.Count > 0;
                                var hasRequiredRightTokens = rightBuffer.Count > 0;

                                if (op.Members == 1) {
                                    if (op.IsPostfix) {
                                        hasRequiredRightTokens = true;
                                    }
                                    else {
                                        hasRequiredLeftTokens = true;
                                    }
                                }

                                if (hasRequiredLeftTokens && hasRequiredRightTokens) {
                                    if (token.Equals("=>", TokenTypes.Punctuator)) {
                                        contextType = ExpressionContextType.Lambda;
                                        return;
                                    }

                                    // Create operation node with type and body.
                                    var newNode = new Node {
                                        Body = Operator.OperationName,
                                        Type = token.Type,
                                        Token = token
                                    };

                                    if (op.Members == 1) {
                                        // Check whether we actually have a value that gets operated on.
                                        if (op.IsPostfix) {
                                            if (leftBuffer.Count == 0) {
                                                _engine.ThrowError($"Syntax error, value expected after {token.Value} operator", token);
                                            }
                                        } else {
                                            if (rightBuffer.Count == 0) {
                                                _engine.ThrowError($"Syntax error, value expected before {token.Value} operator", token);
                                            }
                                        }

                                        // Parse tokens
                                        var leftNode = !op.IsPostfix ? null : ParseExpression(newNode, leftBuffer);
                                        newNode.Add(leftNode);

                                        var rightNode = op.IsPostfix ? null : ParseExpression(newNode, rightBuffer);
                                        newNode.Add(rightNode);
                                    } else {
                                        // Check whether we actually have values that get operated on.
                                        if (leftBuffer.Count == 0) {
                                            _engine.ThrowError($"Syntax error, value expected before {token.Value} operator", token);
                                        }

                                        if (rightBuffer.Count == 0) {
                                            _engine.ThrowError($"Syntax error, value expected after {token.Value} operator", token);
                                        }

                                        // Parse tokens
                                        var leftNode = ParseExpression(newNode, leftBuffer);
                                        newNode.Add(leftNode);

                                        var rightNode = ParseExpression(newNode, rightBuffer);

                                        // Is the operator a combined assignment operator? 
                                        // If so, generate a new node based on the secondary 
                                        // operator, add a copy of the left node to that, and 
                                        // add the right node to that. Then add the new node 
                                        // structure to the existing operator node.
                                        if (typeof(AssignmentOperator).IsAssignableFrom(Operator.GetType())) {
                                            var castOperator = (AssignmentOperator)Operator;
                                            var leftClone = leftNode.Copy();
                                            var secondaryOperatorNode = new Node {
                                                Body = castOperator.SecondaryOperator.OperationName,
                                                Type = TokenTypes.Punctuator,
                                                Token = token
                                            };

                                            secondaryOperatorNode.Add(leftClone);
                                            secondaryOperatorNode.Add(rightNode);

                                            newNode.Add(secondaryOperatorNode);
                                        } else {
                                            newNode.Add(rightNode);
                                        }
                                    }

                                    branch.Add(newNode);
                                    return;
                                }

                                // Only throw an exception if the operator allows it.
                                // This way unary and binary operations with the same token parse correctly.
                                if (Operator.FailOnMissingMembers) {
                                    if (op.Members == 0) {
                                        _engine.ThrowError("'" + Operator.Operation + "' operator cannot be part of expression.", token);
                                    }

                                    if (!hasRequiredRightTokens) {
                                        _engine.ThrowError("Syntax error, missing right hand operand.", token);
                                    }

                                    if (!hasRequiredLeftTokens) {
                                        _engine.ThrowError("Syntax error, missing left hand operand.", token);
                                    }
                                }
                            }

                            // Check if we're still in bounds.
                            canLoop = op.LeftAssociate ? i < tokens.Count - 1 : tokens.Count - 1 - i > 0;
                            i++;
                        }
                    }
                }
            };
            loop();

            switch (contextType) {
                case ExpressionContextType.Parentheses:
                    if (tokens.Count == 2) {
                        _engine.ThrowError("Syntax error, expression expected.", tokens[0]);
                    }

                    return ParseExpression(branch, tokens.GetRange(1, tokens.Count - 2));
                case ExpressionContextType.FunctionCall:
                    return ParseCall(tokens, CallArgsStart).Node;
                case ExpressionContextType.Indexing:
                    return ParseIndexing(tokens, CallArgsStart).Node;
                case ExpressionContextType.ArrayLiteral:
                    return ParseArrayLiteral(tokens).Node;
                case ExpressionContextType.FunctionLiteral:
                    return _engine.MethodParser.ParseFunctionLiteral(tokens.GetRange(1, tokens.Count - 1)).Node;
                case ExpressionContextType.Lambda:
                    return _engine.MethodParser.ParseLambda(tokens).Node;
                case ExpressionContextType.Conditional:
                    return ParseConditional(tokens).Node;
            }

            return null;
        }

        public Node GetOptimisedNode (Node node) {
            var result = _engine.Executor.ExecuteExpression(node, _engine.GlobalScope);
            Node newNode = node;

            Type resultType = result.GetType();

            if (resultType == typeof(Library.Native.System.Numeric)) {
                newNode = new NumericNode {
                    Value = ((Library.Native.System.Numeric)result).Value
                };
            }
            else if (resultType == typeof(Library.Native.System.String)) {
                newNode = new StringNode {
                    Value = ((Library.Native.System.String)result).Value
                };
            }
            else if (resultType == typeof(Library.Native.System.Boolean)) {
                newNode = new BooleanNode {
                    Value = ((Library.Native.System.Boolean)result).Value
                };
            }
            else if (resultType == typeof(Library.Native.System.Null)) {
                newNode = new NullNode();
            }

            newNode.Nodes = node.Nodes;

            return newNode;
        }

        /// <summary>
        ///     Parses individual arguments as expressions.
        /// </summary>
        public void SetArguments(List<List<Token>> arguments, List<Token> tokens) {
            var i = 0;
            var buffer = new List<Token>();
            var isFirst = true;
            var scopeCheck = new ScopeCheck();

            for (i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                buffer.Add(token);

                scopeCheck.Check(token);

                // Only set arguments if they're not inside nested function calls.
                if (scopeCheck.IsInScope) {
                    if (token.Equals(",", TokenTypes.Punctuator)) {
                        isFirst = false;

                        // Arguments cannot be empty.
                        if (buffer.Count == 0) {
                            _engine.ThrowError("Syntax error, missing tokens for argument.", tokens[i]);
                        }

                        buffer.RemoveAt(buffer.Count - 1);

                        arguments.Add(new List<Token>(buffer));
                        buffer.Clear();
                    }

                    // Reached the end of all argument tokens.
                    // All tokens from the start, or from the last ',' token are part of the last argument.
                    if (i == tokens.Count - 1) {
                        // Arguments cannot be empty.
                        if (buffer.Count == 0 && !isFirst) {
                            _engine.ThrowError("Syntax error, missing tokens for argument.", tokens[i]);
                        }

                        arguments.Add(new List<Token>(buffer));
                        buffer.Clear();
                    }
                }
            }
        }

        /// <summary>
        ///     Skip tokens that are surrounded by 'upScope' and 'downScope' punctuator tokens.
        /// </summary>
        public SkipInfo SkipFromTo(string upScope, string downScope, List<Token> tokens, int startingPoint = 0) {
            var start = startingPoint;
            var depth = 0;
            var index = startingPoint;
            var end = 0;
            // The first opening token found - used for exceptions.
            var firstToken = default(Token);

            while (true) {
                if (tokens[index].Value == upScope && tokens[index].Type == TokenTypes.Punctuator) {
                    depth++;

                    if (firstToken == null) {
                        firstToken = tokens[index];
                    }
                } else if (tokens[index].Value == downScope && tokens[index].Type == TokenTypes.Punctuator) {
                    depth--;

                    // A pair of opening and closing tokens have been found.
                    if (depth == 0) {
                        end = index;
                        var delta = index - start;

                        return new SkipInfo {Start = start, End = end, Delta = delta};
                    }
                }

                index++;

                if (index == tokens.Count) {
                    if (depth > 0) {
                        _engine.ThrowError("Syntax error, closing token '" + downScope + "' not found", firstToken);
                    } else if (depth < 0) {
                        _engine.ThrowError("Syntax error, opening token '" + upScope + "' not found", tokens[index]);
                    }
                }
            }
        }

        /// <summary>
        ///     Skip tokens until a token with the given value has been reached.
        /// </summary>
        public void SkipUntil(ref int index, Token comparator, List<Token> tokens) {
            var startToken = tokens[index];

            while (!tokens[index].LazyEqual(comparator)){
                index++;

                if (index == tokens.Count - 1) {
                    _engine.ThrowError("Token '" + comparator + "' expected", startToken);
                }
            }
        }     

        /// <summary>
        ///     Parses a method call with arguments.
        /// </summary>
        public ParseResult ParseCall(List<Token> tokens, int argsStart) {
            var name = tokens[0].Value;
            var node = new CallNode();

            // Parse tokens that define what function is getting called.
            var accessTokens = tokens.GetRange(0, argsStart);
            node.Getter = ParseClean(accessTokens);

            // Parse arguments
            var result = _engine.GeneralParser.ParseSurroundedExpressions("(", ")", argsStart, tokens);
            node.Arguments = new List<Node>(result.Node.Nodes);

            return new ParseResult {Node = node, Delta = tokens.Count - 1 };
        }

        /// <summary>
        ///     Parses an index operation.
        /// </summary>
        public ParseResult ParseIndexing(List<Token> tokens, int argsStart)
        {
            var name = tokens[0].Value;
            var node = new IndexNode();

            // Parse tokens that define what object we're indexing.
            var accessTokens = tokens.GetRange(0, argsStart);
            node.Getter = ParseClean(accessTokens);

            // Parse arguments
            var result = _engine.GeneralParser.ParseSurroundedExpressions("[", "]", argsStart, tokens);

            if (result.Node.Nodes.Count == 0) {
                _engine.ThrowError("Syntax error, index operator arguments can't be empty.", tokens[argsStart + 1]);
            }

            node.Arguments = new List<Node>(result.Node.Nodes);

            return new ParseResult { Node = node, Delta = tokens.Count - 1 };
        }

        /// <summary>
        ///     Parses an array literal.
        /// </summary>
        public ParseResult ParseArrayLiteral(List<Token> tokens) {
            var result = _engine.GeneralParser.ParseSurroundedExpressions("[", "]", 0, tokens);

            var node = new ArrayNode {
                Values = new List<Node>(result.Node.Nodes)
            };

            return new ParseResult {Node = node, Delta = result.Delta };
        }

        /// <summary>
        ///     Parses a using statement.
        /// </summary>
        public ParseResult ParseUsing(List<Token> tokens) {
            var node = new UsingNode();
            var isIdentifier = true;
            var buffer = new List<Token>();
            var skip = _engine.ExpectType(TokenTypes.Identifier, tokens);
            var nextToken = default(Token);
            int i = 1;

            while (i < tokens.Count) {
                var token = tokens[i];

                if (token.Type == TokenTypes.EndOfExpression) {
                    break;
                }

                if (i < tokens.Count - 2) {
                    nextToken = tokens[i + 1];
                }

                // A using statement can only be used on objects, or the members of objects.
                if (isIdentifier) {
                    if (nextToken?.Type != TokenTypes.EndOfExpression && i < tokens.Count - 1) {
                        var s = _engine.ExpectValue(".", tokens, i);
                        isIdentifier = false;
                    }
                } else {
                    var s = _engine.ExpectType(TokenTypes.Identifier, tokens, i);
                    isIdentifier = true;
                }

                buffer.Add(token);

                i++;
            }

            node.Getter = Parse(buffer).Node;

            return new ParseResult { Node = node, Delta = i + 1};
        }

        /// <summary>
        ///     Parses a conditional statement.
        /// </summary>
        public ParseResult ParseConditional(List<Token> tokens) {
            var node = new ConditionalNode();
            var scopeCheck = new ScopeCheck();

            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];

                scopeCheck.Check(token);

                if (token.Equals("?", TokenTypes.Punctuator) && scopeCheck.IsInScope) {
                    SkipInfo skip = SkipFromTo("?", ":", tokens, i);

                    var conditionNode = ParseClean(tokens.GetRange(0, i));
                    var successNode = ParseClean(tokens.GetRange(i + 1, skip.Delta - 1));
                    var failNode = ParseClean(tokens.GetRange(skip.End + 1, tokens.Count - (skip.End + 1)));

                    node.Condition = conditionNode ?? throw new SkryptException("Syntax error, condition statement can't be empty");
                    node.Pass = successNode ?? throw new SkryptException("Syntax error, consequent statement can't be empty");
                    node.Fail = failNode ?? throw new SkryptException("Syntax error, alternative statement can't be empty");

                    return new ParseResult { Node = node, Delta = tokens.Count };
                }
            }

            _engine.ThrowError("Syntax error, conditional statement incomplete", tokens[0]);
            return null;
        }

        /// <summary>
        ///     Parses an expression node without any parenting node
        /// </summary>
        public Node ParseClean(List<Token> tokens) {
            var node = new Node();
            node.Add(ParseExpression(node, tokens));

            // Only return the first subnode, so we don't create a messy AST
            Node returnNode = null;
            if (node.Nodes.Count > 0) { returnNode = node.Nodes[0]; }

            return returnNode;
        }

        /// <summary>
        ///     Parses a list of tokens into an expression node
        /// </summary>
        public ParseResult Parse(List<Token> tokens) {
            var node = new Node();
            var delta = 0;
            var addDelta = 0;
            var deltaOffset = 0;
            var scopeCheck = new ScopeCheck();
            var previousToken = default(Token);

            if (tokens[0].Type == TokenTypes.EndOfExpression) {
                delta++;
                deltaOffset = 1;
            }

            // Loop until we hit the end of an expression, or consumed all tokens.
            while (true) {
                scopeCheck.Check(tokens[delta]);

                if (scopeCheck.IsInScope && tokens[delta].Type == TokenTypes.EndOfExpression) { 
                    if (tokens[delta].Type == TokenTypes.EndOfExpression) {
                        addDelta = 1;
                        break;
                    }
                }

                previousToken = tokens[delta];

                delta++;

                if (delta == tokens.Count) break;
            }

            var returnNode = ParseClean(tokens.GetRange(deltaOffset, delta - deltaOffset));

            delta += addDelta;

            return new ParseResult {Node = returnNode, Delta = delta};
        }

        /// <summary>
        ///     Class representing an operator group
        /// </summary>
        public class OperatorGroup
        {
            public readonly bool IsPostfix;
            public readonly bool LeftAssociate = true;
            public readonly int Members;
            public readonly List<Operator> Operators = new List<Operator>();

            public OperatorGroup(Operator[] ops, bool la = true, int mems = 2, bool pf = false)
            {
                Operators = ops.ToList();
                Members = mems;
                LeftAssociate = la;
                IsPostfix = pf;
            }
        }
    }
}