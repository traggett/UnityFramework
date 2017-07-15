
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
			[SerializedObjectEditor(typeof(TimelineStateRef), "PropertyField")]
			public static class TimelineStateRefEditor
			{
				private enum eType
				{
					Internal,
					External
				}
				
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					TimelineStateRef timelineState = (TimelineStateRef)obj;

					if (label == null)
						label = new GUIContent();

					label.text += " (" + timelineState + ")";

					timelineState._editorFoldout = EditorGUILayout.Foldout(timelineState._editorFoldout, label);
					
					if (timelineState._editorFoldout)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						EditorGUI.BeginChangeCheck();
						eType type = (eType)EditorGUILayout.EnumPopup("Link Type", (timelineState._file.IsValid() || timelineState._stateId == -1) ? eType.External : eType.Internal);
						if (EditorGUI.EndChangeCheck())
						{
							//If type has changed create a new ref with file set to null if internal or a blank asset ref if external.
							TimelineStateMachine stateMachine = timelineState.GetStateMachine();
							bool foldOut = timelineState._editorFoldout;
							timelineState = new TimelineStateRef();
							timelineState._editorFoldout = foldOut;
							timelineState.FixUpRef(stateMachine);
							timelineState._stateId = type == eType.External ? -1 : 0;
							dataChanged = true;
						}

						switch (type)
						{
							case eType.Internal:
								{
									TimelineStateMachine stateMachine = timelineState.GetStateMachine();
									int stateId = timelineState._stateId;

									//If changed state id create new state ref
									if (DrawStateNamePopUps(stateMachine._states, ref stateId))
									{
										timelineState._stateId = stateId;
										dataChanged = true;
									}
								}
								break;
							case eType.External:
								{
									TextAsset asset = EditorGUILayout.ObjectField("File", timelineState._file._editorAsset, typeof(TextAsset), false) as TextAsset;

									//If asset changed update GUIDS
									if (timelineState._file._editorAsset != asset)
									{
										timelineState._file = new AssetRef<TextAsset>(asset);
										timelineState._stateId = -1;
										dataChanged = true;
									}

									if (asset != null)
									{
										TimelineStateMachine stateMachines = Serializer.FromFile<TimelineStateMachine>(AssetDatabase.GetAssetPath(asset));
										int stateId = timelineState._stateId;

										if (DrawStateNamePopUps(stateMachines._states, ref stateId))
										{
											timelineState._stateId = stateId;
											dataChanged = true;
										}
									}
								}
								break;
						}

						EditorGUI.indentLevel = origIndent;
					}

					return timelineState;
				}
				#endregion

				private static bool DrawStateNamePopUps(TimelineState[] states, ref int stateId)
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