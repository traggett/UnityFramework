using System;

using UnityEngine;
using UnityEditor;

namespace Framework
{
	using TimelineSystem.Editor;
	using Utils;
	using Utils.Editor;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[EventCustomEditorGUI(typeof(EventStartBranchingState))]
			public class EventStartBranchingStateEditorGUI : EventEditorGUI
			{
				private static readonly Color _branchLabelColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
				public static readonly int kEventPadding = 4;

				#region EventEditorGUI
				public override bool RenderObjectProperties(GUIContent label)
				{
					Color orig = GUI.backgroundColor;
					EventStartBranchingState evnt = GetEditableObject() as EventStartBranchingState;

					bool dataChanged = false;

					#region Render Brances
					EditorGUILayout.LabelField("Branches:", EditorStyles.boldLabel);
					EditorGUILayout.Separator();

					for (int i = 0; i < evnt._branches.Length; i++)
					{
						GUI.backgroundColor = _branchLabelColor;
						EditorGUILayout.LabelField(GetBranchLabel(evnt._branches[i], i == 0), EditorUtils.TextTitleStyle, GUILayout.Height(24.0f));
						GUI.backgroundColor = orig;

						//Draw branch properties
						SerializationEditorGUILayout.ObjectField(evnt._branches[i], string.Empty, ref dataChanged);

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

					for (int i = 0; i < evnt._backgroundLogic.Length; i++)
					{
						BranchingBackgroundLogic backgroundLogic = evnt._backgroundLogic[i];
						
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

				public override void DrawLabel(GUIStyle style)
				{
					EventStartBranchingState evnt = GetEditableObject() as EventStartBranchingState;

					GUILayout.BeginArea(new Rect(kEventPadding, kEventPadding, GetLabelSize().x, GetLabelSize().y), EditorUtils.ColoredRoundedBoxStyle);
					{
						GUILayout.Label("<b>Branching State:</b>", EditorUtils.TextWhiteStyle);

						bool firstBranch = true;

						foreach (Branch branch in evnt._branches)
						{
							GUILayout.BeginHorizontal();
							{
								GUILayout.Label(GetBranchLabel(branch, firstBranch), EditorUtils.TextStyle);
								GUILayout.FlexibleSpace();
							}
							GUILayout.EndHorizontal();

							firstBranch = false;
						}

						if (evnt._backgroundLogic.Length > 0)
						{
							GUILayout.Label("<b>During Branching State:</b>", EditorUtils.TextWhiteStyle);

							foreach (BranchingBackgroundLogic backgroundLogic in evnt._backgroundLogic)
							{
								GUILayout.BeginHorizontal();
								{
									GUILayout.Label("- " + backgroundLogic.GetDescription(), EditorUtils.TextStyle);
									GUILayout.FlexibleSpace();
								}
								GUILayout.EndHorizontal();
							}
						}
					}
					GUILayout.EndArea();
				}

				public override Vector2 GetLabelSize()
				{
					EventStartBranchingState evnt = GetEditableObject() as EventStartBranchingState;
					Vector2 labelSize = EditorUtils.TextStyle.CalcSize(new GUIContent("<b>Branching State:</b>"));

					bool firstBranch = true;

					foreach (Branch branch in evnt._branches)
					{
						Vector2 branchLabelSize = EditorUtils.TextStyle.CalcSize(new GUIContent(GetBranchLabel(branch, firstBranch)));

						labelSize.y += branchLabelSize.y + kEventPadding;
						labelSize.x = Mathf.Max(branchLabelSize.x, labelSize.x) + kEventPadding;

						firstBranch = false;
					}

					if (evnt._backgroundLogic.Length > 0)
					{
						foreach (BranchingBackgroundLogic backgroundLogic in evnt._backgroundLogic)
						{
							Vector2 branchLabelSize = EditorUtils.TextStyle.CalcSize(new GUIContent("- " + backgroundLogic.GetDescription()));
							labelSize.y += branchLabelSize.y + kEventPadding;
							labelSize.x = Mathf.Max(branchLabelSize.x, labelSize.x) + kEventPadding;
						}

						labelSize.y += kEventPadding * 4;
					}

					labelSize.x += kEventPadding * 2;
					labelSize.y += kEventPadding * 2;

					return labelSize;
				}
				#endregion

				private bool DrawEditBranchButtons(int index)
				{
					EventStartBranchingState evnt = GetEditableObject() as EventStartBranchingState;
					bool changedArray = false;

					EditorGUILayout.BeginHorizontal(GUILayout.Width(20.0f));
					{
						if (GUILayout.Button("\u25b2") && index > 0)
						{
							Branch branch = evnt._branches[index];
							ArrayUtils.Remove(ref evnt._branches, index);
							ArrayUtils.Insert(ref evnt._branches, branch, index-1);
							changedArray = true;
						}
						else if (GUILayout.Button("\u25bc") && index < evnt._branches.Length-1)
						{
							Branch branch = evnt._branches[index];
							ArrayUtils.Remove(ref evnt._branches, index);
							ArrayUtils.Insert(ref evnt._branches, branch, index+1);
							changedArray = true;
						}
						else if (GUILayout.Button("Remove"))
						{
							ArrayUtils.Remove(ref evnt._branches, index);
							changedArray = true;
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Separator();

					return changedArray;
				}

				private bool DrawEditBackgroundLogicButtons(int index)
				{
					EventStartBranchingState evnt = GetEditableObject() as EventStartBranchingState;
					bool changedArray = false;

					EditorGUILayout.BeginHorizontal(GUILayout.Width(20.0f));
					{
						if (GUILayout.Button("Remove"))
						{
							ArrayUtils.Remove(ref evnt._backgroundLogic, index);
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
						EventStartBranchingState evnt = GetEditableObject() as EventStartBranchingState;
						
						ArrayUtils.Add(ref evnt._backgroundLogic, newBackgroundLogic);
						
						GetTimelineEditor().GetParent().OnAddedNewXmlNode(newBackgroundLogic);

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
							EventStartBranchingState evnt = GetEditableObject() as EventStartBranchingState;
							Branch newBranch = new Branch();
							ArrayUtils.Add(ref evnt._branches, newBranch);
							
							GetTimelineEditor().GetParent().OnAddedNewXmlNode(newBranch);

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