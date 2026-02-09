using CppSubmissionChecker_View.Windows;
using CppSubmissionChecker_ViewModel;
using CppSubmissionChecker_ViewModel.DataClasses;
using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;
using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace CppSubmissionChecker_View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private PreferencesWindow? _preferencesWindow = null;
		private SubmissionCheckerViewModel? _viewModel;

		private string[] _nameFilters = Array.Empty<string>();
		private CollectionView? _studentListView;
		public MainWindow()
		{
			this.DataContextChanged += MainWindow_DataContextChanged;
			this.Loaded += MainWindow_Loaded;
			this.Activated += MainWindow_Activated;
			InitializeComponent();

			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void Timer_Tick(object? sender, EventArgs e)
		{
			if (_viewModel?.SelectedArchive?.SubmissionActivatedTimeStamp == null)
			{
				Timer.Visibility = Visibility.Collapsed;
				return;
			}
			Timer.Visibility = Visibility.Visible;
			TimeSpan ts = DateTime.UtcNow - _viewModel.SelectedArchive.SubmissionActivatedTimeStamp.Value;
			Timer.Content = $"Submission was open for {ts.Hours:00}h:{ts.Minutes:00}m:{ts.Seconds:00}s";
		}

		private void MainWindow_Activated(object? sender, EventArgs e)
		{
			if (_preferencesWindow != null)
			{
				_preferencesWindow.Activate();
			}
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			CheckShowPreferences();
		}

		private void PreferencesWindow_Closed(object? sender, EventArgs e)
		{
			if (!Preferences.Validate())
			{
				this.Close();
			}
			_preferencesWindow = null;
			this.IsEnabled = true;
		}


		void CheckShowPreferences()
		{

			if (_preferencesWindow != null && _preferencesWindow.IsActive) { return; }
			if (!Preferences.Validate())
			{
				this.IsEnabled = false;
				_preferencesWindow = new PreferencesWindow();
				_preferencesWindow.Show();
				_preferencesWindow.Closed += PreferencesWindow_Closed;
				_preferencesWindow.Focus();
			}
			else
			{
				this.IsEnabled = true;
			}
		}
		private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_viewModel = this.DataContext as SubmissionCheckerViewModel;
			if (_viewModel == null)
				return;

			_viewModel.SetMainDispatcher(new ViewmodelDispatcher(Dispatcher));
			_viewModel.ExceptionFired += (exception) =>
			{
				MessageBox.Show("SubmissionChecker encountered an exception: " + exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			};

		}

		private void LoadZip_Click(object sender, RoutedEventArgs e)
		{

			if (_viewModel == null) return;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "zip files (.zip)|*.zip";
			openFileDialog.Multiselect = false;
			if (openFileDialog.ShowDialog().GetValueOrDefault())
			{
				string fileName = openFileDialog.FileName;
				if (!string.IsNullOrEmpty(fileName))
				{
					FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
					ZipArchive archive = new ZipArchive(fs);
					_viewModel.LoadArchive(archive, System.IO.Path.GetFileNameWithoutExtension(fileName));
				}
			}

		}

		private void Search_TextChanged(object sender, TextChangedEventArgs e)
		{
			_nameFilters = _studentFilterTxt.Text.ToLowerInvariant().Trim().Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
			if (_studentListView != null)
			{
				_studentListView.Refresh();
			}

		}

		private bool FilterSubmissionsByName(object obj)
		{
			if (_nameFilters.Length == 0) return true;

			if (obj is StudentSubmission submission)
			{
				string lowerSubmissionName = submission.StudentName.ToLowerInvariant();
				foreach (var filter in _nameFilters)
				{
					if (!lowerSubmissionName.Contains(filter))
					{
						return false;
					}
				}
			}
			return true;
		}

		private void _studentList_Loaded(object sender, RoutedEventArgs e)
		{
			if (_studentList.ItemsSource != null)
			{
				_studentListView = (CollectionView)CollectionViewSource.GetDefaultView(_studentList.ItemsSource);
				_studentListView.Filter = new Predicate<object>(FilterSubmissionsByName);
				_studentListView.Refresh();
			}
		}

		private void _studentList_SourceUpdated(object sender, DataTransferEventArgs e)
		{
			if (_studentList.ItemsSource != null)
			{
				_studentListView = (CollectionView)CollectionViewSource.GetDefaultView(_studentList.ItemsSource);
				_studentListView.Filter = new Predicate<object>(FilterSubmissionsByName);
				_studentListView.Refresh();
			}
		}

		private void _studentList_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (_studentList.DataContext is MultiSubmissionZipArchive ziparch)
			{
				ziparch.BatchOperationStarted += Ziparch_BatchOperationStarted;
				_studentListView = (CollectionView)CollectionViewSource.GetDefaultView(ziparch.StudentSubmissions);
				_studentListView.Filter = new Predicate<object>(FilterSubmissionsByName);
				_studentListView.Refresh();

			}

		}

		private void Ziparch_BatchOperationStarted(BatchOperationStats obj)
		{
			BatchOperationWindow window = new BatchOperationWindow();
			window.DataContext = obj;
			window.Show();
		}

		private void ShowPreferences_Click(object sender, RoutedEventArgs e)
		{
			_preferencesWindow = new PreferencesWindow();
			_preferencesWindow.Show();
			_preferencesWindow.Closed += PreferencesWindow_Closed;
			_preferencesWindow.Focus();
		}

		private void FraudDetection_Click(object sender, RoutedEventArgs e)
		{
			if (_viewModel?.SelectedArchive != null)
			{
				new FraudDetectionWindow(_viewModel.SelectedArchive).Show();
			}
		}
	}

}
