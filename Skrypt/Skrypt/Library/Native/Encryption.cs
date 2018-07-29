using Sys = System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Execution;
using System.Security.Cryptography;

namespace Skrypt.Library.Native {
    partial class System {
        [Constant, Static]
        public class Encryption : SkryptObject {
            [Constant]
            public static SkryptObject SHA256(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = TypeConverter.ToString(values, 0);

                var byteArray = Sys.Text.Encoding.ASCII.GetBytes(s.Value);
                var hashValue = new SHA256Managed().ComputeHash(byteArray);
                var array = engine.Create<Array>();

                for (int i = 0; i < hashValue.Length - 1; i++) {
                    array.Value.Add((Numeric)hashValue[i]);
                }

                return array;
            }

            [Constant]
            public static SkryptObject SHA1(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = TypeConverter.ToString(values, 0);

                var byteArray = Sys.Text.Encoding.ASCII.GetBytes(s.Value);
                var hashValue = new SHA1Managed().ComputeHash(byteArray);
                var array = engine.Create<Array>();

                for (int i = 0; i < hashValue.Length - 1; i++) {
                    array.Value.Add((Numeric)hashValue[i]);
                }

                return array;
            }
        }
    }
}
