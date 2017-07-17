using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils.Editor;

	namespace StateMachineSystem
	{
		namespace Editor
		{
			public sealed class StateMachineEditorStyle
			{
				public GUIStyle _titleStyle;
				public GUIStyle _toolbarStyle;
				public GUIStyle _stateIdTextStyle;
				public GUIStyle _stateTextStyle;
				public GUIStyle _externalStateTextStyle;
				public GUIStyle _linkTextStyle;
				public GUIStyle _noteTextStyle;

				public Color _linkColor;
				public Color _linkDescriptionColor;
				public Color _stateBackground;
				public Color _stateBackgroundSelected;
				public Color _defaultStateColor;
				public Color _branchingStateColor;
				public Color _externalStateColor;
				public Color _coroutineStateColor;
				public Color _noteColor;
				public Color _debugCurrentStateColor;

				public int _stateTextStyleFontSize;
				public int _externalStateTextStyleFontSize;
				public int _linkTextStyleFontSize;
				public int _noteTextStyleFontSize;

				public StateMachineEditorStyle()
				{
					_titleStyle = new GUIStyle(EditorStyles.label);
					_titleStyle.richText = true;
					_titleStyle.alignment = TextAnchor.MiddleCenter;
					
					_toolbarStyle = new GUIStyle(EditorStyles.toolbarButton);
					_toolbarStyle.richText = true;

					_stateIdTextStyle = new GUIStyle(EditorUtils.TextStyle);
					_stateIdTextStyle.fontStyle = FontStyle.Italic;
					_stateIdTextStyle.padding = new RectOffset(4, 4, 1, 1);

					_stateTextStyle = new GUIStyle(EditorUtils.TextStyle);
					_stateTextStyle.fontStyle = FontStyle.Bold;

					_externalStateTextStyle = new GUIStyle(EditorUtils.TextWhiteStyle);
					_externalStateTextStyle.fontStyle = FontStyle.Bold;

					_linkTextStyle = new GUIStyle(EditorUtils.TextStyleSmall);
					_linkTextStyle.fontSize = 10;
					_linkTextStyle.padding = new RectOffset(4, 4, 3, 3);

					_noteTextStyle = new GUIStyle(EditorUtils.TextStyle);
					_noteTextStyle.fontStyle = FontStyle.Italic;
					_noteTextStyle.padding = new RectOffset(2, 0, 2, 8);

					_defaultStateColor = Color.grey;
					_linkColor = Color.red;
					_linkDescriptionColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
					_stateBackground = new Color(1.0f, 1.0f, 1.0f, 0.5f);
					_stateBackgroundSelected = new Color(1.0f, 1.0f, 1.0f, 1.0f);
					_externalStateColor = new Color(1.0f, 0.73f, 0.0f, 1.0f);
					_branchingStateColor = new Color(102 / 255f, 129 / 255f, 116 / 255f);
					_noteColor = new Color(0.93f, 0.92f, 0.78f);
					_debugCurrentStateColor = new Color(1.0f, 0.7f, 0.18f);
					_coroutineStateColor = new Color(156 / 255f, 68 / 255f, 68 / 255f);


					_stateTextStyleFontSize = _stateTextStyle.fontSize;
					_externalStateTextStyleFontSize = _externalStateTextStyle.fontSize;
					_linkTextStyleFontSize = _linkTextStyle.fontSize;
					_noteTextStyleFontSize = _noteTextStyle.fontSize;
				}
			}
		}
	}
}