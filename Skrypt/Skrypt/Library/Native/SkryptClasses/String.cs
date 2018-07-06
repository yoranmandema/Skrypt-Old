using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.Native {
    partial class System {
        public class String : SkryptObject {
            public string value;

            public String() {
                Name = "string";
            }

            public String(string v) {
                Name = "string";
                value = v;
            }

            public static implicit operator String(string d) {
                return new String(d);
            }

            public static implicit operator string(String d) {
                return d.value;
            }

            //public override SkryptObject _Add(SkryptObject X) {
            //    return new SkryptString(value + X.ToString());
            //}

            public override string ToString() {
                return value;
            }
        }
    }
}
