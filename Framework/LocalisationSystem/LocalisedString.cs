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
			private LocalisationLocalVariable[] _localVariables;
			private string _cachedText;
			private SystemLanguage _cachedLanguage;
			private LocalisationGlobalVariable[] _cachedVariables;
			#endregion

			private LocalisedString(string key)
			{
				_localisationKey = key;
				_localVariables = null;
				_cachedText = string.Empty;
				_cachedLanguage = SystemLanguage.Unknown;
				_cachedVariables = null;
				
			}

			private LocalisedString(string key, params LocalisationLocalVariable[] variables)
			{
				_localisationKey = key;
				_localVariables = variables;
				_cachedText = string.Empty;
				_cachedLanguage = SystemLanguage.Unknown;
				_cachedVariables = null;
			}

			public static LocalisedString Dynamic(string key, params LocalisationLocalVariable[] variables)
			{
				return new LocalisedString(key, variables);
			}

			public static LocalisedString Dynamic(LocalisedString localisedString, params LocalisationLocalVariable[] variables)
			{
				return new LocalisedString(localisedString.GetLocalisationKey(), variables);
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
					UpdateCachedText(language);
				}

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

			public void SetVariables(params LocalisationLocalVariable[] variables)
			{
				_localVariables = variables;
				UpdateCachedText(Localisation.GetCurrentLanguage());
			}

			private string UpdateCachedText(SystemLanguage language)
			{
				if (_localVariables != null && _localVariables.Length > 0)
					_cachedText = Localisation.Get(language, _localisationKey, _localVariables);
				else
					_cachedText = Localisation.Get(language, _localisationKey);

				_cachedLanguage = language;
				_cachedVariables = Localisation.GetGlobalVariables(_cachedText);

				return _cachedText;
			}

		}
	}
}