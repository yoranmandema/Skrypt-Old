using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using System.Text.RegularExpressions;
using Skrypt.Parsing;
using Skrypt.Analysis;
using Skrypt.Execution;
using Skrypt.Library;
using System.Diagnostics; // Using this so we can check how fast everything is happening
using Skrypt.Library.Methods;

namespace Skrypt.Engine {
    public class ParseResult {
        public Node node;
        //public Exception error;
        public int delta = -1;
    }

    public class SkryptEngine {
        public Tokenizer tokenizer;
        public StatementParser statementParser;
        public ExpressionParser expressionParser;
        public GeneralParser generalParser;
        public MethodParser methodParser;
        public Analizer analizer;
        public Executor executor;
        public StandardMethods standardMethods;

        List<Token> Tokens;
        string Code = "";
        public List<Node> MethodNodes = new List<Node>();
        public List<SkryptMethod> Methods = new List<SkryptMethod>();
        //List<SkryptClass> Classes = new List<SkryptClass>();

        public SkryptEngine() {
            tokenizer = new Tokenizer(this);
            statementParser = new StatementParser(this);
            expressionParser = new ExpressionParser(this);
            generalParser = new GeneralParser(this);
            methodParser = new MethodParser(this);
            analizer = new Analizer(this);
            executor = new Executor(this);
            standardMethods = new StandardMethods(this);

            standardMethods.AddMethodsToEngine();

            // Tokens that are found using a token rule with type defined as 'null' won't get added to the token list.
            // This means you can ignore certain characters, like whitespace in this case, that way.
            tokenizer.AddRule(
                new Regex(@"\s"),
                null
            );

            tokenizer.AddRule(
                new Regex(@"\d+(\.\d+)?"),
                "NumericLiteral"
            );

            tokenizer.AddRule(
                new Regex(@"[_a-zA-Z]+[_a-zA-Z0-9]*"),
                "Identifier"
            );

            tokenizer.AddRule(
                new Regex(@"class|method|if|elseif|else|while"),
                "Keyword"
            );

            tokenizer.AddRule(
                new Regex("true|false"),
                "BooleanLiteral"
            );

            tokenizer.AddRule(
                new Regex(@"(return)|(&&)|(\|\|)|(\|\|\|)|(==)|(!=)|(>=)|(<=)|(<<)|(>>)|(>>>)|(\+\+)|(--)|[~=;:<>+\-*/%^&|!\[\]\(\)\.\,{}]"),
                "Punctuator"
            );

            tokenizer.AddRule(
                new Regex(@""".*?(?<!\\)"""),
                "StringLiteral"
            );

            // Multi line comment
            tokenizer.AddRule(
                new Regex(@"\/\*(.|\n)*\*\/"),
                null
            );

            // Single line comment
            tokenizer.AddRule(
                new Regex(@"\/\/.*\n"),
                null
            );
        }

        /// <summary>
        /// Calculates the line and column of a given index
        /// </summary>
        public string getLineAndRowStringFromIndex(int index) {
            int lines = 0;
            int row = 0;
            int i = 0;

            while (i < index) {
                if (Code[i] == '\n') {
                    lines++;
                    row = 0;
                } else {
                    row++;
                }

                i++;
            }

            return "line: " + lines + ", col: " + row;
        }

        /// <summary>
        /// Skips token if next token has the given value. Throws exception when not found.
        /// </summary>
        public skipInfo expectValue(string Value, List<Token> Tokens, int startingPoint = 0) {
            int start = startingPoint;
            int index = startingPoint;
            string msg = "Token '" + Value + "' expected after " + Tokens[index].Value + " keyword";

            if (index == Tokens.Count - 1) {
                throwError(msg, Tokens[index]);
            }

            if (Tokens[index + 1].Value == Value) {
                index++;
            } else {
                throwError(msg, Tokens[index]);
            }

            int delta = index - startingPoint;

            return new skipInfo {start=start, end=index, delta=delta};
        }

        /// <summary>
        /// Skips token if next token has the given value. Throws exception when not found.
        /// </summary>
        public skipInfo expectType(string Type, List<Token> Tokens, int startingPoint = 0) {
            int start = startingPoint;
            int index = startingPoint;
            string msg = "Token with type '" + Type + "' expected after " + Tokens[index].Value + " keyword";

            if (index == Tokens.Count - 1) {
                throwError(msg, Tokens[index]);
            }

            if (Tokens[index + 1].Type == Type) {
                index++;
            }
            else {
                throwError(msg, Tokens[index]);
            }

            int delta = index - startingPoint;

            return new skipInfo { start = start, end = index, delta = delta };
        }

        /// <summary>
        /// Throws an error with line and colom indicator
        /// </summary>
        public void throwError (string message, Token token = null) {
            string lineRow = token != null ? getLineAndRowStringFromIndex(token.Start) : "";

            throw new Exception(message + " (" + lineRow + ")");
        }

        public Node Parse (string code) {
            Code = code;

            // Tokenize code
            Tokens = tokenizer.Tokenize(code);      
            if (Tokens == null) { return null; }

            // Pre-process tokens so their values are correct
            TokenProcessor.ProcessTokens(Tokens);

            // Generate the program node
            Node ProgramNode = generalParser.Parse(Tokens);

            // Debug program node
            Console.WriteLine("Program:\n" + ProgramNode);

            //ScopeContext AnalizeScope = new ScopeContext();
            //analizer.Analize(ProgramNode, AnalizeScope);

            ScopeContext Scope = new ScopeContext();
            executor.ExecuteBlock(ProgramNode, Scope);

            return ProgramNode;
        }
    }
}
