using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Tokenization {
    /// <summary>
    /// Represents part of a string with a type.
    /// </summary>
    class Token {
        public string Value { get; set; }
        public string Type { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        public override string ToString() {
            string str = "Token[";
            str += (Start != End) && Start != 0 ? "(" + Start + "," + End + ") " : "";
            str += Type != null ? "Type: " + Type : "";
            str += Value != null ? (Type != null ? ", " : "") + "Value: " + Value : "";
            str += "]";

            //return "(" + Start + "," + End + ") Type: " + Type + ", Value: " + Value;
            return str;
        }

        /// <summary>
        /// Returns true if value and type are equal
        /// </summary>
        public bool Equals(Token other) {
            if (this.Value == other.Value && this.Type == other.Type) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if token has the same type and value
        /// </summary>
        public bool Has (string Type, string Value) {
            if (this.Value == Value && this.Type == Type) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if token has the same type and value. Type and value can be left as null to ignore
        /// </summary>
        public bool LazyEqual (Token other) {
            bool hasType = other.Type != null ? this.Type == other.Type : true;
            bool hasValue = other.Value != null ? this.Value == other.Value : true;

            return hasType && hasValue;
        }
    }
}
