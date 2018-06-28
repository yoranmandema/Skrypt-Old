using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.SkryptClasses {
    public class SkryptVoid : SkryptObject {
        public SkryptVoid() {
            Name = "void";
        }

        public override bool ToBoolean() {
            throw new Exception("Can't convert void to boolean!");
        }
    }
}
