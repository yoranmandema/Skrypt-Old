using System.Collections.Generic;

namespace Skrypt.Library
{
    public class Operator
    {
        public static List<Operator> AllOperators = new List<Operator>();
        public bool FailOnMissingMembers = true;
        public string Operation = "";
        public Operators Type = 0;
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

    internal class AssignmentOperator : Operator {
        public Operator SecondaryOperator;
    }

    internal class OpAssignAdd : AssignmentOperator {
        public OpAssignAdd() {
            OperationName = "assign";
            Operation = "+=";
            SecondaryOperator = new OpAdd();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignSubtract : AssignmentOperator {
        public OpAssignSubtract() {
            OperationName = "assign";
            Operation = "-=";
            SecondaryOperator = new OpSubtract();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignDivide : AssignmentOperator {
        public OpAssignDivide() {
            OperationName = "assign";
            Operation = "/=";
            SecondaryOperator = new OpDivide();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignMultiply : AssignmentOperator {
        public OpAssignMultiply() {
            OperationName = "assign";
            Operation = "*=";
            SecondaryOperator = new OpMultiply();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignModulo : AssignmentOperator {
        public OpAssignModulo() {
            OperationName = "assign";
            Operation = "%=";
            SecondaryOperator = new OpModulo();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignPower : AssignmentOperator {
        public OpAssignPower() {
            OperationName = "assign";
            Operation = "^=";
            SecondaryOperator = new OpPower();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignBitShiftL : AssignmentOperator {
        public OpAssignBitShiftL() {
            OperationName = "assign";
            Operation = "<<=";
            SecondaryOperator = new OpBitShiftL();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignBitShiftR : AssignmentOperator {
        public OpAssignBitShiftR() {
            OperationName = "assign";
            Operation = ">>=";
            SecondaryOperator = new OpBitShiftR();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignBitShiftRZ : AssignmentOperator {
        public OpAssignBitShiftRZ() {
            OperationName = "assign";
            Operation = ">>=";
            SecondaryOperator = new OpBitShiftRZ();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignBitOr : AssignmentOperator {
        public OpAssignBitOr() {
            OperationName = "assign";
            Operation = "|=";
            SecondaryOperator = new OpBitOr();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignBitXor : AssignmentOperator {
        public OpAssignBitXor() {
            OperationName = "assign";
            Operation = "|||=";
            SecondaryOperator = new OpBitXor();

            AllOperators.Add(this);
        }
    }

    internal class OpAssignBitAnd : AssignmentOperator {
        public OpAssignBitAnd() {
            OperationName = "assign";
            Operation = "&=";
            SecondaryOperator = new OpBitAnd();

            AllOperators.Add(this);
        }
    }

    internal class OpOr : Operator
    {
        public OpOr()
        {
            OperationName = "or";
            Operation = "||";
            Type = Operators.Or;
            AllOperators.Add(this);
        }
    }

    internal class OpAnd : Operator
    {
        public OpAnd()
        {
            OperationName = "and";
            Operation = "&&";
            Type = Operators.And;
            AllOperators.Add(this);
        }
    }

    internal class OpNotEqual : Operator
    {
        public OpNotEqual()
        {
            OperationName = "nequal";
            Operation = "!=";
            Type = Operators.NotEqual;
            AllOperators.Add(this);
        }
    }

    internal class OpEqual : Operator
    {
        public OpEqual()
        {
            OperationName = "equal";
            Operation = "==";
            Type = Operators.Equal;
            AllOperators.Add(this);
        }
    }

    internal class OpGreater : Operator
    {
        public OpGreater()
        {
            OperationName = "great";
            Operation = ">";
            Type = Operators.Greater;
            AllOperators.Add(this);
        }
    }

    internal class OpLesser : Operator
    {
        public OpLesser()
        {
            OperationName = "less";
            Operation = "<";
            Type = Operators.Lesser;
            AllOperators.Add(this);
        }
    }

    internal class OpGreaterEqual : Operator
    {
        public OpGreaterEqual()
        {
            OperationName = "eqg";
            Operation = ">=";
            Type = Operators.EqualGreater;
            AllOperators.Add(this);
        }
    }

    internal class OpLesserEqual : Operator
    {
        public OpLesserEqual()
        {
            OperationName = "eql";
            Operation = "<=";
            Type = Operators.EqualLesser;
            AllOperators.Add(this);
        }
    }

    internal class OpBitShiftL : Operator {
        public OpBitShiftL() {
            OperationName = "bshiftl";
            Operation = "<<";
            Type = Operators.BitShiftL;
            AllOperators.Add(this);
        }
    }

    internal class OpBitShiftR : Operator {
        public OpBitShiftR() {
            OperationName = "bshiftr";
            Operation = ">>";
            Type = Operators.BitShiftR;
            AllOperators.Add(this);
        }
    }

    internal class OpBitShiftRZ : Operator {
        public OpBitShiftRZ() {
            OperationName = "bshiftrz";
            Operation = ">>>";
            Type = Operators.BitShiftRZ;
            AllOperators.Add(this);
        }
    }

    internal class OpBitAnd : Operator {
        public OpBitAnd() {
            OperationName = "band";
            Operation = "&";
            Type = Operators.BitAnd;
            AllOperators.Add(this);
        }
    }

    internal class OpBitXor : Operator {
        public OpBitXor() {
            OperationName = "bxor";
            Operation = "|||";
            Type = Operators.BitXOr;
            AllOperators.Add(this);
        }
    }

    internal class OpBitOr : Operator {
        public OpBitOr() {
            OperationName = "bor";
            Operation = "|";
            Type = Operators.BitOr;
            AllOperators.Add(this);
        }
    }

    internal class OpBitNot : Operator {
        public OpBitNot() {
            OperationName = "bnot";
            Members = 1;
            Operation = "~";
            Type = Operators.BitNot;
            AllOperators.Add(this);
        }
    }

    internal class OpAdd : Operator
    {
        public OpAdd()
        {
            OperationName = "add";
            Operation = "+";
            Type = Operators.Add;
            AllOperators.Add(this);
        }
    }

    internal class OpSubtract : Operator
    {
        public OpSubtract()
        {
            OperationName = "sub";
            Operation = "-";
            FailOnMissingMembers = false;
            Type = Operators.Subtract;
            AllOperators.Add(this);
        }
    }

    internal class OpMultiply : Operator
    {
        public OpMultiply()
        {
            OperationName = "mul";
            Operation = "*";
            Type = Operators.Multiply;
            AllOperators.Add(this);
        }
    }

    internal class OpDivide : Operator
    {
        public OpDivide()
        {
            OperationName = "div";
            Operation = "/";
            Type = Operators.Divide;
            AllOperators.Add(this);
        }
    }

    internal class OpModulo : Operator
    {
        public OpModulo()
        {
            OperationName = "mod";
            Operation = "%";
            Type = Operators.Modulo;
            AllOperators.Add(this);
        }
    }

    internal class OpPower : Operator
    {
        public OpPower()
        {
            OperationName = "pow";
            Operation = "^";
            Type = Operators.Power;
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
            Type = Operators.Index;
            AllOperators.Add(this);
        }
    }

    internal class OpNegate : Operator
    {
        public OpNegate()
        {
            OperationName = "neg";
            Members = 1;
            Operation = "-";
            Type = Operators.Negate;
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
            Type = Operators.Not;
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
            Type = Operators.PostIncrement;
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
            Type = Operators.PostDecrement;
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