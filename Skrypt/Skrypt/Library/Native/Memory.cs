using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Execution;
using System.Runtime.InteropServices;

namespace Skrypt.Library.Native {
    partial class System {
        [Constant, Static]
        public class Memory : SkryptObject {
            [Constant]
            public static SkryptObject GetSize(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var a = TypeConverter.ToAny(input,0);

                return engine.Create<Numeric>(Marshal.SizeOf(a));
            }

            [Constant]
            public static SkryptObject GetTotalMemory(SkryptObject self, SkryptObject[] input) {
                return new Numeric(GC.GetTotalMemory(true));
            }
        }
    }
}
