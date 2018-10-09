using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Tokenization {
    public enum TokenTypes : byte {
        None,
        ArrayLiteral,
        NumericLiteral,
        BinaryLiteral,
        HexadecimalLiteral,
        BooleanLiteral,
        NullLiteral,
        FunctionLiteral,
        Identifier,
        Keyword,
        EndOfExpression,
        NewLine,
        Punctuator,
        StringLiteral,
        Call,
        Arguments,
        Parameters,
        Parameter,
        Getter,
        Index,
        Using,
        Include,
        Conditional,
        Block,
        Statement,
        Condition,
        ClassDeclaration,
        Inherit,
        MethodDeclaration
    }
}
