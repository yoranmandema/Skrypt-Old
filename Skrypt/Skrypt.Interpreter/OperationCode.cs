using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Interpreter {
    enum OperationCode {
        brequal,
        brfalse,

        add,
        assign,

        stloc,
        ldloc,

        ldnum,
        ldstr,
        ldbool
    }
}
