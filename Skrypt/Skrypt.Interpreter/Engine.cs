using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Interpreter.Compiler;

namespace Skrypt.Interpreter {
    class Engine {
        public Options Options { get; set; } = new Options();

        List<Instruction> Instructions;
        string _code { get; set; }

        public double CompileTime;

        public void Run (string code = "") {
            if (!string.IsNullOrEmpty(code)) {
                _code = code;
            }

            var parseEngine = new SkryptEngine(_code);
            parseEngine.Settings = EngineSettings.NoLogs;
            var programNode = parseEngine.Parse();

            if (Options.CanWrite) programNode.Print();             

            var sw = Stopwatch.StartNew();

            Instructions = InstructionCompiler.Compile(programNode);

            sw.Stop();

            CompileTime = sw.Elapsed.TotalMilliseconds;
        }

        public void PrintInstructions () {
            Console.WriteLine("Instructions: ");

            for (int i = 0; i < Instructions.Count; i++) {
                var index = i.ToString("X4");

                Console.WriteLine(index + ": " + Instructions[i]);
            }
        }
    }
}
