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

    /// <summary>
    ///     The expression parser class.
    ///     Contains all methods to parse expressions, and helper methods
    /// </summary>
    public class ExpressionParser
    {
        // Create list of operator groups with the right precedence order
        public static readonly List<OperatorGroup> OperatorPrecedence = new List<OperatorGroup>
        {
            new OperatorGroup(new Operator[] {new OpAssign()}, false),
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

        public ExpressionParser(SkryptEngine e)
        {
            _engine = e;
        }

        // (debug) Serializes a list of tokens into a string
        public static string TokenString(List<Token> tokens)
        {
            var sb = "";

            foreach (var token in tokens) sb += token.Value;

            return sb;
        }

        private bool IsConditional(List<Token> tokens) {
            bool isConditional = false;

            int depth = 0;

            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];

                if (token.Equals("(", TokenTypes.Punctuator)) {
                    depth++;
                } else if (token.Equals(")", TokenTypes.Punctuator)) {
                    depth--;
                }

                if (token.Equals("?", TokenTypes.Punctuator) && depth == 0) {
                    isConditional = true;
                    SkipInfo skip = SkipFromTo("?", ":", tokens, i);
                }
            }

            return isConditional;
        }

        /// <summary>
        ///     Parses a list of tokens into an expression recursively
        /// </summary>
        public Node ParseExpression(Node branch, List<Token> tokens)
        {
            if (tokens.Count == 1 && tokens[0].Type != TokenTypes.Punctuator) {
                if (GeneralParser.NotPermittedInExpression.Contains(tokens[0].Value))
                    _engine.ThrowError("Syntax error, unexpected keyword '" + tokens[0].Value + "' found.", tokens[0]);

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

            // Create left and right token buffers
            var leftBuffer = new List<Token>();
            var rightBuffer = new List<Token>();

            var isInPars = false;
            var isMethodCall = false;
            var isIndexing = false;
            var isArrayLiteral = false;
            var isFunctionLiteral = false;
            var isChain = false;
            var isConditional = false;
            var isLambda = false;

            var CallArgsStart = 0;

            // Do logic in delegate so we can easily exit out of it when we need to
            Action loop = () =>
            {
                foreach (var op in OperatorPrecedence) {
                    foreach (var Operator in op.Operators) {
                        var i = 0;
                        var canLoop = tokens.Count > 0;

                        while (canLoop) {
                            //var s = SkipChain(tokens, i);

                            //if (s.Delta > 0) {
                            //    i = s.End - 1;
                            //    if (s.Start == 0 && i == tokens.Count - 1 && s.Delta != 0) {
                            //        isChain = true;
                            //        return;
                            //    }
                            //}

                            var token = tokens[i];

                            if (token.Type == TokenTypes.Punctuator) {
                                switch (token.Value) {
                                    case ",":
                                        _engine.ThrowError("Syntax error, unexpected token '" + token.Value + "' found.", token);
                                        break;
                                }
                            }

                            if (GeneralParser.NotPermittedInExpression.Contains(token.Value))
                                _engine.ThrowError("Unexpected keyword '" + token.Value + "' found.", token);

                            var previousToken = i >= 1 ? tokens[i - 1] : null;

                            if (token.Equals("{", TokenTypes.Punctuator) && !previousToken.Equals("=>", TokenTypes.Punctuator)) {
                                _engine.ThrowError("Statement expected.", token);
                            }

                            if (token.Equals("fn", TokenTypes.Keyword)) {
                                var skip = _engine.ExpectValue("(", tokens, i);
                                i += skip.Delta;

                                var start = i;
                                skip = _engine.ExpressionParser.SkipFromTo("(", ")", tokens, i);
                                i += skip.Delta;

                                _engine.ExpectValue("{", tokens, i);

                                skip = _engine.ExpressionParser.SkipFromTo("{", "}", tokens, i);
                                i += skip.Delta;

                                if (start == 1 && skip.End == tokens.Count - 1) {
                                    isFunctionLiteral = true;
                                    return;
                                }
                            }

                            if (Operator.Operation == "=") {
                                if (token.Equals("{", TokenTypes.Punctuator)) {
                                    var skip = _engine.ExpressionParser.SkipFromTo("{", "}", tokens, i);
                                    i += skip.Delta;
                                }
                            }

                            if (token.Equals("(", TokenTypes.Punctuator)) {
                                var skip = _engine.ExpressionParser.SkipFromTo("(", ")", tokens, i);
                                i += skip.Delta;

                                if (skip.Start == 0 && skip.End == tokens.Count - 1) {
                                    isInPars = true;
                                    return;
                                } else if (skip.End == tokens.Count - 1 && skip.Start > 0 && Operator.Operation == "(") {
                                    var before = tokens[skip.Start - 1];
                                    isMethodCall = true;

                                    foreach (var _op in OperatorPrecedence) {
                                        foreach (var _Operator in _op.Operators) {
                                            if (before.Value == _Operator.Operation && before.Type == TokenTypes.Punctuator) {
                                                isMethodCall = false;
                                            }
                                        }
                                    }

                                    if (isMethodCall) {
                                        CallArgsStart = skip.Start;
                                        return;
                                    }
                                }

                                token = tokens[i];
                            }
                            else if (token.Equals("[", TokenTypes.Punctuator)) {
                                var skip = _engine.ExpressionParser.SkipFromTo("[", "]", tokens, i);
                                i += skip.Delta;

                                if (skip.Start == 0 && skip.End == tokens.Count - 1) {
                                    isArrayLiteral = true;
                                    return;
                                }
                                else if (skip.End == tokens.Count - 1 && skip.Start > 0 && Operator.Operation == "[") {
                                    var before = tokens[skip.Start - 1];
                                    isIndexing = true;

                                    foreach (var _op in OperatorPrecedence) {
                                        foreach (var _Operator in _op.Operators) {
                                            if (before.Value == _Operator.Operation && before.Type == TokenTypes.Punctuator) {
                                                isIndexing = false;
                                            }
                                        }
                                    }

                                    if (isIndexing) {
                                        CallArgsStart = skip.Start;
                                        return;
                                    }
                                }
                            }
                            else if (token.Value == Operator.Operation && token.Type == TokenTypes.Punctuator) {
                                if (token.Equals(":", TokenTypes.Punctuator)) {
                                    _engine.ThrowError("Incomplete conditional statement.", token);
                                }
                                else if (token.Equals("?", TokenTypes.Punctuator)) {
                                    if (IsConditional(tokens)) {
                                        isConditional = true;
                                        return;
                                    }
                                    else {
                                        _engine.ThrowError("Incomplete conditional statement.", token);
                                    }
                                }

                                // Fill left and right buffers
                                leftBuffer = tokens.GetRange(0, i);
                                rightBuffer = tokens.GetRange(i + 1, tokens.Count - i - 1);

                                var hasRequiredLeftTokens = leftBuffer.Count > 0;
                                var hasRequiredRightTokens = rightBuffer.Count > 0;

                                if (op.Members == 1) {
                                    if (op.IsPostfix)
                                        hasRequiredRightTokens = true;
                                    else
                                        hasRequiredLeftTokens = true;
                                }

                                if (hasRequiredLeftTokens && hasRequiredRightTokens) {
                                    if (token.Equals("=>", TokenTypes.Punctuator)) {
                                        isLambda = true;
                                        return;
                                    }

                                    // Create operation node with type and body
                                    var newNode = new Node {
                                        Body = Operator.OperationName,
                                        Type = token.Type,
                                        Token = token
                                    };

                                    if (op.Members == 1) {
                                        // Parse unary and do postfix logic

                                        var leftNode = !op.IsPostfix ? null : ParseExpression(newNode, leftBuffer);
                                        newNode.Add(leftNode);

                                        var rightNode = op.IsPostfix ? null : ParseExpression(newNode, rightBuffer);
                                        newNode.Add(rightNode);

                                        if (op.IsPostfix) {
                                            if (leftBuffer.Count == 0) {
                                                _engine.ThrowError($"Syntax error, value expected after {token.Value} operator", token);
                                            }
                                        } else {
                                            if (rightBuffer.Count == 0) {
                                                _engine.ThrowError($"Syntax error, value expected before {token.Value} operator", token);
                                            }
                                        }
                                    }
                                    else {
                                        // Parse operators that need 2 sides

                                        var leftNode = ParseExpression(newNode, leftBuffer);
                                        newNode.Add(leftNode);

                                        var rightNode = ParseExpression(newNode, rightBuffer);
                                        newNode.Add(rightNode);

                                        if (leftBuffer.Count == 0) {
                                            _engine.ThrowError($"Syntax error, value expected before {token.Value} operator", token);
                                        }
                                        if (rightBuffer.Count == 0) {
                                            _engine.ThrowError($"Syntax error, value expected after {token.Value} operator", token);
                                        }
                                    }

                                    branch.Add(newNode);
                                    return;
                                }

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

                            // Check if we're still in bounds
                            canLoop = op.LeftAssociate ? i < tokens.Count - 1 : tokens.Count - 1 - i > 0;
                            i++;
                        }
                    }
                }
            };
            loop();

            if (isChain) return ParseChain(tokens);

            // Parse expression within parenthesis if it's completely surrounded
            if (isInPars) {
                if (tokens.Count == 2) {
                    _engine.ThrowError("Syntax error, expression expected.", tokens[0]);
                }

                return ParseExpression(branch, tokens.GetRange(1, tokens.Count - 2));
            }

            // Parse method call
            if (isMethodCall)
            {
                var result = ParseCall(tokens,CallArgsStart);
                return result.Node;
            }

            // Parse indexing
            if (isIndexing)
            {
                var result = ParseIndexing(tokens, CallArgsStart);
                return result.Node;
            }

            // Parse indexing
            if (isArrayLiteral)
            {
                var result = ParseArrayLiteral(tokens);
                return result.Node;
            }

            // Parse function literal
            if (isFunctionLiteral)
            {
                var result = _engine.MethodParser.ParseFunctionLiteral(tokens.GetRange(1, tokens.Count - 1));
                return result.Node;
            }

            if (isLambda) {
                var result = _engine.MethodParser.ParseLambda(tokens);
                return result.Node;
            }

            if (isConditional) {
                var result = ParseConditional(tokens);
                return result.Node;
            }

            return null;
        }

        /// <summary>
        ///     Sets parses individual arguments as expressions
        /// </summary>
        public void SetArguments(List<List<Token>> arguments, List<Token> tokens)
        {
            var depth = 0;
            var indexDepth = 0;
            var bracketDepth = 0;
            var i = 0;
            var buffer = new List<Token>();
            var isFirst = true;
      

            for (i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                buffer.Add(token);

                if (token.Equals("(", TokenTypes.Punctuator))
                    depth++;
                else if (token.Equals(")", TokenTypes.Punctuator)) depth--;

                if (token.Equals("[", TokenTypes.Punctuator))
                    indexDepth++;
                else if (token.Equals("]", TokenTypes.Punctuator)) indexDepth--;

                if (token.Equals("{", TokenTypes.Punctuator))
                    bracketDepth++;
                else if (token.Equals("}", TokenTypes.Punctuator)) bracketDepth--;

                if (depth == 0 && indexDepth == 0 && bracketDepth == 0) {
                    if (token.Equals(",", TokenTypes.Punctuator)) {
                         isFirst = false;

                        if (buffer.Count == 0) {
                            _engine.ThrowError("Syntax error, missing tokens for argument.", tokens[i]);
                        }

                        buffer.RemoveAt(buffer.Count - 1);

                        arguments.Add(new List<Token>(buffer));
                        buffer.Clear();
                    }

                    if (i == tokens.Count - 1) {
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
        ///     Skip tokens that are surrounded by 'upScope' and 'downScope'
        /// </summary>
        public SkipInfo SkipFromTo(string upScope, string downScope, List<Token> tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var depth = 0;
            var index = startingPoint;
            var end = 0;
            Token firstToken = null;

            while (true)
            {
                if (tokens[index].Value == upScope && tokens[index].Type == TokenTypes.Punctuator)
                {
                    depth++;

                    if (firstToken == null) firstToken = tokens[index];
                }
                else if (tokens[index].Value == downScope && tokens[index].Type == TokenTypes.Punctuator)
                {
                    depth--;

                    if (depth == 0)
                    {
                        end = index;
                        var delta = index - start;

                        return new SkipInfo {Start = start, End = end, Delta = delta};
                    }
                }

                index++;

                if (index == tokens.Count)
                {
                    if (depth > 0)
                        _engine.ThrowError("Syntax error, closing token '" + downScope + "' not found", firstToken);
                    else if (depth < 0) _engine.ThrowError("Syntax error, opening token '" + upScope + "' not found", tokens[index]);
                }
            }
        }

        /// <summary>
        ///     Skip tokens until we hit a token with the given value
        /// </summary>
        public void SkipUntil(ref int index, Token comparator, List<Token> tokens)
        {
            var startToken = tokens[index];

            while (!tokens[index].LazyEqual(comparator))
            {
                index++;

                if (index == tokens.Count - 1) _engine.ThrowError("Token '" + comparator + "' expected", startToken);
            }
        }
       
        public SkipInfo SkipAccess(List<Token> tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var index = startingPoint;
            var end = 0;
            var state = 0;
            var token = tokens[index];

            if (token.IsValuable()) state = 0;

            if (token.Equals(".", TokenTypes.Punctuator)) state = 1;

            while (true)
            {
                if (token.IsValuable() && state == 0)
                    state = 1;
                else if (token.Equals(".", TokenTypes.Punctuator) && state == 1)
                    state = 0;
                else
                    break;

                index++;

                if (index == tokens.Count)
                    break;

                token = tokens[index];
            }

            if (index > 0)
                if (tokens[index - 1].Equals(".", TokenTypes.Punctuator))
                    _engine.ThrowError("Identifier expected!", tokens[index - 1]);

            end = index;
            var delta = index - start;

            return new SkipInfo {Start = start, End = end, Delta = delta};
        }

        public SkipInfo SkipChain(List<Token> tokens, int startingPoint)
        {
            var start = startingPoint;
            var index = startingPoint;
            var end = 0;

            var token = tokens[index];

            if (!token.IsValuable()) return new SkipInfo {Start = start, End = end, Delta = 0};

            while (true)
            {
                if (token.Equals("(", TokenTypes.Punctuator))
                {
                    var skip = _engine.ExpressionParser.SkipFromTo("(", ")", tokens, index);
                    index = skip.End + 1;
                }
                else if (token.Equals("[", TokenTypes.Punctuator))
                {
                    var skip = _engine.ExpressionParser.SkipFromTo("[", "]", tokens, index);
                    index = skip.End + 1;
                }
                else if (token.Equals(".", TokenTypes.Punctuator) || token.IsValuable())
                {
                    var skip = _engine.ExpressionParser.SkipAccess(tokens, index);
                    index = skip.End;
                }
                else
                {
                    break;
                }

                if (index == tokens.Count)
                    break;

                token = tokens[index];
            }

            end = index;
            var delta = index - start;

            return new SkipInfo {Start = start, End = end, Delta = delta};
        }

        public Node ParseChain(List<Token> tokens)
        {
            var node = new Node();

            if (tokens.Count == 2) _engine.ThrowError("Access operator can only be used after a value!", tokens[0]);

            if (tokens.Count == 1) return ParseExpression(node, tokens);

            var reverse = tokens.GetRange(0, tokens.Count);
            reverse.Reverse();

            if (reverse[0].Equals("]", TokenTypes.Punctuator))
            {
                var skip = SkipFromTo("]", "[", reverse, 0);

                if (skip.End + 1 >= reverse.Count)
                    _engine.ThrowError("Indexing operator needs left hand value!", reverse[skip.End]);
                else if (reverse[skip.End + 1].Value == "." && reverse[skip.End + 1].Type == TokenTypes.Punctuator)
                    _engine.ThrowError("Indexing operator needs left hand value!", reverse[skip.End]);

                var getterNode = new Node();
                getterNode.Add(ParseChain(tokens.GetRange(0, tokens.Count - (skip.End + 1))));
                getterNode.Body = "Getter";
                getterNode.Type = TokenTypes.Getter;
                node.Add(getterNode);

                var argumentTokens = reverse.GetRange(0, skip.End + 1);
                argumentTokens.Reverse();

                var result = _engine.GeneralParser.ParseSurroundedExpressions("[", "]", 0, argumentTokens);
                var argumentsNode = result.Node;
                argumentsNode.Body = "Arguments";
                argumentsNode.Type = TokenTypes.Arguments;
                node.Add(argumentsNode);

                node.Body = "Index";
                node.Type = TokenTypes.Index;
            }
            else if (reverse[0].Value == ")")
            {
                var skip = SkipFromTo(")", "(", reverse, 0);

                if (skip.End + 1 >= reverse.Count)
                    _engine.ThrowError("Call operator needs left hand value!", reverse[skip.End]);
                else if (reverse[skip.End + 1].Value == ".")
                    _engine.ThrowError("Call operator needs left hand value!", reverse[skip.End]);

                var getterNode = new Node();
                getterNode.Add(ParseChain(tokens.GetRange(0, tokens.Count - (skip.End + 1))));
                getterNode.Body = "Getter";
                getterNode.Type = TokenTypes.Getter;
                node.Add(getterNode);

                var argumentTokens = reverse.GetRange(0, skip.End + 1);
                argumentTokens.Reverse();

                var result = _engine.GeneralParser.ParseSurroundedExpressions("(", ")", 0, argumentTokens);
                var argumentsNode = result.Node;
                argumentsNode.Body = "Arguments";
                argumentsNode.Type = TokenTypes.Arguments;
                node.Add(argumentsNode);

                node.Body = "Call";
                node.Type = TokenTypes.Call;
                node.Token = tokens[0];
            }
            else
            {
                node.Body = "access";
                node.Type = TokenTypes.Punctuator;

                node.Add(ParseChain(tokens.GetRange(0, tokens.Count - 2)));
                node.Add(ParseExpression(node, new List<Token> {reverse[0]}));
            }

            return node;
        }

        /// <summary>
        ///     Parses a method call with arguments
        /// </summary>
        public ParseResult ParseCall(List<Token> tokens, int argsStart)
        {
            var name = tokens[0].Value;
            var node = new CallNode();

            var accessTokens = tokens.GetRange(0, argsStart);

            node.Getter = ParseClean(accessTokens);

            var result = _engine.GeneralParser.ParseSurroundedExpressions("(", ")", argsStart, tokens);
            node.Arguments = new List<Node>(result.Node.Nodes);

            return new ParseResult {Node = node, Delta = tokens.Count - 1 };
        }

        /// <summary>
        ///     Parses an index operation
        /// </summary>
        public ParseResult ParseIndexing(List<Token> tokens, int argsStart)
        {
            var name = tokens[0].Value;
            var node = new IndexNode();

            var accessTokens = tokens.GetRange(0, argsStart);

            node.Getter = ParseClean(accessTokens);
            var result = _engine.GeneralParser.ParseSurroundedExpressions("[", "]", argsStart, tokens);

            if (result.Node.Nodes.Count == 0)
                _engine.ThrowError("Syntax error, index operator arguments can't be empty.", tokens[argsStart + 1]);

            node.Arguments = new List<Node>(result.Node.Nodes);

            return new ParseResult { Node = node, Delta = tokens.Count - 1 };
        }

        /// <summary>
        ///     Parses an array literal
        /// </summary>
        public ParseResult ParseArrayLiteral(List<Token> tokens) {
            var index = 0;

            var result = _engine.GeneralParser.ParseSurroundedExpressions("[", "]", 0, tokens);

            var node = new ArrayNode {
                Values = new List<Node>(result.Node.Nodes)
            };

            index += result.Delta;

            return new ParseResult {Node = node, Delta = index};
        }

        public ParseResult ParseUsing(List<Token> tokens) {
            var node = new UsingNode();

            int i = 0;
            bool isIdentifier = true;
            var buffer = new List<Token>();
            Token nextToken = null;
            var skip = _engine.ExpectType(TokenTypes.Identifier, tokens);
            i++;

            while (i < tokens.Count) {

                var token = tokens[i];

                if (i < tokens.Count-2) nextToken = tokens[i+1];

                if (token.Type == TokenTypes.EndOfExpression) break;

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

        public ParseResult ParseConditional(List<Token> tokens) {
            var node = new ConditionalNode();
            int depth = 0;

            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];

                if (token.Equals("(", TokenTypes.Punctuator)) {
                    depth++;
                }
                else if (token.Equals(")", TokenTypes.Punctuator)) {
                    depth--;
                }

                if (token.Equals("?", TokenTypes.Punctuator) && depth == 0) {
                    var conditionNode = ParseClean(tokens.GetRange(0, i));

                    SkipInfo skip = SkipFromTo("?", ":", tokens, i);

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
        public Node ParseClean(List<Token> tokens)
        {
            var node = new Node();
            node.Add(ParseExpression(node, tokens));

            // Only return the first subnode, so we don't create a messy AST
            Node returnNode = null;
            if (node.Nodes.Count > 0) returnNode = node.Nodes[0];

            return returnNode;
        }

        /// <summary>
        ///     Parses a list of tokens into an expression node
        /// </summary>
        public ParseResult Parse(List<Token> tokens)
        {
            var node = new Node();
            var delta = 0;
            var pScope = 0;
            var bScope = 0;
            var cScope = 0;
            var addDelta = 0;
            var deltaOffset = 0;

            Token previousToken = null;

            if (tokens[0].Type == TokenTypes.EndOfExpression) {
                delta++;
                deltaOffset = 1;
            }

            // Skip until we hit the end of an expression
            while (true)
            {
                if (tokens[delta].Type == TokenTypes.Punctuator)
                    switch (tokens[delta].Value)
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

                if (pScope == 0 && bScope == 0 && cScope == 0) {
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