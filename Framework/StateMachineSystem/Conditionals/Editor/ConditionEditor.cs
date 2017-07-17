using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils;
	using Serialization;

	namespace StateMachineSystem
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(Condition), "PropertyField")]
			public static class ConditionEditor
			{
				private static Type[] _conditionals;
				private static string[] _conditionalNames;

				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					Condition currentConditional = (Condition)obj;
					Condition conditional = DrawAddConditionalDropDown("Condition", currentConditional);

					if (conditional != currentConditional)
					{
						dataChanged = true;
					}

					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					if (conditional != null)
					{
						if (conditional is ToggableCondition)
						{
							ToggableCondition boolConditional = (ToggableCondition)conditional;

							EditorGUI.BeginChangeCheck();
							boolConditional._not = EditorGUILayout.Toggle("Not", boolConditional._not);
							if (EditorGUI.EndChangeCheck())
							{
								dataChanged = true;
							}
						}						

						bool foldOut = EditorGUILayout.Foldout(conditional._editorFoldout, "Properties");

						if (foldOut != conditional._editorFoldout)
						{
							conditional._editorFoldout = foldOut;
							dataChanged = true;
						}
						
						if (conditional._editorFoldout)
						{
							EditorGUI.indentLevel++;
							conditional = SerializationEditorGUILayout.ObjectField(conditional, "", ref dataChanged);
							EditorGUI.indentLevel--;
						}
					}

					EditorGUI.indentLevel = origIndent;

					return conditional;
				}
				#endregion

				private static Condition DrawAddConditionalDropDown(string label, Condition obj)
				{
					BuildConditionalMap();

					int currIndex = 0;
					if (obj != null)
					{
						for (int i = 0; i < _conditionals.Length; i++)
						{
							if (_conditionals[i] == obj.GetType())
							{
								currIndex = i;
								break;
							}
						}
					}
					
					int index = EditorGUILayout.Popup(label, currIndex, _conditionalNames);

					//If type has changed create new conditional
					if (currIndex != index)
					{
						if (index > 0)
							obj = Activator.CreateInstance(_conditionals[index]) as Condition;
						else
							obj = null;
					}

					return obj;
				}

				private static void BuildConditionalMap()
				{
					if (_conditionals == null)
					{
						List<Type> conditionals = new List<Type>();
						List<string> conditionalNames = new List<string>();

						conditionals.Add(null);
						conditionalNames.Add("<none>");

						Type[] types = SystemUtils.GetAllSubTypes(typeof(Condition));

						foreach (Type type in types)
						{
							ConditionCategoryAttribute conditionalAttribute = SystemUtils.GetAttribute<ConditionCategoryAttribute>(type);

							string category;

							if (conditionalAttribute != null && !string.IsNullOrEmpty(conditionalAttribute.Category))
							{
								category = conditionalAttribute.Category + "/" + type.Name;
							}
							else
							{
								category = type.Name;
							}

							conditionals.Add(type);
							conditionalNames.Add(category);
						}

						_conditionals = conditionals.ToArray();
						_conditionalNames = conditionalNames.ToArray();
					}
				}
			}
		}
	}
}