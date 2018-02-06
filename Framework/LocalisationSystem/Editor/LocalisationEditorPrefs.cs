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
				public float _keyWidth;
				public int _fontSize;
				public AssetRef<Font> _font;
				public string _selectedKey;
			}
		}
	}
}