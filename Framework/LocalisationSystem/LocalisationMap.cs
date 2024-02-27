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
			#region Serialised Data
			[SerializeField] private SystemLanguage _language;
			[SerializeField] private Dictionary<string, string> _strings;
			#endregion

			#region Private Data
			private Dictionary<string, string> _keysToGUIDs;
			private Dictionary<string, string> _GUIDToKeys;
			#endregion

			#region Public Properties
			public SystemLanguage Language { get { return _language; } }
			#endregion

			#region Public Interface
			public LocalisationMap()
			{
				_language = SystemLanguage.Unknown;
				_strings = new Dictionary<string, string>();
				_keysToGUIDs = new Dictionary<string, string>();
				_GUIDToKeys = new Dictionary<string, string>();
			}

			public LocalisationMap(SystemLanguage language)
			{
				_language = language;
				_strings = new Dictionary<string, string>();
				_keysToGUIDs = new Dictionary<string, string>();
				_GUIDToKeys = new Dictionary<string, string>();
			}

			public string Get(string guid, bool silent = false)
			{
				string text;
				
				if (!string.IsNullOrEmpty(guid))
				{
					if (_strings.TryGetValue(guid, out text))
					{
						return text;
					}
					else if (!silent)
					{
						return "<'" + guid + "' NOT FOUND>";
					}
				}

				return string.Empty;
			}

			public string KeyFromGUID(string guid, bool silent = false)
			{
				if (!string.IsNullOrEmpty(guid))
				{
					if (_GUIDToKeys.TryGetValue(guid, out string key))
					{
						return key;
					}
					else if (!silent)
					{
						return "<'" + guid + "' NOT FOUND>";
					}
				}

				return null;
			}

			public string GUIDFromKey(string key, bool silent = false)
			{
				if (!string.IsNullOrEmpty(key))
				{
					if (_keysToGUIDs.TryGetValue(key, out string guid))
					{
						return guid;
					}
					else if (!silent)
					{
						return "<'" + key + "' NOT FOUND>";
					}
				}

				return null;
			}

			public bool IsValidKey(string key)
			{
				return _strings.ContainsKey(key);
			}
			#endregion

			#region Public Editor Functions
#if UNITY_EDITOR
			public void SetString(string guid, string key, string text)
			{
				if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(key))
				{
					if (_strings.ContainsKey(guid))
					{
						_strings[guid] = text;
					}
					else
					{
						_strings.Add(guid, text);
					}

					_GUIDToKeys[guid] = key;
					_keysToGUIDs[key] = guid;
				}
			}
			
			public string[] GetStringGUIDs()
			{
				return _strings.Keys.ToArray();
			}
#endif
			#endregion
		}
	}
}