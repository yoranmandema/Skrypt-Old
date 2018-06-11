using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skrypt.Parsing.SkryptClasses {
    class Numeric : SkryptClass {
        double Value = 0;

        public static double operator +(Numeric a, Numeric b) {
            return a.Value + b.Value;
        }
    }
}
