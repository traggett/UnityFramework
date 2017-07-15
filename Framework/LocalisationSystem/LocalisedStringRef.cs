using System;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace LocalisationSystem
	{
		[Serializable]
		public struct LocalisedStringRef : ISerializationCallbackReceiver
		{
			#region Private Data
			[SerializeField]
			private string _localisationKey;
			private string _text;
			private SystemLanguage _language;
			#endregion

			#region Editor Data
#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorFoldout;
			private string _editorText;		
			private float _editorHeight;
			private string _editorAutoNameParentName;
#endif
			#endregion
			
			public LocalisedStringRef(string key)
			{
				_localisationKey = key;
				_text = string.Empty;
				_language = SystemLanguage.Unknown;
#if UNITY_EDITOR
				_editorText = string.Empty;
				_editorFoldout = true;
				_editorHeight = EditorGUIUtility.singleLineHeight;
				_editorAutoNameParentName = null;
#endif
			}

			public static implicit operator string(LocalisedStringRef property)
			{
				return property.GetLocalisedString();
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

#if UNITY_EDITOR
			public string GetLocalisationKey()
			{
				return _localisationKey;
			}

			public SystemLanguage GetDebugLanguage()
			{
				return _language;
			}

			public string GetDebugText()
			{
				return _text;
			}

			public void SetAutoNameParentName(string stateMachineName)
			{
				_editorAutoNameParentName = stateMachineName;
			}
			
			public string GetAutoKey()
			{
				string autoKey = null;
				
				if (!string.IsNullOrEmpty(_editorAutoNameParentName))
				{
					string statemachineName = _editorAutoNameParentName;

					//Replace _ with / so each bit will appear in separate dropdown menu (eg TextConv_Hath_Birthday will go to TextConv, Hath, Birthday)
					statemachineName = statemachineName.Replace('_', '/');

					//Find first free key
					int index = 0;
					while (Localisation.IsKeyInTable(autoKey = statemachineName + "/" + index.ToString("000")))
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