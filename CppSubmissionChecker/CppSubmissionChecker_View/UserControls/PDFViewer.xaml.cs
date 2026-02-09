using CppSubmissionChecker_ViewModel.Viewmodels.FilePreview;
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
	/// Interaction logic for PDFViewer.xaml
	/// </summary>
	public partial class PDFViewer : UserControl
	{
		private PDFFile_VM? _viewModel;
		public PDFViewer()
		{
			this.DataContextChanged += PDFViewer_DataContextChanged;
			InitializeComponent();
		}

		private void PDFViewer_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (_viewModel == DataContext) return;
			if(_viewModel != null)
			{
				_viewModel.PropertyChanged -= PDFViewer_PropertyChanged;
			}

			_viewModel = DataContext as PDFFile_VM;
			if (_viewModel == null) return;
			_viewModel.PropertyChanged += PDFViewer_PropertyChanged;

			UpdateSource(_viewModel.Path);
		}

		private void PDFViewer_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_viewModel == null) return;
			if(e.PropertyName == nameof(_viewModel.Path))
			{
				UpdateSource(_viewModel.Path);
			}
		}


		void UpdateSource(string path)
		{
			Browser.Navigate(path);
		}
	}
}
