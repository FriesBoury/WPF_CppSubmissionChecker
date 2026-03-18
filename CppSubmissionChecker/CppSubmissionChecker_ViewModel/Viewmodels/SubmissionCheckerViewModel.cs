using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.DataClasses;
using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;
using System.IO.Compression;

namespace CppSubmissionChecker_ViewModel
{

	public partial class SubmissionCheckerViewModel : ViewmodelBase
	{
		public event Action<Exception>? ExceptionFired;
		private MultiSubmissionZipArchive? _selectedArchive;
		public bool HasData => _selectedArchive != null;


		public SubmissionFactory SubmissionFactory { get; private set; }

		public string WindowTitle => _selectedArchive == null ? "DAE Submission Checker" : _selectedArchive.ArchiveName + " - DAE Submission Checker";

		public SubmissionCheckerViewModel()
		{
			SubmissionFactory = new SubmissionFactory();
		}


		public MultiSubmissionZipArchive? SelectedArchive
		{
			get => _selectedArchive;
			set
			{
				_selectedArchive = value;
				OnPropertyChanged(nameof(SelectedArchive));
				OnPropertyChanged(nameof(HasData));
				OnPropertyChanged(nameof(WindowTitle));
			}
		}
		[RelayCommand]
		private void CloseArchive()
		{
			SelectedArchive = null;
		}

		public void LoadArchive(ZipArchive zipArchive, string fileName)
		{
			if (SelectedArchive != null)
			{
				SelectedArchive.ExceptionFired -= SelectedArchive_ExceptionFired;
			}
			SelectedArchive = new MultiSubmissionZipArchive(zipArchive, fileName, SubmissionFactory);
			SelectedArchive.ExceptionFired += SelectedArchive_ExceptionFired;
		}

		private void SelectedArchive_ExceptionFired(Exception obj)
		{
			ExceptionFired?.Invoke(obj);
		}
	}
}
