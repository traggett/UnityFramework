using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Utils;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(List<>), "PropertyField")]
		public static class ListEditor
		{
			//TO DO! Make reorder able.

			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				IList list = (IList)obj;
				Type listType = SystemUtils.GetGenericImplementationType(typeof(List<>), obj.GetType());

				label.text += " (" + SystemUtils.GetTypeName(listType) + ")";

				bool editorFoldout = EditorGUILayout.Foldout(true, label);

				if (editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					int origLength = list != null ? list.Count : 0;

					int length = EditorGUILayout.IntField("Length", origLength);
					length = Math.Max(length, 0);

					if (length < origLength)
					{
						while (origLength > length)
						{
							list.RemoveAt(list.Count-1);
							origLength--;
						}

						dataChanged = true;
					}
					else if (length > origLength)
					{
						while (origLength < length)
						{
							object listItem = null;

							if (!listType.IsInterface && !listType.IsAbstract)
								listItem = Activator.CreateInstance(listType, true);

							list.Add(listItem);
							origLength++;
						}

						dataChanged = true;
					}

					if (!dataChanged && list != null)
					{
						for (int i = 0; i < list.Count; i++)
						{
							bool elementChanged = false;
							object elementObj = list[i];
							Type elementType = elementObj != null ? elementObj.GetType() : listType;
							elementObj = SerializationEditorGUILayout.ObjectField(elementObj, elementType, new GUIContent("Element " + i), ref elementChanged, style, options);
							if (elementChanged)
							{
								list[i] = elementObj;
								dataChanged = true;
							}
						}
					}

					EditorGUI.indentLevel = origIndent;
				}

				return obj;
			}
			#endregion
		}
	}
}