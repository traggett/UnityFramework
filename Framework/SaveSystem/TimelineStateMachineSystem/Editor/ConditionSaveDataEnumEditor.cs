using System;

using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Serialization;
	using StateMachineSystem;
	using StateMachineSystem.Editor;	

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(ConditionSaveDataEnum), "PropertyField")]
			public static class ConditionSaveDataEnumEditor
			{
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					Condition currentConditional = (Condition)obj;
					Condition conditional = ConditionEditor.DrawAddConditionalDropDown("Condition", currentConditional);

					if (conditional != currentConditional)
					{
						dataChanged = true;
					}

					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					if (conditional != null)
					{
						conditional = ConditionEditor.DrawToggle(conditional, ref dataChanged);

						bool editorCollapsed = !EditorGUILayout.Foldout(!conditional._editorCollapsed, "Properties");

						if (editorCollapsed != conditional._editorCollapsed)
						{
							conditional._editorCollapsed = editorCollapsed;
							dataChanged = true;
						}

						if (!editorCollapsed)
						{
							EditorGUI.indentLevel++;

							ConditionSaveDataEnum saveDataEnum = conditional as ConditionSaveDataEnum;

							if (saveDataEnum != null)
							{
								//Save data type (only show enum properties)
								saveDataEnum._saveData = SerializationEditorGUILayout.ObjectField(saveDataEnum._saveData, GUIContent.none, ref dataChanged);

								//Possible values
								Type enumType = saveDataEnum.GetEnumType();

								if (enumType != null)
								{
									Enum enm = (Enum)Enum.ToObject(enumType, saveDataEnum._value);
									EditorGUI.BeginChangeCheck();
									saveDataEnum._enumValue = EditorGUILayout.EnumPopup("Value", enm);
									if (EditorGUI.EndChangeCheck())
									{
										saveDataEnum._value = Convert.ToInt32(saveDataEnum._enumValue);
										dataChanged = true;
									}
								}
								else
								{
									saveDataEnum._enumValue = null;
									saveDataEnum._value = -1;
								}
							}
							else
							{
								conditional = (Condition)SerializationEditorGUILayout.RenderObjectMemebers(conditional, conditional.GetType(), ref dataChanged);
							}

							EditorGUI.indentLevel--;
						}
					}

					EditorGUI.indentLevel = origIndent;

					return conditional;
				}
				#endregion
			}
		}
	}
}