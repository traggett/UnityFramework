/**
 *
 * Copyright (c) 2016 Adriano Tinoco d'Oliveira Rezende
 * 
 * Based on the JSON parser by Patrick van Bergen
 * http://techblog.procurios.nl/k/news/view/14605/14863/how-do-i-write-my-own-parser-(for-json).html
 *
 * Changes made:
 * 
 * 	- Optimized parser speed (deserialize roughly near 3x faster than original)
 *  - Added support to handle lexer/parser error messages with line numbers
 *  - Added more fine grained control over type conversions during the parsing
 *  - Refactory API (Separate Lexer code from Parser code and the Encoder from Decoder)
 *
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace Engine
{
	namespace JSON
	{
		class Lexer
		{
			public enum Token
			{
				None,
				Null,
				True,
				False,
				Colon,
				Comma,
				String,
				Number,
				CurlyOpen,
				CurlyClose,
				SquaredOpen,
				SquaredClose,
			};

			public bool hasError
			{
				get
				{
					return !success;
				}
			}

			public int lineNumber
			{
				get;
				private set;
			}

			public bool parseNumbersAsFloat
			{
				get;
				set;
			}

			private char[] json;
			private int index = 0;
			private bool success = true;
			private char[] stringBuffer = new char[4096];

			public Lexer(string text)
			{
				Reset();

				json = text.ToCharArray();
				parseNumbersAsFloat = false;
			}

			public void Reset()
			{
				index = 0;
				lineNumber = 1;
				success = true;
			}

			public string ParseString()
			{
				int idx = 0;
				StringBuilder builder = null;

				SkipWhiteSpaces();

				// "
				char c = json[index++];

				bool failed = false;
				bool complete = false;

				while (!complete && !failed)
				{
					if (index == json.Length)
						break;

					c = json[index++];
					if (c == '"')
					{
						complete = true;
						break;
					}
					else if (c == '\\')
					{
						if (index == json.Length)
							break;

						c = json[index++];

						switch (c)
						{
							case '"':
								stringBuffer[idx++] = '"';
								break;
							case '\\':
								stringBuffer[idx++] = '\\';
								break;
							case '/':
								stringBuffer[idx++] = '/';
								break;
							case 'b':
								stringBuffer[idx++] = '\b';
								break;
							case 'f':
								stringBuffer[idx++] = '\f';
								break;
							case 'n':
								stringBuffer[idx++] = '\n';
								break;
							case 'r':
								stringBuffer[idx++] = '\r';
								break;
							case 't':
								stringBuffer[idx++] = '\t';
								break;
							case 'u':
								int remainingLength = json.Length - index;
								if (remainingLength >= 4)
								{
									var hex = new string(json, index, 4);

									// XXX: handle UTF
									stringBuffer[idx++] = (char)Convert.ToInt32(hex, 16);

									// skip 4 chars
									index += 4;
								}
								else {
									failed = true;
								}
								break;
						}
					}
					else {
						stringBuffer[idx++] = c;
					}

					if (idx >= stringBuffer.Length)
					{
						if (builder == null)
							builder = new StringBuilder();

						builder.Append(stringBuffer, 0, idx);
						idx = 0;
					}
				}

				if (!complete)
				{
					success = false;
					return null;
				}

				if (builder == null)
				{
					return new string(stringBuffer, 0, idx);
				}
				else {
					if (idx > 0)
						builder.Append(stringBuffer, 0, idx);

					return builder.ToString();
				}
			}

			private string GetNumberString()
			{
				SkipWhiteSpaces();

				int lastIndex = GetLastIndexOfNumber(index);
				int charLength = (lastIndex - index) + 1;

				var result = new string(json, index, charLength);

				index = lastIndex + 1;

				return result;
			}

			public float ParseFloatNumber()
			{
				float number;
				var str = GetNumberString();

				if (!float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
					return 0;

				return number;
			}

			public double ParseDoubleNumber()
			{
				double number;
				var str = GetNumberString();

				if (!double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out number))
					return 0;

				return number;
			}

			private int GetLastIndexOfNumber(int index)
			{
				int lastIndex;

				for (lastIndex = index; lastIndex < json.Length; lastIndex++)
				{
					char ch = json[lastIndex];

					if ((ch < '0' || ch > '9') && ch != '+' && ch != '-'
						&& ch != '.' && ch != 'e' && ch != 'E')
						break;
				}

				return lastIndex - 1;
			}

			private void SkipWhiteSpaces()
			{
				for (; index < json.Length; index++)
				{
					char ch = json[index];

					if (ch == '\n')
						lineNumber++;

					if (!char.IsWhiteSpace(json[index]))
						break;
				}
			}

			public Token LookAhead()
			{
				SkipWhiteSpaces();

				int savedIndex = index;
				return NextToken(json, ref savedIndex);
			}

			public Token NextToken()
			{
				SkipWhiteSpaces();
				return NextToken(json, ref index);
			}

			private static Token NextToken(char[] json, ref int index)
			{
				if (index == json.Length)
					return Token.None;

				char c = json[index++];

				switch (c)
				{
					case '{':
						return Token.CurlyOpen;
					case '}':
						return Token.CurlyClose;
					case '[':
						return Token.SquaredOpen;
					case ']':
						return Token.SquaredClose;
					case ',':
						return Token.Comma;
					case '"':
						return Token.String;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					case '-':
						return Token.Number;
					case ':':
						return Token.Colon;
				}

				index--;

				int remainingLength = json.Length - index;

				// false
				if (remainingLength >= 5)
				{
					if (json[index] == 'f' &&
						json[index + 1] == 'a' &&
						json[index + 2] == 'l' &&
						json[index + 3] == 's' &&
						json[index + 4] == 'e')
					{
						index += 5;
						return Token.False;
					}
				}

				// true
				if (remainingLength >= 4)
				{
					if (json[index] == 't' &&
						json[index + 1] == 'r' &&
						json[index + 2] == 'u' &&
						json[index + 3] == 'e')
					{
						index += 4;
						return Token.True;
					}
				}

				// null
				if (remainingLength >= 4)
				{
					if (json[index] == 'n' &&
						json[index + 1] == 'u' &&
						json[index + 2] == 'l' &&
						json[index + 3] == 'l')
					{
						index += 4;
						return Token.Null;
					}
				}

				return Token.None;
			}
		}

		public class JsonParser
		{
			public string errorMessage
			{
				get;
				private set;
			}

			private Lexer lexer;

			public JsonParser()
			{
				errorMessage = null;
			}

			public JSONElement Decode(string text)
			{
				errorMessage = null;
				lexer = new Lexer(text);
				return ParseValue();
			}

			public static JSONElement DecodeText(string text)
			{
				var builder = new JsonParser();
				return builder.Decode(text);
			}

			private JSONObject ParseObject()
			{
				JSONObject obj = new JSONObject();
				obj._fields = new Dictionary<string, JSONElement>();

				// {
				lexer.NextToken();

				while (true)
				{
					var token = lexer.LookAhead();

					switch (token)
					{
						case Lexer.Token.None:
							TriggerError("Invalid token");
							return null;
						case Lexer.Token.Comma:
							lexer.NextToken();
							break;
						case Lexer.Token.CurlyClose:
							lexer.NextToken();
							return obj;
						default:
							// name
							string name = EvalLexer(lexer.ParseString());

							if (errorMessage != null)
								return null;

							// :
							token = lexer.NextToken();

							if (token != Lexer.Token.Colon)
							{
								TriggerError("Invalid token; expected ':'");
								return null;
							}

							// value
							JSONElement value = ParseValue();

							if (errorMessage != null)
								return null;

							obj._fields[name] = value;
							break;
					}
				}

			}

			private JSONArray ParseArray()
			{
				JSONArray array = new JSONArray();
				array._elements = new List<JSONElement>();

				// [
				lexer.NextToken();

				while (true)
				{
					var token = lexer.LookAhead();

					switch (token)
					{
						case Lexer.Token.None:
							TriggerError("Invalid token");
							return null;
						case Lexer.Token.Comma:
							lexer.NextToken();
							break;
						case Lexer.Token.SquaredClose:
							lexer.NextToken();
							return array;
						default:
							JSONElement value = ParseValue();

							if (errorMessage != null)
								return null;

							array._elements.Add(value);
							break;
					}
				}

			}

			private JSONElement ParseValue()
			{
				switch (lexer.LookAhead())
				{
					case Lexer.Token.String:
						{
							JSONString jsonString = new JSONString();
							jsonString._value = EvalLexer(lexer.ParseString());
							return jsonString;
						}						
					case Lexer.Token.Number:
						{
							JSONNumber jsonNumber = new JSONNumber();
							jsonNumber._value = EvalLexer(lexer.ParseDoubleNumber());
							return jsonNumber;
						}
						
					case Lexer.Token.CurlyOpen:
						return ParseObject();
					case Lexer.Token.SquaredOpen:
						return ParseArray();
					case Lexer.Token.True:
						{
							lexer.NextToken();
							JSONBool jsonBool = new JSONBool();
							jsonBool._value = true;
							return jsonBool;
						}
					case Lexer.Token.False:
						{
							lexer.NextToken();
							JSONBool jsonBool = new JSONBool();
							jsonBool._value = false;
							return jsonBool;
						}
					case Lexer.Token.Null:
						lexer.NextToken();
						return null;
					case Lexer.Token.None:
						break;
				}

				TriggerError("Unable to parse value");
				return null;
			}

			private void TriggerError(string message)
			{
				errorMessage = string.Format("Error: '{0}' at line {1}",
											 message, lexer.lineNumber);
			}

			private T EvalLexer<T>(T value)
			{
				if (lexer.hasError)
					TriggerError("Lexical error occurred");

				return value;
			}
		}
	}
}