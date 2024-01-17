using CppSubmissionChecker_ViewModel.DataClasses;
using CppSubmissionChecker_ViewModel.FraudDetection;
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
using System.Windows.Shapes;

namespace CppSubmissionChecker_View.Windows
{
    /// <summary>
    /// Interaction logic for FraudDetectionWindow.xaml
    /// </summary>
    public partial class FraudDetectionWindow : Window
    {
        public FraudDetectionWindow(MultiSubmissionZipArchive multiSubmissionZipArchive)
        {
            
            InitializeComponent();
            this.DataContext = new FraudDetection(multiSubmissionZipArchive);
        }
    }
}
