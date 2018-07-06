using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.Native {
    partial class System {
        public class Array : SkryptObject {
            public List<SkryptObject> value = new List<SkryptObject>();

            public Array() {
                Name = "array";
            }

            //static public SkryptArray _add(SkryptObject A, SkryptArray B) {
            //    List<SkryptObject> newValue = new List<SkryptObject>() {
            //        A
            //    };

            //    newValue.AddRange(B.value);

            //    return new SkryptArray { value = newValue };
            //}

            //static public SkryptArray _add(SkryptArray A, SkryptObject B) {
            //    List<SkryptObject> newValue = new List<SkryptObject>(A.value) {
            //        B
            //    };

            //    return new SkryptArray { value = newValue };
            //}

            public override string ToString() {
                return "[" + string.Join(",", value) + "]";
            }
        }
    }
}
