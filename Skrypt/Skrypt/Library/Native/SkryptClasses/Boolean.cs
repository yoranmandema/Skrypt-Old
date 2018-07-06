using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.Native {
    partial class System {
        public class Boolean : SkryptObject {
            public bool value;

            public Boolean() {
                Name = "boolean";
            }

            public Boolean(bool v) {
                Name = "boolean";
                value = v;
            }

            public override bool ToBoolean() {
                return value;
            }

            public static implicit operator Boolean(bool d) {
                return new Boolean(d);
            }

            public static implicit operator bool(Boolean d) {
                return d.value;
            }

            static public Boolean _and(Boolean A, Boolean B) {
                return new Boolean { value = A.value && B.value };
            }

            static public Boolean _or(Boolean A, Boolean B) {
                return new Boolean { value = A.value || B.value };
            }

            static public Boolean _not(Boolean A) {
                return new Boolean { value = !A.value };
            }

            public override string ToString() {
                return value.ToString();
            }
        }
    }
}
