using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils;
	using Utils.Editor;

	namespace StateMachineSystem
	{
		namespace Editor
		{
			public class StateEditorGUI : ScriptableObjectHierarchyEditorObjectGUI<StateMachine, State>
			{
				#region Private Data
				private static Dictionary<Type, Type> _editorGUIConstructorMap = null;
				protected static readonly Color _titleLabelColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);

				public static readonly float kMaxBorderSize = 2.0f;
				public static readonly float kStateSeperationSize = 4.0f;
				public static readonly float kLabelBottom = 4.0f;
				public static readonly float kLabelPadding = 8.0f;
				
				
				private bool _labelFoldout = true;
				protected Rect _rect = new Rect();
				#endregion

				#region ScriptableObjectHierarchyEditorObjectGUI
				public override void SetPosition(Vector2 position)
				{
					Undo.RecordObject(Asset, "Move State");
					Asset._editorPosition = position;
				}

				public override Vector2 GetPosition()
				{
					return Asset._editorPosition;
				}

				public override Rect GetBounds()
				{
					return _rect;
				}

				protected override void OnSetObject()
				{
				}
				#endregion

				#region Public Interface
				public string GetStateDescription()
				{
					if (Asset._editorAutoDescription)
						return Asset.GetEditorDescription();

					return Asset._editorDescription;
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

					StateEditorGUI editorGUI = (StateEditorGUI)editorGUIType.GetConstructor(new Type[0]).Invoke(new object[0]);
					editorGUI.Init(editor, state);

					return editorGUI;
				}
				#endregion

				#region Virtual Interface
				public virtual void CalcRenderRect(StateMachineEditorStyle style)
				{
					GUIContent label = new GUIContent(Asset.GetEditorLabel());
					GUIContent content = new GUIContent(GetStateDescription());

					GUIStyle labelStyle = style._stateLabelStyle;
					GUIStyle textStyle = style._stateTextStyle;

					Vector2 labelDimensions = labelStyle.CalcSize(label);
					Vector2 contentDimensions = textStyle.CalcSize(content);

					float areaWidth = Mathf.Max(labelDimensions.x, contentDimensions.x) + kLabelPadding + style._shadowSize + (kMaxBorderSize * 2.0f);
					float areaHeight = labelDimensions.y + kStateSeperationSize + contentDimensions.y + kLabelBottom + style._shadowSize + (kMaxBorderSize * 2.0f);

					_rect.position = GetPosition();

					_rect.width = areaWidth;
					_rect.height = areaHeight;

					_rect.x -= areaWidth * 0.5f;
					_rect.y -= areaHeight * 0.5f;

					_rect.x = Mathf.Round(_rect.position.x);
					_rect.y = Mathf.Round(_rect.position.y);
				}

				public virtual void Render(Rect renderedRect, StateMachineEditorStyle style, Color borderColor, float borderSize)
				{
					GUIStyle labelStyle = style._stateLabelStyle;
					GUIStyle contentStyle = style._stateTextStyle;

					Color stateColor = Asset.GetEditorColor();

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

					GUIContent stateId = new GUIContent(Asset.GetEditorLabel());
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

				public virtual void OnDoubleClick()
				{
					
				}
				#endregion

				#region Protected Functions
				protected bool RenderStateDescriptionField()
				{
					bool dataChanged = false;

					_labelFoldout = EditorGUILayout.Foldout(_labelFoldout, "State Description");
					if (_labelFoldout)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						EditorGUI.BeginChangeCheck();
						Asset._editorAutoDescription = EditorGUILayout.Toggle("Auto", Asset._editorAutoDescription);
						dataChanged = EditorGUI.EndChangeCheck();

						if (!Asset._editorAutoDescription)
						{
							EditorGUI.BeginChangeCheck();
							Asset._editorDescription = EditorGUILayout.TextArea(Asset._editorDescription);
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
						Asset._editorAutoColor = EditorGUILayout.Toggle("Auto", Asset._editorAutoColor);
						dataChanged = EditorGUI.EndChangeCheck();

						if (!Asset._editorAutoColor)
						{
							EditorGUI.BeginChangeCheck();
							Asset._editorColor = EditorGUILayout.ColorField("Color", Asset._editorColor);
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