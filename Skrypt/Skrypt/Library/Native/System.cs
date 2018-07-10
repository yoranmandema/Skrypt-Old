using System;
using Skrypt.Execution;

namespace Skrypt.Library.Native
{
    public partial class System
    {
        [Constant]
        public static SkryptObject Print(SkryptObject self, SkryptObject[] values)
        {
            var a = TypeConverter.ToAny(values, 0);

            Console.WriteLine(a);

            return new Void();
        }
    }
}