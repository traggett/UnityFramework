using System;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Serialization;
	using TimelineStateMachineSystem;

	namespace LocalisationSystem
	{
		[Serializable]
		public class LocalisedStringRef : ISerializationCallbackReceiver, ICustomEditorInspector
		{
			#region Private Data
			[SerializeField]
			private string _localisationKey;
			[SerializeField]
			private string _text;
			[SerializeField]
			private SystemLanguage _language;
			#endregion

			#region Editor Data
#if UNITY_EDITOR
			private string _editorText;
			private bool _editorFoldout;
			private float _editorHeight;
			private TimelineStateMachine _editorStateMachine;
#endif
			#endregion

			public LocalisedStringRef() : this(string.Empty)
			{
			}

			public LocalisedStringRef(string key)
			{
				_localisationKey = key;
				_text = string.Empty;
				_language = SystemLanguage.Unknown;
#if UNITY_EDITOR
				_editorText = string.Empty;
				_editorFoldout = false;
				_editorHeight = EditorGUIUtility.singleLineHeight;
#endif
			}

			public static implicit operator string(LocalisedStringRef property)
			{
				if (property != null)
					return property.GetLocalisedString();

				return string.Empty;
			}

			public static implicit operator LocalisedStringRef(string key)
			{
				return new LocalisedStringRef(key);
			}

			public string GetLocalisedString()
			{
				if (_language != Localisation.GetCurrentLanguage())
				{
					_language = Localisation.GetCurrentLanguage();
					_text = Localisation.GetString(_localisationKey);
				}

				return _text;
			}

			public void UpdateVariables(params KeyValuePair<string, string>[] variables)
			{
				_language = Localisation.GetCurrentLanguage();
				_text = Localisation.GetString(_localisationKey, variables);
			}

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
				_language = Localisation.GetCurrentLanguage();
				_text = Localisation.GetString(_localisationKey);
			}
			#endregion

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;
				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);

				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					//Draw list of possible keys
					int currentKey = 0;
					{
						string[] keys = Localisation.GetStringKeys();

						for (int i = 0; i < keys.Length; i++)
						{
							if (keys[i] == _localisationKey)
							{
								currentKey = i;
								break;
							}
						}

						EditorGUI.BeginChangeCheck();
						currentKey = EditorGUILayout.Popup("Localisation Key", currentKey, keys);

						if (EditorGUI.EndChangeCheck())
						{
							if (currentKey == 0)
							{
								_localisationKey = null;
								_language = SystemLanguage.Unknown;
								_text = "";
							}
							else
							{
								_localisationKey = keys[currentKey];
								_language = Localisation.GetCurrentLanguage();
								_text = Localisation.GetString(_localisationKey);
							}

							dataChanged = true;
						}
					}

					//Draw buttons for adding new key
					if (currentKey == 0)
					{
						EditorGUILayout.BeginHorizontal();
						{
							_localisationKey = EditorGUILayout.DelayedTextField("New Key", _localisationKey);

							if (GUILayout.Button("Auto", GUILayout.Width(36)))
							{
								_localisationKey = GetAutoKey();
							}

							if (GUILayout.Button("Add", GUILayout.Width(32)) && !string.IsNullOrEmpty(_localisationKey))
							{
								//Add new string for new key
								_language = Localisation.GetCurrentLanguage();
								Localisation.UpdateString(_localisationKey, _language, _text);

								string[] keys = Localisation.GetStringKeys();
								for (int i = 0; i < keys.Length; i++)
								{
									if (keys[i] == _localisationKey)
									{
										currentKey = i;
										break;
									}
								}

								dataChanged = true;
							}
						}
						EditorGUILayout.EndHorizontal();
					}

					//Draw displayed text (can be edited to update localisation file)
					{
						EditorGUI.BeginChangeCheck();
						_text = EditorGUILayout.TextArea(_text);
						if (EditorGUI.EndChangeCheck() && currentKey != 0)
						{
							_language = Localisation.GetCurrentLanguage();
							Localisation.UpdateString(_localisationKey, _language, _text);
							dataChanged = true;
						}
					}

					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion

#if UNITY_EDITOR
			public void SetEditorStateMachine(TimelineStateMachine stateMachine)
			{
				_editorStateMachine = stateMachine;
			}
			
			private string GetAutoKey()
			{
				string autoKey = null;
				
				if (_editorStateMachine != null && !string.IsNullOrEmpty(_editorStateMachine._name))
				{
					string statemachineName = _editorStateMachine._name;

					//Replace _ with / so each bit will appear in separate dropdown menu (eg TextConv_Hath_Birthday will go to TextConv, Hath, Birthday)
					statemachineName = statemachineName.Replace('_', '/');

					//Find first free key
					int index = 0;
					while (Localisation.IsValidKey(autoKey = statemachineName + "/" + index.ToString("000")))
					{
						index++;
					}
				}

				return autoKey;
			}		
#endif
		}
	}
}