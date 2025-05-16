namespace AutoGrading
{
	public class RubricItem
	{
		public Guid Id { get; set; }
		public float Score { get; set; }
		public string ItemInfo { get; set; } = "";
		public string FileNamePattern { get; set; } = "";
	}
	public class AutoGrader
	{
		private readonly IRubricProvider _rubricProvider;
		private readonly IGradingProvider _gradingProvider;
		private readonly IFileContentProvider _fileContentProvider;

		public AutoGrader(IRubricProvider rubricProvider, IGradingProvider gradingProvider, IFileContentProvider fileProvider)
		{
			_rubricProvider = rubricProvider;
			_gradingProvider = gradingProvider;
			_fileContentProvider = fileProvider;
		}

		public List<RubricItem> GetAllRubricItems()
		{
			return _rubricProvider.GetRubric()?.Items ?? new();
		}
		public async Task<List<GradingResult>> GradeAllAsync()
		{

			var results = new List<GradingResult>();
			var rubric = _rubricProvider.GetRubric();
			if (rubric == null) return results;


			List<Task<GradingResult?>> runningTasks = [];
			foreach (var item in rubric.Items)
			{
				var filePaths = _fileContentProvider.GetFilePaths(item.FileNamePattern);
				try
				{
					runningTasks.Add(_gradingProvider.GradeAsync(rubric, item, filePaths));
				}
				catch (Exception e)
				{

				}
			}

			try
			{
				await Task.WhenAll(runningTasks);
			}
			catch(Exception e)
			{

			}
			for (int idx = 0; idx < runningTasks.Count; ++idx)
			{

				var result = runningTasks[idx].Result;
				if (result != null)
				{
					result.GradedItem = rubric.Items[idx];
					results.Add(result);
				}
				else
				{
					results.Add(new() { Comment = "Failed to assess item.", GradedItem = rubric.Items[idx] });
				}
			}

			return results;

		}


	}

}
