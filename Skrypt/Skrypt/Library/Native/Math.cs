using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library.SkryptClasses;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    public class SkryptMath {
        static public SkryptObject Round(SkryptObject[] Values) {
            var a = TypeConverter.ToNumeric(Values, 0);

            return (Numeric)Math.Round(a.value);
        }
    }
}
