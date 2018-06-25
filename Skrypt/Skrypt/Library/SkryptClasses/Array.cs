using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.SkryptClasses {
    class SkryptArray : SkryptObject {
        public List<SkryptObject> value = new List<SkryptObject>();

        public SkryptArray() {
            Name = "array";
        }

        public override string ToString() {
            return "[" + string.Join(",", value) + "]";
        }
    }
}
