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
				public static readonly float kLabelBottom = 4.0f;
				public static readonly float kLabelPadding = 8.0f;
				
				
				private bool _labelFoldout = true;
				private Rect _rect = new Rect();
				#endregion

				#region SerializedObjectEditorGUI
				public override void SetPosition(Vector2 position)
				{
					GetEditableObject()._editorPosition = position;
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

					//Render default properties
					UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(GetEditableObject());

					dataChanged |= editor.DrawDefaultInspector();

					return dataChanged;
				}
				#endregion

				#region Public Interface
				public virtual int GetStateId()
				{
					return GetEditableObject()._stateId;
				}

				public string GetStateDescription()
				{
					if (GetEditableObject()._editorAutoDescription)
						return StringUtils.GetFirstLine(GetEditableObject().GetEditorDescription());

					return StringUtils.GetFirstLine(GetEditableObject()._editorDescription);
				}

				public void Render(Rect renderedRect, StateMachineEditorStyle style, Color borderColor, float borderSize)
				{
					GUIStyle labelStyle = style._stateLabelStyle;
					GUIStyle contentStyle = GetTextStyle(style);

					Color stateColor = GetEditableObject().GetEditorColor();

					//Update text colors to contrast state color
					{
						Color.RGBToHSV(stateColor, out _, out _, out float v);
						Color textColor = v > 0.66f ? Color.black : Color.white;
						labelStyle.normal.textColor = textColor;
						contentStyle.normal.textColor = textColor;
						contentStyle.active.textColor = textColor;
						contentStyle.focused.textColor = textColor;
						contentStyle.hover.textColor = textColor;
					}

					GUIContent stateId = new GUIContent(GetEditableObject().GetEditorLabel());
					float labelHeight = labelStyle.CalcSize(stateId).y;
					borderSize = Mathf.Min(borderSize, kMaxBorderSize);
					float borderOffset = kMaxBorderSize - borderSize;

					Rect mainBoxRect = new Rect(renderedRect.x + kMaxBorderSize, renderedRect.y + kMaxBorderSize, renderedRect.width - style._shadowSize - (kMaxBorderSize * 2.0f), renderedRect.height - style._shadowSize - (kMaxBorderSize * 2.0f));

					//Draw shadow
					Rect shadowRect = new Rect(mainBoxRect.x + borderOffset + style._shadowSize, mainBoxRect.y + borderOffset + style._shadowSize, mainBoxRect.width + borderSize * 2f, mainBoxRect.height + borderSize * 2f);
					EditorUtils.DrawColoredRoundedBox(shadowRect, style._shadowColor, style._stateCornerRadius + borderSize);

					//Draw border
					Rect borderRect = new Rect(renderedRect.x + borderOffset, renderedRect.y + borderOffset, mainBoxRect.width + borderSize * 2f, mainBoxRect.height + borderSize * 2f);
					EditorUtils.DrawColoredRoundedBox(borderRect, borderColor, style._stateCornerRadius + borderSize);

					//Draw main box
					EditorUtils.DrawColoredRoundedBox(mainBoxRect, stateColor, style._stateCornerRadius);

					//Draw line seperating label and content	
					Rect seperatorRect = new Rect(mainBoxRect.x, mainBoxRect.y + labelHeight + kStateSeperationSize * 0.5f, mainBoxRect.width, 1);
					EditorUtils.DrawColoredRoundedBox(seperatorRect, borderColor, 0f);

					//Draw label
					GUI.Label(new Rect(mainBoxRect.x, mainBoxRect.y, mainBoxRect.width, labelHeight), stateId, labelStyle);

					//Draw content
					GUI.Label(new Rect(mainBoxRect.x, mainBoxRect.y + labelHeight + kStateSeperationSize, mainBoxRect.width, mainBoxRect.height - (labelHeight + kStateSeperationSize)), GetStateDescription(), contentStyle);
				}

				public void CalcBounds(StateMachineEditorStyle style)
				{
					GUIContent stateId = new GUIContent(GetEditableObject().GetEditorLabel());

					GUIStyle stateLabelStyle = style._stateLabelStyle;
					GUIStyle labelStyle = GetTextStyle(style);

					Vector2 stateIdDimensions = stateLabelStyle.CalcSize(stateId);
					Vector2 labelDimensions = GetLabelSize(labelStyle);

					float areaWidth = Mathf.Max(stateIdDimensions.x, labelDimensions.x) + kLabelPadding + style._shadowSize + (kMaxBorderSize * 2.0f);
					float areaHeight = stateIdDimensions.y + kStateSeperationSize + labelDimensions.y + kLabelBottom + style._shadowSize + (kMaxBorderSize * 2.0f);

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

				public virtual GUIStyle GetTextStyle(StateMachineEditorStyle style)
				{
					return style._stateTextStyle;
				}

				public virtual float GetBorderSize(bool selected)
				{
					return 2f;
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