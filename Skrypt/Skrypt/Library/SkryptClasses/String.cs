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

        public SkryptString(string v) {
            Name = "string";
            value = v;
        }

        public override SkryptObject _Add(SkryptObject X) {
            return new SkryptString(value + X.ToString());
        }

        public override string ToString() {
            return value;
        }
    }
}
