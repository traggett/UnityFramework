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
				public GUIStyle _stateLabelStyle;
				public GUIStyle _stateTextStyle;
				public GUIStyle _externalStateTextStyle;
				public GUIStyle _linkTextStyle;
				public GUIStyle _noteTextStyle;

				public Color _linkColor;
				public Color _linkInactiveColor;
				public Color _linkDescriptionColor;

				public Color _stateBorderColor;
				public Color _stateBorderSelectedColor;
				public Color _stateBorderColorDebug;

				public Color _shadowColor;
				public float _shadowSize;

				public float _stateCornerRadius;

				public int _stateLabelFontSize;
				public int _stateTextFontSize;
				public int _externalStateTextStyleFontSize;
				public int _linkTextFontSize;
				public int _noteFontSize;

				public float _lineLineWidth;
				public float _linkArrowHeight;
				public float _linkArrowWidth;
				public float _linkIconWidth;

				public StateMachineEditorStyle()
				{
					_titleStyle = new GUIStyle(EditorStyles.label);
					_titleStyle.richText = true;
					_titleStyle.alignment = TextAnchor.MiddleCenter;
					
					_toolbarStyle = new GUIStyle(EditorStyles.toolbarButton);
					_toolbarStyle.richText = true;

					_stateLabelStyle = new GUIStyle(EditorUtils.TextStyle);
					_stateLabelStyle.fontStyle = FontStyle.Italic;
					_stateLabelStyle.padding = new RectOffset(8, 8, 1, 1);
					_stateLabelFontSize = _stateLabelStyle.fontSize;

					_stateTextStyle = new GUIStyle(EditorUtils.TextStyle);
					_stateTextStyle.fontStyle = FontStyle.Bold;
					_stateTextStyle.padding = new RectOffset(8, 8, 1, 1);
					_stateTextFontSize = _stateTextStyle.fontSize;

					_externalStateTextStyle = new GUIStyle(EditorUtils.TextWhiteStyle);
					_externalStateTextStyle.fontStyle = FontStyle.Bold;
					_externalStateTextStyleFontSize = _externalStateTextStyle.fontSize;

					_linkTextStyle = new GUIStyle(EditorUtils.TextStyleSmall);
					_linkTextStyle.fontSize = 10;
					_linkTextStyle.padding = new RectOffset(8, 8, 3, 3);
					_linkTextFontSize = _linkTextStyle.fontSize;

					_noteTextStyle = new GUIStyle(EditorUtils.TextStyle);
					_noteTextStyle.fontStyle = FontStyle.Italic;
					_noteTextStyle.padding = new RectOffset(2, 0, 2, 8);
					_noteFontSize = _noteTextStyle.fontSize;

					_stateBorderColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
					_stateBorderSelectedColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
					_stateBorderColorDebug = new Color(1.0f, 0.7f, 0.18f);

					_linkColor = Color.white;
					_linkInactiveColor = new Color(190f / 255f, 190f / 255f, 190f / 255f);
					_linkDescriptionColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);

					_shadowColor = new Color(0.0f, 0.0f, 0.0f, 0.6f);
					_shadowSize = 4.0f;

					_stateCornerRadius = 10f;

					_lineLineWidth = 3.0f;
					_linkArrowHeight = 8.0f;
					_linkArrowWidth = 5.0f;
					_linkIconWidth = 9.5f;
				}
			}
		}
	}
}