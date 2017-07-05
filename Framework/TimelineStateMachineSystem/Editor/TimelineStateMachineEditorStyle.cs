using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils.Editor;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			public sealed class TimelineStateMachineEditorStyle
			{
				public GUIStyle _titleStyle;
				public GUIStyle _stateTitleStyle;
				public GUIStyle _stateTextStyle;
				public GUIStyle _externalStateTextStyle;
				public GUIStyle _linkTextStyle;
				public GUIStyle _noteTextStyle;

				public Color _defaultStateColor;
				public Color _linkColor;
				public Color _linkDescriptionColor;
				public Color _stateBackground;
				public Color _stateBackgroundSelected;
				public Color _externalStateColor;
				public Color _noteColor;
				public Color _debugCurrentStateColor;

				public int _stateTextStyleFontSize;
				public int _externalStateTextStyleFontSize;
				public int _linkTextStyleFontSize;
				public int _noteTextStyleFontSize;

				public TimelineStateMachineEditorStyle()
				{
					_titleStyle = new GUIStyle(EditorStyles.label);
					_titleStyle.richText = true;
					_titleStyle.alignment = TextAnchor.MiddleCenter;
					
					_stateTitleStyle = new GUIStyle(EditorStyles.toolbarButton);
					_stateTitleStyle.richText = true;
					_stateTextStyle = new GUIStyle(EditorUtils.TextStyle);
					_stateTextStyle.fontStyle = FontStyle.Bold;

					_externalStateTextStyle = new GUIStyle(EditorUtils.TextWhiteStyle);
					_externalStateTextStyle.fontStyle = FontStyle.Bold;

					_linkTextStyle = new GUIStyle(EditorUtils.TextStyleSmall);
					_linkTextStyle.fontSize = 11;
					_linkTextStyle.padding = new RectOffset(4, 4, 1, 1);

					_noteTextStyle = new GUIStyle(EditorUtils.TextStyle);
					_noteTextStyle.fontStyle = FontStyle.Italic;
					_noteTextStyle.padding = new RectOffset(4, 4, 1, 1);

					_defaultStateColor = Color.grey;
					_linkColor = Color.red;
					_linkDescriptionColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
					_stateBackground = new Color(1.0f, 1.0f, 1.0f, 0.5f);
					_stateBackgroundSelected = new Color(1.0f, 1.0f, 1.0f, 1.0f);
					_externalStateColor = new Color(1.0f, 0.73f, 0.0f, 1.0f);
					_noteColor = new Color(0.93f, 0.92f, 0.78f);
					_debugCurrentStateColor = new Color(1.0f, 0.7f, 0.18f);

					_stateTextStyleFontSize = _stateTextStyle.fontSize;
					_externalStateTextStyleFontSize = _externalStateTextStyle.fontSize;
					_linkTextStyleFontSize = _linkTextStyle.fontSize;
					_noteTextStyleFontSize = _noteTextStyle.fontSize;
				}
			}
		}
	}
}