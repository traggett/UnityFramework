using System;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils;
	using Utils.Editor;
	using Serialization;
	using StateMachineSystem.Editor;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(BranchingState))]
			public class BranchingStateEditorGUI : StateEditorGUI
			{
				private static readonly Color _branchLabelColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);

				#region Public Interface			
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

				protected override string GetStateIdLabel()
				{
					return "Branch (State" + GetStateId().ToString("00") + ")";
				}

				public override bool RenderObjectProperties(GUIContent label)
				{
					bool dataChanged = false;

					BranchingState branchState = (BranchingState)GetEditableObject();
					Color orig = GUI.backgroundColor;

					dataChanged |= RenderStateDescriptionField();
					dataChanged |= RenderStateColorField();

					EditorGUILayout.Separator();

					#region Render Brances
					EditorGUILayout.LabelField("Branches:", EditorStyles.boldLabel);
					EditorGUILayout.Separator();

					for (int i = 0; i < branchState._branches.Length; i++)
					{
						GUI.backgroundColor = _branchLabelColor;
						EditorGUILayout.LabelField(GetBranchLabel(branchState._branches[i], i == 0), EditorUtils.TextTitleStyle, GUILayout.Height(24.0f));
						GUI.backgroundColor = orig;

						//Draw branch properties
						SerializationEditorGUILayout.ObjectField(branchState._branches[i], string.Empty, ref dataChanged);

						if (DrawEditBranchButtons(i))
						{
							dataChanged = true;
							break;
						}
					}

					dataChanged |= DrawAddBranchButton();
					#endregion

					EditorGUILayout.Separator();
					EditorGUILayout.Separator();

					#region Render Background Logic Threads
					EditorGUILayout.LabelField("Background Logic:", EditorStyles.boldLabel);
					EditorGUILayout.Separator();

					for (int i = 0; i < branchState._backgroundLogic.Length; i++)
					{
						BranchingBackgroundLogic backgroundLogic = branchState._backgroundLogic[i];

						GUI.backgroundColor = _branchLabelColor;
						EditorGUILayout.LabelField(backgroundLogic.GetDescription(), EditorUtils.TextTitleStyle, GUILayout.Height(24.0f));
						GUI.backgroundColor = orig;

						//Draw backgroundLogic properties
						{
							int origIndent = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							backgroundLogic = SerializationEditorGUILayout.ObjectField(backgroundLogic, "", ref dataChanged);

							EditorGUI.indentLevel = origIndent;
						}

						if (DrawEditBackgroundLogicButtons(i))
						{
							dataChanged = true;
							break;
						}
					}

					dataChanged |= DrawAddBackgroundLogicButton();
					#endregion

					return dataChanged;
				}
				#endregion

				private bool DrawEditBranchButtons(int index)
				{
					BranchingState branchState = (BranchingState)GetEditableObject();
					bool changedArray = false;

					EditorGUILayout.BeginHorizontal(GUILayout.Width(20.0f));
					{
						if (GUILayout.Button("\u25b2") && index > 0)
						{
							Branch branch = branchState._branches[index];
							ArrayUtils.Remove(ref branchState._branches, index);
							ArrayUtils.Insert(ref branchState._branches, branch, index - 1);
							changedArray = true;
						}
						else if (GUILayout.Button("\u25bc") && index < branchState._branches.Length - 1)
						{
							Branch branch = branchState._branches[index];
							ArrayUtils.Remove(ref branchState._branches, index);
							ArrayUtils.Insert(ref branchState._branches, branch, index + 1);
							changedArray = true;
						}
						else if (GUILayout.Button("Remove"))
						{
							ArrayUtils.Remove(ref branchState._branches, index);
							changedArray = true;
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Separator();

					return changedArray;
				}

				private bool DrawEditBackgroundLogicButtons(int index)
				{
					BranchingState branchState = (BranchingState)GetEditableObject();
					bool changedArray = false;

					EditorGUILayout.BeginHorizontal(GUILayout.Width(20.0f));
					{
						if (GUILayout.Button("Remove"))
						{
							ArrayUtils.Remove(ref branchState._backgroundLogic, index);
							changedArray = true;
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Separator();

					return changedArray;
				}

				private bool DrawAddBackgroundLogicButton()
				{
					int index = 0;

					Type[] branchTypes = SystemUtils.GetAllSubTypes(typeof(BranchingBackgroundLogic));

					string[] branchTypeNames = new string[branchTypes.Length + 1];
					branchTypeNames[index++] = "(Add Background Logic)";

					foreach (Type type in branchTypes)
					{
						branchTypeNames[index] = type.Name;
						index++;
					}

					int newIndex = EditorGUILayout.Popup(string.Empty, 0, branchTypeNames);

					if (0 != newIndex)
					{
						Type branchType = branchTypes[newIndex - 1];

						BranchingBackgroundLogic newBackgroundLogic = Activator.CreateInstance(branchType) as BranchingBackgroundLogic;
						BranchingState branchState = (BranchingState)GetEditableObject();

						ArrayUtils.Add(ref branchState._backgroundLogic, newBackgroundLogic);

						StateMachineEditor editor = (StateMachineEditor)GetEditor();
						editor.OnAddedNewObjectToTimeline(newBackgroundLogic);

						return true;
					}

					return false;
				}

				private bool DrawAddBranchButton()
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(20.0f));
					{
						if (GUILayout.Button("Add Branch"))
						{
							BranchingState branchState = (BranchingState)GetEditableObject();
							Branch newBranch = new Branch();
							ArrayUtils.Add(ref branchState._branches, newBranch);

							StateMachineEditor editor = (StateMachineEditor)GetEditor();
							editor.OnAddedNewObjectToTimeline(newBranch);

							return true;
						}

						GUILayout.FlexibleSpace();
					}
					GUILayout.EndHorizontal();

					return false;
				}

				private static string GetBranchLabel(Branch branch, bool firstBranch)
				{
					string description = branch.GetDescription() + " go to <b>" + branch._goToState + "</b>";

					if (!firstBranch)
						description = "<b>Else</b> " + description;

					return description;
				}
			}
		}
	}
}