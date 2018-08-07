using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SkryptEditor.Output {
    public class TextBoxOutputter : TextWriter {
        TextEditor textBox = null;

        public TextBoxOutputter(TextEditor output) {
            textBox = output;
        }

        public override void Write(char value) {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() => {
                textBox.AppendText(value.ToString());
            }));
        }

        public void Clear() {
            textBox.Dispatcher.BeginInvoke(new Action(() => {
                textBox.Text = String.Empty;
            }));
        }

        public override Encoding Encoding {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
