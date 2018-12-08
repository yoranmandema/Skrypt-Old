using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Interpreter {
    class Program {
        static void Main(string[] args) {
            var engine = new Engine();

            engine.Run(
@"
import System

print(""Hello world"")
"
                );

            engine.PrintInstructions();

            Console.WriteLine($"Compile time: {engine.CompileTime}ms");

            engine.Options = new Options {
                CanWrite = false
            };

            int iterations = 1000;
            double totalTime = 0;

            for (int i = 0; i < iterations; i++) {
                engine.Run();
                totalTime += engine.CompileTime;
            }

            Console.WriteLine($"Compile time average: {totalTime / iterations}ms");

            Console.ReadKey();
        }
    }
}
