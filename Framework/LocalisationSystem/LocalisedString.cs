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
			#endregion

			private LocalisedString(string key)
			{
				_localisationKey = key;
				_localVariables = null;
			}

			private LocalisedString(string key, params LocalisationLocalVariable[] variables)
			{
				_localisationKey = key;
				_localVariables = variables;
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
				if (!string.IsNullOrEmpty(_localisationKey))
					return Localisation.Get(language, _localisationKey, _localVariables);

				return string.Empty;
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
			}
		}
	}
}