using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(ComponentMethodRef<>), "PropertyField")]
			public static class ComponentMethodRefEditor
			{
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
				{
					Type methodReturnType = SystemUtils.GetGenericImplementationType(typeof(ComponentMethodRef<>), obj.GetType());

					if (methodReturnType != null)
					{
						MethodInfo genericFieldMethod = typeof(ComponentMethodRefEditor).GetMethod("ComponentMethodRefField", BindingFlags.Static | BindingFlags.Public);
						MethodInfo typedFieldMethod = genericFieldMethod.MakeGenericMethod(methodReturnType);

						if (typedFieldMethod != null)
						{
							object[] args = new object[] { obj, methodReturnType, label, dataChanged };
							obj = typedFieldMethod.Invoke(null, args);

							if ((bool)args[3])
								dataChanged = true;
						}
					}

					return obj;
				}
				#endregion

				public static ComponentMethodRef<T> ComponentMethodRefField<T>(ComponentMethodRef<T> componentMethodRef, Type returnType, GUIContent label, ref bool dataChanged)
				{
					if (label == null)
						label = new GUIContent();

					label.text += " (" + componentMethodRef + ")";

					bool editorCollapsed = !EditorGUILayout.Foldout(!componentMethodRef._editorCollapsed, label);

					if (editorCollapsed != componentMethodRef._editorCollapsed)
					{
						componentMethodRef._editorCollapsed = editorCollapsed;
						dataChanged = true;
					}
					
					if (!editorCollapsed)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						bool componentChanged = false;
						ComponentRef<Component> component = SerializationEditorGUILayout.ObjectField(componentMethodRef.GetComponentRef(), new GUIContent("Object"), ref componentChanged);

						Component currentComponent = component.GetBaseComponent();

						if (currentComponent != null)
						{
							string[] methodNames = GetMethods(currentComponent, returnType);

							if (methodNames.Length > 0)
							{
								int currentIndex = 0;

								for (int i = 0; i < methodNames.Length; i++)
								{
									if (methodNames[i] == componentMethodRef.GetMethodName())
									{
										currentIndex = i;
										break;
									}
								}

								EditorGUI.BeginChangeCheck();

								int newIndex = EditorGUILayout.Popup("Method", currentIndex, methodNames);

								if (EditorGUI.EndChangeCheck() || componentChanged)
								{
									componentMethodRef = new ComponentMethodRef<T>(component, methodNames[newIndex]);
									dataChanged = true;
								}
							}
							else if (componentChanged)
							{
								componentMethodRef = new ComponentMethodRef<T>(component, string.Empty);
								dataChanged = true;
							}
						}
						else if (componentChanged)
						{
							componentMethodRef = new ComponentMethodRef<T>(component, string.Empty);
							dataChanged = true;
						}

						EditorGUI.indentLevel = origIndent;
					}

					return componentMethodRef;
				}

				private static string[] GetMethods(Component component, Type returnType)
				{
					//Only allow parameterless methods with return type of T?
					List<string> methodNames = new List<string>();

					MethodInfo[] methods = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

					foreach (MethodInfo method in methods)
					{
						if (method.ReturnType == returnType && method.GetParameters().Length == 0)
						{
							methodNames.Add(method.Name);
						}
					}

					return methodNames.ToArray();
				}
			}
		}
	}
}