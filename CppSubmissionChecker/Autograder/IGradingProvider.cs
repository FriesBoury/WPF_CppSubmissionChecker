namespace AutoGrading
{
	public interface IGradingProvider
	{
		Task<GradingResult?> GradeAsync(Rubric rubric, RubricItem item, List<string?> contentFilePaths);
	}

}
