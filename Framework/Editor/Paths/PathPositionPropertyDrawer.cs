using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Paths
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(PathPosition))]
			public class PathPositionPropertyDrawer : PropertyDrawer
			{
				private float _height = EditorGUIUtility.singleLineHeight * 4;

				protected virtual Path GetPath(SerializedProperty pathProp)
				{
					return pathProp.objectReferenceValue as Path;
				}

				protected virtual PathNode GetPathNode(SerializedProperty nodeProp)
				{
					return nodeProp.objectReferenceValue as PathNode;
				}

				protected virtual void SetPathNode(SerializedProperty nodeProp, PathNode node)
				{
					nodeProp.objectReferenceValue = node;
				}

				// Draw the property inside the given rect
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					{
						Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

						property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, property.displayName);
						_height = EditorGUIUtility.singleLineHeight;

						if (property.isExpanded)
						{
							int origIndent = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							//Render path field
							SerializedProperty pathProp = property.FindPropertyRelative("_path");
							Rect pathPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
							EditorGUI.PropertyField(pathPosition, pathProp, new GUIContent("Path"));
							_height += EditorGUIUtility.singleLineHeight;

							Path path = GetPath(pathProp);

							if (path != null)
							{
								//Render node drop down
								SerializedProperty nodeProp = property.FindPropertyRelative("_pathNode");
								{
									Rect nodePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);
									_height += EditorGUIUtility.singleLineHeight;
									string[] nodeNames = new string[path._nodes.Length + 1];
									nodeNames[0] = "Free path point";
									int index = 0;
									
									for (int i = 0; i < path._nodes.Length; i++)
									{
										nodeNames[i + 1] = path._nodes[i]._node.name;

										if (GetPathNode(nodeProp) == path._nodes[i]._node)
										{
											index = i + 1;
										}
									}

									index = EditorGUI.Popup(nodePosition, "Node", index, nodeNames);

									if (index == 0)
									{
										SetPathNode(nodeProp, null);
									}
									else
									{
										SetPathNode(nodeProp, path._nodes[index - 1]._node);
									}
								}

								SerializedProperty pathPosProp = property.FindPropertyRelative("_pathPosition");
								SerializedProperty pathForwardProp = property.FindPropertyRelative("_pathForward");
								SerializedProperty pathUpProp = property.FindPropertyRelative("_pathUp");
								SerializedProperty pathTProp = property.FindPropertyRelative("_pathT");

								//Render Path T field
								if (GetPathNode(nodeProp) == null)
								{
									Rect pathTPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 3, position.width, EditorGUIUtility.singleLineHeight);
									EditorGUI.Slider(pathTPosition, pathTProp, 0.0f, 1.0f);
									_height += EditorGUIUtility.singleLineHeight;

									PathPosition pathPos = path.GetPoint(pathTProp.floatValue);
									pathPosProp.vector3Value = pathPos._pathPosition;
									pathForwardProp.vector3Value = pathPos._pathForward;
									pathUpProp.vector3Value = pathPos._pathUp;
								}
								else
								{
									PathNode node = nodeProp.objectReferenceValue as PathNode;
									pathTProp.floatValue = path.GetPathT(node);
									pathPosProp.vector3Value = node.transform.position;
									pathForwardProp.vector3Value = node.transform.forward;
									pathUpProp.vector3Value = node.transform.up;
								}
							}

							EditorGUI.indentLevel = origIndent;
						}


					}
					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return _height;
				}
			}
		}
	}
}