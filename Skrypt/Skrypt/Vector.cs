using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library;
using Skrypt.Library.Native;
using Classes = Skrypt.Library.Native.System;
using Skrypt.Execution;

namespace Skrypt {

    [Constant]
    public class Vector : SkryptType {
        static Vector ToVector (SkryptObject[] args, int index) {
            if (index > args.Length - 1) return new Vector();

            return (Vector)args[index];
        }

        public new List<Operation> Operations = new List<Operation>
        {
            new Operation("add", typeof(Vector), typeof(Vector),
                input =>
                {
                    var a = ToVector(input, 0);
                    var b = ToVector(input, 1);

                    return new Vector(
                        a.X + b.X,
                        a.Y + b.Y,
                        b.Z + b.Z
                        );
                }),
        };

        public Classes.Numeric X;
        public Classes.Numeric Y;
        public Classes.Numeric Z;

        public Vector() {
            Name = "vector";
            CreateCopyOnAssignment = true;
        }

        public Vector(double x, double y, double z) {
            Name = "vector";
            X = x;
            Y = y;
            Z = z;
            CreateCopyOnAssignment = true;
        }

        public static SkryptObject Constructor(SkryptObject self, SkryptObject[] input) {
            var x = TypeConverter.ToNumeric(input, 0);
            var y = TypeConverter.ToNumeric(input, 1);
            var z = TypeConverter.ToNumeric(input, 2);

            var vec = (Vector)self;

            vec.X = x;
            vec.Y = y;
            vec.Z = z;

            return new SkryptObject();
        }

        [Instance, Getter] // Get X component
        public static SkryptObject _X(SkryptObject self, SkryptObject[] values) {
            return ((Vector)self).X;
        }

        [Instance, Getter] // Get Y component
        public static SkryptObject _Y(SkryptObject self, SkryptObject[] values) {
            return ((Vector)self).Y;
        }

        [Instance, Getter] // Get Z component
        public static SkryptObject _Z(SkryptObject self, SkryptObject[] values) {
            return ((Vector)self).Z;
        }

        [Instance, Getter] // Get length of the vector
        public static SkryptObject _Length(SkryptObject self, SkryptObject[] values) {
            var e = (Math.Pow(((Vector)self).X, 2) + Math.Pow(((Vector)self).Y, 2) + Math.Pow(((Vector)self).Z, 2));
            var r = Math.Sqrt(e);

            return (Classes.Numeric)r;
        }

        public override string ToString() {
            return $"Vector({X},{Y},{Z})";
        }
    }
}
