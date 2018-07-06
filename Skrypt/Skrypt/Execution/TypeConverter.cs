using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library;
using Sys = Skrypt.Library.Native.System;

namespace Skrypt.Execution {
    static class TypeConverter {
        public static Sys.Numeric ToNumeric (SkryptObject[] Args, int Index) {
            if (Index > Args.Length - 1) {
                return new Sys.Numeric(0);
            }

            return (Sys.Numeric)Args[Index];
        }

        public static Sys.Boolean ToBoolean(SkryptObject[] Args, int Index) {
            if (Index > Args.Length - 1) {
                return new Sys.Boolean(false);
            }

            return Args[Index].ToBoolean();
        }

        public static Sys.String ToString(SkryptObject[] Args, int Index) {
            if (Index > Args.Length - 1) {
                return new Sys.String("");
            }

            return Args[Index].ToString();
        }

        public static SkryptObject ToAny(SkryptObject[] Args, int Index) {
            if (Index > Args.Length - 1) {
                return new Sys.Void();
            }

            return Args[Index];
        }
    }
}
