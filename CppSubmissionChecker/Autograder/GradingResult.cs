using Newtonsoft.Json;
namespace AutoGrading
{
	public class GradingResult
	{
		[JsonIgnore]
		public RubricItem? GradedItem { get; set; }
		[JsonProperty("comment")]
		public string? Comment { get; set; }
		[JsonProperty("percentage")]
		public float SuggestedPercentage { get; set; }
		[JsonProperty("markedcode")]
		public CodeMarking MarkedCodePart { get; set; } = new();
	}

}
