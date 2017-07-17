using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils;
	using Serialization;

	namespace TimelineStateMachineSystem
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
					Condition condition = (Condition)obj;

					IConditional conditional = DrawAddConditionalDropDown("Condition", condition._conditional);

					if (condition._conditional != conditional)
					{
						condition._conditional = conditional;
						dataChanged = true;
					}

					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					if (condition._conditional != null)
					{
						if (condition._conditional.AllowInverseVariant())
						{
							EditorGUI.BeginChangeCheck();
							condition._not = EditorGUILayout.Toggle("Not", condition._not);
							if (EditorGUI.EndChangeCheck())
							{
								dataChanged = true;
							}
						}
						else if (condition._not)
						{
							condition._not = false;
							dataChanged = true;
						}

						bool foldOut = EditorGUILayout.Foldout(condition._editorFoldout, "Properties");

						if (foldOut != condition._editorFoldout)
						{
							condition._editorFoldout = foldOut;
							dataChanged = true;
						}
						
						if (condition._editorFoldout)
						{
							EditorGUI.indentLevel++;
							condition._conditional = SerializationEditorGUILayout.ObjectField(condition._conditional, "", ref dataChanged);
							EditorGUI.indentLevel--;
						}
					}

					EditorGUI.indentLevel = origIndent;

					return condition;
				}
				#endregion

				private static IConditional DrawAddConditionalDropDown(string label, IConditional obj)
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
							obj = Activator.CreateInstance(_conditionals[index]) as IConditional;
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

						Type[] types = SystemUtils.GetAllSubTypes(typeof(IConditional));

						foreach (Type type in types)
						{
							ConditionalCategoryAttribute conditionalAttribute = SystemUtils.GetAttribute<ConditionalCategoryAttribute>(type);

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