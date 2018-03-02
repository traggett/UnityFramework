using System;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Utils;
	using Serialization;

	namespace LocalisationSystem
	{
#if UNITY_EDITOR
		public class LocalisationUndoState : ScriptableObject
		{
			public string _serialisedLocalisationMap;
		}
#endif

		public struct LocalisationVariableInfo
		{
			public string _key;
			public int _version;
		}

		public static class Localisation
		{
			#region Public Data
			public static readonly string kDefaultLocalisationFilePath = "Localisation/Localisation";
			public static readonly string kVariableStartChars = "${";
			public static readonly string kVariableEndChars = "}";
			#endregion

			#region Private Data
			private static LocalisationMap _localisationMap;
			private static SystemLanguage _currentLanguage = SystemLanguage.English;
			private static SystemLanguage _fallBackLanguage = SystemLanguage.English;

			private struct VariableInfo
			{
				public string _value;
				public int _version;
			}
			private static Dictionary<string, VariableInfo> _variables = new Dictionary<string, VariableInfo>();
			private static bool _dirty;
#if UNITY_EDITOR
			private static string[] _editorKeys;
			private static string[] _editorFolders;
			private static LocalisationUndoState _undoObject;
			private class LocalisationEditorListener : UnityEditor.AssetModificationProcessor
			{
				private static string[] OnWillSaveAssets(string[] paths)
				{
					WarnIfDirty();
					return paths;
				}
			}
#endif
			#endregion

			#region Public Interface
			public static void LoadStrings()
			{
				_localisationMap = null;
				_dirty = false;

				TextAsset asset = Resources.Load(kDefaultLocalisationFilePath) as TextAsset;

				if (asset != null)
				{
					_localisationMap = Serializer.FromTextAsset<LocalisationMap>(asset);
				}				
				else
				{
					_localisationMap = new LocalisationMap();
				}
#if UNITY_EDITOR
				RefreshEditorKeys();
#endif
			}
			
			public static KeyValuePair<string, string> VariablePair(string key, string value)
			{
				return new KeyValuePair<string, string>(key, value);
			}

			public static string GetString(string key, params KeyValuePair<string, string>[] variables)
			{
				return GetString(key, _currentLanguage, variables);
			}

			public static SystemLanguage GetCurrentLanguage()
			{
				return _currentLanguage;
			}

			public static void SetLanguage(SystemLanguage language)
			{
				_currentLanguage = language;
			}

			public static void SetVaraiable(string key, string value)
			{
				VariableInfo info;

				if (_variables.TryGetValue(key, out info))
				{
					info._version++;
				}

				info._value = value;

				_variables[key] = info;
			}

			public static void ClearVaraiable(string key)
			{
				_variables.Remove(key);
			}

			public static LocalisationVariableInfo[] GetGlobalVariableKeys(string text)
			{
				int index = 0;

				List<LocalisationVariableInfo> keys = new List<LocalisationVariableInfo>();

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

						VariableInfo info;
						_variables.TryGetValue(variableKey, out info);
						keys.Add(new LocalisationVariableInfo { _key = variableKey, _version = info._version } );

						index = variableEndIndex + kVariableEndChars.Length;
					}
					else
					{
						break;
					}
				}

				return keys.ToArray();
			}

			public static bool AreGlobalVariablesOutOfDate(params LocalisationVariableInfo[] varaiables)
			{
				if (varaiables != null)
				{
					for (int i = 0; i < varaiables.Length; i++)
					{
						VariableInfo info;

						if (_variables.TryGetValue(varaiables[i]._key, out info))
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

#if UNITY_EDITOR
			public static void UpdateString(string key, SystemLanguage language, string text)
			{
				OnPreEditorChange();

				_localisationMap.UpdateString(key, language, text);

				OnPostEditorChange();
			}

			public static void DeleteString(string key)
			{
				OnPreEditorChange();

				_localisationMap.RemoveString(key);

				OnPostEditorChange();
			}

			public static void ChangeKey(string key, string newKey)
			{
				OnPreEditorChange();

				_localisationMap.ChangeKey(key, newKey);

				OnPostEditorChange();
			}

			public static void WarnIfDirty()
			{
				if (HasUnsavedChanges())
				{
					int option = EditorUtility.DisplayDialogComplex("Localization strings have Been Modified",
																   "Do you want to save the changes you made to the localization table?",
																   "Save",
																   "Save",
																   "Revert");

					switch (option)
					{
						case 0:
						case 1:
							SaveStrings(); break;
						case 2:
							{
								_dirty = false;
								LoadStrings();
							}
							break;
					}
				}
			}

#if UNITY_EDITOR
			public static void SaveStrings()
			{
				if (_localisationMap!= null)
				{
					string path;

					TextAsset asset = Resources.Load(kDefaultLocalisationFilePath) as TextAsset;
					if (asset != null)
					{
						path = AssetDatabase.GetAssetPath(asset);
					}
					else
					{
						path = Application.dataPath + "/Resources/" + kDefaultLocalisationFilePath + ".xml";
					}

					Serializer.ToFile(_localisationMap, path);
				}

				_dirty = false;
			}
#endif

			public static bool HasUnsavedChanges()
			{
				return _dirty;
			}

			public static string[] GetStringKeys()
			{
				if (_localisationMap == null)
					LoadStrings();
						
				return _editorKeys;
			}

			public static string[] GetStringFolders()
			{
				if (_localisationMap == null)
					LoadStrings();

				return _editorFolders;
			}

			public static bool IsKeyInTable(string key)
			{
				if (_localisationMap == null)
					LoadStrings();

				return _localisationMap.IsValidKey(key);
			}
			
			public static bool GetFolderIndex(string key, out int folder, out string exKey)
			{
				folder = 0;
				exKey = key;

				if (!string.IsNullOrEmpty(key))
				{
					int longestFolder = -1;

					for (int i = 0; i < _editorFolders.Length; i++)
					{
						if (key.StartsWith(_editorFolders[i]) && (longestFolder == -1 || _editorFolders[i].Length > _editorFolders[longestFolder].Length))
						{
							longestFolder = i;
						}
					}

					if (longestFolder != -1)
					{
						if (key.Length > _editorFolders[longestFolder].Length)
						{
							exKey = key.Substring(_editorFolders[longestFolder].Length + 1);
						}
						else
						{
							exKey = "";
						}
						
						folder = longestFolder;

						return true;
					}

				}

				return false;
			}
			
			public static string GetRawString(string key)
			{
				if (_localisationMap == null)
					LoadStrings();

				return _localisationMap.GetString(key, _currentLanguage, _fallBackLanguage);
			}
#endif
			#endregion

			#region Private Functions
			private static string GetString(string key, SystemLanguage language, params KeyValuePair<string, string>[] variables)
			{
				if (_localisationMap == null)
					LoadStrings();

				string text = _localisationMap.GetString(key, language, _fallBackLanguage);
				text = ReplaceVariables(text, variables);

				return text;
			}

			//As well as passed in variables store a table of stored variables
			private static string ReplaceVariables(string text, params KeyValuePair<string, string>[] variables)
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
							Debug.LogError("Can't find matching end bracket for variable in localised string");
							return null;
						}

						fullText += text.Substring(index, variableStartIndex - index);

						int variableKeyStartIndex = variableStartIndex + kVariableEndChars.Length + 1;
						string variableKey = text.Substring(variableKeyStartIndex, variableEndIndex - variableKeyStartIndex);

						bool foundKey = false;

						//First check provided variables
						foreach (KeyValuePair<string, string> variable in variables)
						{
							if (variable.Key == variableKey)
							{
								fullText += variable.Value;
								foundKey = true;
								break;
							}
						}

						//If not found in there check global variables
						if (!foundKey)
						{
							VariableInfo info;
							if (_variables.TryGetValue(variableKey, out info))
							{
								fullText += info._value;
							}
							else if (Application.isPlaying)
							{
								Debug.LogError("Can't find variable to replace key '" + variableKey + "'");
							}
						}

						index = variableEndIndex + kVariableEndChars.Length;
					}
					else
					{
						fullText += text.Substring(index, text.Length - index);
						break;
					}
				}

				return fullText;
			}

#if UNITY_EDITOR
			private static void RefreshEditorKeys()
			{
				_editorKeys = _localisationMap.GetStringKeys();
				ArrayUtils.Insert(ref _editorKeys, "(Add New Key)", 0);

				List<string> folders = new List<string>();

				folders.Add("(Root)");

				//Also want to add sub folders eg Menus/Options/Volume should add Menus and Menus/Options as folder options
				for (int i=0; i<_editorKeys.Length; i++)
				{
					string[] keyFolders = _editorKeys[i].Split('/');

					//want to add first item
					// first item + next
					if (keyFolders.Length > 1)
					{
						string folder = keyFolders[0];

						for (int j = 1; j < keyFolders.Length; j++)
						{
							if (!string.IsNullOrEmpty(folder) && !folders.Contains(folder))
								folders.Add(folder);

							folder += "/" + keyFolders[j];
						}
					}
				}

				_editorFolders = folders.ToArray();
			}

#endif

#if UNITY_EDITOR
			private static void UndoRedoCallback()
			{
				if (_undoObject != null && !string.IsNullOrEmpty(_undoObject._serialisedLocalisationMap))
				{
					_localisationMap = (LocalisationMap)Serializer.FromString(typeof(LocalisationMap), _undoObject._serialisedLocalisationMap);
					if (_localisationMap == null)
						throw new Exception();

					_undoObject._serialisedLocalisationMap = null;
					_dirty = true;
				}
			}

			private static void OnPreEditorChange()
			{
				if (_localisationMap == null)
					LoadStrings();

				if (_undoObject == null)
				{
					_undoObject = (LocalisationUndoState)ScriptableObject.CreateInstance(typeof(LocalisationUndoState));
					_undoObject.name = "LocalisationUndoState";
					Undo.undoRedoPerformed += UndoRedoCallback;
				}
				_undoObject._serialisedLocalisationMap = Serializer.ToString(_localisationMap);
			}

			private static void OnPostEditorChange()
			{
				_dirty = true;

				RefreshEditorKeys();

				Undo.RegisterCompleteObjectUndo(_undoObject, "Localisation strings changed");
				_undoObject._serialisedLocalisationMap = Serializer.ToString(_localisationMap);
			}
#endif
			#endregion
		}
	}
}