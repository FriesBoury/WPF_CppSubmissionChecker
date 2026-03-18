using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.Viewmodels;
using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;
using System.IO.Compression;

namespace CppSubmissionChecker_ViewModel.DataClasses
{
	public partial class MultiSubmissionZipArchive : ViewmodelBase
	{
		public event Action<Exception> ExceptionFired;

		public event Action<BatchOperationStats> BatchOperationStarted;
		//Private fields
		private const string _subDirName = "SubmissionChecker";
		private ZipArchive _zipArchive;
		private readonly SubmissionFactory _submissionFactory;
		private List<StudentSubmission> _studentSubmissions;
		private StudentSubmission? _selectedStudentSubmission;
		private MarkedFileTracker _markedFileTracker;


		public AutoGraderViewModel AutoGraderVM { get; }
		//Public Properties
		public MarkedFileTracker MarkedFileTracker => _markedFileTracker;
		public bool HasData => _studentSubmissions.Count > 0;
		public bool Loading { get; private set; }

		public float LoadingProgress { get; private set; }
		public string ArchiveName { get; }

		public bool CanBatchOpen => StudentSubmissions.Any(s => s is StudentSubmission_Unity || s is StudentSubmission_Unreal);
		public string BatchOpenText => _submissionFactory.SelectedType.Contains("Unity") ? "Batch open in Unity" : "Batch build Unreal projects";
		public AsyncRelayCommand BatchCommand { get; private set; }
		public bool HasSelectedSubmission => _selectedStudentSubmission != null && !Loading;


		[ObservableProperty]
		private int _batchStartIndex;
		[ObservableProperty]
		private int _batchCount;


		[ObservableProperty]
		private bool _rubricOpened = false;

		[ObservableProperty]
		private DateTime? _submissionActivatedTimeStamp = null;

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
				StudentSubmission.ActiveSubmission = _selectedStudentSubmission;
			}
		}

		[RelayCommand]
		private void ToggleRubric()
		{
			RubricOpened = !RubricOpened;
		}
		async Task BatchBuildUnreal()
		{

			BatchOperationStats operationStats = new BatchOperationStats("Batch building Unreal projects.");
			var submissionsToDo = StudentSubmissions.Skip(BatchStartIndex).Take(BatchCount);

			operationStats.NumActionsToDo = submissionsToDo.Count();

			BatchOperationStarted?.Invoke(operationStats);

			foreach (var submission in submissionsToDo)
			{
				operationStats.ActionsStarted++;
				if (submission is not StudentSubmission_Unreal ss_unreal)
				{
					operationStats.ActionsCompleted++;
					continue;
				}
				SelectedSubmission = submission;
				ss_unreal.SelectedOutputTabIndex = 0;
				await ExtractSubmission(submission);
				await ss_unreal.BuildAndRun_Unreal(false);
				operationStats.ActionsCompleted++;

			}

			operationStats.OnCompleted();
		}
		async Task BatchOpenInUnity()
		{
			BatchOperationStats operationStats = new BatchOperationStats("Batch opening unity projects.");
			var submissionsToDo = StudentSubmissions.Skip(BatchStartIndex).Take(BatchCount);

			operationStats.NumActionsToDo = submissionsToDo.Count();

			BatchOperationStarted?.Invoke(operationStats);

			foreach (var submission in submissionsToDo)
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
			SubmissionActivatedTimeStamp = DateTime.UtcNow;
		}

		//Constructor
		public MultiSubmissionZipArchive(ZipArchive archive, string archiveName, SubmissionFactory submissionFactory)
		{
			_zipArchive = archive;
			ArchiveName = archiveName;
			_submissionFactory = submissionFactory;
			_studentSubmissions = new List<StudentSubmission>();
			_markedFileTracker = new MarkedFileTracker(Preferences.TempFolderPath);
			AutoGraderVM = new AutoGraderViewModel(this);
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

			if (_submissionFactory.SelectedType.Contains("Unity"))
			{
				BatchCommand = new AsyncRelayCommand(BatchOpenInUnity);
			}
			else if (_submissionFactory.SelectedType.Contains("Unreal"))
			{
				BatchCommand = new AsyncRelayCommand(BatchBuildUnreal);
			}

			BatchStartIndex = 0;
			BatchCount = StudentSubmissions.Count;

		}

		private void SetLoadingProgress(float progress)
		{
			LoadingProgress = progress * 100;
			OnPropertyChanged(nameof(LoadingProgress));
		}

		async Task UnloadSubmission(StudentSubmission submission)
		{
			await submission.Unload();
			SubmissionActivatedTimeStamp = null;
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
