using System;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils;
	using Utils.Editor;
	using Serialization;
	
	namespace StateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(ConditionalState))]
			public class ConditionalStateEditorGUI : StateEditorGUI
			{
				#region StateEditorGUI			
				public override void OnDoubleClick()
				{
					
				}

				public override Color GetColor(StateMachineEditorStyle style)
				{
					if (GetEditableObject()._editorAutoColor)
						return style._branchingStateColor;
					else
						return GetEditableObject()._editorColor;
				}

				public override bool RenderObjectProperties(GUIContent label)
				{
					bool dataChanged = false;

					ConditionalState conditionalState = (ConditionalState)GetEditableObject();
					Color orig = GUI.backgroundColor;

					dataChanged |= RenderStateDescriptionField();
					dataChanged |= RenderStateColorField();

					EditorGUILayout.Separator();

					#region Render Brances
					EditorGUILayout.LabelField("State Exit Conditions:", EditorStyles.boldLabel);
					EditorGUILayout.Separator();

					if (conditionalState._branches != null)
					{
						for (int i = 0; i < conditionalState._branches.Length; i++)
						{
							GUI.backgroundColor = _titleLabelColor;
							EditorGUILayout.LabelField(GetConditionLabel(conditionalState._branches[i], i == 0), EditorUtils.InspectorSubHeaderStyle, GUILayout.Height(24.0f));
							GUI.backgroundColor = orig;

							//Draw condition properties
							SerializationEditorGUILayout.ObjectField(conditionalState._branches[i], string.Empty, ref dataChanged);

							if (DrawEditConditionsButtons(i))
							{
								dataChanged = true;
								break;
							}
						}
					}

					dataChanged |= DrawAddConditionButton();
					#endregion

					EditorGUILayout.Separator();
					EditorGUILayout.Separator();

					return dataChanged;
				}
				#endregion

				private bool DrawEditConditionsButtons(int index)
				{
					ConditionalState conditionalState = (ConditionalState)GetEditableObject();
					bool changedArray = false;

					EditorGUILayout.BeginHorizontal(GUILayout.Width(20.0f));
					{
						if (GUILayout.Button("\u25b2") && index > 0)
						{
							ConditionalStateBranch condition = conditionalState._branches[index];
							ArrayUtils.RemoveAt(ref conditionalState._branches, index);
							ArrayUtils.Insert(ref conditionalState._branches, condition, index - 1);
							changedArray = true;
						}
						else if (GUILayout.Button("\u25bc") && index < conditionalState._branches.Length - 1)
						{
							ConditionalStateBranch condition = conditionalState._branches[index];
							ArrayUtils.RemoveAt(ref conditionalState._branches, index);
							ArrayUtils.Insert(ref conditionalState._branches, condition, index + 1);
							changedArray = true;
						}
						else if (GUILayout.Button("Remove"))
						{
							ArrayUtils.RemoveAt(ref conditionalState._branches, index);
							changedArray = true;
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Separator();

					return changedArray;
				}

				private bool DrawAddConditionButton()
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(20.0f));
					{
						if (GUILayout.Button("Add New Condition"))
						{
							ConditionalState conditionalState = (ConditionalState)GetEditableObject();
							ConditionalStateBranch newCondition = new ConditionalStateBranch();
							ArrayUtils.Add(ref conditionalState._branches, newCondition);

							StateMachineEditor editor = (StateMachineEditor)GetEditor();
							editor.OnAddedNewObjectToTimeline(newCondition);

							return true;
						}

						GUILayout.FlexibleSpace();
					}
					GUILayout.EndHorizontal();

					return false;
				}

				private static string GetConditionLabel(ConditionalStateBranch condition, bool firstBranch)
				{
					string description = "<b>" + condition.GetDescription() + "</b>, then go to <b>" + condition._goToState + "</b>";

					if (!firstBranch)
						description = "Else " + description;

					return description;
				}
			}
		}
	}
}