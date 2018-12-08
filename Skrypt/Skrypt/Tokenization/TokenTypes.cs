using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Tokenization {
    [Flags]
    public enum TokenTypes {
        None                = 0,
        ArrayLiteral        = 1 << 0,
        NumericLiteral      = 1 << 1,
        BinaryLiteral       = 1 << 2,
        HexadecimalLiteral  = 1 << 3,
        BooleanLiteral      = 1 << 4,
        NullLiteral         = 1 << 5,
        FunctionLiteral     = 1 << 6, 
        Identifier          = 1 << 7,
        Keyword             = 1 << 8,
        EndOfExpression     = 1 << 9,
        NewLine             = 1 << 10,
        Punctuator          = 1 << 11,
        StringLiteral       = 1 << 12,
        Call                = 1 << 13,
        Arguments           = 1 << 14,
        Parameters          = 1 << 15,
        Parameter           = 1 << 16,
        Getter              = 1 << 17,
        Index               = 1 << 18,
        Import               = 1 << 19,
        Include             = 1 << 20,
        Conditional         = 1 << 21,
        Block               = 1 << 22,
        Statement           = 1 << 23,
        Condition           = 1 << 24,
        ClassDeclaration    = 1 << 25,
        Inherit             = 1 << 26,
        MethodDeclaration   = 1 << 27,
        Literal             = ArrayLiteral | NumericLiteral | BooleanLiteral | NullLiteral | FunctionLiteral
    }
}
