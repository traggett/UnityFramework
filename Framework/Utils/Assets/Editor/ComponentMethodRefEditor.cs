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
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					Type methodReturnType = SystemUtils.GetGenericImplementationType(typeof(ComponentMethodRef<>), obj.GetType());

					if (methodReturnType != null)
					{
						MethodInfo genericFieldMethod = typeof(ComponentMethodRefEditor).GetMethod("ComponentMethodRefField", BindingFlags.Static | BindingFlags.NonPublic);
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

					bool foldOut = EditorGUILayout.Foldout(componentMethodRef._editorFoldout, label);

					if (foldOut != componentMethodRef._editorFoldout)
					{
						componentMethodRef._editorFoldout = foldOut;
						dataChanged = true;
					}
					
					if (foldOut)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						componentMethodRef._object = SerializationEditorGUILayout.ObjectField(componentMethodRef._object, new GUIContent("Object"), ref dataChanged);

						if (componentMethodRef._object._editorComponent != null)
						{
							string[] methodNames = GetMethods(componentMethodRef._object._editorComponent, returnType);

							if (methodNames.Length > 0)
							{
								int currentIndex = 0;

								for (int i = 0; i < methodNames.Length; i++)
								{
									if (methodNames[i] == componentMethodRef._methodName)
									{
										currentIndex = i;
										break;
									}
								}

								EditorGUI.BeginChangeCheck();

								int newIndex = EditorGUILayout.Popup("Method", currentIndex, methodNames);

								if (EditorGUI.EndChangeCheck())
								{
									componentMethodRef._methodName = methodNames[newIndex];
									dataChanged = true;
								}
							}
							else
							{
								componentMethodRef._methodName = null;
							}
						}

						EditorGUI.indentLevel = origIndent;
					}

					return componentMethodRef;
				}

				private static string[] GetMethods(Component component, Type returnType)
				{
					//Only allow parameterless methods with return type of T?
					List<string> methodNames = new List<string>();

					MethodInfo[] methods = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);

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