#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public static class EditorUtils
			{
				public static double kDoubleClickTime = 0.5d;

				private static GUIStyle _coloredRoundedBoxStyle = null;
				public static GUIStyle ColoredRoundedBoxStyle
				{
					get
					{
						if (_coloredRoundedBoxStyle == null)
						{
							_coloredRoundedBoxStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
							_coloredRoundedBoxStyle.alignment = TextAnchor.MiddleCenter;
							_coloredRoundedBoxStyle.fontSize = 0;
							_coloredRoundedBoxStyle.padding = new RectOffset(0, 0, 0, 0);
							_coloredRoundedBoxStyle.margin = new RectOffset(0, 0, 0, 0);
							_coloredRoundedBoxStyle.overflow = new RectOffset(0, 0, 0, 0);
							_coloredRoundedBoxStyle.border = new RectOffset(8, 8, 8, 8);
							_coloredRoundedBoxStyle.richText = false;
							_coloredRoundedBoxStyle.normal.background = RoundRectTexture;
							_coloredRoundedBoxStyle.onFocused = _coloredRoundedBoxStyle.normal;
							_coloredRoundedBoxStyle.onHover = _coloredRoundedBoxStyle.normal;
							_coloredRoundedBoxStyle.onActive = _coloredRoundedBoxStyle.normal;
							_coloredRoundedBoxStyle.onNormal = _coloredRoundedBoxStyle.normal;
						}

						return _coloredRoundedBoxStyle;
					}
				}

				private static GUIStyle _coloredHalfRoundedBoxStyle = null;
				public static GUIStyle ColoredHalfRoundedBoxStyle
				{
					get
					{
						if (_coloredHalfRoundedBoxStyle == null)
						{
							_coloredHalfRoundedBoxStyle = new GUIStyle(ColoredRoundedBoxStyle);
							_coloredHalfRoundedBoxStyle.border = new RectOffset(0, 8, 8, 8);
							_coloredHalfRoundedBoxStyle.normal.background = RoundHalfRectTexture;
						}

						return _coloredHalfRoundedBoxStyle;
					}
				}


				private static GUIStyle _toggleButton = null;
				public static GUIStyle ToggleButtonStyle
				{
					get
					{
						if (_toggleButton == null)
						{
							_toggleButton = new GUIStyle(GUI.skin.GetStyle("Button"));
						}

						return _toggleButton;
					}
				}

				private static GUIStyle _toggleButtonToggled = null;
				public static GUIStyle ToggleButtonToggledStyle
				{
					get
					{
						if (_toggleButtonToggled == null)
						{
							_toggleButtonToggled = new GUIStyle(ToggleButtonStyle);
							_toggleButtonToggled.normal.background = ToggleButtonStyle.active.background;
						}

						return _toggleButtonToggled;
					}
				}

				private static GUIStyle _textStyle = null;
				public static GUIStyle TextStyle
				{
					get
					{
						if (_textStyle == null)
						{
							_textStyle = new GUIStyle(GUI.skin.GetStyle("TextArea"));
							_textStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
							_textStyle.fontSize = 11;
							_textStyle.fontStyle = FontStyle.Normal;
							_textStyle.alignment = TextAnchor.UpperLeft;
							_textStyle.padding = new RectOffset(4, 4, 4, 4);
							_textStyle.richText = true;
							_textStyle.stretchWidth = true;
							_textStyle.stretchHeight = false;
							_textStyle.normal.background = null;
						}

						return _textStyle;
					}
				}

				private static GUIStyle _textWhiteStyle = null;
				public static GUIStyle TextWhiteStyle
				{
					get
					{
						if (_textWhiteStyle == null)
						{
							_textWhiteStyle = new GUIStyle(TextStyle);
							_textWhiteStyle.normal.textColor = Color.white;
						}

						return _textWhiteStyle;
					}
				}

				private static GUIStyle _textStyleSmall = null;
				public static GUIStyle TextStyleSmall
				{
					get
					{
						if (_textStyleSmall == null)
						{
							_textStyleSmall = new GUIStyle(TextStyle);
							_textStyleSmall.fontSize = 9;
							_textStyleSmall.fontStyle = FontStyle.Italic;
							_textStyleSmall.normal.background = OnePixelTexture;
						}

						return _textStyleSmall;
					}
				}
				
				private static GUIStyle _readonlyTextBoxStyle = null;
				public static GUIStyle ReadOnlyTextBoxStyle
				{
					get
					{
						if (_readonlyTextBoxStyle == null)
						{
							_readonlyTextBoxStyle = new GUIStyle(GUI.skin.GetStyle("TextArea"));
							_readonlyTextBoxStyle.padding = new RectOffset(6, 6, 2, 0);
							_readonlyTextBoxStyle.richText = true;
							_readonlyTextBoxStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						}

						return _readonlyTextBoxStyle;
					}
				}


				private static GUIStyle _inspectorHeaderStyle = null;
				public static GUIStyle InspectorHeaderStyle
				{
					get
					{
						if (_inspectorHeaderStyle == null)
						{
							_inspectorHeaderStyle = new GUIStyle(GUI.skin.GetStyle("In BigTitle"));
							_inspectorHeaderStyle.fontStyle = FontStyle.Bold;
						}

						return _inspectorHeaderStyle;
					}
				}

				private static GUIStyle _inspectorSubHeaderStyle = null;
				public static GUIStyle InspectorSubHeaderStyle
				{
					get
					{
						if (_inspectorSubHeaderStyle == null)
						{
							_inspectorSubHeaderStyle = new GUIStyle(GUI.skin.GetStyle("In BigTitle"));
							_inspectorSubHeaderStyle.richText = true;
							_inspectorSubHeaderStyle.font = EditorStyles.label.font;
							_inspectorSubHeaderStyle.fontSize = EditorStyles.label.fontSize;
							_inspectorSubHeaderStyle.fontStyle = FontStyle.Normal;
							_inspectorSubHeaderStyle.alignment = TextAnchor.UpperLeft;
							_inspectorSubHeaderStyle.stretchWidth = true;
							_inspectorSubHeaderStyle.stretchHeight = false;
							_inspectorSubHeaderStyle.padding = new RectOffset(8, 8, 3, 0);
						}

						return _inspectorSubHeaderStyle;
					}
				}

				private static GUIStyle _selectionRectStyle = null;
				public static GUIStyle SelectionRectStyle
				{
					get
					{
						if (_selectionRectStyle == null)
						{
							_selectionRectStyle = new GUIStyle(GUI.skin.GetStyle("selectionRect"));
						}

						return _selectionRectStyle;
					}
				}
				
				private static Texture2D _onePixelTexture = null;
				public static Texture2D OnePixelTexture
				{
					get
					{
						if (_onePixelTexture == null)
						{
							_onePixelTexture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
							_onePixelTexture.SetPixel(0, 0, Color.white);
							_onePixelTexture.Apply();
							_onePixelTexture.hideFlags = HideFlags.DontSave;
						}

						return _onePixelTexture;
					}
				}

				private static Texture2D _roundRectTexture = null;
				public static Texture2D RoundRectTexture
				{
					get
					{
						if (_roundRectTexture == null)
						{
							_roundRectTexture = Resources.Load<Texture2D>("RoundRect");
						}

						return _roundRectTexture;
					}
				}

				private static Texture2D _roundHalfRectTexture = null;
				public static Texture2D RoundHalfRectTexture
				{
					get
					{
						if (_roundHalfRectTexture == null)
						{
							_roundHalfRectTexture = Resources.Load<Texture2D>("HalfRoundRect");
						}

						return _roundHalfRectTexture;
					}
				}

				private static Texture2D _bezierAATexture = null;
				public static Texture2D BezierAATexture
				{
					get
					{
						if (_bezierAATexture == null)
						{
							_bezierAATexture = new Texture2D(1, 2, TextureFormat.ARGB32, true);
							_bezierAATexture.SetPixel(0, 0, new Color(1,1,1,0));
							_bezierAATexture.SetPixel(0, 1, Color.white);
							_bezierAATexture.Apply();
							_bezierAATexture.hideFlags = HideFlags.DontSave;
						}

						return _bezierAATexture;
					}
				}

				public static void DrawSelectionRect(Rect rect)
				{
					GUI.Label(rect, GUIContent.none, SelectionRectStyle);
				}

				public static void DrawColoredRoundedBox(Rect box, Color color)
				{
					Color origColor = GUI.backgroundColor;
					GUI.backgroundColor = color;
					GUI.Label(box, GUIContent.none, ColoredRoundedBoxStyle);
					GUI.backgroundColor = origColor;
				}

				public static void DrawColoredHalfRoundedBox(Rect box, Color color)
				{
					Color origColor = GUI.backgroundColor;
					GUI.backgroundColor = color;
					GUI.Label(box, GUIContent.none, ColoredHalfRoundedBoxStyle);
					GUI.backgroundColor = origColor;
				}

				public static void DrawSimpleInspectorHeader(string title)
				{
					Rect rect = GUILayoutUtility.GetRect(0f, 30f);
					GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height + 3f), GUIContent.none, InspectorHeaderStyle);
					EditorGUI.LabelField(new Rect(rect.x + 20, rect.y + 8, rect.width - 40, 16f), title, EditorStyles.boldLabel);
				}

				public static T GetDraggingComponent<T>() where T : class
				{
					T typedComp = null;

					foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
					{
						if (obj is Component)
						{
							typedComp = obj as T;
						}
						else
						{
							GameObject gameObj = obj as GameObject;

							if (gameObj != null)
							{
								Component[] components = gameObj.GetComponents<Component>();

								foreach (Component component in components)
								{
									typedComp = component as T;

									if (typedComp != null)
									{
										break;
									}
								}
							}
						}

						if (typedComp != null)
							break;
					}

					return typedComp;
				}

				public static float GetLabelWidth()
				{
					return EditorGUIUtility.labelWidth - 3.0f - (EditorGUI.indentLevel * 14.0f);
				}

				private static MethodInfo _editorGUIGradient_MethodInfo;
				public static Gradient GradientField(GUIContent label, Rect position, Gradient gradient)
				{
					if (_editorGUIGradient_MethodInfo == null)
					{
						BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
						_editorGUIGradient_MethodInfo = typeof(EditorGUI).GetMethod("GradientField", bindingFlags,  null, new[] { typeof(GUIContent), typeof(Rect), typeof(Gradient) }, null);
					}

					if (_editorGUIGradient_MethodInfo != null)
					{
						return (Gradient)_editorGUIGradient_MethodInfo.Invoke(null, new object[] { label, position, gradient});
					}

					return gradient;
				}

				//Draws a object field for a component type (including interfaces), which also shows a slider for choosing between multiple valid component types on the same gameObject (rather than just picking the first like unity)
				public static Component ComponentField<T>(GUIContent label, Rect position, Component currentComponent, out float height) where T : class
				{
					//Draw box for selected type of component (if T is an interface need to allow all components)
					Type componentType = typeof(T).IsInterface ? typeof(Component) : typeof(T);
					currentComponent = (Component)EditorGUI.ObjectField(position, label, currentComponent, componentType, true);
					height = EditorGUIUtility.singleLineHeight;
					
					//If the component is null or the same as before return it
					if (currentComponent == null)
					{
						return null;
					}
					//If selected a valid component
					else
					{
						Component component = currentComponent as T != null ? currentComponent : null;

						//Show drop down to allow selecting different components on same game object
						int currentIndex = 0;
						Component[] allComponents = currentComponent.gameObject.GetComponents<Component>();

						List<GUIContent> validComponentLabels = new List<GUIContent>();
						List<Component> validComponents = new List<Component>();
						List<Type> validComponentTypes = new List<Type>();

						for (int i = 0; i < allComponents.Length; i++)
						{
							T typedComponent = allComponents[i] as T;

							if (typedComponent != null)
							{
								int numberComponentsTheSameType = 0;
								foreach (Type type in validComponentTypes)
								{
									if (type == allComponents[i].GetType())
									{
										numberComponentsTheSameType++;
									}
								}

								validComponentLabels.Add(new GUIContent(allComponents[i].GetType().Name + (numberComponentsTheSameType > 0 ? " (" + numberComponentsTheSameType + ")" : "")));
								validComponents.Add(allComponents[i]);
								validComponentTypes.Add(allComponents[i].GetType());							

								if (allComponents[i] == component || component == null)
								{
									currentIndex = validComponents.Count - 1;
									component = allComponents[i];
								}
							}
						}

						//If theres only one possible component return it
						if (validComponents.Count == 1)
						{
							return component;
						}
						else if (validComponents.Count == 0)
						{
							return null;
						}
						//Otherwise if theres more than one of these components on a gameObject show slider to pick them
						else
						{
							Rect sliderPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
							int selectedIndex = EditorGUI.Popup(sliderPosition, new GUIContent(" "), currentIndex, validComponentLabels.ToArray());
							height += EditorGUIUtility.singleLineHeight;

							return validComponents[selectedIndex];
						}
					}
				}

				public enum eEditorAxis
				{
					Forward,
					Up,
					Right,
					Custom,
				}


				public static void AxisPropertyField(SerializedProperty property, GUIContent label)
				{
					Vector3 axis = property.vector3Value;

					eEditorAxis axisType = eEditorAxis.Custom;

					if (axis == Vector3.up)
						axisType = eEditorAxis.Up;
					else if (axis == Vector3.forward)
						axisType = eEditorAxis.Forward;
					else if (axis == Vector3.right)
						axisType = eEditorAxis.Right;

					EditorGUI.BeginChangeCheck();
					axisType = (eEditorAxis)EditorGUILayout.EnumPopup(label, axisType);

					if (EditorGUI.EndChangeCheck())
					{
						switch (axisType)
						{
							case eEditorAxis.Forward: property.vector3Value = Vector3.forward; break;
							case eEditorAxis.Up: property.vector3Value = Vector3.up; break;
							case eEditorAxis.Right: property.vector3Value = Vector3.right; break;
							case eEditorAxis.Custom: property.vector3Value = new Vector3(45f, 45f, 45f); break;
						}
					}

					if (axisType == eEditorAxis.Custom)
					{
						Quaternion rotation = Quaternion.identity;

						if (property.vector3Value.sqrMagnitude > 0.0f)
							rotation = Quaternion.FromToRotation(Vector3.forward, property.vector3Value);

						Vector3 eulerAngles = EditorGUILayout.Vector3Field(" ", rotation.eulerAngles);
						rotation = Quaternion.Euler(eulerAngles);

						property.vector3Value = rotation * Vector3.forward;
					}
				}

				public static float GetComponentFieldHeight<T>(Component currentComponent) where T : class
				{
					float height = EditorGUIUtility.singleLineHeight;

					if (currentComponent != null)
					{
						int numTypedComponents = 0;
						Component[] components = currentComponent.gameObject.GetComponents<Component>();

						for (int i = 0; i < components.Length; i++)
						{
							if (components[i] as T != null)
								numTypedComponents++;
						}

						if (numTypedComponents > 1)
							height += EditorGUIUtility.singleLineHeight;
					}

					return height;
				}

				public static void SetBoldDefaultFont(bool value)
				{
					MethodInfo boldFontMethodInfo = typeof(EditorGUIUtility).GetMethod("SetBoldDefaultFont", BindingFlags.Static | BindingFlags.NonPublic);
					boldFontMethodInfo.Invoke(null, new[] { value as object });
				}
			}
		}
	}
}

#endif