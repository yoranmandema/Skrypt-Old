using System;
using System.Collections.Generic;
using Skrypt.Library;
using Skrypt.Library.Native;
using Classes = Skrypt.Library.Native.System;
using Skrypt.Execution;
using Skrypt.Engine;

namespace SkryptEditor.Canvas {
    [Constant]
    public class Drawing : SkryptObject {
        [Constant, Static]
        public class Color : SkryptType {
            public static Color ToColor(SkryptObject[] args, int index) {
                if (index > args.Length - 1) return new Color();

                return (Color)args[index];
            }

            public Classes.Numeric R { get; set; } = 0;
            public Classes.Numeric G { get; set; } = 0;
            public Classes.Numeric B { get; set; } = 0;

            public static SkryptObject Constructor(SkryptEngine engine, SkryptObject self, SkryptObject[] input) {
                var r = TypeConverter.ToNumeric(input, 0);
                var g = TypeConverter.ToNumeric(input, 1);
                var b = TypeConverter.ToNumeric(input, 2);

                var col = (Color)self;

                col.R = r;
                col.G = g;
                col.B = b;

                return col;
            }
        }

        [Constant, Static]
        public class Canvas : SkryptType {
            static Canvas ToCanvas(SkryptObject[] args, int index) {
                if (index > args.Length - 1) return new Canvas();

                return (Canvas)args[index];
            }

            public CanvasWindow Window;
            public Classes.Numeric Width { get; set; } = 0;
            public Classes.Numeric Height { get; set; } = 0;

            [Constant]
            public SkryptObject SetPixel(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = (Canvas)self;
                var x = TypeConverter.ToNumeric(values, 0);
                var y = TypeConverter.ToNumeric(values, 1);
                var c = Color.ToColor(values, 2);

                s.Window.SetPixel((int)x, (int)y,System.Drawing.Color.FromArgb((int)c.R, (int)c.B, (int)c.G));

                return s;
            }

            [Constant]
            public SkryptObject Draw(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = (Canvas)self;

                s.Window.Draw();

                return s;
            }

            [Constant]
            public static SkryptObject Constructor(SkryptEngine engine, SkryptObject self, SkryptObject[] values) {
                var s = (Canvas)self;
                var w = TypeConverter.ToNumeric(values,0);
                var h = TypeConverter.ToNumeric(values,1);

                s.Window = new CanvasWindow((int)w, (int)h);
                s.Window.Show();
                s.Window.Width = 500;
                s.Window.Height = 500;

                s.Width = w;
                s.Height = h;

                return (Canvas)self;
            }
        }
    }
}
