using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Serialization;
	using Utils.Editor;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			public sealed class TimelineStateEditorGUI : SerializedObjectEditorGUI<TimelineState>
			{
				#region Public Data
				public bool IsNote
				{
					get
					{
						return GetEditableObject() is TimelineStateMachineNote;
					}
				}
				public bool IsExternal
				{
					get
					{
						return _external;
					}

					set
					{
						_external = value;
					}
				}
				private bool _external;
				public TimelineStateRef ExternalStateRef
				{
					get
					{
						return _externalStateRef;
					}

					set
					{
						_externalStateRef = value;
					}
				}
				private TimelineStateRef _externalStateRef;
				public bool ExternalHasRendered
				{
					get
					{
						return _externalHasRendered;
					}

					set
					{
						_externalHasRendered = value;
					}
				}
				#endregion

				#region Private Data
				public static readonly float kMaxBorderSize = 2.0f;
				public static readonly float kShadowSize = 4.0f;
				public static readonly Color kShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.35f);
				
				private bool _externalHasRendered;
				private bool _labelFoldout = true;
				private Rect _rect = new Rect();
				#endregion

				#region SerializedObjectEditorGUI
				public override void SetPosition(Vector2 position)
				{
					if (IsExternal)
					{
						ExternalStateRef._editorExternalLinkPosition = position;
					}
					else
					{
						GetEditableObject()._editorPosition = position;
					}

					MarkAsDirty(true);
				}

				public override Vector2 GetPosition()
				{
					if (IsExternal)
					{
						return ExternalStateRef._editorExternalLinkPosition;
					}
					else
					{
						return GetEditableObject()._editorPosition;
					}
				}

				public override Rect GetBounds()
				{
					return _rect;
				}

				protected override void OnSetObject()
				{
				}
				#endregion

				#region ICustomEditable
				public override bool RenderObjectProperties(GUIContent label)
				{
					EditorGUI.BeginChangeCheck();

					TimelineState state = GetEditableObject();

					if (IsExternal)
					{
						GUILayout.Label("External State Link: " + ExternalStateRef, EditorStyles.centeredGreyMiniLabel);
						
						EditorGUILayout.Separator();

						if (GUILayout.Button("Open State machine"))
						{
							TimelineStateMachineEditor timelineStateMachineEditor = (TimelineStateMachineEditor)GetEditor();
							timelineStateMachineEditor.LoadExternalState(this);
						}
					}
					else if (IsNote)
					{
						state._editorDescription = EditorGUILayout.TextArea(state._editorDescription);
					}
					else
					{
						_labelFoldout = EditorGUILayout.Foldout(_labelFoldout, "State Description");
						if (_labelFoldout)
						{
							int origIndent = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							state._editorAutoDescription = EditorGUILayout.Toggle("Auto", state._editorAutoDescription);

							if (!state._editorAutoDescription)
								state._editorDescription = EditorGUILayout.DelayedTextField(state._editorDescription);

							EditorGUI.indentLevel = origIndent;
						}

						_labelFoldout = EditorGUILayout.Foldout(_labelFoldout, "State Color");
						if (_labelFoldout)
						{
							int origIndent = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							state._editorAutoColor = EditorGUILayout.Toggle("Auto", state._editorAutoColor);

							if (!state._editorAutoColor)
								state._editorColor = EditorGUILayout.ColorField("Color", state._editorColor);

							EditorGUI.indentLevel = origIndent;
						}

						if (GUILayout.Button("Edit"))
						{
							TimelineStateMachineEditor timelineStateMachineEditor = (TimelineStateMachineEditor)GetEditor();
							timelineStateMachineEditor.SwitchToStateView(state._stateId);
						}
					}

					return EditorGUI.EndChangeCheck();
				}
				#endregion

				#region Public Interface
				public int GetStateId()
				{
					return GetEditableObject()._stateId;
				}

				public string GetEditorDescrition()
				{
					return GetEditableObject().GetDescription();
				}

				public void Render(Rect renderedRect, Color borderColor, Color stateColor, GUIStyle style, float borderSize)
				{
					Color origBackgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = Color.clear;

					GUI.BeginGroup(renderedRect, EditorUtils.ColoredRoundedBoxStyle);
					{
						Rect labelRect = new Rect(kMaxBorderSize, kMaxBorderSize, renderedRect.width - kShadowSize - (kMaxBorderSize * 2.0f), renderedRect.height - kShadowSize - (kMaxBorderSize * 2.0f));

						float borderOffset = kMaxBorderSize - borderSize;

						//Draw shadow
						EditorUtils.DrawColoredRoundedBox(new Rect(labelRect.x + kShadowSize, labelRect.x + kShadowSize, labelRect.width, labelRect.height), kShadowColor);

						//Draw white background
						EditorUtils.DrawColoredRoundedBox(new Rect(borderOffset, borderOffset, renderedRect.width - kShadowSize - (borderOffset * 2.0f), renderedRect.height - kShadowSize - (borderOffset * 2.0f)), borderColor);

						//Draw label with colored background
						GUI.backgroundColor = stateColor;

						GUI.BeginGroup(labelRect, EditorUtils.ColoredRoundedBoxStyle);
						{
							float h, s, v;
							Color.RGBToHSV(stateColor, out h, out s, out v);
							style.normal.textColor = v > 0.66f ? Color.black : Color.white;
							DrawLabel(style);
						}
						GUI.EndGroup();
					}
					GUI.EndGroup();

					GUI.backgroundColor = origBackgroundColor;			
				}

				public void CalcBounds(GUIStyle style)
				{
					Vector2 labelDimensions = GetLabelSize(style);

					float areaWidth = labelDimensions.x + kShadowSize + (kMaxBorderSize * 2.0f);
					float areaHeight = labelDimensions.y + kShadowSize + (kMaxBorderSize * 2.0f);

					_rect.position = GetPosition();

					_rect.width = areaWidth;
					_rect.height = areaHeight;

					_rect.x -= labelDimensions.x * 0.5f;
					_rect.y -= labelDimensions.y * 0.5f;

					_rect.x = Mathf.Round(_rect.position.x);
					_rect.y = Mathf.Round(_rect.position.y);
				}
				#endregion

				#region Private Functions
				private void DrawLabel(GUIStyle style)
				{
					string labelText = GetStateDescription();
					GUI.Label(new Rect(0, 0, _rect.width, _rect.height), labelText, style);
				}
				
				private Vector2 GetLabelSize(GUIStyle style)
				{
					string labelText = GetStateDescription();
					Vector2 labelSize = style.CalcSize(new GUIContent(labelText));
					return labelSize;
				}

				private string GetStateDescription()
				{
					if (IsExternal)
					{
						return "(External) " + (ExternalStateRef._file._editorAsset != null ? ExternalStateRef._file._editorAsset.name : null);
					}
					else
					{
						return GetEditableObject().GetDescription();
					}
				}
				#endregion
			}
		}
	}
}