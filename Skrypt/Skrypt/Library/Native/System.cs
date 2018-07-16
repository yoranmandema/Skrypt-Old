using System;
using Skrypt.Execution;
using Skrypt.Engine;

namespace Skrypt.Library.Native
{
    [Constant, Static]
    public partial class System : SkryptObject
    {
        [Constant]
        public static SkryptObject Print(SkryptObject self, SkryptObject[] values)
        {
            var a = TypeConverter.ToAny(values, 0);

            Console.WriteLine(a);

            return new Null();
        }
    }
}