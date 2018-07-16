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
        static Vector ToVector(SkryptObject[] args, int index) {
            if (index > args.Length - 1) return new Vector(0, 0, 0);

            return (Vector)args[index];
        }

        public new List<Operation> Operations = new List<Operation>
        {
            new Operation("add", typeof(Vector), typeof(Vector),
                input =>
                {
                    var a = ToVector(input, 0);
                    var b = ToVector(input, 1);

                    return new Vector(a.X + b.X,a.Y + b.Y,a.Z + b.Z);
                }),
            new Operation("add", typeof(Vector), typeof(Classes.Numeric),
                input =>
                {
                    var a = ToVector(input, 0);
                    var b = TypeConverter.ToNumeric(input, 1);

                    return new Vector(a.X + b,a.Y + b,a.Z + b);
                }),
            new Operation("add", typeof(Classes.Numeric), typeof(Vector),
                input =>
                {
                    var a = TypeConverter.ToNumeric(input, 0);
                    var b = ToVector(input, 1);

                    return new Vector(a + b.X,a + b.Y,a + b.Z);
                }),
            new Operation("multiply", typeof(Vector), typeof(Vector),
                input =>
                {
                    var a = ToVector(input, 0);
                    var b = ToVector(input, 1);

                    return new Vector(a.X * b.X,a.Y * b.Y,a.Z * b.Z);
                }),
            new Operation("multiply", typeof(Vector), typeof(Classes.Numeric),
                input =>
                {
                    var a = ToVector(input, 0);
                    var b = TypeConverter.ToNumeric(input, 1);

                    return new Vector(a.X * b,a.Y * b,a.Z * b);
                }),
            new Operation("multiply", typeof(Classes.Numeric), typeof(Vector),
                input =>
                {
                    var a = TypeConverter.ToNumeric(input, 0);
                    var b = ToVector(input, 1);

                    return new Vector(a * b.X,a * b.Y,a * b.Z);
                }),
        };

        public Classes.Numeric X { get; set; } = 0;
        public Classes.Numeric Y { get; set; } = 0;
        public Classes.Numeric Z { get; set; } = 0;
        public Classes.Numeric Length => length(this);
        public Classes.Numeric Length2 => length2(this);
        public Vector Normalized => normalize(this);

        public override bool CreateCopyOnAssignment => true;
        public override string Name => "vector";

        public Vector() {

        }

        public Vector(double x, double y, double z) {
            X = x;
            Y = y;
            Z = z;
        }

        private static double length2 (Vector v) {
            return (Math.Pow(v.X, 2) + Math.Pow(v.Y, 2) + Math.Pow(v.Z, 2));
        }

        private static double length(Vector v) {
            return Math.Sqrt(length2(v));
        }

        private static Vector normalize(Vector v) {
            return new Vector(v.X / length(v),v.Y / length(v),v.Z / length(v));
        }

        private static Vector Cross(Vector v1, Vector v2) {
            return new Vector(
                v1.Y * v2.Z - v1.Z * v2.Y,
                v1.Z * v2.X - v1.X * v2.Z,
                v1.X * v2.Y - v1.Y * v2.X
                );
        }

        private static double Dot (Vector v1, Vector v2) {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public override string ToString() {
            return $"Vector({X},{Y},{Z})";
        }

        [Constant]
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

        [Constant]
        public SkryptObject Cross(SkryptObject self, SkryptObject[] values) {
            return Cross((Vector)self, ToVector(values,0));
        }

        [Constant]
        public SkryptObject Dot(SkryptObject self, SkryptObject[] values) {
            return (Classes.Numeric)Dot((Vector)self, ToVector(values, 0));
        }
    }
}
