using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Library.Native {
    public partial class StandardMethods {
        public static SkryptObject print(SkryptObject[] Values) {
            Console.WriteLine(Values[0]);

            return new System.Void();
        }
    }
}
