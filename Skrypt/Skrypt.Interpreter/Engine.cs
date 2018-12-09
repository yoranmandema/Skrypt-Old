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

            //if (Options.CanWrite) programNode.Print();             

            var sw = Stopwatch.StartNew();

            Instructions = InstructionCompiler.Compile(programNode);

            sw.Stop();

            CompileTime = sw.Elapsed.TotalMilliseconds;
        }

        public static void PrintInstructions (List<Instruction> instructions) {
            //Console.WriteLine("Instructions: ");

            for (int i = 0; i < instructions.Count; i++) {
                var index = i.ToString();
                var instr = instructions[i];

                Console.WriteLine(index + ":\t" + instr.ToString());

                if (instr.OpCode == OperationCode.brfalse || instr.OpCode == OperationCode.move) {
                    Console.WriteLine();
                }
            }
        }

        public void PrintInstructions() {
            PrintInstructions(Instructions);
        }
    }
}
