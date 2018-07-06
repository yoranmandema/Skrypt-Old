using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Execution;

namespace Skrypt.Library.Native {
    public partial class System {
        public static SkryptObject print(SkryptObject[] Values) {
            var a = TypeConverter.ToAny(Values, 0);

            Console.WriteLine(a);

            return new Void();
        }      
    }
}
