using Framework.Utils;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace LocalisationSystem
	{
		public class LocalisedStringSourceAsset : ScriptableObject
		{
			#region Serialised Data 
			[Serializable]
			public struct LanguageText
			{
				public SystemLanguage Language;
				public string Text;
			}

			[SerializeField] private LanguageText[] _text;
			[SerializeField] private LocalisedStringTableAsset _parent;
			#endregion

			#region Private Data 
#if UNITY_EDITOR
			private string _guid;
			private string _key;
#endif
			#endregion

			#region Public Methods
#if UNITY_EDITOR
			public string GUID
			{
				get 
				{ 
					return _guid; 
				} 
			}

			public string Key
			{
				get
				{
					return _key;
				}
			}

			public LocalisedStringTableAsset ParentAsset
			{
				get
				{
					return _parent;
				}
			}


			public string GetText(SystemLanguage language)
			{
				for (int i=0; i < _text.Length; i++)
				{
					if (language == _text[i].Language)
					{
						return _text[i].Text;
					}
				}

				return null;
			}

			public void SetText(SystemLanguage language, string text)
			{
				if (_text != null)
				{
					for (int i = 0; i < _text.Length; i++)
					{
						if (language == _text[i].Language)
						{
							_text[i].Text = text;
							return;
						}
					}
				}
				

				LanguageText languageText = new LanguageText()
				{
					Text = text,
					Language = language
				};


				if (_text == null || _text.Length == 0)
				{
					_text = new LanguageText[] { languageText };
				}
				else
				{
					ArrayUtils.Add(ref _text, languageText);
				}
			}

			public void CachedEditorData(LocalisedStringTableAsset root, string rootFolder)
			{
				_parent = root;

				string path = AssetDatabase.GetAssetPath(this);

				_key = path.Substring(path.IndexOf(rootFolder) + rootFolder.Length + 1);
				_key = _key.Substring(0, _key.IndexOf(".asset"));

				_guid = AssetDatabase.AssetPathToGUID(path);
			}

			public SystemLanguage[] GetLanguages()
			{
				SystemLanguage[] languages = new SystemLanguage[_text.Length];

				for (int i=0; i<_text.Length; i++)
				{
					languages[i] = _text[i].Language;
				}

				return languages;
			}
#endif
			#endregion
		}
	}
}