using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Serialization;
	using Utils;
	using Utils.Editor;

	namespace StateMachineSystem
	{
		namespace Editor
		{
			public class StateEditorGUI : SerializedObjectEditorGUI<State>
			{
				#region Private Data
				private static Dictionary<Type, Type> _editorGUIConstructorMap = null;
				protected static readonly Color _titleLabelColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);

				public static readonly float kMaxBorderSize = 2.0f;
				public static readonly float kStateSeperationSize = 4.0f;
				public static readonly float kLabelPadding = 8.0f;
				public static readonly float kShadowSize = 4.0f;
				public static readonly Color kShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.35f);
				
				
				private bool _labelFoldout = true;
				private Rect _rect = new Rect();
				#endregion

				#region SerializedObjectEditorGUI
				public override void SetPosition(Vector2 position)
				{
					CacheUndoState();
					GetEditableObject()._editorPosition = position;
					SaveUndoState();
					MarkAsDirty(true);
				}

				public override Vector2 GetPosition()
				{
					return GetEditableObject()._editorPosition;
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
					bool dataChanged = false;

					dataChanged |= RenderStateDescriptionField();
					dataChanged |= RenderStateColorField();

					return EditorGUI.EndChangeCheck();
				}
				#endregion

				#region Public Interface
				public int GetStateId()
				{
					return GetEditableObject()._stateId;
				}

				public string GetStateDescription()
				{
					return GetEditableObject().GetDescription();
				}

				public void Render(Rect renderedRect, Color borderColor, Color stateColor, StateMachineEditorStyle style, float borderSize)
				{
					Color origBackgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = Color.clear;

					GUIStyle stateLabelStyle = style._stateIdTextStyle;
					GUIStyle labelStyle = GetTextStyle(style);

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

						GUIContent stateId = new GUIContent(GetEditableObject().GetStateIdLabel());

						GUI.BeginGroup(labelRect, EditorUtils.ColoredRoundedBoxStyle);
						{
							float h, s, v;
							Color.RGBToHSV(stateColor, out h, out s, out v);
							Color textColor = v > 0.66f ? Color.black : Color.white;
							stateLabelStyle.normal.textColor = textColor;
							labelStyle.normal.textColor = textColor;
							labelStyle.active.textColor = textColor; 
							labelStyle.focused.textColor = textColor;
							labelStyle.hover.textColor = textColor;
							
							GUI.Label(new Rect(0, 0, _rect.width, _rect.height), stateId, stateLabelStyle);

							float seperatorY = stateLabelStyle.CalcSize(stateId).y;
							Color origGUIColor = GUI.color;
							GUI.color = borderColor;
							GUI.DrawTexture(new Rect(0, seperatorY, labelRect.width, 1), EditorUtils.OnePixelTexture);
							GUI.color = origGUIColor;

							Rect labelTextRect = new Rect(kLabelPadding * 0.5f, seperatorY + kStateSeperationSize, _rect.width - kLabelPadding, _rect.height);

							GUI.Label(labelTextRect, GetStateDescription(), labelStyle);

						}
						GUI.EndGroup();
					}
					GUI.EndGroup();

					GUI.backgroundColor = origBackgroundColor;			
				}

				public void CalcBounds(StateMachineEditorStyle style)
				{
					GUIContent stateId = new GUIContent(GetEditableObject().GetStateIdLabel());

					GUIStyle stateLabelStyle = style._stateIdTextStyle;
					GUIStyle labelStyle = GetTextStyle(style);

					Vector2 stateIdDimensions = stateLabelStyle.CalcSize(stateId);
					Vector2 labelDimensions = GetLabelSize(labelStyle);

					float areaWidth = Mathf.Max(stateIdDimensions.x, labelDimensions.x) + kLabelPadding + kShadowSize + (kMaxBorderSize * 2.0f);
					float areaHeight = stateIdDimensions.y + labelDimensions.y + kStateSeperationSize + kShadowSize + (kMaxBorderSize * 2.0f);

					_rect.position = GetPosition();

					_rect.width = areaWidth;
					_rect.height = areaHeight;

					if (IsCentred())
					{
						_rect.x -= areaWidth * 0.5f;
						_rect.y -= areaHeight * 0.5f;
					}		

					_rect.x = Mathf.Round(_rect.position.x);
					_rect.y = Mathf.Round(_rect.position.y);
				}

				public static StateEditorGUI CreateStateEditorGUI(StateMachineEditor editor, State state)
				{
					if (_editorGUIConstructorMap == null)
					{
						_editorGUIConstructorMap = new Dictionary<Type, Type>();
						Type[] types = SystemUtils.GetAllSubTypes(typeof(StateEditorGUI));

						foreach (Type type in types)
						{
							StateCustomEditorGUIAttribute eventAttribute = SystemUtils.GetAttribute<StateCustomEditorGUIAttribute>(type);
							if (eventAttribute != null)
							{
								_editorGUIConstructorMap.Add(eventAttribute.StateType, type);
							}
						}
					}

					//Check for custom editor class
					Type editorGUIType;
					if (!_editorGUIConstructorMap.TryGetValue(state.GetType(), out editorGUIType))
					{
						//Use generic editor gui class
						editorGUIType = typeof(StateEditorGUI);
					}

					StateEditorGUI editorGUI = (StateEditorGUI)CreateInstance(editorGUIType);
					editorGUI.Init(editor, state);

					return editorGUI;
				}
				#endregion

				#region Virtual Interface
				public virtual void OnDoubleClick()
				{

				}

				public virtual Color GetColor(StateMachineEditorStyle style)
				{
					if (GetEditableObject()._editorAutoColor)
						return style._defaultStateColor;
					else
						return GetEditableObject()._editorColor;
				}

				public virtual GUIStyle GetTextStyle(StateMachineEditorStyle style)
				{
					return style._stateTextStyle;
				}

				public virtual float GetBorderSize(bool selected)
				{
					return selected ? 2.0f : 1.0f;
				}

				public virtual bool IsCentred()
				{
					return true;
				}
				#endregion

				#region Protected Functions
				protected Vector2 GetLabelSize(GUIStyle style)
				{
					string labelText = GetStateDescription();
					Vector2 labelSize = style.CalcSize(new GUIContent(labelText));
					return labelSize;
				}

				protected bool RenderStateDescriptionField()
				{
					bool dataChanged = false;

					_labelFoldout = EditorGUILayout.Foldout(_labelFoldout, "State Description");
					if (_labelFoldout)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						EditorGUI.BeginChangeCheck();
						GetEditableObject()._editorAutoDescription = EditorGUILayout.Toggle("Auto", GetEditableObject()._editorAutoDescription);
						dataChanged = EditorGUI.EndChangeCheck();

						if (!GetEditableObject()._editorAutoDescription)
						{
							EditorGUI.BeginChangeCheck();
							GetEditableObject()._editorDescription = EditorGUILayout.TextArea(GetEditableObject()._editorDescription);
							dataChanged |= EditorGUI.EndChangeCheck();
						}


						EditorGUI.indentLevel = origIndent;
					}

					return dataChanged;
				}

				protected bool RenderStateColorField(string label = null)
				{
					bool dataChanged = false;

					_labelFoldout = EditorGUILayout.Foldout(_labelFoldout, label == null ? "State Color" : label);
					if (_labelFoldout)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						EditorGUI.BeginChangeCheck();
						GetEditableObject()._editorAutoColor = EditorGUILayout.Toggle("Auto", GetEditableObject()._editorAutoColor);
						dataChanged = EditorGUI.EndChangeCheck();

						if (!GetEditableObject()._editorAutoColor)
						{
							EditorGUI.BeginChangeCheck();
							GetEditableObject()._editorColor = EditorGUILayout.ColorField("Color", GetEditableObject()._editorColor);
							dataChanged |= EditorGUI.EndChangeCheck();
						}


						EditorGUI.indentLevel = origIndent;
					}

					return dataChanged;
				}
				#endregion
			}
		}
	}
}