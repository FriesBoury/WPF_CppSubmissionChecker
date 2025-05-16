namespace AutoGrading
{
	public class Rubric
	{
		public List<RubricItem> Items { get; set; } = new List<RubricItem>();
		public string Info { get; set; } = "";
		public string Topic { get; set; } = "";
		public float TotalScore => Items.Sum(i => i.Score);
	}

}
