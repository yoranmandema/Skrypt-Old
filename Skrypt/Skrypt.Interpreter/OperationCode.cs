using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Interpreter {
    enum OperationCode {
        br_equal,
        br_false,

        add,
        assign,

        stloc,
        number
    }
}
