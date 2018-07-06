using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.Native {
    partial class System {
        public class Void : SkryptObject {
            public Void() {
                Name = "void";
            }

            public override bool ToBoolean() {
                throw new Exception("Can't convert void to boolean!");
            }
        }
    }
}
