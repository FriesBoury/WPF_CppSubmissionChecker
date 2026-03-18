using CppSubmissionChecker_ViewModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using System.IO;
using System.Windows;



namespace CppSubmissionChecker_View.Windows
{
	/// <summary>
	/// Interaction logic for PreferencesWindow.xaml
	/// </summary>
	public partial class PreferencesWindow : Window
	{
		private PreferencesWindow_VM? _viewModel;
		public PreferencesWindow()
		{
			this.Loaded += PreferencesWindow_Loaded;
			InitializeComponent();
		}

		protected override void OnClosing(CancelEventArgs e)
		{

			if (_viewModel == null || !_viewModel.IsValid)
			{
				if (MessageBox.Show("Preferences are not configured correctly. Do you want to exit the application?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
				{
					e.Cancel = true;
				}

			}
			base.OnClosing(e);
		}

		private void PreferencesWindow_Loaded(object sender, RoutedEventArgs e)
		{
			_viewModel = DataContext as PreferencesWindow_VM;
		}

		private void SelectVSLocation_Click(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.IsFolderPicker = true;
			openFileDialog.InitialDirectory = "%ProgramFiles%";
			openFileDialog.Title = "Select Visual Studio Directory";

			if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				string fileName = openFileDialog.FileName;
				_viewModel?.SetVisualstudioFolder(fileName);
			}
		}

		private void SelectTempFolder_Click(object sender, RoutedEventArgs e)
		{
			if (_viewModel == null)
				return;
			CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.IsFolderPicker = true;
			openFileDialog.InitialDirectory = "%ProgramFiles%";
			openFileDialog.Title = "Select Temp Directory";




			if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				string fileName = openFileDialog.FileName;
				_viewModel.TempFolderPath = fileName;
			}
		}
		private void SelectRiderInstallation_click(object sender, RoutedEventArgs e)
		{
			if (_viewModel == null)
				return;
			CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.IsFolderPicker = true;
			openFileDialog.InitialDirectory = "%ProgramFiles%";
			openFileDialog.Title = "Select Rider Directory";

			if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				string fileName = openFileDialog.FileName;
				_viewModel?.SetRiderFolder(fileName);
			}
		}
		private void SelectUnrealInstallation_click(object sender, RoutedEventArgs e)
		{
			if (_viewModel == null)
				return;
			CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.IsFolderPicker = true;
			openFileDialog.InitialDirectory = "%ProgramFiles%";
			openFileDialog.Title = "Select Unreal Directory";

			if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				string fileName = openFileDialog.FileName;
				_viewModel?.SetUnrealFolder(fileName);
			}
		}
		private void SelectUnityInstallation_Click(object sender, RoutedEventArgs e)
		{
			if (_viewModel == null)
				return;
			CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
			openFileDialog.Multiselect = false;
			openFileDialog.IsFolderPicker = true;
			openFileDialog.InitialDirectory = "%ProgramFiles%";
			openFileDialog.Title = "Select Unity Directory";

			if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
			{

				string fileName = openFileDialog.FileName;
				fileName += "\\Unity.exe";

				if (File.Exists(fileName))
				{
					_viewModel.UnityInstallation = fileName;
				}
			}
		}
	}
}
