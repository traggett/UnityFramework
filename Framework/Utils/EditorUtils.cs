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

				private static GUIStyle _titleStyle = null;
				public static GUIStyle TextTitleStyle
				{
					get
					{
						if (_titleStyle == null)
						{
							_titleStyle = new GUIStyle(GUI.skin.GetStyle("In BigTitle"));
							_titleStyle.richText = true;
							_titleStyle.font = EditorStyles.label.font;
							_titleStyle.fontSize = EditorStyles.label.fontSize;
							_titleStyle.fontStyle = FontStyle.Normal;
							_titleStyle.alignment = TextAnchor.UpperLeft;
							_titleStyle.stretchWidth = true;
							_titleStyle.stretchHeight = false;
						}

						return _titleStyle;
					}
				}

				private static GUIStyle _readOnlyTextBoxStyle = null;
				public static GUIStyle ReadOnlyTextBoxStyle
				{
					get
					{
						if (_readOnlyTextBoxStyle == null)
						{
							_readOnlyTextBoxStyle = new GUIStyle(EditorStyles.textArea);
							_readOnlyTextBoxStyle.fontStyle = FontStyle.Italic;
							_readOnlyTextBoxStyle.normal.textColor = new Color(0, 0, 0, 0.6f);
							_readOnlyTextBoxStyle.active.textColor = _readOnlyTextBoxStyle.normal.textColor;
							_readOnlyTextBoxStyle.focused.textColor = _readOnlyTextBoxStyle.normal.textColor;
						}

						return _readOnlyTextBoxStyle;
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
						}

						return _inspectorHeaderStyle;
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

				public static T GetPropertyDrawerTargetObject<T>(PropertyDrawer propertyDrawer, SerializedProperty property)
				{
					return (T)propertyDrawer.fieldInfo.GetValue(property.serializedObject.targetObject);
				}

				public static T[] GetSelectedPropertyDrawerTargetObject<T>(PropertyDrawer propertyDrawer, SerializedProperty property)
				{
					T[] selectedStructs = new T[property.serializedObject.targetObjects.Length];

					for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
					{
						selectedStructs[i] = (T)propertyDrawer.fieldInfo.GetValue(property.serializedObject.targetObjects[i]);
					}

					return selectedStructs;
				}

				public static void SavePropertyDrawerTargetObject<T>(PropertyDrawer propertyDrawer, SerializedProperty property, T newValue)
				{
					propertyDrawer.fieldInfo.SetValue(property.serializedObject.targetObject, newValue);
				}

				public static void SaveSelectedPropertyDrawerTargetObject<T>(PropertyDrawer propertyDrawer, SerializedProperty property, T[] newValues)
				{
					for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
					{
						propertyDrawer.fieldInfo.SetValue(property.serializedObject.targetObjects[i], newValues[i]);
					}
				}

				public static void OnPropertyDrawerTargetObjectsChanged(SerializedProperty property, string info)
				{
					Undo.RecordObject(property.serializedObject.targetObject, info);

					for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
					{
						Undo.RecordObject(property.serializedObject.targetObjects[i], info);
					}
				}

				public static float GetLabelWidth()
				{
					return EditorGUIUtility.labelWidth - 4.0f - (EditorGUI.indentLevel * 13.0f);
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

				public static object GetTargetObjectOfProperty(SerializedProperty prop)
				{
					var path = prop.propertyPath.Replace(".Array.data[", "[");
					object obj = prop.serializedObject.targetObject;
					var elements = path.Split('.');
					foreach (var element in elements)
					{
						if (element.Contains("["))
						{
							var elementName = element.Substring(0, element.IndexOf("["));
							var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
							obj = GetValue_Imp(obj, elementName, index);
						}
						else
						{
							obj = GetValue_Imp(obj, element);
						}
					}
					return obj;
				}

				private static object GetValue_Imp(object source, string name)
				{
					if (source == null)
						return null;
					var type = source.GetType();

					while (type != null)
					{
						var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
						if (f != null)
							return f.GetValue(source);

						var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
						if (p != null)
							return p.GetValue(source, null);

						type = type.BaseType;
					}
					return null;
				}

				private static object GetValue_Imp(object source, string name, int index)
				{
					var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
					if (enumerable == null) return null;
					var enm = enumerable.GetEnumerator();
					//while (index-- >= 0)
					//    enm.MoveNext();
					//return enm.Current;

					for (int i = 0; i <= index; i++)
					{
						if (!enm.MoveNext()) return null;
					}
					return enm.Current;
				}
			}
		}
	}
}

#endif