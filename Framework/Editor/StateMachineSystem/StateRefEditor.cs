
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
			[SerializedObjectEditor(typeof(StateRef), "PropertyField")]
			public static class StateRefEditor
			{
				private enum eType
				{
					Internal,
					External
				}

				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
				{
					StateRef state = (StateRef)obj;

					if (label == null)
						label = new GUIContent();

					label.text += " (" + state + ")";
					
					bool editorCollapsed = !EditorGUILayout.Foldout(!state._editorCollapsed, label);

					if (editorCollapsed != state._editorCollapsed)
					{
						state._editorCollapsed = editorCollapsed;
						dataChanged = true;
					}

					if (!editorCollapsed)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;


						EditorGUI.BeginChangeCheck();
						eType type = (eType)EditorGUILayout.EnumPopup("Location", state.IsInternal() ? eType.Internal : eType.External);
						if (EditorGUI.EndChangeCheck())
						{
							//If type has changed create a new ref with file set to null if internal or a blank asset ref if external.
							state = new StateRef(type == eType.External ? -1 : 0, state.GetParentStateMachine());
							dataChanged = true;
						}

						switch (type)
						{
							case eType.Internal:
								{
									StateMachine stateMachine = state.GetParentStateMachine();
									if (stateMachine != null)
									{
										int stateId = state.GetStateID();

										//If changed state id create new state ref
										if (DrawStateNamePopUps(stateMachine._states, ref stateId))
										{
											state = new StateRef(stateId, stateMachine);
											dataChanged = true;
										}
									}	
								}
								break;
							case eType.External:
								{
									TextAsset asset = EditorGUILayout.ObjectField("File", state.GetExternalFile()._editorAsset, typeof(TextAsset), false) as TextAsset;

									//If asset changed update GUIDS
									if (state.GetExternalFile()._editorAsset != asset)
									{
										state = new StateRef(asset, -1, state.GetParentStateMachine());
										dataChanged = true;
									}

									if (asset != null)
									{
										StateMachine stateMachines = Serializer.FromFile<StateMachine>(AssetDatabase.GetAssetPath(asset));
										int stateId = state.GetStateID();

										if (DrawStateNamePopUps(stateMachines._states, ref stateId))
										{
											state = new StateRef(asset, stateId, state.GetParentStateMachine());
											dataChanged = true;
										}
									}
								}
								break;
						}

						EditorGUI.indentLevel = origIndent;
					}

					return state;
				}
				#endregion

				private static bool DrawStateNamePopUps(State[] states, ref int stateId)
				{
					if (states != null && states.Length > 0)
					{
						string[] stateNames = new string[states.Length + 1];
						int index = 0;
						stateNames[0] = "<none>";

						for (int i = 0; i < states.Length; i++)
						{
							string description;

							if (states[i]._editorAutoDescription)
								description = StringUtils.RemoveRichText(StringUtils.GetFirstLine(states[i].GetEditorDescription()));
							else
								description = StringUtils.RemoveRichText(StringUtils.GetFirstLine(states[i]._editorDescription));

							stateNames[i + 1] = states[i].GetEditorLabel() + " " + description;

							if (states[i]._stateId == stateId)
							{
								index = i + 1;
							}
						}

						EditorGUI.BeginChangeCheck();

						index = EditorGUILayout.Popup("State", index, stateNames);

						if (EditorGUI.EndChangeCheck())
						{
							stateId = index == 0 ? -1 : (int)states[index - 1]._stateId;
							return true;
						}
					}

					return false;
				}
			}
		}
	}
}