using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library;
using Skrypt.Library.SkryptClasses;

namespace Skrypt.Execution {
    static class TypeConverter {
        public static Numeric ToNumeric (SkryptObject[] Args, int Index) {
            if (Index > Args.Length - 1) {
                return new Numeric(0);
            }

            return (Numeric)Args[Index];
        }

        public static SkryptObject ToAny(SkryptObject[] Args, int Index) {
            if (Index > Args.Length - 1) {
                return new SkryptNull();
            }

            return Args[Index];
        }
    }
}
