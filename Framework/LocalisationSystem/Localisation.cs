using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	using Utils;
	using Serialization;

	namespace LocalisationSystem
	{
		public static class Localisation
		{
			#region Public Data
			public static readonly string kDefaultLocalisationFilePath = "Localisation";
			public static readonly string kDefaultLocalisationFileName = "LocalisedStrings";
			public static readonly string kVariableStartChars = "${";
			public static readonly string kVariableEndChars = "}";

			public static Action OnLanguageChanged;
			#endregion

			#region Private Data
			private static Dictionary<string, LocalisationMap> _localisationMaps = new Dictionary<string, LocalisationMap>();
			private static SystemLanguage _currentLanguage = SystemLanguage.Unknown;

			private struct GlobalVariable
			{
				public int _version;
				public bool _localised;
				public string _value;
			}
			private static readonly Dictionary<string, GlobalVariable> _globalVariables = new Dictionary<string, GlobalVariable>();
			#endregion

			#region Public Interface
			public static void LoadStrings(SystemLanguage language)
			{
				string resourceName = GetLocalisationMapName(language);
				string resourcePath = AssetUtils.GetResourcePath(LocalisationProjectSettings.Get()._localisationFolder) + resourceName;

				var asset = Resources.Load(resourcePath);
				TextAsset textAsset = asset as TextAsset;
				LocalisationMap localisationMap;

				if (asset != null)
				{
					localisationMap = Serializer.FromTextAsset<LocalisationMap>(textAsset);
				}				
				else
				{
					localisationMap = new LocalisationMap(language);
				}

				_localisationMaps[LanguageCodes.GetLanguageCode(language)] = localisationMap;
			}

			public static void UnloadStrings(SystemLanguage language)
			{
				if (_localisationMaps.ContainsKey(LanguageCodes.GetLanguageCode(language)))
				{
					_localisationMaps.Remove(LanguageCodes.GetLanguageCode(language));
				}
			}

			public static bool Exists(string key)
			{
				return Exists(key, GetCurrentLanguage());
			}

			public static bool Exists(string key, SystemLanguage language)
			{
				MakeSureStringsAreLoaded();

				if (_localisationMaps.TryGetValue(LanguageCodes.GetLanguageCode(language), out LocalisationMap map))
				{
					return map.IsValidKey(key);
				}

				return false;
			}

			public static LocalisationLocalVariable Variable(string key, LocalisedString value)
			{
				return new LocalisationLocalVariable(key, value.GetLocalisationKey(), true);
			}

			public static LocalisationLocalVariable Variable(string key, string value)
			{
				return new LocalisationLocalVariable(key, value);
			}

			public static LocalisationLocalVariable Variable(string key, int value)
			{
				return new LocalisationLocalVariable(key, Convert.ToString(value));
			}

			public static LocalisationLocalVariable Variable(string key, float value)
			{
				return new LocalisationLocalVariable(key, Convert.ToString(value));
			}

			public static string Get(SystemLanguage language, string key, params LocalisationLocalVariable[] localVariables)
			{
				MakeSureStringsAreLoaded(language);

				if (_localisationMaps.TryGetValue(LanguageCodes.GetLanguageCode(language), out LocalisationMap map))
				{
					string guid = map.GUIDFromKey(key);

					if (!string.IsNullOrEmpty(guid))
					{
						string text = map.Get(guid);
						text = ReplaceVariables(text, localVariables);

						return text;
					}
				}

				return "No Localisation Map found for " + language;
			}

			public static string Get(string key, params LocalisationLocalVariable[] localVariables)
			{
				return Get(GetCurrentLanguage(), key, localVariables);
			}

			public static string GetGUID(SystemLanguage language, string guid, params LocalisationLocalVariable[] localVariables)
			{
				MakeSureStringsAreLoaded(language);

				if (_localisationMaps.TryGetValue(LanguageCodes.GetLanguageCode(language), out LocalisationMap map))
				{
					string text = map.Get(guid);
					text = ReplaceVariables(text, localVariables);

					return text;
				}

				return "No Localisation Map found for " + language;
			}

			public static string GetGUID(string guid, params LocalisationLocalVariable[] localVariables)
			{
				return GetGUID(GetCurrentLanguage(), guid, localVariables);
			}

			public static string FindKeyGUID(SystemLanguage language, string key)
			{
				MakeSureStringsAreLoaded(language);

				if (_localisationMaps.TryGetValue(LanguageCodes.GetLanguageCode(language), out LocalisationMap map))
				{
					return map.KeyFromGUID(key);
				}

				return "No Localisation Map found for " + language;
			}

			public static string FindKeyGUID(string key)
			{
				return FindKeyGUID(GetCurrentLanguage(), key);
			}
		
			public static SystemLanguage GetCurrentLanguage()
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
					return Application.systemLanguage;
#endif

				if (_currentLanguage == SystemLanguage.Unknown)
				{
					SetLanguage(Application.systemLanguage);
				}
				
				return _currentLanguage;
			}

			public static void SetLanguage(SystemLanguage language)
			{
				if (_currentLanguage != language)
				{
					UnloadStrings(_currentLanguage);

					_currentLanguage = language;
					LoadStrings(language);

					OnLanguageChanged?.Invoke();
				}
			}

			public static void SetGlobalVaraiable(string key, string value, bool isLocalised = false)
			{
				if (_globalVariables.TryGetValue(key, out GlobalVariable info))
				{
					info._version++;
				}

				info._value = value;
				info._localised = isLocalised;

				_globalVariables[key] = info;
			}

			public static void ClearGlobalVaraiable(string key)
			{
				_globalVariables.Remove(key);
			}

			public static LocalisationGlobalVariable[] GetGlobalVariables(string key, SystemLanguage language)
			{
				if (_localisationMaps.TryGetValue(LanguageCodes.GetLanguageCode(language), out LocalisationMap map))
				{
					string text = map.Get(key, true);

					List<LocalisationGlobalVariable> variables = new List<LocalisationGlobalVariable>();

					int index = 0;

					while (index < text.Length)
					{
						int variableStartIndex = text.IndexOf(kVariableStartChars, index);

						if (variableStartIndex != -1)
						{
							int variableEndIndex = text.IndexOf(kVariableEndChars, variableStartIndex);
							if (variableEndIndex == -1)
								throw new Exception("Can't find matching end bracket for variable in localised string");

							int variableKeyStartIndex = variableStartIndex + kVariableEndChars.Length + 1;
							string variableKey = text.Substring(variableKeyStartIndex, variableEndIndex - variableKeyStartIndex);

							_globalVariables.TryGetValue(variableKey, out GlobalVariable info);
							variables.Add(new LocalisationGlobalVariable(variableKey, info._version));

							index = variableEndIndex + kVariableEndChars.Length;
						}
						else
						{
							break;
						}
					}

					return variables.ToArray();
				}

				return null;
			}

			public static LocalisationGlobalVariable[] GetGlobalVariables()
			{
				List<LocalisationGlobalVariable> variables = new List<LocalisationGlobalVariable>();

				foreach (KeyValuePair<string, GlobalVariable> keyValuePair in _globalVariables)
				{
					variables.Add(new LocalisationGlobalVariable(keyValuePair.Key, keyValuePair.Value._version));
				}
				
				return variables.ToArray();
			}

			public static bool AreGlobalVariablesOutOfDate(LocalisationGlobalVariable[] varaiables)
			{
				if (varaiables != null)
				{
					for (int i = 0; i < varaiables.Length; i++)
					{
						GlobalVariable info;

						if (_globalVariables.TryGetValue(varaiables[i]._key, out info))
						{
							if (varaiables[i]._version != info._version)
							{
								return true;
							}
						}
					}
				}

				return false;
			}
			#endregion

			#region Public Editor Interface
			public static string GetLocalisationMapName(SystemLanguage language)
			{
				return kDefaultLocalisationFileName + "_" + LanguageCodes.GetLanguageCode(language).ToUpper();
			}
			#endregion

			#region Private Functions
			private static void MakeSureStringsAreLoaded()
			{
				MakeSureStringsAreLoaded(GetCurrentLanguage());
			}

			private static void MakeSureStringsAreLoaded(SystemLanguage language)
			{
				if (!_localisationMaps.ContainsKey(LanguageCodes.GetLanguageCode(language)))
					LoadStrings(language);
			}

			private static string ReplaceVariables(string text, LocalisationLocalVariable[] localVariables)
			{
				string fullText = "";
				int index = 0;
				
				while (index < text.Length)
				{
					int variableStartIndex = text.IndexOf(kVariableStartChars, index);

					if (variableStartIndex != -1)
					{
						int variableEndIndex = text.IndexOf(kVariableEndChars, variableStartIndex);
						if (variableEndIndex == -1)
						{
							UnityEngine.Debug.LogError("Can't find matching end bracket for variable in localised string");
							return null;
						}

						fullText += text.Substring(index, variableStartIndex - index);

						int variableKeyStartIndex = variableStartIndex + kVariableEndChars.Length + 1;
						string variableKey = text.Substring(variableKeyStartIndex, variableEndIndex - variableKeyStartIndex);

						bool foundKey = false;

						if (Application.isPlaying)
						{
							//First check provided local variables
							if (localVariables != null)
							{
								for (int i = 0; i < localVariables.Length; i++)
								{
									if (localVariables[i]._key == variableKey)
									{
										fullText += localVariables[i]._localised ? Get(localVariables[i]._value) : localVariables[i]._value;
										foundKey = true;
										break;
									}
								}
							}
							
							//If not found in there check global variables
							if (!foundKey)
							{
								if (_globalVariables.TryGetValue(variableKey, out GlobalVariable variable))
								{
									fullText += variable._localised ? Get(variable._value) : variable._value;
								}
								else if (Application.isPlaying)
								{
									UnityEngine.Debug.LogWarning("Can't find variable to replace key '" + variableKey + "'");
								}
							}
						}
						else
						{
							fullText += "<" + variableKey + ">";
						}

						index = variableEndIndex + kVariableEndChars.Length;
					}
					else
					{
						if (index == 0)
							fullText = text;
						else
							fullText += text.Substring(index, text.Length - index);

						break;
					}
				}

				return fullText;
			}
			#endregion
		}
	}
}