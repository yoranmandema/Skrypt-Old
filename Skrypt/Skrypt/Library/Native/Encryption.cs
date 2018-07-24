using Sys = System;
using System.Collections.Generic;
using System.Linq;
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

                var byteArray = Encoding.ASCII.GetBytes(s.Value);
                var hashValue = new SHA256Managed().ComputeHash(byteArray);
                var str = Sys.BitConverter.ToString(hashValue).Replace("-", "");

                return engine.Create<String>(str);
            }

            [Constant]
            public static SkryptObject SHA1(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = TypeConverter.ToString(values, 0);

                var byteArray = Encoding.ASCII.GetBytes(s.Value);
                var hashValue = new SHA1Managed().ComputeHash(byteArray);
                var str = Sys.BitConverter.ToString(hashValue).Replace("-", "");

                return engine.Create<String>(str);
            }
        }
    }
}
