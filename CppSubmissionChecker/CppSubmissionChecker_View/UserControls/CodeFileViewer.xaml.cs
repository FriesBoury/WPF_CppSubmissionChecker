using CppSubmissionChecker_ViewModel;
using ICSharpCode.AvalonEdit;
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

namespace CppSubmissionChecker_View.UserControls
{
    /// <summary>
    /// Interaction logic for CodeFileViewer.xaml
    /// </summary>
    public partial class CodeFileViewer : UserControl
    {
        private CodeFileViewer_VM? _viewModel;
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

        private void _fileTxt_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is string fileContent && sender is TextEditor txtEditor)
            {
                txtEditor.Text = fileContent; 
            }
        }

        private void _fileTxt_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextEditor txtEditor && txtEditor.DataContext is string fileContent)
            {
                txtEditor.Text = fileContent;
            }
        }
    }
}
