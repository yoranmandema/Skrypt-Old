using System;
using SysMath = System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library.SkryptClasses;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    partial class System {
        public class Math {
            public class Test {
                static public Numeric TestValue = new Numeric(10);
            }

            static public Numeric PI = new Numeric(SysMath.PI);

            static public SkryptObject Round(SkryptObject[] Values) {
                var a = TypeConverter.ToNumeric(Values, 0);

                return (Numeric)SysMath.Round(a.value);
            }
        }
    }
}
