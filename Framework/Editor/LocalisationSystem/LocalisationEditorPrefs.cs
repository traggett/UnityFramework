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
				public EditorAssetRef<LocalisedStringSourceTable> _table;
				public float _keyWidth = LocalisationEditorWindow.kDefaultKeysWidth;
				public float _firstLanguageWidth = LocalisationEditorWindow.kDefaultFirstLangagueWidth;
				public SystemLanguage _secondLanguage = SystemLanguage.Unknown;
				public int _tableFontSize = LocalisationEditorWindow.kDefaultFontSize;
				public int _editorFontSize = LocalisationEditorWindow.kDefaultFontSize;
				public EditorAssetRef<Font> _font;
				public string[] _selectedItemGUIDs = new string[0];
			}
		}
	}
}