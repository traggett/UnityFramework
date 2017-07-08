using System;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Utils;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public sealed class Condition : ICustomEditorInspector
		{
			#region Public Data		
			public IConditional _condition;
			public bool _not;
			#endregion

			#region Private Data		
#if UNITY_EDITOR
			private bool _editorFoldout = true;
			private static Type[] _conditionals;
			private static string[] _conditionalNames;
#endif
			#endregion

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				IConditional condition = DrawAddConditionalDropDown("Condition", _condition);

				if (_condition != condition)
				{
					_condition = condition;
					dataChanged = true;
				}
				
				int origIndent = EditorGUI.indentLevel;
				EditorGUI.indentLevel++;

				if (_condition != null)
				{
					if (_condition.AllowInverseVariant())
					{
						EditorGUI.BeginChangeCheck();
						_not = EditorGUILayout.Toggle("Not", _not);
						dataChanged |= EditorGUI.EndChangeCheck();
					}
					else
						_not = false;

					_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, "Properties");
					if (_editorFoldout)
					{
						EditorGUI.indentLevel++;
						bool objectChanged;
						_condition = SerializationEditorGUILayout.ObjectField(_condition, "", out objectChanged);
						dataChanged |= objectChanged;

						EditorGUI.indentLevel--;
					}
				}
				
				EditorGUI.indentLevel = origIndent;

				return dataChanged;
			}
#endif
			#endregion


#if UNITY_EDITOR
			#region Public Functions	
			public static IConditional DrawAddConditionalDropDown(string label, IConditional obj)
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

				EditorGUI.BeginChangeCheck();
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
			#endregion

			#region Private Functions	
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
			#endregion
#endif
		}
	}
}