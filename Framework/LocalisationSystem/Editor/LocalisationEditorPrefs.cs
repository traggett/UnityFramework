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
				public int _fontSize = LocalisationEditorWindow.kDefaultFontSize;
				public EditorAssetRef<Font> _font;
				public string _selectedKey;
			}
		}
	}
}