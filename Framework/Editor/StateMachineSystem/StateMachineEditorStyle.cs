﻿using UnityEngine;
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
				public Color _stateBackground;
				public Color _stateBackgroundSelected;
				public Color _defaultStateColor;
				public Color _branchingStateColor;
				public Color _externalStateColor;
				public Color _coroutineStateColor;
				public Color _playableGraphStateColor;
				public Color _noteColor;
				public Color _debugCurrentStateColor;

				public Color _shadowColor;
				public float _shadowSize;

				public float _stateCornerRadius;
				
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

					_stateLabelStyle = new GUIStyle(EditorUtils.TextStyle);
					_stateLabelStyle.fontStyle = FontStyle.Italic;
					_stateLabelStyle.padding = new RectOffset(8, 8, 1, 1);

					_stateTextStyle = new GUIStyle(EditorUtils.TextStyle);
					_stateTextStyle.fontStyle = FontStyle.Bold;
					_stateTextStyle.padding = new RectOffset(8, 8, 1, 1);

					_externalStateTextStyle = new GUIStyle(EditorUtils.TextWhiteStyle);
					_externalStateTextStyle.fontStyle = FontStyle.Bold;

					_linkTextStyle = new GUIStyle(EditorUtils.TextStyleSmall);
					_linkTextStyle.fontSize = 10;
					_linkTextStyle.padding = new RectOffset(8, 8, 3, 3);

					_noteTextStyle = new GUIStyle(EditorUtils.TextStyle);
					_noteTextStyle.fontStyle = FontStyle.Italic;
					_noteTextStyle.padding = new RectOffset(2, 0, 2, 8);

					_defaultStateColor = Color.grey;
					_linkColor = Color.red;
					_linkInactiveColor = new Color(0.7f, 0.0f, 0.0f, 1.0f);
					_linkDescriptionColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
					_stateBackground = new Color(1.0f, 1.0f, 1.0f, 0.5f);
					_stateBackgroundSelected = new Color(1.0f, 1.0f, 1.0f, 1.0f);
					_externalStateColor = new Color(1.0f, 0.73f, 0.0f, 1.0f);
					_branchingStateColor = new Color(102 / 255f, 129 / 255f, 116 / 255f);
					_noteColor = new Color(224 / 255f, 223 / 255f, 188 / 255f);
					_debugCurrentStateColor = new Color(1.0f, 0.7f, 0.18f);
					_coroutineStateColor = new Color(156 / 255f, 68 / 255f, 68 / 255f);
					_playableGraphStateColor = new Color(245 / 255f, 212 / 255f, 179 / 255f);
					
					_shadowColor = new Color(0.0f, 0.0f, 0.0f, 0.6f);
					_shadowSize = 4.0f;

					_stateCornerRadius = 10f;
					
					_stateTextStyleFontSize = _stateTextStyle.fontSize;
					_externalStateTextStyleFontSize = _externalStateTextStyle.fontSize;
					_linkTextStyleFontSize = _linkTextStyle.fontSize;
					_noteTextStyleFontSize = _noteTextStyle.fontSize;
				}
			}
		}
	}
}