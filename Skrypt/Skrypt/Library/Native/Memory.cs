using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    partial class System {
        [Constant, Static]
        public class Memory : SkryptObject {
            [Constant]
            public static SkryptObject GetSize(SkryptObject self, SkryptObject[] input) {
                var a = TypeConverter.ToAny(input,0);

                long before = GC.GetTotalMemory(true);
                var b = a.Clone();
                long after = GC.GetTotalMemory(true);

                return new Numeric(after - before);
            }

            [Constant]
            public static SkryptObject GetTotalMemory(SkryptObject self, SkryptObject[] input) {
                return new Numeric(GC.GetTotalMemory(true));
            }
        }
    }
}
