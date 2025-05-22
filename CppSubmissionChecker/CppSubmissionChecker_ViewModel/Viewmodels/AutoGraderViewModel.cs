using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGrading;
using Newtonsoft.Json;
using AutoGrading.DeepSeek;
using System.Collections.ObjectModel;
using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;
using System.ComponentModel;
using CppSubmissionChecker_ViewModel.DataClasses;
using System.Formats.Asn1;

namespace CppSubmissionChecker_ViewModel.Viewmodels
{
	public partial class RubricItemViewModel : ObservableObject
	{
		[ObservableProperty]
		private string _filePattern = "";
		[ObservableProperty]
		private string _itemInfo = "";
		[ObservableProperty]
		private float _score = 1;
		[ObservableProperty]
		private bool _grading = false;

		public Guid ItemId { get; } = Guid.NewGuid();

		public bool HasResult => Result != null;

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(GotoGradeCommand))]
		[NotifyPropertyChangedFor(nameof(HasResult))]
		private GradingResult? _result;

		public RubricItem Item => new RubricItem()
		{
			Id = ItemId,
			FileNamePattern = FilePattern,
			ItemInfo = ItemInfo,
			Score = Score
		};
		public RubricItemViewModel(RubricItem? item)
		{
			if (item == null) return;
			ItemId = item.Id;
			_filePattern = item.FileNamePattern ?? "";
			_itemInfo = item.ItemInfo ?? "";
			_score = item.Score;
		}
		public bool CanGoToGrade => Result?.MarkedCodePart != null;

		[RelayCommand(CanExecute = nameof(CanGoToGrade))]
		private void GotoGrade()
		{
			var matchingFilePath = Item.FileNamePattern.Split(',')
				.Select(StudentSubmission.ActiveSubmission.GetRelativeFile)
				.FirstOrDefault(f => Path.GetFileName(f) == Result.MarkedCodePart.FileName);
			if (string.IsNullOrEmpty(matchingFilePath)) return;

			StudentSubmission.ActiveSubmission.WatchCodeFile(matchingFilePath, _result.MarkedCodePart);

		}
	}

	public partial class RubricViewModel : ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<RubricItemViewModel> _items;
		[ObservableProperty]
		private string _info;
		[ObservableProperty]
		private string _topic;

		public Rubric Rubric => new Rubric()
		{
			Items = Items.Select(i => i.Item).ToList(),
			Info = Info,
			Topic = Topic
		};

		public RubricViewModel(Rubric? rubric)
		{
			_items = new(rubric?.Items?.Select(i => new RubricItemViewModel(i)) ?? []);
			_info = rubric?.Info ?? "";
			_topic = rubric?.Topic ?? "";

			if (!_items.Any())
			{
				_items.Add(new RubricItemViewModel(null));
			}
		}

		[RelayCommand]
		void AddItem()
		{
			Items.Add(new RubricItemViewModel(null));
		}
		[RelayCommand]
		void RemoveItem(RubricItemViewModel item)
		{
			Items.Remove(item);
		}
	}
	public partial class AutoGraderViewModel : ObservableObject, IRubricProvider, IFileContentProvider
	{
		public IDialogService? DialogService { get; set; }

		[ObservableProperty]
		RubricViewModel _rubricViewModel = new RubricViewModel(new Rubric());

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(StartGradingCommand))]
		private bool _isGrading;

		public bool CanStartGrading => !IsGrading;

		private AutoGrader _autoGrader;
		private readonly MultiSubmissionZipArchive _mainVM;

		[ObservableProperty]
		private bool _hasAPIKey = false;

		private string _apiKey = "";

		public AutoGraderViewModel(MultiSubmissionZipArchive mainVM)
		{
			_mainVM = mainVM;
			RefreshAPIkey();
			if (!HasAPIKey)
			{
				return;
			}

		}

		private bool ValidateAPIKey(string key)
		{
			if (string.IsNullOrEmpty(key)) return false;
			return (key.StartsWith("sk-"));
		}

		[RelayCommand]
		private void RefreshAPIkey()
		{
			_apiKey = Preferences.DeepSeekAPIKey;
			HasAPIKey = ValidateAPIKey(_apiKey);

			if (HasAPIKey && _autoGrader == null)
			{
				_autoGrader = new AutoGrader(this, new DeepSeekGrader(_apiKey), this);
			}

		}
		public AutoGraderViewModel()
		{

		}

		//Rubrics
		[RelayCommand]
		async Task LoadRubric()
		{
			if (DialogService == null) return;

			string? path = DialogService.OpenFile();
			if (string.IsNullOrWhiteSpace(path)) return;

			string contents = await File.ReadAllTextAsync(path);
			Rubric? rubric = JsonConvert.DeserializeObject<Rubric>(contents);
			if (rubric == null) return;
			RubricViewModel = new RubricViewModel(rubric);
		}

		[RelayCommand]
		async Task SaveRubric()
		{
			if (DialogService == null) return;

			string? path = DialogService.SaveFile();
			if (string.IsNullOrWhiteSpace(path)) return;

			string contents = JsonConvert.SerializeObject(RubricViewModel.Rubric, Formatting.Indented);
			await File.WriteAllTextAsync(path, contents);
		}

		public List<string?> GetFilePaths(string matchPattern)
		{
			if (_mainVM?.SelectedSubmission == null) return [];
			StudentSubmission submission = _mainVM.SelectedSubmission;

			string[] split = matchPattern.Split(',');
			return split.Select(s => submission.GetRelativeFile(s.Trim())).ToList();
		}

		public Rubric? GetRubric()
		{
			return RubricViewModel.Rubric;
		}

		[RelayCommand]
		private async Task GradeItem(RubricItemViewModel item)
		{
			await _autoGrader.GradeItemAsync(item.Item);
		}


		[RelayCommand(CanExecute = nameof(CanStartGrading))]

		private async Task StartGrading()
		{
			IsGrading = true;

			foreach (var item in RubricViewModel.Items)
			{
				item.Grading = true;
			}

			var results = await _autoGrader.GradeAllAsync();


			foreach (var result in results)
			{
				if (result?.GradedItem == null) continue;
				RubricItemViewModel? matchingVM = RubricViewModel.Items.FirstOrDefault(i => i.ItemId == result.GradedItem.Id);

				if (matchingVM == null) continue;


				matchingVM.Result = result;
				matchingVM.Grading = false;
			}
			foreach (var item in RubricViewModel.Items)
			{
				item.Grading = false;
			}

			IsGrading = false;


		}
	}
}
