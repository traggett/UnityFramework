using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		[Serializable]
		public sealed class LocalisationMap
		{
			#region Public Data
			public SystemLanguage _language;
			public Dictionary<string, string> _strings = new Dictionary<string, string>();
			#endregion
			
			public string Get(string key, bool silent = false)
			{
				string text;
				
				if (!string.IsNullOrEmpty(key))
				{
					if (_strings.TryGetValue(key, out text))
					{
						return text;
					}
					else if (!silent)
					{
						return "<'" + key + "' NOT FOUND>";
					}
				}

				return string.Empty;
			}

			public bool IsValidKey(string key)
			{
				return _strings.ContainsKey(key);
			}
			
#if UNITY_EDITOR
			public void SetString(string key, string text)
			{
				if (!string.IsNullOrEmpty(key))
				{
					if (_strings.ContainsKey(key))
					{
						_strings[key] = text;
					}
					else
					{
						_strings.Add(key, text);
					}
				}
			}

			public void RemoveString(string key)
			{
				if (!string.IsNullOrEmpty(key))
				{
					_strings.Remove(key);
				}
			}

			public void ChangeKey(string key, string newKey)
			{
				string value;

				if (_strings.TryGetValue(key, out value))
				{
					_strings.Remove(key);
					_strings.Add(newKey, value);
				}
			}

			public string[] GetStringKeys()
			{
				return _strings.Keys.ToArray();
			}

			public void SortStrings()
			{
				_strings = _strings.Keys.OrderBy(k => k).ToDictionary(k => k, k => _strings[k]);
			}
#endif
		}
	}
}