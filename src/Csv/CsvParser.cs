using System;
using System.Linq;
using System.Collections.Generic;

namespace Common.Data.Csv
{
	public static class CsvParser
	{
		public static string[] CsvSplit(this String source, char Delimiter)
		{
			List<string> splitString = new List<string>();
			List<int> slashesToRemove = null;
			CsvParseStateType state = CsvParseStateType.AtBeginningOfToken;
			char[] sourceCharArray = source.ToCharArray();
			int tokenStart = 0;
			int len = sourceCharArray.Length;

			for (int i = 0; i < len; ++i)
			{
				switch (state)
				{
					case CsvParseStateType.AtBeginningOfToken:
						if (sourceCharArray[i] == '"')
						{
							state = CsvParseStateType.InQuotedToken;
							slashesToRemove = new List<int>();
							continue;
						}
						if (sourceCharArray[i] == Delimiter)
						{
							splitString.Add("");
							tokenStart = i + 1;
							continue;
						}
						state = CsvParseStateType.InNonQuotedToken;
						continue;
					case CsvParseStateType.InNonQuotedToken:
						if (sourceCharArray[i] == Delimiter)
						{
							splitString.Add(source.Substring(tokenStart, i - tokenStart));
							state = CsvParseStateType.AtBeginningOfToken;
							tokenStart = i + 1;
						}
						continue;
					case CsvParseStateType.InQuotedToken:
						if (sourceCharArray[i] == '"')
						{
							state = CsvParseStateType.ExpectingComma;
							continue;
						}

						if (sourceCharArray[i] == '\\')
						{
							state = CsvParseStateType.InEscapedCharacter;
							slashesToRemove.Add(i - tokenStart);
							continue;
						}
						continue;
					case CsvParseStateType.ExpectingComma:
						if (sourceCharArray[i] != Delimiter)
						{
							throw new CsvParseException("Expecting delimiter");
						}

						string stringWithSlashes = source.Substring(tokenStart, i - tokenStart);

						foreach (int item in slashesToRemove.Reverse<int>())
						{
							stringWithSlashes = stringWithSlashes.Remove(item, 1);
						}

						splitString.Add(stringWithSlashes.Substring(1, stringWithSlashes.Length - 2));
						state = CsvParseStateType.AtBeginningOfToken;
						tokenStart = i + 1;
						continue;
					case CsvParseStateType.InEscapedCharacter:
						state = CsvParseStateType.InQuotedToken;
						continue;
				}
			}
			switch (state)
			{
				case CsvParseStateType.AtBeginningOfToken:
					splitString.Add("");
					return splitString.ToArray();
				case CsvParseStateType.InNonQuotedToken:
					splitString.Add(source.Substring(tokenStart, source.Length - tokenStart));
					return splitString.ToArray();
				case CsvParseStateType.InQuotedToken:
					throw new CsvParseException("Expecting ending quote");
				case CsvParseStateType.ExpectingComma:
					string stringWithSlashes = source.Substring(tokenStart, source.Length - tokenStart);
					
					foreach (int item in slashesToRemove.Reverse<int>())
					{
						stringWithSlashes = stringWithSlashes.Remove(item, 1);
					}
					
					splitString.Add(stringWithSlashes.Substring(1, stringWithSlashes.Length - 2));
					return splitString.ToArray();
				case CsvParseStateType.InEscapedCharacter:
					throw new CsvParseException("Expecting escaped character");
			}
			throw new CsvParseException("Unexpected error");
		}

		private enum CsvParseStateType
		{
			AtBeginningOfToken,
			InNonQuotedToken,
			InQuotedToken,
			ExpectingComma,
			InEscapedCharacter
		};

	}
}