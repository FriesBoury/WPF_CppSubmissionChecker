using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangChain.Providers;
using LangChain.Providers.DeepSeek;
using LangChain.Providers.OpenAI;
using Newtonsoft.Json;

namespace AutoGrading.DeepSeek
{
	public class DeepSeekGrader : IGradingProvider
	{
		private readonly DeepSeekModel _deepSeekModel;
		GradingResult exampleResult = new GradingResult()
		{
			Comment = "<comment message>",
			MarkedCodePart =
				new()
				{
					StartLine = 15,
					EndLine = 16,
					StartCharIndex = 4,
					EndCharIndex = 50,
					FileName = "<fileName>"
				},
			SuggestedPercentage = 0.5f

		};
		public DeepSeekGrader(string apiKey)
		{
			_deepSeekModel = new DeepSeekModel(new DeepSeekProvider(apiKey), DeepSeekModels.DeepSeekChat);
		}

		private Message _systemMessage;

		public async Task<GradingResult?> GradeAsync(Rubric rubric, RubricItem item, List<string?> contentFilePaths)
		{
			List<Message> messages = new List<Message>();

			messages.Add(CreateContextMessage(rubric, item));
			messages.Add(CreateContentsMessage(contentFilePaths));

			ChatRequest request = new ChatRequest() { Messages = messages };
			OpenAiChatSettings chatSettings = new() { MaxCompletionTokens = 400, UseStreaming = false };

			var response = await _deepSeekModel.GenerateAsync(request, chatSettings);
			if (response.LastMessage.HasValue)
			{
				string messageTrimmed = response.LastMessageContent.Replace("```json", "").Replace("```", "");
				GradingResult?  result = JsonConvert.DeserializeObject<GradingResult>(messageTrimmed);
				
				return result;
			}
			else
			{
				return null;
			}
		}

		private Message CreateContextMessage(Rubric rubric, RubricItem item)
		{
			string exampleResultJson = JsonConvert.SerializeObject(exampleResult, Formatting.Indented);
			string topicInfo = string.IsNullOrWhiteSpace(rubric.Topic) ? "" : $"about {rubric.Topic}";
			string message =
				$"You grade a student submission {topicInfo}." +
				"\nThe submission is provided as all relevant file contents, preceded by the the line \"FILE<filename>:\"" +
				$"\the response must be in JSON with the following format: \n{exampleResultJson}\n" +
				$"If the student submission is correct, the comment should be \"OK\"." +
				$"Only grade the following: \"{item.ItemInfo}\"";

			return new Message(message, MessageRole.System);
		}

		private Message CreateContentsMessage(List<string?> files)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var file in files)
			{
				if (file == null) continue;

				string fileName = Path.GetFileName(file);
				sb.AppendLine($"FILE<{fileName}>:");
				sb.Append(File.ReadAllText(file));
			}
			return new Message(sb.ToString(), MessageRole.Human);
		}

	}
}
