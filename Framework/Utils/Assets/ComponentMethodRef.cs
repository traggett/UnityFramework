using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		[Serializable]
		public class ComponentMethodRef<T> : ICustomEditorInspector
		{
			#region Public Data
			public ComponentRef<Component> _object;
			public string _methodName = string.Empty;
			#endregion

#if UNITY_EDITOR
			private bool _editorFoldout = true;
#endif

			public static implicit operator string(ComponentMethodRef<T> property)
			{
				return property._object + "." + property._methodName + "()";
			}

			public T RunMethod()
			{
				T returnObj = default(T);

				Component component = _object.GetComponent();

				if (component != null)
				{
					MethodInfo method = component.GetType().GetMethod(_methodName);

					if (method != null)
					{
						returnObj = (T)method.Invoke(component, null);
					}
				}

				return returnObj;
			}

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				if (label == null)
					label = new GUIContent();

				label.text += " (" + this + ")";

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);
				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					dataChanged |= _object.RenderObjectProperties(new GUIContent("Object"));

					if (_object.GetEditorComponent() != null)
					{
						string[] methodNames = GetMethods(_object.GetEditorComponent());
				
						if (methodNames.Length > 0)
						{
							int currentIndex = 0;

							for (int i= 0; i < methodNames.Length; i++)
							{
								if (methodNames[i] == _methodName)
								{
									currentIndex = i;
									break;
								}
							}

							EditorGUI.BeginChangeCheck();

							int newIndex = EditorGUILayout.Popup("Method", currentIndex, methodNames);

							if (EditorGUI.EndChangeCheck())
							{
								_methodName = methodNames[newIndex];
								dataChanged = true;
							}
						}
						else
						{
							_methodName = null;
						}
					}

					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion

			#region Private Functions
			protected virtual Type GetReturnType()
			{
				return typeof(T);
			}

			private string[] GetMethods(Component component)
			{
				//Only allow parameterless methods with return type of T?
				List<string> methodNames = new List<string>();

				MethodInfo[] methods = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);

				foreach (MethodInfo method in methods)
				{
					if (method.ReturnType == GetReturnType() && method.GetParameters().Length == 0)
					{
						methodNames.Add(method.Name);
					}
				}

				return methodNames.ToArray();
			}
			#endregion
		}
	}
}