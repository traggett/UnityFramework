using System;
using System.Collections.Generic;

using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		[Serializable]
		public struct LocalisedStringRef
		{
			#region Serialized Data
			[SerializeField]
			private string _localisationKey;
			#endregion

			#region Private Data
			private string _cachedText;
			private SystemLanguage _cachedLanguage;
			#endregion

			#region Editor Data
#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorCollapsed;	
			private string _editorAutoNameParentName;
#endif
			#endregion
			
			public LocalisedStringRef(string key)
			{
				_localisationKey = key;
				_cachedText = string.Empty;
				_cachedLanguage = SystemLanguage.Unknown;
#if UNITY_EDITOR
				_editorCollapsed = false;
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
				if (_cachedLanguage != Localisation.GetCurrentLanguage())
				{
					_cachedLanguage = Localisation.GetCurrentLanguage();
					_cachedText = Localisation.GetString(_localisationKey);
				}

				return _cachedText;
			}

			public void UpdateVariables(params KeyValuePair<string, string>[] variables)
			{
				_cachedLanguage = Localisation.GetCurrentLanguage();
				_cachedText = Localisation.GetString(_localisationKey, variables);
			}

#if UNITY_EDITOR
			public string GetLocalisationKey()
			{
				return _localisationKey;
			}

			public SystemLanguage GetDebugLanguage()
			{
				return _cachedLanguage;
			}

			public string GetDebugText()
			{
				return _cachedText;
			}

			public void SetAutoNameParentName(string stateMachineName)
			{
				_editorAutoNameParentName = stateMachineName;
			}
			
			public string GetAutoNameParentName()
			{
				return _editorAutoNameParentName;
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