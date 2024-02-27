using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
			[SerializeField] private string _localisationKey;
			[SerializeField] private string _localisationGUID;
			#endregion

			#region Private Data
			private LocalisationLocalVariable[] _localVariables;
			#endregion

			private LocalisedString(string key)
			{
				_localisationKey = key;
				_localisationGUID = Localisation.GUIDFromKey(key);
				_localVariables = null;
			}

			private LocalisedString(string key, params LocalisationLocalVariable[] variables)
			{
				_localisationKey = key;
				_localisationGUID = Localisation.GUIDFromKey(key);
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

			#region Equality
			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;

				LocalisedString casted = (LocalisedString)obj;
				return Equals(casted);
			}

			public bool Equals(LocalisedString other)
			{
				if (_localisationKey == other._localisationKey)
				{
					//If one has variables and one doesn't then not equal
					if (_localVariables != null && other._localVariables == null
						|| _localVariables == null && other._localVariables != null)
						return false;

					//If both have variables then check they all match
					if (_localVariables != null && other._localVariables != null
						&& _localVariables.Length == other._localVariables.Length)
					{
						for (int i = 0; i < _localVariables.Length; i++)
						{
							if (_localVariables[i]._key != other._localVariables[i]._key
								|| _localVariables[i]._value != other._localVariables[i]._value)
							{
								return false;
							}
						}
					}

					return true;
				}

				return false;
			}

			public static bool operator ==(LocalisedString a, LocalisedString b)
			{
				// If both are null, or both are same instance, return true.
				if (ReferenceEquals(a, b))
					return true;

				return Equals(a, b);
			}

			public static bool operator !=(LocalisedString a, LocalisedString b)
			{
				return !(a == b);
			}

			public override int GetHashCode()
			{
				return _localisationKey.GetHashCode();
			}
			#endregion

			public string GetLocalisedString()
			{
				return GetLocalisedString(Localisation.GetCurrentLanguage());
			}

			public string GetLocalisedString(SystemLanguage language)
			{
#if UNITY_EDITOR
				// When the application isn't running (ie in editor) then get text directly from source asset
				if (!Application.isPlaying)
				{
					var path = AssetDatabase.GUIDToAssetPath(_localisationGUID);
					LocalisedStringSourceAsset sourceAsset = AssetDatabase.LoadAssetAtPath(path, typeof(LocalisedStringSourceAsset)) as LocalisedStringSourceAsset;

					if (sourceAsset != null)
					{
						return sourceAsset.GetText(language);
					}
					else
					{
						return string.Empty;
					}
				}
#endif

				if (!string.IsNullOrEmpty(_localisationGUID))
					return Localisation.GetGUID(_localisationGUID);

				if (!string.IsNullOrEmpty(_localisationKey))
					return Localisation.Get(language, _localisationKey, _localVariables);

				return string.Empty;
			}

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(_localisationGUID) || !string.IsNullOrEmpty(_localisationKey);
			}

			public string GetLocalisationGUID()
			{
				return _localisationGUID;
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