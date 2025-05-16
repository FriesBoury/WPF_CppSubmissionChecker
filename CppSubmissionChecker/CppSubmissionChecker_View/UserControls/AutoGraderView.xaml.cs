using CppSubmissionChecker_View.Utilities;
using CppSubmissionChecker_ViewModel.Viewmodels;
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
    /// Interaction logic for AutoGraderView.xaml
    /// </summary>
    public partial class AutoGraderView : UserControl
    {
        public AutoGraderView()
        {
            InitializeComponent();
        }

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
            if (this.DataContext is AutoGraderViewModel vm)
                vm.DialogService = new DialogService();
        }
    }
}
