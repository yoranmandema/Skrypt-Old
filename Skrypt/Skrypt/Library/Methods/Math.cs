using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library.SkryptClasses;

namespace Skrypt.Library.Methods {
    public partial class StandardMethods {
        public static Numeric Round(params SkryptObject[] input) {
            return new Numeric(Math.Round(((Numeric)input[0]).value));
        }
    }
}
