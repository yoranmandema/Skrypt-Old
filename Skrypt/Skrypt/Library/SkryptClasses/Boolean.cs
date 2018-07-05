using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.SkryptClasses {
    public class SkryptBoolean : SkryptObject {
        public bool value;

        public SkryptBoolean() {
            Name = "boolean";
        }

        public SkryptBoolean(bool v) {
            Name = "boolean";
            value = v;
        }

        public override bool ToBoolean() {
            return value;
        }

        public static implicit operator SkryptBoolean(bool d) {
            return new SkryptBoolean(d);
        }

        public static implicit operator bool(SkryptBoolean d) {
            return d.value;
        }

        static public SkryptBoolean _and (SkryptBoolean A, SkryptBoolean B) {
            return new SkryptBoolean { value = A.value && B.value };
        }

        static public SkryptBoolean _or(SkryptBoolean A, SkryptBoolean B) {
            return new SkryptBoolean { value = A.value || B.value };
        }

        static public SkryptBoolean _not(SkryptBoolean A) {
            return new SkryptBoolean { value = !A.value};
        }

        public override string ToString() {
            return value.ToString();
        }
    }
}
