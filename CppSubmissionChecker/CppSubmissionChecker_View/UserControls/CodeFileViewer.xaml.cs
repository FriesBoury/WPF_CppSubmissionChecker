using CppSubmissionChecker_ViewModel;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;
using System.Xml;

namespace CppSubmissionChecker_View.UserControls
{
    /// <summary>
    /// Interaction logic for CodeFileViewer.xaml
    /// </summary>
    public partial class CodeFileViewer : UserControl
    {
        private CodeFileViewer_VM? _viewModel;
        private static IHighlightingDefinition? _syntaxHighlighting = null;
        public CodeFileViewer()
        {
            this.DataContextChanged += CodeFileViewer_DataContextChanged;
            InitializeComponent();
           
        }

        private void CodeFileViewer_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as CodeFileViewer_VM;
        }

        public void OpenFile(string? path)
        {
            _viewModel?.AddCodeFile(path);
            _emptyTabControl.Visibility = Visibility.Hidden;
        }
       
        public void CloseAllFiles()
        {
            _viewModel?.Clear();
            _emptyTabControl.Visibility = Visibility.Visible;
        }

        private void File_Close(object sender, RoutedEventArgs e)
        {
            if(sender is FrameworkElement visual)
            {
                if(visual.DataContext is CodeFile_VM fileVm)
                {
                    fileVm.Close();
                    if(_viewModel.CodeFiles.Count == 0)
                    {
                        _emptyTabControl.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void LoadAvalonEditSyntaxHighlighting()
        {
            //if (_syntaxHighlighting != null) return;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = $"CppSubmissionChecker_View.Resources.CSharp_Dark.xml";

            using (Stream? s = assembly.GetManifestResourceStream(resourceName))
            {
                if (s == null)
                    return;

                using (XmlReader reader = XmlReader.Create(s, new XmlReaderSettings { }))
                {
                    _syntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);

                }
            }

        }
        private void _fileTxt_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadAvalonEditSyntaxHighlighting();
            if (e.NewValue is string fileContent && sender is TextEditor txtEditor)
            {
                txtEditor.Text = fileContent;
                txtEditor.SyntaxHighlighting = _syntaxHighlighting;
            }
        }

        private void _fileTxt_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAvalonEditSyntaxHighlighting();
            
            if (sender is TextEditor txtEditor && txtEditor.DataContext is string fileContent)
            {
                txtEditor.Text = fileContent;
                txtEditor.SyntaxHighlighting = _syntaxHighlighting;
                txtEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.White);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string? path = ((sender as FrameworkElement)?.DataContext as CodeFile_VM)?.Path;
            string? text = (sender as FrameworkElement)?.Parent.FindChild<TextEditor>()?.Text;

            if(!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(text))
            {
                File.WriteAllText(path, text);
            }
        }
    }
}
