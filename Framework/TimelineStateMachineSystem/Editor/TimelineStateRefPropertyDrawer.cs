using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Serialization;
	
	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(TimelineStateRefProperty))]
			public sealed class TimelineStateRefPropertyDrawer : PropertyDrawer
			{
				private bool _foldout = true;
				private float _height = EditorGUIUtility.singleLineHeight * 4;

				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					
					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

					_foldout = EditorGUI.Foldout(foldoutPosition, _foldout, property.displayName);
					_height = EditorGUIUtility.singleLineHeight;

					if (_foldout)
					{
						
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						Rect filePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

						SerializedProperty fileProp = property.FindPropertyRelative("_file");
						fileProp.objectReferenceValue = EditorGUI.ObjectField(filePosition, "File", fileProp.objectReferenceValue, typeof(TextAsset), true);

						_height += EditorGUIUtility.singleLineHeight;

						if (fileProp.objectReferenceValue != null)
						{
							SerializedProperty TimelineProp = property.FindPropertyRelative("_timelineId");

							//Load all time lines from a file
							//Get gameobject from  property
							TimelineStateMachine stateMachine = TimelineStateMachine.FromTextAsset((TextAsset)fileProp.objectReferenceValue, FindParentGameObject(property));

							if (stateMachine != null)
							{
								TimelineState[] states = stateMachine._states;

								if (states != null && states.Length > 0)
								{
									string[] stateNames = new string[states.Length];
									int index = 0;
									for (int i = 0; i < states.Length; i++)
									{
										stateNames[i] = "State" + states[i]._stateId + " (" + states[i]._editorDescription + ")";

										if (states[i]._stateId == TimelineProp.intValue)
										{
											index = i;
										}
									}

									Rect TimelinePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);

									index = EditorGUI.Popup(TimelinePosition, "Timeline", index, stateNames);
									TimelineProp.intValue = states[index]._stateId;

									_height += EditorGUIUtility.singleLineHeight;
								}

								Rect editButtonPosition = new Rect(position.x + EditorGUIUtility.labelWidth, position.y + EditorGUIUtility.singleLineHeight * 3, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

								if (GUI.Button(editButtonPosition, "Edit"))
								{
									TimelineStateMachineEditorWindow.Load(fileProp.objectReferenceValue as TextAsset);
								}

								_height += EditorGUIUtility.singleLineHeight;
							}
							
						}
					
						EditorGUI.indentLevel = origIndent;
					}

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return _height;
				}

				private static GameObject FindParentGameObject(SerializedProperty property)
				{
					if (property.serializedObject.targetObject is Component)
					{
						Component component = property.serializedObject.targetObject as Component;
						return component.gameObject;
					}

					return null;
				}
			}
		}
	}
}