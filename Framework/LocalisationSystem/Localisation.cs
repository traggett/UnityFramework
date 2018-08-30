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
			public string _serialisedLocalisationMaps;
		}
#endif

		public static class Localisation
		{
			#region Public Data
			public static readonly string kDefaultLocalisationFilePath = "Localisation";
			public static readonly string kDefaultLocalisationFileName = "LocalisedStrings";
			public static readonly string kVariableStartChars = "${";
			public static readonly string kVariableEndChars = "}";
			#endregion

			#region Private Data
			private static Dictionary<SystemLanguage, LocalisationMap> _localisationMaps = new Dictionary<SystemLanguage, LocalisationMap>();
			private static SystemLanguage _currentLanguage = SystemLanguage.Unknown;

			private struct VariableInfo
			{
				public string _value;
				public int _version;
			}
			private static Dictionary<string, VariableInfo> _globalVariables = new Dictionary<string, VariableInfo>();
			private static bool _dirty;
#if UNITY_EDITOR
			private static string[] _editorKeys;
			private static string[] _editorFolders;
			private static LocalisationUndoState _undoObject;
			private class LocalisationEditorListener : UnityEditor.AssetModificationProcessor
			{
				private static string[] OnWillSaveAssets(string[] paths)
				{
					EditorWarnIfDirty();
					return paths;
				}
			}
#endif
			#endregion

			#region Public Interface
			public static void LoadStrings(SystemLanguage language)
			{
				string resourceName = GetLocalisationMapName(language);
				string resourcePath = AssetUtils.GetResourcePath(LocalisationProjectSettings.Get()._localisationFolder) + "/" + resourceName;

				TextAsset asset = Resources.Load(resourcePath) as TextAsset;
				LocalisationMap localisationMap;

				if (asset != null)
				{
					localisationMap = Serializer.FromTextAsset<LocalisationMap>(asset);
				}				
				else
				{
					localisationMap = new LocalisationMap();
				}

				_localisationMaps[language] = localisationMap;

#if UNITY_EDITOR
				RefreshEditorKeys();
#endif
			}

			public static void UnloadStrings(SystemLanguage language)
			{
				if (_localisationMaps.ContainsKey(language))
				{
#if UNITY_EDITOR
					EditorWarnIfDirty();
#endif
					_localisationMaps.Remove(language);
				}
			}

			public static bool Exists(string key)
			{
				MakeSureStringsAreLoaded();

				return _localisationMaps[_currentLanguage].IsValidKey(key);
			}

			public static LocalisationLocalVariable Variable(string key, string value)
			{
				return new LocalisationLocalVariable(key, value);
			}

			public static string Get(string key, params LocalisationLocalVariable[] localVariables)
			{
				return Get(key, _currentLanguage, localVariables);
			}

			public static SystemLanguage GetCurrentLanguage()
			{
				if (_currentLanguage == SystemLanguage.Unknown)
					_currentLanguage = Application.systemLanguage;

				return _currentLanguage;
			}

			public static void SetLanguage(SystemLanguage language)
			{
				UnloadStrings(_currentLanguage);
				
				_currentLanguage = language;
				LoadStrings(language);
			}

			public static void SetGlobalVaraiable(string key, string value)
			{
				VariableInfo info;

				if (_globalVariables.TryGetValue(key, out info))
				{
					info._version++;
				}

				info._value = value;

				_globalVariables[key] = info;
			}

			public static void ClearGlobalVaraiable(string key)
			{
				_globalVariables.Remove(key);
			}

			public static LocalisationGlobalVariable[] GetGlobalVariables(string text)
			{
				int index = 0;

				List<LocalisationGlobalVariable> keys = new List<LocalisationGlobalVariable>();

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
						_globalVariables.TryGetValue(variableKey, out info);
						keys.Add(new LocalisationGlobalVariable { _key = variableKey, _version = info._version } );

						index = variableEndIndex + kVariableEndChars.Length;
					}
					else
					{
						break;
					}
				}

				return keys.ToArray();
			}

			public static bool AreGlobalVariablesOutOfDate(params LocalisationGlobalVariable[] varaiables)
			{
				if (varaiables != null)
				{
					for (int i = 0; i < varaiables.Length; i++)
					{
						VariableInfo info;

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
#if UNITY_EDITOR
			public static void Set(string key, SystemLanguage language, string text)
			{
				OnPreEditorChange();

				_localisationMaps[language].SetString(key, text);

				OnPostEditorChange();
			}

			public static void Remove(string key)
			{
				OnPreEditorChange();

				foreach (KeyValuePair<SystemLanguage, LocalisationMap> languagePair in _localisationMaps)
				{
					languagePair.Value.RemoveString(key);
				}

				OnPostEditorChange();
			}

			public static void ChangeKey(string key, string newKey)
			{
				OnPreEditorChange();

				foreach (KeyValuePair<SystemLanguage, LocalisationMap> languagePair in _localisationMaps)
				{
					languagePair.Value.ChangeKey(key, newKey);
				}
				
				OnPostEditorChange();
			}

			public static string GetDefaultLocalisationFolder()
			{
				return Application.dataPath + "/Resources";
			}

			public static void SaveStrings()
			{
				foreach (KeyValuePair<SystemLanguage, LocalisationMap> languagePair in _localisationMaps)
				{
					string resourceName = GetLocalisationMapName(languagePair.Key);
					string assetsPath = LocalisationProjectSettings.Get()._localisationFolder;
					string resourcePath = AssetUtils.GetResourcePath(assetsPath) + "/" + resourceName;
					string filePath = AssetUtils.GetAppllicationPath();

					TextAsset asset = Resources.Load(resourcePath) as TextAsset;
					if (asset != null)
					{
						filePath += AssetDatabase.GetAssetPath(asset);
					}
					else
					{
						filePath += assetsPath + "/" + resourceName + ".xml";
					}

					Serializer.ToFile(languagePair.Value, filePath);
				}

				_dirty = false;
			}


			public static void ReloadStrings(bool warnIfDirty = false)
			{
				if (warnIfDirty)
					EditorWarnIfDirty();

				SystemLanguage[] languages = new SystemLanguage[_localisationMaps.Keys.Count];
				_localisationMaps.Keys.CopyTo(languages, 0);

				foreach (SystemLanguage language in languages)
				{
					LoadStrings(language);
				}

				_dirty = false;
			}

			public static bool HasUnsavedChanges()
			{
				return _dirty;
			}

			public static string[] GetStringKeys()
			{
				MakeSureStringsAreLoaded();

				return _editorKeys;
			}

			public static string[] GetStringFolders()
			{
				MakeSureStringsAreLoaded();

				return _editorFolders;
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
			
			public static string GetRawString(string key, SystemLanguage language)
			{
				MakeSureStringsAreLoaded(language);

				return _localisationMaps[language].GetString(key, true);
			}
#endif
			#endregion

			#region Private Functions
			public static string Get(string key, SystemLanguage language, params LocalisationLocalVariable[] localVariables)
			{
				MakeSureStringsAreLoaded(language);

				string text = _localisationMaps[language].GetString(key);
				text = ReplaceVariables(text, localVariables);

				return text;
			}

			private static string GetLocalisationMapName(SystemLanguage language)
			{
				return kDefaultLocalisationFileName + "_" + LanguageCodes.GetLanguageCode(language).ToUpper();
			}

			private static void MakeSureStringsAreLoaded()
			{
				MakeSureStringsAreLoaded(GetCurrentLanguage());
			}

			private static void MakeSureStringsAreLoaded(SystemLanguage language)
			{
				if (!_localisationMaps.ContainsKey(language))
					LoadStrings(language);
			}

			private static string ReplaceVariables(string text, params LocalisationLocalVariable[] localVariables)
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

						//First check provided local variables
						for (int i=0; i<localVariables.Length; i++)
						{
							if (localVariables[i]._key == variableKey)
							{
								fullText += localVariables[i]._value;
								foundKey = true;
								break;
							}
						}

						//If not found in there check global variables
						if (!foundKey)
						{
							VariableInfo info;
							if (_globalVariables.TryGetValue(variableKey, out info))
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

			#region Private Editor Functions
#if UNITY_EDITOR
			private static void EditorWarnIfDirty()
			{
				if (HasUnsavedChanges())
				{
					bool save = EditorUtility.DisplayDialog("Localization strings have Been Modified",
																   "Do you want to save the changes you made to the localization table?",
																   "Save",
																   "Discard");

					if (save)
					{
						SaveStrings();
					}
					else
					{
						ReloadStrings();
					}
				}
			}

			private static void RefreshEditorKeys()
			{
				HashSet<string> allKeys = new HashSet<string>();
				foreach (KeyValuePair<SystemLanguage, LocalisationMap> languagePair in _localisationMaps)
				{
					allKeys.UnionWith(languagePair.Value.GetStringKeys());
				}

				_editorKeys = new string[allKeys.Count];
				allKeys.CopyTo(_editorKeys);

				ArrayUtils.Insert(ref _editorKeys, "(None)", 0);

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

			//Need to save all loaded strings tables!
			private static void UndoRedoCallback()
			{
				if (_undoObject != null && !string.IsNullOrEmpty(_undoObject._serialisedLocalisationMaps))
				{
					_localisationMaps = (Dictionary<SystemLanguage, LocalisationMap>)Serializer.FromString(typeof(Dictionary<SystemLanguage, LocalisationMap>), _undoObject._serialisedLocalisationMaps);
					if (_localisationMaps == null)
						throw new Exception();

					_undoObject._serialisedLocalisationMaps = null;
					_dirty = true;
				}
			}

			private static void OnPreEditorChange()
			{
				MakeSureStringsAreLoaded();

				if (_undoObject == null)
				{
					_undoObject = (LocalisationUndoState)ScriptableObject.CreateInstance(typeof(LocalisationUndoState));
					_undoObject.name = "LocalisationUndoState";
					Undo.undoRedoPerformed += UndoRedoCallback;
				}
				_undoObject._serialisedLocalisationMaps = Serializer.ToString(_localisationMaps);
			}

			private static void OnPostEditorChange()
			{
				_dirty = true;

				RefreshEditorKeys();

				Undo.RegisterCompleteObjectUndo(_undoObject, "Localisation strings changed");
				_undoObject._serialisedLocalisationMaps = Serializer.ToString(_localisationMaps);
			}
#endif
			#endregion
		}
	}
}