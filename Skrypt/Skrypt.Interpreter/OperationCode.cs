using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Interpreter {
    enum OperationCode {
        brequal,
        brfalse,

        import,
        call,
        access,

        add,
        sub,
        mul,
        div,

        stloc,
        ldloc,
        setfn,

        ldnum,
        ldstr,
        ldbool,
        ldnull
    }
}
