using CppSubmissionChecker_ViewModel.Viewmodels.TextLog;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CppSubmissionChecker_View.UserControls
{
    /// <summary>
    /// Interaction logic for TextLog.xaml
    /// </summary>
    public partial class TextLog : UserControl
    {
        private TextLog_VM? _viewModel;
        public TextLog()
        {
            this.DataContextChanged += TextLog_DataContextChanged;
            InitializeComponent();
          
        }

        private void TextLog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(_viewModel != null)
            {
                _viewModel.AppendedLine -= _viewModel_AppendedLine;
                _viewModel.Cleared -= _viewModel_Cleared;
            }

            _viewModel = DataContext as TextLog_VM;
            if (_viewModel != null)
            {
                _viewModel.AppendedLine += _viewModel_AppendedLine;
                _viewModel.Cleared += _viewModel_Cleared; 
            }
        }

        private void _viewModel_Cleared(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(console.Clear);
          
        }

        private void _viewModel_AppendedLine(object? sender, AppendLineEventArgs e)
        {
            Dispatcher.Invoke(() => { console.AppendText(e.Line + "\n"); });
        }

    }
}
