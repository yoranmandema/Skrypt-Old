using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skrypt.Library.SkryptClasses;

namespace Skrypt.Library.Methods {
    public partial class StandardMethods {
        public static SkryptObject print(params SkryptObject[] input) {
            Console.WriteLine(input[0]);

            return new SkryptVoid();
        }
    }
}
