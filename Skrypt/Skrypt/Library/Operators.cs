using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library {
    class Operator {
        public int Members { get; set; } = 2;
        public string OperationName { get; set; } = "";
        public string Operation = "";
        static public List<Operator> AllOperators = new List<Operator>();
    }

    class Op_Access : Operator {
        public Op_Access() {
            OperationName = "access";
            Operation = ".";

            AllOperators.Add(this);
        }
    }

    class Op_Assign : Operator {
        public Op_Assign() {
            OperationName = "assign";
            Operation = "=";

            AllOperators.Add(this);
        }
    }

    class Op_Or : Operator {
        public Op_Or() {
            OperationName = "Or";
            Operation = "||";

            AllOperators.Add(this);
        }
    }

    class Op_And : Operator {
        public Op_And() {
            OperationName = "And";
            Operation = "&&";

            AllOperators.Add(this);
        }
    }

    class Op_NotEqual : Operator {
        public Op_NotEqual() {
            OperationName = "NotEqual";
            Operation = "!=";

            AllOperators.Add(this);
        }
    }

    class Op_Equal : Operator {
        public Op_Equal() {
            OperationName = "Equal";
            Operation = "==";

            AllOperators.Add(this);
        }
    }

    class Op_Greater : Operator {
        public Op_Greater() {
            OperationName = "Greater";
            Operation = ">";

            AllOperators.Add(this);
        }
    }

    class Op_Lesser : Operator {
        public Op_Lesser() {
            OperationName = "Lesser";
            Operation = "<";

            AllOperators.Add(this);
        }
    }

    class Op_GreaterEqual : Operator {
        public Op_GreaterEqual() {
            OperationName = "GreaterOrEqual";
            Operation = ">=";

            AllOperators.Add(this);
        }
    }

    class Op_LesserEqual : Operator {
        public Op_LesserEqual() {
            OperationName = "LesserOrEqual";
            Operation = "<=";

            AllOperators.Add(this);
        }
    }

    class Op_Add : Operator {
        public Op_Add() {
            OperationName = "Add";
            Operation = "+";

            AllOperators.Add(this);
        }
    }

    class Op_Subtract : Operator {
        public Op_Subtract() {
            OperationName = "Subtract";
            Operation = "-";

            AllOperators.Add(this);
        }
    }

    class Op_Multiply : Operator {
        public Op_Multiply() {
            OperationName = "Multiply";
            Operation = "*";

            AllOperators.Add(this);
        }
    }

    class Op_Divide : Operator {
        public Op_Divide() {
            OperationName = "Divide";
            Operation = "/";

            AllOperators.Add(this);
        }
    }

    class Op_Modulo : Operator {
        public Op_Modulo() {
            OperationName = "Modulo";
            Operation = "%";

            AllOperators.Add(this);
        }
    }

    class Op_Power : Operator {
        public Op_Power() {
            OperationName = "Power";
            Operation = "^";

            AllOperators.Add(this);
        }
    }

    class Op_Negate : Operator {
        public Op_Negate() {
            OperationName = "Negate";
            Members = 1;
            Operation = "-";

            AllOperators.Add(this);
        }
    }

    class Op_Not : Operator {
        public Op_Not() {
            OperationName = "Not";
            Members = 1;
            Operation = "!";

            AllOperators.Add(this);
        }
    }

    class Op_PostInc : Operator {
        public Op_PostInc() {
            OperationName = "PostIncremenet";
            Members = 1;
            Operation = "++";

            AllOperators.Add(this);
        }
    }

    class Op_PostDec : Operator {
        public Op_PostDec() {
            OperationName = "PostDecremenet";
            Members = 1;
            Operation = "--";

            AllOperators.Add(this);
        }
    }

    class Op_Return : Operator {
        public Op_Return() {
            OperationName = "return";
            Members = 0;
            Operation = "return";

            AllOperators.Add(this);
        }
    }
}
