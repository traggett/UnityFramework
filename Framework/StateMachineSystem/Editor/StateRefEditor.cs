
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
			[SerializedObjectEditor(typeof(StateRef), "PropertyField")]
			public static class StateRefEditor
			{
				private enum eType
				{
					Internal,
					External
				}
				
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					StateRef state = (StateRef)obj;

					if (label == null)
						label = new GUIContent();

					label.text += " (" + state + ")";
					
					bool foldOut = EditorGUILayout.Foldout(state._editorFoldout, label);

					if (foldOut != state._editorFoldout)
					{
						state._editorFoldout = foldOut;
						dataChanged = true;
					}

					if (foldOut)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						EditorGUI.BeginChangeCheck();
						eType type = (eType)EditorGUILayout.EnumPopup("Link Type", (state._file.IsValid() || state._stateId == -1) ? eType.External : eType.Internal);
						if (EditorGUI.EndChangeCheck())
						{
							//If type has changed create a new ref with file set to null if internal or a blank asset ref if external.
							StateMachine stateMachine = state.GetStateMachine();
							state = new StateRef();
							state._editorFoldout = foldOut;
							state.FixUpRef(stateMachine);
							state._stateId = type == eType.External ? -1 : 0;
							dataChanged = true;
						}

						switch (type)
						{
							case eType.Internal:
								{
									StateMachine stateMachine = state.GetStateMachine();
									if (stateMachine != null)
									{
										int stateId = state._stateId;

										//If changed state id create new state ref
										if (DrawStateNamePopUps(stateMachine._states, ref stateId))
										{
											state._stateId = stateId;
											dataChanged = true;
										}
									}	
								}
								break;
							case eType.External:
								{
									TextAsset asset = EditorGUILayout.ObjectField("File", state._file._editorAsset, typeof(TextAsset), false) as TextAsset;

									//If asset changed update GUIDS
									if (state._file._editorAsset != asset)
									{
										state._file = new AssetRef<TextAsset>(asset);
										state._stateId = -1;
										dataChanged = true;
									}

									if (asset != null)
									{
										StateMachine stateMachines = Serializer.FromFile<StateMachine>(AssetDatabase.GetAssetPath(asset));
										int stateId = state._stateId;

										if (DrawStateNamePopUps(stateMachines._states, ref stateId))
										{
											state._stateId = stateId;
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
							stateNames[i + 1] = "State" + states[i]._stateId + " (" + StringUtils.RemoveRichText(StringUtils.GetFirstLine(states[i].GetDescription())) + ")";

							if (states[i]._stateId == stateId)
							{
								index = i + 1;
							}
						}

						EditorGUI.BeginChangeCheck();

						index = EditorGUILayout.Popup("Timeline", index, stateNames);

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