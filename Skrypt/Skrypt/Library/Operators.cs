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
            OperationName = "or";
            Operation = "||";

            AllOperators.Add(this);
        }
    }

    class Op_And : Operator {
        public Op_And() {
            OperationName = "and";
            Operation = "&&";

            AllOperators.Add(this);
        }
    }

    class Op_NotEqual : Operator {
        public Op_NotEqual() {
            OperationName = "notequal";
            Operation = "!=";

            AllOperators.Add(this);
        }
    }

    class Op_Equal : Operator {
        public Op_Equal() {
            OperationName = "equal";
            Operation = "==";

            AllOperators.Add(this);
        }
    }

    class Op_Greater : Operator {
        public Op_Greater() {
            OperationName = "greater";
            Operation = ">";

            AllOperators.Add(this);
        }
    }

    class Op_Lesser : Operator {
        public Op_Lesser() {
            OperationName = "lesser";
            Operation = "<";

            AllOperators.Add(this);
        }
    }

    class Op_GreaterEqual : Operator {
        public Op_GreaterEqual() {
            OperationName = "equalgreater";
            Operation = ">=";

            AllOperators.Add(this);
        }
    }

    class Op_LesserEqual : Operator {
        public Op_LesserEqual() {
            OperationName = "equallesser";
            Operation = "<=";

            AllOperators.Add(this);
        }
    }

    class Op_Add : Operator {
        public Op_Add() {
            OperationName = "add";
            Operation = "+";

            AllOperators.Add(this);
        }
    }

    class Op_Subtract : Operator {
        public Op_Subtract() {
            OperationName = "subtract";
            Operation = "-";

            AllOperators.Add(this);
        }
    }

    class Op_Multiply : Operator {
        public Op_Multiply() {
            OperationName = "multiply";
            Operation = "*";

            AllOperators.Add(this);
        }
    }

    class Op_Divide : Operator {
        public Op_Divide() {
            OperationName = "divide";
            Operation = "/";

            AllOperators.Add(this);
        }
    }

    class Op_Modulo : Operator {
        public Op_Modulo() {
            OperationName = "modulo";
            Operation = "%";

            AllOperators.Add(this);
        }
    }

    class Op_Power : Operator {
        public Op_Power() {
            OperationName = "power";
            Operation = "^";

            AllOperators.Add(this);
        }
    }

    class Op_Negate : Operator {
        public Op_Negate() {
            OperationName = "negate";
            Members = 1;
            Operation = "-";

            AllOperators.Add(this);
        }
    }

    class Op_Not : Operator {
        public Op_Not() {
            OperationName = "not";
            Members = 1;
            Operation = "!";

            AllOperators.Add(this);
        }
    }

    class Op_PostInc : Operator {
        public Op_PostInc() {
            OperationName = "postincrement";
            Members = 1;
            Operation = "++";

            AllOperators.Add(this);
        }
    }

    class Op_PostDec : Operator {
        public Op_PostDec() {
            OperationName = "postdecrement";
            Members = 1;
            Operation = "--";

            AllOperators.Add(this);
        }
    }
}
