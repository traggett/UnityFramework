using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils;
	using Serialization;
	using System.Reflection;

	namespace SaveSystem
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(SaveDataValueRef<>), "PropertyField")]
			public static class SaveDataValueRefEditor
			{
				private static Type[] _saveDataTypes = null;

				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
				{
					Type saveDataRefType = SystemUtils.GetGenericImplementationType(typeof(SaveDataValueRef<>), obj.GetType());

					if (saveDataRefType != null)
					{
						MethodInfo genericFieldMethod = typeof(SaveDataValueRefEditor).GetMethod("SaveDataRefField", BindingFlags.Static | BindingFlags.NonPublic);
						MethodInfo typedFieldMethod = genericFieldMethod.MakeGenericMethod(saveDataRefType);

						if (typedFieldMethod != null)
						{
							if (label == null || label == GUIContent.none)
							{
								label = new GUIContent("Save Data (" + obj + ")");
							}

							object[] args = new object[] { obj, label, dataChanged, saveDataRefType };
							obj = typedFieldMethod.Invoke(null, args);

							if ((bool)args[2])
								dataChanged = true;
						}
					}

					return obj;
				}
				#endregion

				private static SaveDataValueRef<T> SaveDataRefField<T>(SaveDataValueRef<T> saveDataValueRef, GUIContent label, ref bool dataChanged, Type allowedType)
				{
					bool editorCollapsed = !EditorGUILayout.Foldout(!saveDataValueRef._editorCollapsed, label);

					if (editorCollapsed != saveDataValueRef._editorCollapsed)
					{
						saveDataValueRef._editorCollapsed = editorCollapsed;
						dataChanged = true;
					}

					if (!editorCollapsed)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						//Draw save data type dropdown
						bool hasChangedSaveDataType = false;
						{
							if (_saveDataTypes == null)
							{
								_saveDataTypes = SaveData.GetSaveDataBlockTypes();
							}

							string[] branchTypeNames = new string[_saveDataTypes.Length];
							int index = 0;
							int saveTypeCurrIndex = 0;
							foreach (Type type in _saveDataTypes)
							{
								if (type == saveDataValueRef.GetSaveDataType())
									saveTypeCurrIndex = index;

								branchTypeNames[index++] = type.Name;
							}

							EditorGUI.BeginChangeCheck();
							saveTypeCurrIndex = EditorGUILayout.Popup("Save Data Type", saveTypeCurrIndex, branchTypeNames);
							if (EditorGUI.EndChangeCheck())
							{
								saveDataValueRef = new SaveDataValueRef<T>(_saveDataTypes[saveTypeCurrIndex]);
								hasChangedSaveDataType = true;
							}
						}

						List<string> propertyNames = new List<string>();
						List<SerializedObjectMemberInfo> properties = new List<SerializedObjectMemberInfo>();

						int currIndex = -1;
						int count = 0;

						if (saveDataValueRef.GetSaveDataType() != null)
						{
							foreach (SerializedObjectMemberInfo childField in SerializedObjectMemberInfo.GetSerializedFields(saveDataValueRef.GetSaveDataType()))
							{
								if (allowedType == null || SystemUtils.IsTypeOf(allowedType, childField.GetFieldType()))
								{
									string valueLabel = StringUtils.FromCamelCase(childField.GetID());

									if (string.Equals(childField.GetID(), saveDataValueRef.GetSaveValueID()))
									{
										currIndex = count;
									}

									propertyNames.Add(valueLabel);
									properties.Add(childField);
									count++;
								}
							}
						}

						if (propertyNames.Count > 0)
						{
							currIndex = Mathf.Clamp(currIndex, 0, propertyNames.Count);
							int newIndex = EditorGUILayout.Popup("Property", currIndex, propertyNames.ToArray());

							if (hasChangedSaveDataType || newIndex != currIndex)
							{
								dataChanged = true;
								saveDataValueRef = new SaveDataValueRef<T>(saveDataValueRef.GetSaveDataType(), properties[newIndex].GetID(), propertyNames[newIndex]);
							}
						}
						else if (hasChangedSaveDataType)
						{
							dataChanged = true;
							saveDataValueRef = new SaveDataValueRef<T>(saveDataValueRef.GetSaveDataType());
						}

						EditorGUI.indentLevel = origIndent;
					}

					return saveDataValueRef;
				}
				
			}
		}
	}
}