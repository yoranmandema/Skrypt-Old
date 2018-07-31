using Sys = System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    public partial class System : SkryptObject {
        [Constant, Static]
        public class Encoding : SkryptObject {
            [Constant]
            public static SkryptObject ToBinary(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var n = TypeConverter.ToNumeric(input, 0);
                var str = Sys.Convert.ToString((long)n, 2);
                return engine.Create<String>(str);
            }

            [Constant]
            public static SkryptObject ToHex(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var n = TypeConverter.ToNumeric(input, 0);
                var str = Sys.Convert.ToString((long)n, 16);
                return engine.Create<String>(str);
            }

            [Constant]
            public static SkryptObject ToString(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var n = TypeConverter.ToNumeric(input, 0);
                var b = TypeConverter.ToNumeric(input, 1);
                var str = Sys.Convert.ToString((int)n, (int)b);
                return engine.Create<String>(str);
            }

            [Constant]
            public static SkryptObject ToUTF8(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var str = TypeConverter.ToString(input, 0);
                byte[] bytes = Sys.Text.Encoding.Default.GetBytes(str);
                var encoded = Sys.Text.Encoding.UTF8.GetString(bytes);

                return engine.Create<String>(encoded);
            }

            [Constant]
            public static SkryptObject ToUTF16(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var str = TypeConverter.ToString(input, 0);
                byte[] bytes = Sys.Text.Encoding.Default.GetBytes(str);
                var encoded = Sys.Text.UnicodeEncoding.Unicode.GetString(bytes);

                return engine.Create<String>(encoded);
            }
        }
    }
}
