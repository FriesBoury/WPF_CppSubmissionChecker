using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CppSubmissionChecker_View.Utilities
{

	public class StringIndexHelper
	{
		public static int GetAbsoluteIndex(string input, int lineIndex, int characterIndex)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));
			if (lineIndex < 0 || characterIndex < 0) throw new ArgumentOutOfRangeException("Indices must be non-negative.");

			// Split the input into lines using all common line separators
			var lines = Regex.Split(input, @"\r\n|\n|\r");

			if (lineIndex >= lines.Length)
				lineIndex = lines.Length - 1;

			if (characterIndex > lines[lineIndex].Length)
				characterIndex = lines[lineIndex].Length - 1;

			int absoluteIndex = 0;
			for (int i = 0; i < lineIndex; i++)
			{
				absoluteIndex += lines[i].Length;

				// Add the length of the line separator
				int separatorLength = GetLineSeparatorLength(input, absoluteIndex);
				absoluteIndex += separatorLength;
			}

			absoluteIndex += characterIndex;
			return absoluteIndex;
		}

		private static int GetLineSeparatorLength(string input, int index)
		{
			if (index >= input.Length) return 0;
			if (input[index] == '\r')
			{
				if (index + 1 < input.Length && input[index + 1] == '\n')
					return 2; // \r\n
				return 1; // \r
			}
			if (input[index] == '\n') return 1; // \n
			return 0;
		}
	}

}
