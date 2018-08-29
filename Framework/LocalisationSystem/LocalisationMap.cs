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
			public string _language;
			public Dictionary<string, string> _strings = new Dictionary<string, string>();
			#endregion
			
			public string GetString(string key)
			{
				string text;
				
				if (!string.IsNullOrEmpty(key))
				{
					if (_strings.TryGetValue(key, out text))
					{
						return text;
					}
					else
					{
						Debug.Log("Can't find localised version of string " + key);
						return "<'" + key + "' NOT FOUND>";
					}
				}

				return string.Empty;
			}

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

			public bool IsValidKey(string key)
			{
				return _strings.ContainsKey(key);
			}

			public string[] GetStringKeys()
			{
				return _strings.Keys.ToArray();
			}



		}
	}
}