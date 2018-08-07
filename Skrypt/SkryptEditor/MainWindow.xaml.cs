using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using Skrypt.Engine;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.Win32;
using System.ComponentModel;

namespace SkryptEditor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        SkryptEngine _engine;
        string _documentPath;

        public MainWindow() {
            InitializeComponent();

            _engine = new SkryptEngine();

            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"..\..\SkryptHighlighting.xml");

            using (XmlTextReader reader = new XmlTextReader(new StreamReader(path))) {
                textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            try {
                _documentPath = Serializing.ReadFromBinaryFile<string>(Serializing.AssemblyDirectory + "/lastDocument.txt");
                textEditor.Text = File.ReadAllText(_documentPath);
            } catch {
                _documentPath = string.Empty;
            }

            Console.WriteLine(_documentPath);
        }

        private void SaveFile () {
            Console.WriteLine(_documentPath);

            if (_documentPath == string.Empty) {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog {
                    InitialDirectory = "c:\\",
                    Filter = "skt files (*.skt)|*.skt|All files (*.*)|*.*",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                saveFileDialog1.ShowDialog();
                _documentPath = saveFileDialog1.FileName;
            }

            if (_documentPath != string.Empty)
                File.WriteAllText(_documentPath, textEditor.Text);
        }

        private void OpenFile() {
            Console.WriteLine("Calling open file");

            OpenFileDialog openFileDialog1 = new OpenFileDialog {
                InitialDirectory = _documentPath,
                Filter = "skt files (*.skt)|*.skt|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            openFileDialog1.ShowDialog();

            Console.WriteLine(openFileDialog1.FileName);

            if (openFileDialog1.FileName != string.Empty) {
                _documentPath = openFileDialog1.FileName;

                if (_documentPath != string.Empty)
                    textEditor.Text = File.ReadAllText(_documentPath);
            }
        }


        private void OnNew(object sender, RoutedEventArgs e) {
            textEditor.Text = string.Empty;
            _documentPath = string.Empty;
        }

        private void OnExit(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnRun(object sender, RoutedEventArgs e) {
            Console.Clear();

            if (_documentPath != string.Empty)
                File.WriteAllText(_documentPath, textEditor.Text);

            var engine = new SkryptEngine();
            engine.AddClass(typeof(Canvas.Drawing));
            var code = textEditor.Text;

            try {
                engine.Parse(code);
            }
            catch (Exception exception) {

                if (exception.GetType() == typeof(SkryptException)) {
                    if (((SkryptException)exception).Token != null)
                        textEditor.CaretOffset = ((SkryptException)exception).Token.Start;
                }
            }
        }

        private void OnSave(object sender, RoutedEventArgs e) {
            SaveFile();
        }

        private void OnOpen(object sender, RoutedEventArgs e) {
            OpenFile();
        }

        private void Window_Exit(object sender, CancelEventArgs e) {
            if (_documentPath != string.Empty)
                File.WriteAllText(_documentPath, textEditor.Text);

            Serializing.WriteToBinaryFile<string>(Serializing.AssemblyDirectory + "/lastDocument.txt", _documentPath);
        }
    }
}
