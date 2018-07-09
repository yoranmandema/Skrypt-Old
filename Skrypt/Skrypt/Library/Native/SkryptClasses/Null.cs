namespace Skrypt.Library.Native
{
    partial class System
    {
        public class Null : SkryptObject
        {
            public Null()
            {
                Name = "null";
            }

            public override Boolean ToBoolean()
            {
                return false;
            }

            public override string ToString()
            {
                return "null";
            }
        }
    }
}