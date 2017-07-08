using UnityEngine;
using UnityEditor;

using System;
using System.Reflection;

namespace Framework
{
	using Utils;
	using Utils.Editor;
	using DynamicValueSystem;
	using Serialization;

	namespace NodeGraphSystem
	{
		namespace Editor
		{
			public sealed class NodeEditorGUI : SerializedObjectEditorGUI<Node>
			{
				#region Private Data
				private static readonly float kShadowSize = 3.0f;
				private static readonly float kLableHeight = 20.0f;
				private static readonly float kLineHeight = 16.0f;
				private static readonly float kTextPadding = 4.0f;
				private static readonly float kFieldIconPadding = 8.0f;
				private static readonly Color kFieldColor = Color.grey;
				private static readonly Color kFieldColorHighlighted = new Color(1, 1, 1, 1);
				private static readonly Color kFieldColorDisabled = new Color(0.35f, 0.35f, 0.35f, 1);
				private static readonly Color kBorderColor = new Color(1, 1, 1, 0.66f);
				private static readonly Color kBorderColorHighlighted = new Color(1, 1, 1, 1);
				private static readonly Color kShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.35f);

				private Rect _rect;
				private NodeEditorField[] _inputNodes;
				private NodeEditorField _outputNode;
				private float _inputFieldWidth;
				private float _outputFieldWidth;
				#endregion

				#region SerializedObjectEditorGUI
				protected override void OnSetObject()
				{
					FieldInfo[] inputFields = GetEditableObject().GetEditorInputFields();
					_inputNodes = new NodeEditorField[inputFields.Length];

					for (int i=0; i< _inputNodes.Length; i++)
					{
						_inputNodes[i] = new NodeEditorField();
						_inputNodes[i]._nodeEditorGUI = this;
						_inputNodes[i]._name = StringUtils.FromPropertyCamelCase(inputFields[i].Name);
						_inputNodes[i]._fieldInfo = inputFields[i];
						_inputNodes[i]._type = SystemUtils.GetGenericImplementationType(typeof(NodeInputFieldBase<>), inputFields[i].FieldType);
					}

					_outputNode = null;
					Type outputType = SystemUtils.GetGenericImplementationType(typeof(IValueSource<>), GetEditableObject().GetType());
					if (outputType != null)
					{
						_outputNode = new NodeEditorField();
						_outputNode._nodeEditorGUI = this;
						_outputNode._name = "Output";
						_outputNode._type = outputType;
					}

					_rect = new Rect();
				}

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
				#endregion

				#region ICustomEditable
				public override bool RenderObjectProperties(GUIContent label)
				{
					EditorGUI.BeginChangeCheck();
					GetEditableObject()._editorDescription = EditorGUILayout.DelayedTextField("Editor Description", GetEditableObject()._editorDescription);

					bool dataChanged = EditorGUI.EndChangeCheck();

					//Render Inputs
					bool renderedFirstInput = false;
					{
						foreach(NodeEditorField input in _inputNodes)
						{
							if (!renderedFirstInput)
							{
								EditorGUILayout.Separator();
								EditorGUILayout.LabelField("Inputs", EditorStyles.boldLabel);
								EditorGUILayout.Separator();
								renderedFirstInput = true;
							}

							string fieldName = StringUtils.FromCamelCase(input._name);
							TooltipAttribute fieldToolTipAtt = SystemUtils.GetAttribute<TooltipAttribute>(input._fieldInfo);
							GUIContent labelContent = fieldToolTipAtt != null ? new GUIContent(fieldName, fieldToolTipAtt.tooltip) : new GUIContent(fieldName);

							bool fieldChanged;
							object nodeFieldObject = input._fieldInfo.GetValue(GetEditableObject());
							nodeFieldObject = SerializationEditorGUILayout.ObjectField(nodeFieldObject, labelContent, out fieldChanged);
							if (fieldChanged)
							{
								dataChanged = true;
								input._fieldInfo.SetValue(GetEditableObject(), nodeFieldObject);
							}
						}
					}

					//Render other properties
					bool renderedFirstProperty = false;
					{
						SerializedObjectMemberInfo[] serializedFields = SerializedObjectMemberInfo.GetSerializedFields(GetEditableObject().GetType());
						foreach (SerializedObjectMemberInfo serializedField in serializedFields)
						{
							if (!serializedField.HideInEditor() && !SystemUtils.IsSubclassOfRawGeneric(typeof(NodeInputFieldBase<>), serializedField.GetFieldType()))
							{
								if (!renderedFirstProperty)
								{
									EditorGUILayout.Separator();
									EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
									EditorGUILayout.Separator();
									renderedFirstProperty = true;
								}

								string fieldName = StringUtils.FromCamelCase(serializedField.GetID());
								TooltipAttribute fieldToolTipAtt = SystemUtils.GetAttribute<TooltipAttribute>(serializedField);
								GUIContent labelContent = fieldToolTipAtt != null ? new GUIContent(fieldName, fieldToolTipAtt.tooltip) : new GUIContent(fieldName);

								bool fieldChanged;
								object nodeFieldObject = serializedField.GetValue(GetEditableObject());
								nodeFieldObject = SerializationEditorGUILayout.ObjectField(nodeFieldObject, labelContent, out fieldChanged);
								if (fieldChanged)
								{
									dataChanged = true;
									serializedField.SetValue(GetEditableObject(), nodeFieldObject);
								}
							}
						}
					}

					return dataChanged;
				}
				#endregion

				#region Public Interface
				public NodeEditorField[] GetInputFields()
				{
					return _inputNodes;
				}

				public NodeEditorField GetOutputField()
				{
					return _outputNode;
				}

				public void Render(Rect renderedRect, bool selected, GUIStyle titleStyle, GUIStyle textStyle, float scale, NodeEditorField highlightedField, NodeEditorField draggingFromField)
				{
					Color origBackgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = Color.clear;

					GUI.BeginGroup(renderedRect, EditorUtils.ColoredRoundedBoxStyle);
					{
						Rect mainBox = new Rect(0.0f, 0.0f, renderedRect.width - kShadowSize, renderedRect.height - kShadowSize);

						//Draw shadow
						EditorUtils.DrawColoredRoundedBox(new Rect(mainBox.x + kShadowSize, mainBox.y + kShadowSize, mainBox.width, mainBox.height), kShadowColor);

						//Draw white background
						EditorUtils.DrawColoredRoundedBox(mainBox, selected ? kBorderColorHighlighted : kBorderColor);

						//Draw label
						Rect labelRect = new Rect(mainBox.x + 1.0f, mainBox.y + 1.0f, mainBox.width - 2.0f, kLineHeight * 2.0f * scale);
						GUI.backgroundColor = GetEditableObject().GetEditorColor();
						GUI.BeginGroup(labelRect, EditorUtils.ColoredRoundedBoxStyle);
						{
							float h, s, v;
							Color.RGBToHSV(GetEditableObject().GetEditorColor(), out h, out s, out v);
							titleStyle.normal.textColor = v > 0.66f ? Color.black : Color.white;

							GUI.backgroundColor = Color.clear;
							titleStyle.fontStyle = FontStyle.Bold;
							//GetEditableObject()._editorDescription = GUI.TextField(new Rect(kTextPadding * scale, -2.0f * scale, labelRect.width - kTextPadding * 2.0f * scale, kLableHeight * scale), GetEditableObject()._editorDescription, titleStyle);
							EditorGUI.LabelField(new Rect(kTextPadding * scale, -2.0f * scale, labelRect.width - kTextPadding * 2.0f * scale, kLableHeight * scale), GetEditableObject()._editorDescription, titleStyle);

							string nodeTypeText = "<b>(" + StringUtils.FromPropertyCamelCase(GetEditableObject().GetType().Name) + ")</b>";
							textStyle.alignment = TextAnchor.MiddleLeft;
							GUI.Label(new Rect(kTextPadding * scale, (kLineHeight - 4.0f) * scale, labelRect.width - kTextPadding * 2.0f * scale, kLableHeight * scale), nodeTypeText, textStyle);
						}
						GUI.EndGroup();

						//Draw inputs / outputs
						Rect fieldBox = new Rect(labelRect.x, labelRect.y + labelRect.height + 1.0f, labelRect.width, kLineHeight * scale);

						//Draw output
						if (HasOutput())
						{
							fieldBox.width = _outputFieldWidth;
							fieldBox.x = labelRect.x + _inputFieldWidth + 1.0f;

							GUI.backgroundColor = kFieldColor;
							GUI.BeginGroup(fieldBox, EditorUtils.ColoredRoundedBoxStyle);
							{
								GUI.backgroundColor = Color.clear;
								string text = "Output";
								textStyle.alignment = TextAnchor.MiddleRight;
								GUI.Label(new Rect(0, 0, _outputFieldWidth - (kFieldIconPadding * scale), fieldBox.height), text, textStyle);
							}
							GUI.EndGroup();

							_outputNode._position = new Vector3(renderedRect.x + fieldBox.x + fieldBox.width, renderedRect.y + fieldBox.y + (kLineHeight * scale * 0.5f));
						}

						//Draw inputs
						{
							fieldBox.width = _inputFieldWidth;
							fieldBox.x = labelRect.x;
							foreach (NodeEditorField inputField in _inputNodes)
							{
								if (inputField == highlightedField)
									GUI.backgroundColor = kFieldColorHighlighted;
								else if (draggingFromField != null && !draggingFromField._type.IsAssignableFrom(inputField._type))
									GUI.backgroundColor = kFieldColorDisabled;
								else
									GUI.backgroundColor = kFieldColor;

								GUI.BeginGroup(fieldBox, EditorUtils.ColoredRoundedBoxStyle);
								{
									GUI.backgroundColor = Color.clear;
									textStyle.alignment = TextAnchor.MiddleLeft;
									GUI.Label(new Rect(kFieldIconPadding * scale, 0, _inputFieldWidth - (kFieldIconPadding * scale), fieldBox.height), inputField._name, textStyle);
								}
								GUI.EndGroup();

								inputField._position = new Vector2(renderedRect.x + fieldBox.x, renderedRect.y + fieldBox.y + (kLineHeight * scale * 0.5f));

								fieldBox.y += (kLineHeight * scale) + 1.0f;
							}
						}
					}
					GUI.EndGroup();

					GUI.backgroundColor = origBackgroundColor;
				}

				public void CalcBounds(GUIStyle titleStyle, GUIStyle textStyle, float scale)
				{
					_rect.position = GetPosition();

					//Work out width and height
					{
						string nodeDescriptionText = "<b>" + GetEditableObject()._editorDescription + "</b>";
						float descriptionWidth = titleStyle.CalcSize(new GUIContent(nodeDescriptionText)).x;

						string nodeTypeText = "<b>(" + StringUtils.FromPropertyCamelCase(GetEditableObject().GetType().Name) + ")</b>";
						float nodeTypeWidth = textStyle.CalcSize(new GUIContent(nodeTypeText)).x;

						float titleBoxwidth = Mathf.Max(nodeTypeWidth, descriptionWidth) + ((kTextPadding * 2.0f) * scale) + 2.0f;
						float titleBoxHeight = (2.0f * kLineHeight * scale) + 2.0f;

						int numberIOLines = 0;
						_outputFieldWidth = 0.0f;
						if (HasOutput())
						{
							_outputFieldWidth = textStyle.CalcSize(new GUIContent("Output")).x + (kFieldIconPadding + kTextPadding) * scale;
							numberIOLines = 1;
						}

						_inputFieldWidth = 0.0f;
						if (_inputNodes.Length > 0)
						{
							foreach (NodeEditorField inputField in _inputNodes)
							{
								float fieldWidth = textStyle.CalcSize(new GUIContent(inputField._name)).x + (kFieldIconPadding + kTextPadding) * scale;
								_inputFieldWidth = Mathf.Max(fieldWidth, _inputFieldWidth);
							}
							numberIOLines = Math.Max(numberIOLines, _inputNodes.Length);
						}

						float ioBoxwidth = _outputFieldWidth + _inputFieldWidth + 3.0f;
						float ioBoxHeight = 0.0f;
						if (numberIOLines > 0)
						{
							ioBoxHeight = (numberIOLines * ((kLineHeight * scale) + 1.0f));
						}

						if (ioBoxwidth < titleBoxwidth)
						{
							_rect.width = titleBoxwidth;

							float halfWidth = (titleBoxwidth - 3.0f) / 2.0f;

							if (_inputFieldWidth > halfWidth)
							{
								_outputFieldWidth = (titleBoxwidth - _inputFieldWidth) - 2.0f;
							}
							else if (_outputFieldWidth > halfWidth)
							{
								_inputFieldWidth = (titleBoxwidth - _outputFieldWidth) - 2.0f;
							}
							else
							{
								_inputFieldWidth = halfWidth;
								_outputFieldWidth = halfWidth;
							}
						}
						else
						{
							_rect.width = ioBoxwidth;
						}

						_rect.height = titleBoxHeight + ioBoxHeight;
					}

					_rect.width += kShadowSize;
					_rect.height += kShadowSize;

					_rect.x -= _rect.width * 0.5f;
					_rect.y -= _rect.height * 0.5f;

					_rect.x = Mathf.Round(_rect.position.x);
					_rect.y = Mathf.Round(_rect.position.y);
				}

				public bool HasOutput()
				{
					return !SystemUtils.IsSubclassOfRawGeneric(typeof(OutputNode<,>), GetEditableObject().GetType()) && SystemUtils.IsSubclassOfRawGeneric(typeof(IValueSource<>), GetEditableObject().GetType());
				}
				#endregion
			}
		}
	}
}