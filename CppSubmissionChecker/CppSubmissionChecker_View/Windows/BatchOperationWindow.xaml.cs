using CppSubmissionChecker_ViewModel.DataClasses;
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
	/// Interaction logic for BatchOperationWindow.xaml
	/// </summary>
	public partial class BatchOperationWindow : Window
	{
		public BatchOperationWindow()
		{
			this.DataContextChanged += BatchOperationWindow_DataContextChanged;
			InitializeComponent();
		}

		private void BatchOperationWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if(this.DataContext is BatchOperationStats stats)
			{
				stats.Completed += Stats_Completed;
			}
		}

		private void Stats_Completed(object? sender, EventArgs e)
		{
			this.Close();
		}
	}
}
