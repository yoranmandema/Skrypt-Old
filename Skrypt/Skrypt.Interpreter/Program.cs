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
a = 1
b = 123
c = b + a
"
                );

            Console.ReadKey();
        }
    }
}
