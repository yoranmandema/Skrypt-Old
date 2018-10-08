using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Skrypt.Analysis;
using Skrypt.Execution;
using Skrypt.Library;
using Skrypt.Library.Reflection;
using Skrypt.Parsing;
using Skrypt.Tokenization;

namespace Skrypt.Engine
{
    public class ParseResult {
        public int Delta = -1;
        public Node Node;
    }

    public enum EngineSettings : byte {
        NoLogs = 1,
    }

    public class SkryptEngine {
        public Analizer Analizer;
        public ClassParser ClassParser;
        public Executor Executor;
        public ExpressionParser ExpressionParser;
        public GeneralParser GeneralParser;
        public ScopeContext GlobalScope = new ScopeContext();
        public List<Node> MethodNodes = new List<Node>();
        public FunctionParser MethodParser;
        public ModifierChecker ModifierChecker;
        public List<SkryptMethod> Methods = new List<SkryptMethod>();
        public StatementParser StatementParser;
        public Tokenizer Tokenizer;
        public TokenProcessor TokenProcessor;

        public EngineSettings Settings;

        internal Stopwatch Stopwatch { get; set; }
        internal CallStack CurrentStack { get; set; }
        internal ScopeContext CurrentScope { get; set; }
        internal Node CurrentNode { get; set; }

        private List<Token> _tokens;
        private string _code = "";

        public SkryptEngine(string code = "") {
            _code = code;

            Tokenizer = new Tokenizer(this);
            TokenProcessor = new TokenProcessor(this);
            StatementParser = new StatementParser(this);
            ExpressionParser = new ExpressionParser(this);
            GeneralParser = new GeneralParser(this);
            MethodParser = new FunctionParser(this);
            ModifierChecker = new ModifierChecker(this);
            ClassParser = new ClassParser(this);
            Analizer = new Analizer(this);
            Executor = new Executor(this);

            var systemObject = ObjectGenerator.MakeObjectFromClass(typeof(Library.Native.System), this);

            GlobalScope.SetVariable(systemObject.Name, systemObject, Modifier.Const);

            // Tokens that are found using a token rule with type defined as 'null' won't get added to the token list.
            // This means you can ignore certain characters, like whitespace in this case, that way.
            Tokenizer.AddRule(
                new Regex(@"\s"),
                TokenTypes.None
            );

            Tokenizer.AddRule(
                new Regex(@"\d+(\.\d+)?([eE][-+]?\d+)?"),
                TokenTypes.NumericLiteral
            );

            Tokenizer.AddRule(
                new Regex(@"0x([A-Fa-f\d])*"),
                TokenTypes.HexadecimalLiteral
            );

            Tokenizer.AddRule(
                new Regex(@"0b([01])*"),
                TokenTypes.BinaryLiteral
            );

            Tokenizer.AddRule(
                new Regex(@"[_a-zA-Z]+[_a-zA-Z0-9]*"),
                TokenTypes.Identifier
            );

            Tokenizer.AddRule(
                new Regex(@"const|using|public|private|strong|in|class|fn|if|elseif|else|while"),
                TokenTypes.Keyword
            );

            Tokenizer.AddRule(
                new Regex("true|false"),
                TokenTypes.BooleanLiteral
            );

            Tokenizer.AddRule(
                new Regex("null"),
                TokenTypes.NullLiteral
            );

            Tokenizer.AddRule(
                new Regex(@"[;]"),
                TokenTypes.EndOfExpression
            );

            Tokenizer.AddRule(
                new Regex(
                    @"(using)|(return)|(continue)|(break)|(&&)|(\+=)|(\-=)|(\/=)|(\*=)|(\%=)|(\^=)|(\&=)|(\|=)|(\|\|\|=)|(\|\|\|)|(\|\|)|(=>)|(==)|(!=)|(>=)|(<=)|(<<)|(>>>)|(>>)|(\+\+)|(--)|[~=<>+\-*/%^&|!\[\]\(\)\.\,{}\?\:]"),
                TokenTypes.Punctuator
            );

            // Single line comment
            Tokenizer.AddRule(
                new Regex(@"\/\/.*\n?"),
                TokenTypes.None
            );

            // Multi line comment
            Tokenizer.AddRule(
                new Regex(@"\/\*.*?\*\/", RegexOptions.Singleline),
                TokenTypes.None
            );

            Tokenizer.AddRule(
                new Regex(@""".*?(?<!\\)""", RegexOptions.Singleline),
                TokenTypes.StringLiteral
            );
        }

        /// <summary>
        ///     Adds a type as a variable to the engine's global scope.
        /// </summary>
        public void AddType(Type type) {
            var generated = ObjectGenerator.MakeObjectFromClass(type, this);
            GlobalScope.SetVariable(generated.Name, generated, Modifier.Const);
        }

        /// <summary>
        ///     Creates a new object of type T, and gives it all properties from its parent class.
        /// </summary>
        public T Create<T>(params object[] input) {
            var newObject = (SkryptType)Activator.CreateInstance(typeof(T), input);
            var baseType = Executor.GetType(newObject.TypeName, CurrentScope);
            newObject.ScopeContext = CurrentScope;
            newObject.Engine = this;
            newObject.GetPropertiesFrom(baseType);

            return (T)((Object)newObject);
        }

        /// <summary>
        ///     Evaluates a binary operation.
        /// </summary>
        public SkryptObject Eval(Operator operation, SkryptObject leftObject, SkryptObject rightObject, Node node = null) {
            dynamic left = Convert.ChangeType(leftObject, leftObject.GetType());
            dynamic right = Convert.ChangeType(rightObject, rightObject.GetType());

            Operation opLeft = SkryptObject.GetOperation(
                operation.Type,
                leftObject.GetType(),
                rightObject.GetType(),
                left.Operations
                );

            Operation opRight = SkryptObject.GetOperation(
                operation.Type,
                leftObject.GetType(),
                rightObject.GetType(),
                right.Operations
                );

            OperationDelegate operationDel = null;

            if (opLeft != null) {
                operationDel = opLeft.OperationDelegate;
            }

            if (opRight != null) {
                operationDel = opRight.OperationDelegate;
            }

            if (operationDel == null) {
                ThrowError("No such operation as " + left.Name + " " + operation.Operation + " " + right.Name,node.Token);
            }

            var result = (SkryptType)operationDel(new[] { leftObject, rightObject }, this);

            result.GetPropertiesFrom(Executor.GetType(result.TypeName, GlobalScope));

            return result;
        }

        /// <summary>
        ///     Evaluates a unary operation.
        /// </summary>
        public SkryptObject Eval(Operator operation, SkryptObject leftObject, Node node = null) {
            dynamic left = Convert.ChangeType(leftObject, leftObject.GetType());

            Operation opLeft = SkryptObject.GetOperation(operation.Type, leftObject.GetType(), null, left.Operations);

            OperationDelegate operationDel = null;

            if (opLeft != null) {
                operationDel = opLeft.OperationDelegate;
            }
            else {
                ThrowError("No such operation as " + left.Name + " " + operation.Operation,node?.Nodes[0].Token);
            }

            var result = (SkryptType)operationDel(new[] { leftObject }, this);

            result.GetPropertiesFrom(Executor.GetType(result.TypeName, GlobalScope));

            return result;
        }

        /// <summary>
        ///     Calculates the line and column of a given index.
        /// </summary>
        public string GetLineAndRowStringFromIndex(int index) {
            var lines = 1;
            var row = 1;
            var i = 0;

            while (i < index) {
                if (_code[i] == '\n') {
                    lines++;
                    row = 1;
                }
                else {
                    row++;
                }

                i++;
            }

            return "line: " + lines + ", col: " + row;
        }

        /// <summary>
        ///     Skips token if next token has the given value. Throws exception when not found.
        /// </summary>
        public SkipInfo ExpectValue(string value, List<Token> tokens, int startingPoint = 0) {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Syntax error, token '" + value + "' expected.";

            if (index == tokens.Count - 1) ThrowError(msg, tokens[index]);

            if (tokens[index + 1].Value == value)
                index++;
            else
                ThrowError(msg, tokens[index]);

            var delta = index - startingPoint;

            return new SkipInfo { Start = start, End = index, Delta = delta };
        }

        /// <summary>
        ///     Skips token if next token has one of the given values. Throws exception when nothing is found.
        /// </summary>
        public SkipInfo ExpectValue(string[] values, List<Token> tokens, int startingPoint = 0) {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Syntax error, tokens '" + string.Join(",", values) + "' expected.";

            // index is at the end of the token list, meaning the required token doesn't exist.
            if (index == tokens.Count - 1) {
                ThrowError(msg, tokens[index]);
            }

            if (Array.Exists(values, e => e == tokens[index + 1].Value)) {
                index++;
            }
            else {
                ThrowError(msg, tokens[index]);
            }

            var delta = index - startingPoint;

            return new SkipInfo { Start = start, End = index, Delta = delta };
        }

        /// <summary>
        ///     Skips token if next token has the given value. Throws exception when not found.
        /// </summary>
        public SkipInfo ExpectType(TokenTypes type, List<Token> tokens, int startingPoint = 0) {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Syntax error, tokens with type '" + type + "' expected.";

            // index is at the end of the token list, meaning the required token doesn't exist.
            if (index == tokens.Count - 1) {
                ThrowError(msg, tokens[index]);
            }

            if (tokens[index + 1].Type == type) {
                index++;
            }
            else {
                ThrowError(msg, tokens[index]);
            }

            var delta = index - startingPoint;

            return new SkipInfo { Start = start, End = index, Delta = delta };
        }

        /// <summary>
        ///     Throws an error with line and colom indicator
        /// </summary>
        public void ThrowError(string message, Token token = null) {
            var lineRow = token != null ? " (" + GetLineAndRowStringFromIndex(token.Start) + ")" : "";

            Console.WriteLine();
            Console.WriteLine(message);
            if (token != null) Console.WriteLine($"\n\t(line: {token.Line}, column: {token.Colom})\n");
            Console.WriteLine(CurrentStack);

            throw new SkryptException(message + lineRow, token);
        }

        /// <summary>
        ///     Set the value of a variable within the engine's global scope to a function.
        /// </summary
        public SkryptEngine SetValue(string name, Delegate value) {
            return SetValue(name, new SharpMethod(value));
        }

        /// <summary>
        ///     Set the value of a variable within the engine's global scope to a new value.
        /// </summary
        public SkryptEngine SetValue(string name, SkryptObject value) {
            GlobalScope.SetVariable(name, value);
            return this;
        }

        /// <summary>
        ///     Parses a string of code into a program node.
        /// </summary>
        public Node Parse(string code = "") {
            if (code != string.Empty)
                _code = code;

            Stopwatch = Stopwatch.StartNew();

            // Tokenize code
            _tokens = Tokenizer.Tokenize(_code);
            if (_tokens == null) return null;

            // Pre-process tokens so their values are correct
            TokenProcessor.ProcessTokens(_tokens);

            Stopwatch.Stop();
            double token = Stopwatch.ElapsedMilliseconds;

            //foreach (var t in _tokens) Console.WriteLine(t);

            // Generate the program node
            Stopwatch = Stopwatch.StartNew();
            var programNode = GeneralParser.Parse(_tokens);
            Stopwatch.Stop();
            double parse = Stopwatch.ElapsedMilliseconds;

            // Debug program node
            //Console.WriteLine("Program:\n" + programNode);
            //programNode.Print();

            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(programNode, Newtonsoft.Json.Formatting.Indented));

            Stopwatch = Stopwatch.StartNew();
            GlobalScope = Executor.ExecuteBlock(programNode, GlobalScope);
            Stopwatch.Stop();
            double execute = Stopwatch.Elapsed.TotalMilliseconds;
            ExecutionTime = execute;

            if (!Settings.HasFlag(EngineSettings.NoLogs)) 
                Console.WriteLine($"\nExecution: {execute}ms, Parsing: {parse}ms, Tokenization: {token}ms, Total: {execute + parse + token}ms");

            return programNode;
        }

        public double ExecutionTime { get; private set; }
    }
}