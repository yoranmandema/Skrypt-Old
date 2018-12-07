using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Interpreter {
    struct Instruction {
        public OperationCode OpCode { get; set; }
        public object Value { get; set; }

        public override string ToString() {
            var str = OpCode.ToString();

            if (Value != null) {
                str += "\t" + Value.ToString();
            }

            return str;
        }
    }
}
