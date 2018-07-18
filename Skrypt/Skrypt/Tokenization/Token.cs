using System;

namespace Skrypt.Tokenization
{
    /// <summary>
    ///     Represents part of a string with a type.
    /// </summary>
    public class Token
    {
        public string Value { get; set; }
        public TokenTypes Type { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        public override string ToString()
        {
            var str = "Token[";
            str += Start != End && Start != 0 ? "(" + Start + "," + End + ") " : "";
            str += "Type: " + Type;
            str += Value != null ? "Value: " + Value : "";
            str += "]";

            //return "(" + Start + "," + End + ") Type: " + Type + ", Value: " + Value;
            return str;
        }

        /// <summary>
        ///     Returns true if value and type are equal
        /// </summary>
        public bool Equals(Token other)
        {
            if (Value == other.Value && Type == other.Type) return true;

            return false;
        }

        /// <summary>
        ///     Returns true if token has the same type and value
        /// </summary>
        public bool Has(TokenTypes type, string value)
        {
            if (this.Value == value && this.Type == type) return true;

            return false;
        }

        public bool IsValuable()
        {
            return Type == TokenTypes.Identifier ||
                   Type == TokenTypes.NumericLiteral ||
                   Type == TokenTypes.BooleanLiteral ||
                   Type == TokenTypes.NullLiteral ||
                   Type == TokenTypes.StringLiteral;
        }

        public bool IsLiteral() {
            return Type == TokenTypes.NumericLiteral ||
                   Type == TokenTypes.BooleanLiteral ||
                   Type == TokenTypes.NullLiteral ||
                   Type == TokenTypes.StringLiteral;
        }

        /// <summary>
        ///     Returns true if token has the same type and value. Type and value can be left as null to ignore
        /// </summary>
        public bool LazyEqual(Token other)
        {
            var hasType = Type == other.Type;
            var hasValue = other.Value != null ? Value == other.Value : true;

            return hasType && hasValue;
        }
    }
}