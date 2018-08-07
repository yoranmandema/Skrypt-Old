using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
//using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace SkryptEditor {
    public static class ConvertBitmapToBitmapImage {
        /// <summary>
        /// Takes a bitmap and converts it to an image that can be handled by WPF ImageBrush
        /// </summary>
        /// <param name="src">A bitmap image</param>
        /// <returns>The image as a BitmapImage for WPF</returns>
        public static BitmapImage Convert(Bitmap src) {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CanvasWindow : Window {

        public CanvasWindow(int w, int h) {
            InitializeComponent();
            _image = new Bitmap(w,h);
        }

        public void SetPixel (int x, int y, System.Drawing.Color color) {
            _image.SetPixel(x, y, color);
        }

        public void Draw () {
            CanvasImage.Source = ConvertBitmapToBitmapImage.Convert(_image);
        }

        private Bitmap _image;
    }
}
