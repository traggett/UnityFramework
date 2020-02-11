using System;
using UnityEngine;

namespace Framework
{
	using Utils;

	namespace LocalisationSystem
	{
		namespace Editor
		{
			[Serializable]
			public sealed class LocalisationEditorPrefs
			{
				public float _keyWidth = LocalisationEditorWindow.kDefaultKeysWidth;
				public float _firstLanguageWidth = LocalisationEditorWindow.kDefaultFirstLangagueWidth;
				public SystemLanguage _secondLanguage = SystemLanguage.Russian;
				public int _fontSize = LocalisationEditorWindow.kDefaultFontSize;
				public EditorAssetRef<Font> _font;
				public string[] _selectedKeys = new string[0];
			}
		}
	}
}