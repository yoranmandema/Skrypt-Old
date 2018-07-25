using System.Collections.Generic;

namespace Skrypt.Library
{
    public class Operator
    {
        public static List<Operator> AllOperators = new List<Operator>();
        public bool FailOnMissingMembers = true;
        public string Operation = "";
        public int Members { get; set; } = 2;
        public string OperationName { get; set; } = "";
    }

    internal class OpAccess : Operator
    {
        public OpAccess()
        {
            OperationName = "access";
            Operation = ".";

            AllOperators.Add(this);
        }
    }

    internal class OpAssign : Operator
    {
        public OpAssign()
        {
            OperationName = "assign";
            Operation = "=";

            AllOperators.Add(this);
        }
    }

    internal class OpOr : Operator
    {
        public OpOr()
        {
            OperationName = "or";
            Operation = "||";

            AllOperators.Add(this);
        }
    }

    internal class OpAnd : Operator
    {
        public OpAnd()
        {
            OperationName = "and";
            Operation = "&&";

            AllOperators.Add(this);
        }
    }

    internal class OpNotEqual : Operator
    {
        public OpNotEqual()
        {
            OperationName = "notequal";
            Operation = "!=";

            AllOperators.Add(this);
        }
    }

    internal class OpEqual : Operator
    {
        public OpEqual()
        {
            OperationName = "equal";
            Operation = "==";

            AllOperators.Add(this);
        }
    }

    internal class OpGreater : Operator
    {
        public OpGreater()
        {
            OperationName = "greater";
            Operation = ">";

            AllOperators.Add(this);
        }
    }

    internal class OpLesser : Operator
    {
        public OpLesser()
        {
            OperationName = "lesser";
            Operation = "<";

            AllOperators.Add(this);
        }
    }

    internal class OpGreaterEqual : Operator
    {
        public OpGreaterEqual()
        {
            OperationName = "equalgreater";
            Operation = ">=";

            AllOperators.Add(this);
        }
    }

    internal class OpLesserEqual : Operator
    {
        public OpLesserEqual()
        {
            OperationName = "equallesser";
            Operation = "<=";

            AllOperators.Add(this);
        }
    }

    internal class OpBitShiftL : Operator {
        public OpBitShiftL() {
            OperationName = "bitshiftl";
            Operation = "<<";

            AllOperators.Add(this);
        }
    }

    internal class OpBitShiftR : Operator {
        public OpBitShiftR() {
            OperationName = "bitshiftr";
            Operation = ">>";

            AllOperators.Add(this);
        }
    }

    internal class OpBitShiftRZ : Operator {
        public OpBitShiftRZ() {
            OperationName = "bitshiftrz";
            Operation = ">>>";

            AllOperators.Add(this);
        }
    }

    internal class OpBitAnd : Operator {
        public OpBitAnd() {
            OperationName = "bitand";
            Operation = "&";

            AllOperators.Add(this);
        }
    }

    internal class OpBitXor : Operator {
        public OpBitXor() {
            OperationName = "bitxor";
            Operation = "|||";

            AllOperators.Add(this);
        }
    }

    internal class OpBitOr : Operator {
        public OpBitOr() {
            OperationName = "bitor";
            Operation = "|";

            AllOperators.Add(this);
        }
    }

    internal class OpBitNot : Operator {
        public OpBitNot() {
            OperationName = "bitnot";
            Members = 1;
            Operation = "~";

            AllOperators.Add(this);
        }
    }

    internal class OpAdd : Operator
    {
        public OpAdd()
        {
            OperationName = "add";
            Operation = "+";

            AllOperators.Add(this);
        }
    }

    internal class OpSubtract : Operator
    {
        public OpSubtract()
        {
            OperationName = "subtract";
            Operation = "-";
            FailOnMissingMembers = false;

            AllOperators.Add(this);
        }
    }

    internal class OpMultiply : Operator
    {
        public OpMultiply()
        {
            OperationName = "multiply";
            Operation = "*";

            AllOperators.Add(this);
        }
    }

    internal class OpDivide : Operator
    {
        public OpDivide()
        {
            OperationName = "divide";
            Operation = "/";

            AllOperators.Add(this);
        }
    }

    internal class OpModulo : Operator
    {
        public OpModulo()
        {
            OperationName = "modulo";
            Operation = "%";

            AllOperators.Add(this);
        }
    }

    internal class OpPower : Operator
    {
        public OpPower()
        {
            OperationName = "power";
            Operation = "^";

            AllOperators.Add(this);
        }
    }

    internal class OpLambda : Operator {
        public OpLambda() {
            OperationName = "lambda";
            Operation = "=>";

            AllOperators.Add(this);
        }
    }

    internal class OpCall : Operator {
        public OpCall() {
            OperationName = "call";
            Operation = "(";

            AllOperators.Add(this);
        }
    }

    internal class OpIndex : Operator {
        public OpIndex() {
            OperationName = "index";
            Operation = "[";

            AllOperators.Add(this);
        }
    }

    internal class OpNegate : Operator
    {
        public OpNegate()
        {
            OperationName = "negate";
            Members = 1;
            Operation = "-";

            AllOperators.Add(this);
        }
    }

    internal class OpNot : Operator
    {
        public OpNot()
        {
            OperationName = "not";
            Members = 1;
            Operation = "!";

            AllOperators.Add(this);
        }
    }

    internal class OpPostInc : Operator
    {
        public OpPostInc()
        {
            OperationName = "postincrement";
            Members = 1;
            Operation = "++";

            AllOperators.Add(this);
        }
    }

    internal class OpPostDec : Operator
    {
        public OpPostDec()
        {
            OperationName = "postdecrement";
            Members = 1;
            Operation = "--";

            AllOperators.Add(this);
        }
    }

    internal class OpReturn : Operator
    {
        public OpReturn()
        {
            OperationName = "return";
            Members = 0;
            Operation = "return";

            AllOperators.Add(this);
        }
    }

    internal class OpBreak : Operator {
        public OpBreak() {
            OperationName = "break";
            Members = 0;
            Operation = "break";

            AllOperators.Add(this);
        }
    }

    internal class OpContinue : Operator {
        public OpContinue() {
            OperationName = "continue";
            Members = 0;
            Operation = "continue";

            AllOperators.Add(this);
        }
    }

    internal class OpConditional : Operator {
        public OpConditional() {
            OperationName = "conditional";
            Members = 0;
            Operation = "?";

            AllOperators.Add(this);
        }
    }
    internal class OpConditionalElse : Operator {
        public OpConditionalElse() {
            OperationName = "else";
            Members = 0;
            Operation = ":";

            AllOperators.Add(this);
        }
    }
}