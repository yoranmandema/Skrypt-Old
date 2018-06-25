using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.SkryptClasses {
    class SkryptString : SkryptObject {
        public string value;

        public SkryptString() {
            Name = "string";
        }

        static public SkryptString _add(SkryptString A, SkryptObject B) {
            return new SkryptString { value = A.value + B.ToString() };
        }

        static public SkryptString _add(SkryptObject A, SkryptString B) {
            return new SkryptString { value = A.ToString() + B.value };
        }

        public override string ToString() {
            return value;
        }
    }
}
