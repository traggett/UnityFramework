using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Framework.Utils;

namespace Framework
{
	namespace Paths
	{
		namespace Editor
		{
			[CustomEditor(typeof(Path), true)]
			public class PathInspector : UnityEditor.Editor
			{
				private ReorderableList _list;
				private PathNodeData[] _oldArray;

				private void OnEnable()
				{
					Path path = (Path)target;
					SerializedProperty property = serializedObject.FindProperty("_nodes");

					_list = new ReorderableList(serializedObject, property, true, true, true, true);
					_list.drawHeaderCallback = OnDrawListHeader;
					_list.drawElementCallback = OnDrawListItem;
					_list.onReorderCallback = OnListReordered;
					_list.onAddCallback = OnAddNode;
					_list.onRemoveCallback = onRemoveNode;

					if (path._nodes != null)
					{
						for (int i = 0; i < path._nodes.Length;)
						{
							if (path._nodes[i]._node == null)
								ArrayUtils.RemoveAt(ref path._nodes, i);
							else
								i++;
						}

						path.RefreshNodes(path._nodes);
						_oldArray = (PathNodeData[])path._nodes.Clone();
					}
				}

				public void OnSceneGUI()
				{
					Path path = (Path)target;

					//Draw node up directions
					if (path._nodes != null)
					{
						Handles.color = path._pathColor;

						for (int i = 0; i < path._nodes.Length; i++)
						{
							Quaternion nodeUpRotation = Quaternion.FromToRotation(Vector3.up, path._nodes[i]._up);
							Vector3 nodePos = path.GetNodePosition(i);
							Vector3 controlPos = nodePos + nodeUpRotation * (Vector3.up * path._nodes[i]._width);

							EditorGUI.BeginChangeCheck();

							nodeUpRotation = Handles.RotationHandle(nodeUpRotation, controlPos);

							if (EditorGUI.EndChangeCheck())
							{
								Undo.RecordObject(this, "Changed Path Node Up");

								path._nodes[i]._up = nodeUpRotation * Vector3.up;
							}

							path.OnNodeSceneGUI(path._nodes[i]._node);

							if (Handles.Button(controlPos, Quaternion.identity, path._nodes[i]._width * 0.166f, path._nodes[i]._width * 0.166f * 2f, Handles.SphereHandleCap))
							{
								GenericMenu menu = new GenericMenu();
								menu.AddDisabledItem(new GUIContent("Node Up Direction"));
								menu.AddSeparator("");
								menu.AddItem(new GUIContent("Reset To World Up"), false, Callback, new ContextMenuData(i, 0));
								menu.AddItem(new GUIContent("Reset to World Forward"), false, Callback, new ContextMenuData(i, 1));
								menu.AddItem(new GUIContent("Reset to World Right"), false, Callback, new ContextMenuData(i, 2));
								menu.ShowAsContext();
							}
						}
					}
				}

				private struct ContextMenuData
				{
					public int _nodeIndex;
					public int _menuCommand;
					
					public ContextMenuData(int node, int menuCommand)
					{
						_nodeIndex = node;
						_menuCommand = menuCommand;
					}
				}

				private void Callback(object value)
				{
					Path path = (Path)target;
					ContextMenuData data = (ContextMenuData)value;

					Undo.RecordObject(this, "Changed Path Node Up");

					switch (data._menuCommand)
					{
						case 0:
							{
								path._nodes[data._nodeIndex]._up = Vector3.up;
							}
							break;
						case 1:
							{
								path._nodes[data._nodeIndex]._up = Vector3.forward;
							}
							break;
						case 2:
							{
								path._nodes[data._nodeIndex]._up = Vector3.right;
							}
							break;
					}
				}

				public override void OnInspectorGUI()
				{
					serializedObject.Update();
					_list.DoLayoutList();
					serializedObject.ApplyModifiedProperties();
				}

				private void OnAddNode(ReorderableList list)
				{
					Path path = (Path)target;

					GameObject newNodeObj = new GameObject("PathNode " + list.count);
					newNodeObj.transform.parent = path.transform;
					PathNodeData nodeData = new PathNodeData();
					nodeData._node = newNodeObj.AddComponent<PathNode>();

					if (path._nodes != null && path._nodes.Length > 0)
					{
						nodeData._width = path._nodes[path._nodes.Length - 1]._width;
						nodeData._up = path._nodes[path._nodes.Length - 1]._up;
					}
					else
					{
						nodeData._width = 0.5f;
						nodeData._up = Vector3.up;
					}
					
					ArrayUtils.Add(ref path._nodes, nodeData);
					path.RefreshNodes(path._nodes);
					_oldArray = (PathNodeData[])path._nodes.Clone();

					OnAddedNode(nodeData._node);
				}

				private void onRemoveNode(ReorderableList list)
				{
					Path path = (Path)target;
					path._nodes[list.index]._node.ClearParent(path);
					ArrayUtils.RemoveAt(ref path._nodes, list.index);
					path.RefreshNodes(path._nodes);
					_oldArray = (PathNodeData[])path._nodes.Clone();
					EditorUtility.SetDirty(target);

					OnRemovedNode(list.index);
				}

				private void OnListReordered(ReorderableList list)
				{
					Path path = (Path)target;

					//Work out which items have changed, call function saying index x moved to i?
					int newIndex = list.index;
					int oldIndex = -1;

					//find prev position
					for (int i = 0; i < _oldArray.Length; i++)
					{
						if (_oldArray[i] == path._nodes[newIndex])
						{
							oldIndex = i;
							break;
						}
					}

					OnNodeChangedPosition(oldIndex, newIndex);

					int siblingIndex = 0;
					for (int i = 0; i < path._nodes.Length; i++)
					{
						if (path._nodes[i]._node.transform.parent == path.transform)
						{
							path._nodes[i]._node.transform.SetSiblingIndex(siblingIndex);
							siblingIndex++;
						}						
					}
				}

				private void OnDrawListItem(Rect rect, int index, bool isActive, bool isFocused)
				{
					Path path = (Path)target;

					Rect objectRect = new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight);

					PathNode node = EditorGUI.ObjectField(objectRect, path._nodes[index]._node, typeof(PathNode), true) as PathNode;
					objectRect.x += objectRect.width;

					path._nodes[index]._width = EditorGUI.FloatField(objectRect, path._nodes[index]._width);

					//If a node is changed to reference a different one...
					if (node != path._nodes[index]._node)
					{
						if (node == null)
						{
							path._nodes[index]._node.ClearParent(path);
							ArrayUtils.RemoveAt(ref path._nodes, index);
							path.RefreshNodes(path._nodes);
							_oldArray = (PathNodeData[])path._nodes.Clone();
						}
						else
						{
							path._nodes[index]._node = node;
							path.RefreshNodes(path._nodes);
							_oldArray = (PathNodeData[])path._nodes.Clone();
						}
					}
				}

				private void OnDrawListHeader(Rect rect)
				{
					rect.width *= 0.5f;
					EditorGUI.LabelField(rect, new GUIContent("Node"));
					rect.x += rect.width;
					EditorGUI.LabelField(rect, new GUIContent("Width"));
				}


				protected virtual void OnAddedNode(PathNode node)
				{

				}

				protected virtual void OnRemovedNode(int index)
				{

				}

				protected virtual void OnNodeChangedPosition(int oldIndex, int newIndex)
				{

				}
			}
		}
	}
}