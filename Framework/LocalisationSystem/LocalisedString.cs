using System;
using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		[Serializable]
		public struct LocalisedString
		{
			#region Public Data
			public static LocalisedString Empty = new LocalisedString(string.Empty);
			#endregion

			#region Serialized Data
			[SerializeField]
			private string _localisationKey;
			#endregion

			#region Private Data
			private string _cachedText;
			private SystemLanguage _cachedLanguage;
			private LocalisationGlobalVariable[] _cachedVariables;
			private LocalisationLocalVariable[] _localVariables;
			#endregion

			#region Editor Data
#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorCollapsed;	
			private string _editorAutoNameParentName;
#endif
			#endregion
			
			private LocalisedString(string key, params LocalisationLocalVariable[] variables)
			{
				_localisationKey = key;
				_cachedText = string.Empty;
				_cachedLanguage = SystemLanguage.Unknown;
				_cachedVariables = null;
				_localVariables = variables;

#if UNITY_EDITOR
				_editorCollapsed = false;
				_editorAutoNameParentName = null;
#endif
			}

			public static LocalisedString Create(string key, params LocalisationLocalVariable[] variables)
			{
				return new LocalisedString(key, variables);
			}

			public static implicit operator string(LocalisedString property)
			{
				return property.GetLocalisedString();
			}

			public static implicit operator LocalisedString(string key)
			{
				return new LocalisedString(key);
			}

			public string GetLocalisedString()
			{
				return GetLocalisedString(Localisation.GetCurrentLanguage());
			}

			public string GetLocalisedString(SystemLanguage language)
			{
				if (_cachedLanguage != language || Localisation.AreGlobalVariablesOutOfDate(_cachedVariables)
#if UNITY_EDITOR
					|| !Application.isPlaying
#endif
					)
				{
					if (_localVariables != null && _localVariables.Length > 0)
						_cachedText = Localisation.Get(language, _localisationKey, _localVariables);
					else
						_cachedText = Localisation.Get(language, _localisationKey);

					_cachedLanguage = language;
					_cachedVariables = Localisation.GetGlobalVariables(_cachedText);
				}

				return _cachedText;
			}

			public string GetLocalisedString(params LocalisationLocalVariable[] variables)
			{
				_cachedLanguage = Localisation.GetCurrentLanguage();
				_cachedText = Localisation.Get(_localisationKey, variables);
				_cachedVariables = Localisation.GetGlobalVariables(_cachedText);

				return _cachedText;
			}

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(_localisationKey);
			}

			public bool Equals(string text)
			{
				return GetLocalisedString() == text;
			}

            public string GetLocalisationKey()
            {
                return _localisationKey;
            }

			public string SetVariables(params LocalisationLocalVariable[] variables)
			{
				_localVariables = variables;
				//Update cached string
				return GetLocalisedString(_localVariables);
			}

#if UNITY_EDITOR
            public SystemLanguage GetDebugLanguage()
			{
				return _cachedLanguage;
			}

			public string GetDebugText()
			{
				return _cachedText;
			}

			public void SetAutoNameParentName(string parentName)
			{
				_editorAutoNameParentName = parentName;
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
					string autoNameParent = _editorAutoNameParentName;

					//Replace _ with / so each bit will appear in separate dropdown menu (eg TextConv_Hath_Birthday will go to TextConv, Hath, Birthday)
					autoNameParent = autoNameParent.Replace('_', '/');

					//Find first free key
					int index = 0;
					while (Localisation.Exists(autoKey = autoNameParent + "/" + index.ToString("000")))
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