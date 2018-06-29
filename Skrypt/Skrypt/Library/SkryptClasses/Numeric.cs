using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.SkryptClasses {
    public class Numeric : SkryptObject {
        public double value;

        public Numeric() {
            Name = "numeric";
        }

        public static implicit operator Numeric(double d) {
            return new Numeric { value=d };
        }

        static public Numeric _negate(Numeric A) {
            return new Numeric { value = -A.value};
        }

        static public Numeric _add(Numeric A, Numeric B) {
            return new Numeric { value = A.value + B.value };
        }

        static public Numeric _subtract(Numeric A, Numeric B) {
            return new Numeric { value = A.value - B.value };
        }

        static public Numeric _multiply(Numeric A, Numeric B) {
            return new Numeric { value = A.value * B.value };
        }

        static public Numeric _divide(Numeric A, Numeric B) {
            return new Numeric { value = A.value / B.value };
        }

        static public Numeric _modulo(Numeric A, Numeric B) {
            return new Numeric { value = A.value % B.value };
        }

        static public Numeric _postincrement(Numeric A) {
            double value = A.value;
            A.value++;
            return new Numeric { value = value };
        }

        static public SkryptBoolean _lesser(Numeric A, Numeric B) {
            return new SkryptBoolean { value = A.value < B.value };
        }

        static public SkryptBoolean _greater(Numeric A, Numeric B) {
            return new SkryptBoolean { value = A.value > B.value };
        }

        static public SkryptBoolean _equal(Numeric A, Numeric B) {
            return new SkryptBoolean { value = A.value == B.value };
        }

        public override string ToString() {
            return "" + value;
        }
    }
}
