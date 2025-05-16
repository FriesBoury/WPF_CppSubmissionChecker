namespace AutoGrading
{
	public interface IFileContentProvider
	{
		public List<string?> GetFilePaths(string matchPattern);
	}

}
