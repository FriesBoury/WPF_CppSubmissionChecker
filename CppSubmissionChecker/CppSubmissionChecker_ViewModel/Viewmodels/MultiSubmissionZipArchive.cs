using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.Data;
using CppSubmissionChecker_ViewModel.Viewmodels;
using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CppSubmissionChecker_ViewModel.DataClasses
{
	public class MultiSubmissionZipArchive : ViewmodelBase
	{
		public event Action<Exception> ExceptionFired;

		public event Action<BatchOperationStats> BatchOperationStarted;
		//Private fields
		private const string _subDirName = "SubmissionChecker";
		private ZipArchive _zipArchive;
		private List<StudentSubmission> _studentSubmissions;
		private StudentSubmission? _selectedStudentSubmission;
		private MarkedFileTracker _markedFileTracker;



		//Public Properties
		public MarkedFileTracker MarkedFileTracker => _markedFileTracker;
		public bool HasData => _studentSubmissions.Count > 0;
		public bool Loading { get; private set; }

		public float LoadingProgress { get; private set; }

		public AsyncRelayCommand BatchOpenInUnityCommand { get; private set; }

		public bool HasSelectedSubmission => _selectedStudentSubmission != null && !Loading;

		public List<StudentSubmission> StudentSubmissions
		{
			get => _studentSubmissions; private set
			{
				_studentSubmissions = value;
				OnPropertyChanged(nameof(StudentSubmissions));
				OnPropertyChanged(nameof(HasData));
			}
		}
		public StudentSubmission? SelectedSubmission
		{
			get => _selectedStudentSubmission;
			set
			{
				SwapSubmission(value);
			}
		}

		async Task BatchOpenInUnity()
		{
			BatchOperationStats operationStats = new BatchOperationStats("Batch opening unity projects.");
			operationStats.NumActionsToDo = StudentSubmissions.Count;

			BatchOperationStarted?.Invoke(operationStats);

			foreach (var submission in StudentSubmissions)
			{
				operationStats.ActionsStarted++;
				if (submission is not StudentSubmission_Unity ss_unity)
				{
					operationStats.ActionsCompleted++;
					continue;
				}

				await ExtractSubmission(submission);
				await ss_unity.OpenProject(true);
				operationStats.ActionsCompleted++;

			}

			operationStats.OnCompleted();
		}

		async void SwapSubmission(StudentSubmission? value)
		{
			if (_selectedStudentSubmission != null)
			{
				await UnloadSubmission(_selectedStudentSubmission);
			}
			_selectedStudentSubmission = value;
			if (_selectedStudentSubmission != null)
			{
				await ExtractSubmission(_selectedStudentSubmission);
			}
			OnPropertyChanged(nameof(SelectedSubmission));
			OnPropertyChanged(nameof(HasSelectedSubmission));
		}

		//Constructor
		public MultiSubmissionZipArchive(ZipArchive archive, SubmissionFactory submissionFactory)
		{
			_zipArchive = archive;
			_studentSubmissions = new List<StudentSubmission>();
			_markedFileTracker = new MarkedFileTracker(Preferences.TempFolderPath);
			List<string> usedNames = new List<string>();
			foreach (var entry in archive.Entries)
			{
				if (entry == null)
				{
					continue;
				}
				string longName = entry.Name;

				string shortName = entry.Name;
				int separatorIndex = shortName.IndexOf('_');
				if (separatorIndex != -1)

				{
					shortName = shortName.Substring(0, separatorIndex);
				}

				if (usedNames.Contains(shortName))
				{
					if (int.TryParse(shortName.Substring(shortName.Length - 1, 1), out int result))
					{
						shortName = shortName.Substring(0, shortName.Length - 1) + (result + 1).ToString();
					}
					else
					{
						shortName += "_1";
					}
				}
				usedNames.Add(shortName);
				StudentSubmissions.Add(submissionFactory.CreateSubmission(longName, shortName, entry, _markedFileTracker));
			}

			StudentSubmissions = StudentSubmissions.OrderBy(x => x.StudentName).ToList();

			BatchOpenInUnityCommand = new AsyncRelayCommand(BatchOpenInUnity);
		}

		private void SetLoadingProgress(float progress)
		{
			LoadingProgress = progress * 100;
			OnPropertyChanged(nameof(LoadingProgress));
		}

		async Task UnloadSubmission(StudentSubmission submission)
		{
			await submission.Unload();
		}

		public async Task ExtractSubmission(StudentSubmission submission)
		{

			Loading = true;
			OnPropertyChanged(nameof(Loading));

			try
			{
				string fullPath = Path.GetFullPath(Path.Combine(Preferences.TempFolderPath, _subDirName));
				if (!Directory.Exists(fullPath))
				{
					Directory.CreateDirectory(fullPath);
				}
				await Task.Run(() =>
				{
					submission.ExtractToPath(fullPath, !Preferences.KeepSubmissions, SetLoadingProgress);
				});
			}
			catch (Exception e)
			{
				ExceptionFired?.Invoke(e);
			}

			Loading = false;
			OnPropertyChanged(nameof(Loading));
		}
	}
}
