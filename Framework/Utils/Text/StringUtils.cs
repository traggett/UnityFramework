using UnityEngine;
using System;
using System.Collections.Generic;

namespace Framework
{
	namespace Utils
	{
		public static class StringUtils
		{
			//To do remove empties!
			public static int GetNumberOfLines(string text)
			{
				int numLines = 1;

				if (!string.IsNullOrEmpty(text))
				{
					foreach (char c in text)
					{
						if (c == '\n')
						{
							numLines++;
						}
					}
				}

				return numLines;
			}

			public static string[] SplitIntoLines(string text)
			{
				return text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			}

			public static void WrapText(string text, int maxCharactersPerLine, out string[] wrappedTextLines, out int numLines)
			{
				int charCount = 0;
				int lastSpace = -1;
				int lastLine = 0;
				numLines = 1;
				List<string> wrappedLines = new List<string>();

				for (int i = 0; i < text.Length; i++)
				{
					charCount++;

					if (text[i] == '\n')
					{
						wrappedLines.Add(text.Substring(lastLine, i - lastLine));
						lastLine = i + 1;
						lastSpace = -1;
						charCount = 0;
						numLines++;
						continue;
					}

					if (text[i] == ' ')
					{
						lastSpace = i;
					}

					if (charCount > maxCharactersPerLine)
					{
						if (lastSpace != -1)
						{
							wrappedLines.Add(text.Substring(lastLine, lastSpace - lastLine));
							i = lastSpace + 1;
							lastSpace = -1;
						}
						else
						{
							wrappedLines.Add(text.Substring(lastLine, i - lastLine));
						}

						lastLine = i;
						charCount = 0;
						numLines++;
					}
				}

				if (charCount > 0)
				{
					wrappedLines.Add(text.Substring(lastLine, text.Length - lastLine));
				}

				wrappedTextLines = wrappedLines.ToArray();
			}

			public static void WrapText(string text, int maxCharactersPerLine, out string wrappedText, out int numLines)
			{
				int charCount = 0;
				int lastSpace = -1;
				numLines = 1;
				char[] charArray = text.ToCharArray();

				for (int i = 0; i < charArray.Length; i++)
				{
					charCount++;

					if (text[i] == '\n')
					{
						lastSpace = -1;
						charCount = 0;
						numLines++;
						continue;
					}

					if (charArray[i] == ' ')
					{
						lastSpace = i;
					}

					if (charCount > maxCharactersPerLine)
					{
						if (lastSpace != -1)
						{
							charArray[lastSpace] = '\n';
							i = lastSpace + 1;
							lastSpace = -1;
						}
						else
						{
							char[] newArray = new char[charArray.Length + 1];
							Array.Copy(charArray, newArray, i);
							newArray[i] = '\n';
							Array.Copy(charArray, i, newArray, i + 1, charArray.Length - i);
							charArray = newArray;
						}

						charCount = 0;
						numLines++;
					}
				}

				wrappedText = new string(charArray);
			}

			public static string[] GetLines(string text)
			{
				string[] lines = null;

				if (text != null)
					lines = text.Split('\n');

				return lines;
			}

			public static string GetFirstLine(string text)
			{
				string[] lines = GetLines(text);

				if (lines != null && lines.Length > 0)
				{
					return lines[0];
				}

				return text;
			}

			public static float GetTextWidth(TextMesh textMesh, string text)
			{
				float characterWidth = 0.0f;
				CharacterInfo characterInfo;

				for (int i = 0; i < text.Length; i++)
				{
					textMesh.font.GetCharacterInfo(text[i], out characterInfo);
					characterWidth += (float)characterInfo.advance;
				}

				//Have to divide by ten to get height in world units (grr stupid unity!!)
				return characterWidth * textMesh.characterSize * 0.1f;
			}

			public static float GetTextHeight(TextMesh textMesh)
			{
				//Have to divide by ten to get height in world units (grr stupid unity!!)
				return (float)textMesh.font.lineHeight * textMesh.characterSize * 0.1f;
			}

			public static float GetTextHeight(TextMesh textMesh, string text)
			{
				return GetTextHeight(textMesh) * (float)GetNumberOfLines(text);
			}

			public static string ColorToHex(Color32 color)
			{
				string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
				return hex;
			}

			public static Color HexToColor(string hex)
			{
				byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
				byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
				byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
				return new Color32(r, g, b, a);
			}

			public static string FromCamelCase(string camelCase)
			{
				List<char> chars = new List<char>();

				char[] charString = camelCase.ToCharArray();
				for (int i = 0; i < charString.Length; i++)
				{
					if (i == 0)
					{
						chars.Add(char.ToUpper(charString[i]));
					}
					else
					{
						//Check for two capitals in a row?
						if (char.IsUpper(charString[i]))
						{
							chars.Add(' ');
							chars.Add(charString[i]);
						}
						else
							chars.Add(charString[i]);
					}
				}

				return new string(chars.ToArray());
			}

			public static string FromPropertyCamelCase(string propertyName)
			{
				string name = propertyName;

				if (name.StartsWith("_"))
					name = name.Substring(1);
				
				return FromCamelCase(name);
			}
			
			public static string RemoveRichText(string source)
			{
				char[] array = new char[source.Length];
				int arrayIndex = 0;
				bool inside = false;

				for (int i = 0; i < source.Length; i++)
				{
					char let = source[i];
					if (let == '<')
					{
						inside = true;
						continue;
					}
					if (let == '>')
					{
						inside = false;
						continue;
					}
					if (!inside)
					{
						array[arrayIndex] = let;
						arrayIndex++;
					}
				}
				return new string(array, 0, arrayIndex);
			}
		}
	}
}