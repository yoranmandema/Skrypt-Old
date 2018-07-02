using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Parsing;

namespace Skrypt.Library.SkryptClasses {
    public class Numeric : SkryptObject {
        public double value;

        public Numeric() {
            Name = "numeric";
        }

        public Numeric(double v) {
            Name = "numeric";
            value = v;
        }

        public static implicit operator Numeric(double d) {
            return new Numeric(d);
        }

        public static SkryptObject Add(SkryptObject A, SkryptObject B) {
            if (A.GetType() != typeof(Numeric) || B.GetType() != typeof(Numeric)) {
                throw new SkryptException();
            }

            return new Numeric(((Numeric)A).value + ((Numeric)B).value);
        }

        public static Numeric Negate(Numeric A) {
            return new Numeric(-A.value);
        }

        public static Numeric Subtract(Numeric A, Numeric B) {
            return new Numeric(A.value - B.value);
        }

        public static Numeric Multiply(Numeric A, Numeric B) {
            return new Numeric(A.value * B.value);
        }

        public static Numeric Divide(Numeric A, Numeric B) {
            return new Numeric(A.value / B.value);
        }

        public static Numeric Modulo(Numeric A, Numeric B) {
            return new Numeric(A.value % B.value);
        }

        public static Numeric PostIncrement(Numeric A) {
            double value = A.value;
            A.value++;
            return new Numeric(value);
        }

        public static SkryptBoolean Lesser(Numeric A, Numeric B) {
            return new SkryptBoolean(A.value < B.value);
        }

        public static SkryptBoolean Greater(Numeric A, Numeric B) {
            return new SkryptBoolean { value = A.value > B.value };
        }

        public static SkryptBoolean Equal(Numeric A, Numeric B) {
            return new SkryptBoolean { value = A.value == B.value };
        }

        public override string ToString() {
            return "" + value;
        }
    }
}
