using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.SkryptClasses {
    public class SkryptNull : SkryptObject {
        public SkryptNull() {
            Name = "null";
        }

        public override bool ToBoolean() {
            return false;
        }
    }
}
