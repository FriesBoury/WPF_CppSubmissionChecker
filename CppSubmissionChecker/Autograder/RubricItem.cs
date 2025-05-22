namespace AutoGrading
{
	public class RubricItem
	{
		public Guid Id { get; set; }
		public float Score { get; set; }
		public string ItemInfo { get; set; } = "";
		public string FileNamePattern { get; set; } = "";
	}

}
