using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Skrypt.Analysis;
using Skrypt.Execution;
using Skrypt.Library;
using Skrypt.Library.Native;
using Skrypt.Library.Reflection;
using Skrypt.Parsing;
using Skrypt.Tokenization;
// Using this so we can check how fast everything is happening

namespace Skrypt.Engine
{
    public class ParseResult
    {
        //public Exception error;
        public int Delta = -1;
        public Node Node;
    }

    public class SkryptEngine
    {
        public Analizer Analizer;
        public ClassParser ClassParser;
        public Executor Executor;
        public ExpressionParser ExpressionParser;
        public GeneralParser GeneralParser;
        public ScopeContext GlobalScope = new ScopeContext();
        public List<Node> MethodNodes = new List<Node>();
        public MethodParser MethodParser;
        public ModifierChecker ModifierChecker;
        public List<SkryptMethod> Methods = new List<SkryptMethod>();
        public StatementParser StatementParser;
        public Tokenizer Tokenizer;
        public TokenProcessor TokenProcessor;

        internal Stopwatch Stopwatch { get; set; }
        internal CallStack CurrentStack { get; set; }
        internal ScopeContext CurrentScope { get; set; }

        private List<Token> _tokens;
        private string _code = "";

        public SkryptEngine()
        {
            Tokenizer = new Tokenizer(this);
            TokenProcessor = new TokenProcessor(this);
            StatementParser = new StatementParser(this);
            ExpressionParser = new ExpressionParser(this);
            GeneralParser = new GeneralParser(this);
            MethodParser = new MethodParser(this);
            ModifierChecker = new ModifierChecker(this);
            ClassParser = new ClassParser(this);
            Analizer = new Analizer(this);
            Executor = new Executor(this);

            var systemObject = ObjectGenerator.MakeObjectFromClass(typeof(Library.Native.System), this);

            GlobalScope.AddVariable(systemObject.Name, systemObject, Modifier.Const);

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
                new Regex(@"const|using|public|strong|static|class|fn|if|elseif|else|while"),
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
                    @"(using)|(return)|(continue)|(break)|(&&)|(\|\|\|)|(\|\|)|(=>)|(==)|(!=)|(>=)|(<=)|(<<)|(>>>)|(>>)|(\+\+)|(--)|[~=<>+\-*/%^&|!\[\]\(\)\.\,{}\?\:]"),
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

        public Dictionary<string, SkryptObject> Types { get; set; } = new Dictionary<string, SkryptObject>();

        public void AddClass(Type type) {
            var generated = ObjectGenerator.MakeObjectFromClass(type, this);
            GlobalScope.AddVariable(generated.Name, generated, Modifier.Const);
        }

        public T Create<T>(params object[] input) {
            var newObject = (SkryptType)Activator.CreateInstance(typeof(T), input);
            var baseType = Executor.GetType(newObject.TypeName, CurrentScope);
            newObject.ScopeContext = CurrentScope;
            newObject.Engine = this;
            newObject.SetPropertiesTo(baseType);

            return (T)((Object)newObject);
        }

        public SkryptObject Eval (Operator operation, SkryptObject leftObject, SkryptObject rightObject, Node node = null) {
            dynamic left = Convert.ChangeType(leftObject, leftObject.GetType());
            dynamic right = Convert.ChangeType(rightObject, rightObject.GetType());

            Operation opLeft = left.GetOperation(
                operation.OperationName, 
                leftObject.GetType(),
                rightObject.GetType(),
                left.Operations
                );

            Operation opRight = right.GetOperation(
                operation.OperationName,
                leftObject.GetType(),
                rightObject.GetType(), 
                right.Operations
                );

            OperationDelegate operationDel = null;

            if (opLeft != null)
                operationDel = opLeft.OperationDelegate;
            else if (opRight != null)
                operationDel = opRight.OperationDelegate;

            if (operationDel == null) {
                ThrowError("No such operation as " + left.Name + " " + operation.Operation + " " + right.Name,
                    node.Token);
            }

            var result = (SkryptType)operationDel(new[] { leftObject, rightObject });

            result.SetPropertiesTo(Executor.GetType(result.TypeName, GlobalScope));

            return result;
        }

        public SkryptObject Eval(Operator operation, SkryptObject leftObject, Node node = null) {
            dynamic left = Convert.ChangeType(leftObject, leftObject.GetType());

            Operation opLeft = left.GetOperation(operation.OperationName, leftObject.GetType(), null, left.Operations);

            OperationDelegate operationDel = null;

            if (opLeft != null)
                operationDel = opLeft.OperationDelegate;
            else
                ThrowError("No such operation as " + left.Name + " " + operation.Operation,
                    node?.SubNodes[0].Token);

            var result = (SkryptType)operationDel(new[] { leftObject });

            result.SetPropertiesTo(Executor.GetType(result.TypeName, GlobalScope));

            return result;
        }

        /// <summary>
        ///     Calculates the line and column of a given index
        /// </summary>
        public string GetLineAndRowStringFromIndex(int index)
        {
            var lines = 1;
            var row = 1;
            var i = 0;

            while (i < index)
            {
                if (_code[i] == '\n')
                {
                    lines++;
                    row = 1;
                }
                else
                {
                    row++;
                }

                i++;
            }

            return "line: " + lines + ", col: " + row;
        }

        /// <summary>
        ///     Skips token if next token has the given value. Throws exception when not found.
        /// </summary>
        public SkipInfo ExpectValue(string value, List<Token> tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Syntax error, token '" + value + "' expected.";

            if (index == tokens.Count - 1) ThrowError(msg, tokens[index]);

            if (tokens[index + 1].Value == value)
                index++;
            else
                ThrowError(msg, tokens[index]);

            var delta = index - startingPoint;

            return new SkipInfo {Start = start, End = index, Delta = delta};
        }

        public SkipInfo ExpectValue(string[] values, List<Token> tokens, int startingPoint = 0) {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Syntax error, tokens '" + string.Join(",",values) + "' expected.";

            if (index == tokens.Count - 1) ThrowError(msg, tokens[index]);

            if (Array.Exists(values,e => e == tokens[index + 1].Value))
                index++;
            else
                ThrowError(msg, tokens[index]);

            var delta = index - startingPoint;

            return new SkipInfo { Start = start, End = index, Delta = delta };
        }

        /// <summary>
        ///     Skips token if next token has the given value. Throws exception when not found.
        /// </summary>
        public SkipInfo ExpectType(TokenTypes type, List<Token> tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Syntax error, tokens with type '" + type + "' expected.";

            if (index == tokens.Count - 1) ThrowError(msg, tokens[index]);

            if (tokens[index + 1].Type == type)
                index++;
            else
                ThrowError(msg, tokens[index]);

            var delta = index - startingPoint;

            return new SkipInfo {Start = start, End = index, Delta = delta};
        }

        /// <summary>
        ///     Throws an error with line and colom indicator
        /// </summary>
        public void ThrowError(string message, Token token = null)
        {
            var lineRow = token != null ? " (" + GetLineAndRowStringFromIndex(token.Start) + ")" : "";

            Console.WriteLine();
            Console.WriteLine(message);
            if (token != null) Console.WriteLine($"\n\t(line: {token.Line}, column: {token.Colom})\n");
            Console.WriteLine(CurrentStack);

            throw new SkryptException(message + lineRow, token);
        }

        public Node Parse(string code)
        {
            _code = code;

            Stopwatch = Stopwatch.StartNew();

            // Tokenize code
            _tokens = Tokenizer.Tokenize(code);
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

            //ScopeContext AnalizeScope = new ScopeContext();
            //analizer.Analize(ProgramNode, AnalizeScope);

            Stopwatch = Stopwatch.StartNew();
            GlobalScope = Executor.ExecuteBlock(programNode, GlobalScope);
            Stopwatch.Stop();
            double execute = Stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"\nExecution: {execute}ms, Parsing: {parse}ms, Tokenization: {token}ms, Total: {execute + parse + token}ms");

            int instances = 0;
            Stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < instances; i++) {
                GlobalScope = Executor.ExecuteBlock(programNode, GlobalScope);
            }
            Stopwatch.Stop();

            if (instances > 0) {
                Console.WriteLine($"Average ({instances} instances): {Stopwatch.Elapsed.TotalMilliseconds / instances}ms");
                Console.WriteLine($"Per second: {1000 / (Stopwatch.Elapsed.TotalMilliseconds / instances)}");
            }

            return programNode;
        }
    }
}