using CppSubmissionChecker_ViewModel.DataClasses;
using CppSubmissionChecker_ViewModel.Interfaces;
using CppSubmissionChecker_ViewModel.Viewmodels;
using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel
{

	public class SubmissionCheckerViewModel : ViewmodelBase
	{
		public event Action<Exception>? ExceptionFired;
		private MultiSubmissionZipArchive? _selectedArchive;
		public bool HasData => _selectedArchive != null;

		public SubmissionFactory SubmissionFactory { get; private set; }



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
			}
		}

		public void LoadArchive(ZipArchive zipArchive)
		{
			if (SelectedArchive != null)
			{
				SelectedArchive.ExceptionFired -= SelectedArchive_ExceptionFired;
			}
			SelectedArchive = new MultiSubmissionZipArchive(zipArchive, SubmissionFactory);
			SelectedArchive.ExceptionFired += SelectedArchive_ExceptionFired;
		}

		private void SelectedArchive_ExceptionFired(Exception obj)
		{
			ExceptionFired?.Invoke(obj);
		}
	}
}
