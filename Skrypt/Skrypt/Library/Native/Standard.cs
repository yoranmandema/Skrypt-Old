using System;

namespace Skrypt.Library.Native
{
    public partial class StandardMethods
    {
        public static SkryptObject Print(SkryptObject[] values)
        {
            Console.WriteLine(values[0]);

            return new System.Void();
        }
    }
}