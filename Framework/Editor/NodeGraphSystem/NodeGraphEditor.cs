using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace Framework
{
	using Utils.Editor;
	using Utils;
	using Serialization;
	
	namespace NodeGraphSystem
	{
		namespace Editor
		{
			public sealed class NodeGraphEditor : ScriptableObjectHierarchyGridEditor<NodeGraph, Node>
			{
				#region Private Data
				private static readonly Color kLinkLineColor = Color.white;
				private static readonly Color kUnusableLinkIconColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
				private static readonly Color kOutputLinkIconColor = Color.red;
				private static readonly Color kInputLinkIconColor = Color.white;
				private static readonly Color kInputLinkHighlightColor = Color.green;
				private static readonly float kLinkIconWidth = 12.0f;
				private static readonly float kLinkLineNormalDist = 80.0f;
				
				private static Dictionary<Type, string> _addNodeContextMenu;
				private static Dictionary<Type, string> _addInputNodeContextMenu;
				private static Dictionary<Type, string> _addOutputNodeContextMenu;
				private string _title;
				private string _editorPrefsTag;
				private NodeGraphEditorPrefs _editorPrefs;

				private GUIStyle _titleStyle;
				private GUIStyle _nodeTitleTextStyle;
				private GUIStyle _nodeTextStyle;
				private GUIStyle _nodeBoldTextStyle;

				private NodeEditorField _draggingNodeFieldFrom = null;
				private NodeEditorField _draggingNodeFieldTo = null;
				#endregion

				#region Public Interfacce
				public void Init(string title, IEditorWindow editorWindow, string editorPrefsTag)
				{
					_title = title;
					_editorPrefsTag = editorPrefsTag;

					string editorPrefsText = ProjectEditorPrefs.GetString(_editorPrefsTag, "");
					_editorPrefs = Serializer.FromString<NodeGraphEditorPrefs>(editorPrefsText);

					if (_editorPrefs == null)
						_editorPrefs = new NodeGraphEditorPrefs();

					SetEditorWindow(editorWindow);
					CreateViews();
				}			

				public void UpdateEditor()
				{
				}
				
				public void OnQuit()
				{
					if (HasChanges())
					{
						if (EditorUtility.DisplayDialog("Node Machine Has Been Modified", "Do you want to save the changes you made to the node machine:\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save"))
						{
							Save();
						}
					}
				}

				public void New()
				{
					if (ShowOnLoadSaveChangesDialog())
					{
						string path = EditorUtility.SaveFilePanelInProject("New Node Graph", "NodeGraph", "asset", "Please enter a file name to save the node graph to");

						if (!string.IsNullOrEmpty(path))
						{
							_editorPrefs._fileName = path;
							SaveEditorPrefs();

							Create(path);
						}
						else
						{
							_editorPrefs._fileName = null;
							SaveEditorPrefs();

							ClearAsset();
						}
					}
				}

				public void SaveAs()
				{
					string path = EditorUtility.SaveFilePanelInProject("Save Node Graph", Asset.name, "asset", "Please enter a file name to save the node graph to");

					if (!string.IsNullOrEmpty(path))
					{
						SaveAs(path);
					}
				}

				public void Render(Vector2 windowSize)
				{
					InitGUIStyles();

					EditorGUILayout.BeginVertical();
					{
						RenderTitleBar(windowSize);

						float toolBarHeight = EditorStyles.toolbar.fixedHeight * 3f;
						Rect area = new Rect(0.0f, toolBarHeight, windowSize.x, windowSize.y - toolBarHeight);
						
						RenderGridView(area);
					}
					EditorGUILayout.EndVertical();

					if (NeedsRepaint())
					{
						GetEditorWindow().DoRepaint();
					}
				}
				#endregion

				#region ScriptableObjectHierarchyGridEditor
				protected override void OnLoadAsset(NodeGraph asset)
				{
					List<Node> nodes = new List<Node>();

					foreach (NodeEditorGUI node in _editableObjects)
					{
						nodes.Add(node.Asset);
					}

					asset._nodes = nodes.ToArray();

					CenterCamera();
				}

				protected override void OnZoomChanged(float zoom)
				{
					SaveEditorPrefs();
				}

				protected override void RenderObjectsOnGrid()
				{
					_nodeTitleTextStyle.fontSize = Mathf.RoundToInt(11 * _currentZoom);
					_nodeTextStyle.fontSize = Mathf.RoundToInt(9 * _currentZoom);

					NodeEditorField highlightedField = GetHighlightedNode(Event.current.mousePosition);

					List<NodeEditorGUI> toRender = new List<NodeEditorGUI>();
					foreach (NodeEditorGUI editorGUI in _editableObjects)
						toRender.Add(editorGUI);

					foreach (NodeEditorGUI node in toRender)
					{
						bool selected = _selectedObjects.Contains(node);
						node.CalcBounds(_nodeTitleTextStyle, _nodeTextStyle, _currentZoom);
						Rect renderedRect = GetScreenRect(node.GetBounds());
						node.Render(renderedRect, selected, _nodeTitleTextStyle, _nodeTextStyle, _currentZoom, highlightedField, _draggingNodeFieldFrom);
					}

					RenderLinks(_currentZoom, highlightedField);
				}
				
				protected override ScriptableObjectHierarchyEditorObjectGUI<NodeGraph, Node> CreateObjectEditorGUI(Node node)
				{
					NodeEditorGUI editorGUI = new NodeEditorGUI();
					editorGUI.Init(this, node);
					return editorGUI;
				}

				protected override void OnCreatedNewObject(Node node)
				{
					node._nodeId = GenerateNewNodeId();
					node.name = "Node_" + node._nodeId.ToString("000");

					if (string.IsNullOrEmpty(node._editorDescription))
					{
						if (SystemUtils.IsSubclassOfRawGeneric(typeof(InputNode<>), node.GetType()))
						{
							Type inputType = SystemUtils.GetGenericImplementationType(typeof(InputNode<>), node.GetType());
							node._editorDescription = SystemUtils.GetTypeName(inputType) + " Input";
						}
						else if (SystemUtils.IsSubclassOfRawGeneric(typeof(OutputNode<,>), node.GetType()))
						{
							Type outputType = SystemUtils.GetGenericImplementationType(typeof(OutputNode<,>), node.GetType(), 1);
							node._editorDescription = SystemUtils.GetTypeName(outputType) + " Output";
						}
						else
						{
							node._editorDescription = "Node" + node._nodeId.ToString("000");
						}
					}

					ArrayUtils.Add(ref Asset._nodes, node);
				}

				protected override void AddContextMenu(GenericMenu menu)
				{
					BuildNodeMap();

					foreach (KeyValuePair<Type, string> pair in _addNodeContextMenu)
					{
						string menuItemName = "Add Node/";
						menu.AddItem(new GUIContent(menuItemName + pair.Value), false, AddNewNodeMenuCallback, pair.Key);
					}

					foreach (KeyValuePair<Type, string> pair in _addInputNodeContextMenu)
					{
						string menuItemName = "Add Input/";
						menu.AddItem(new GUIContent(menuItemName + pair.Value), false, AddNewNodeMenuCallback, pair.Key);
					}

					foreach (KeyValuePair<Type, string> pair in _addOutputNodeContextMenu)
					{
						string menuItemName = "Add Output/";
						menu.AddItem(new GUIContent(menuItemName + pair.Value), false, AddNewNodeMenuCallback, pair.Key);
					}
				}

				protected override void OnDragging(Event inputEvent)
				{
					if (_dragMode == DragType.Custom)
					{
						SetNeedsRepaint();
					}
					else
					{
						base.OnDragging(inputEvent);
					}
				}

				protected override void OnStopDragging(Event inputEvent, bool cancelled)
				{
					if (_dragMode == DragType.Custom)
					{
						if (!cancelled)
						{
							//if over a input node field of correct type, set link on node
							NodeEditorField mouseOverNodeField = GetHighlightedNode(inputEvent.mousePosition);

							bool linkChanged = (_draggingNodeFieldTo == null && mouseOverNodeField != null) ||
												(_draggingNodeFieldTo != null && _draggingNodeFieldTo != mouseOverNodeField);

							if (linkChanged)
							{
								List<Node> effectedNodes = new List<Node>();

								if (_draggingNodeFieldTo != null)
								{
									effectedNodes.Add(_draggingNodeFieldTo._nodeEditorGUI.Asset);
									SetNodeInputFieldLinkNodeID(_draggingNodeFieldTo, -1);
								}

								if (mouseOverNodeField != null)
								{
									effectedNodes.Add(mouseOverNodeField._nodeEditorGUI.Asset);
									SetNodeInputFieldLinkNodeID(mouseOverNodeField, _draggingNodeFieldFrom._nodeEditorGUI.Asset._nodeId);
								}

								Undo.RecordObjects(effectedNodes.ToArray(), "Edit Node Link(s)");
							}
						}				

						inputEvent.Use();
						_dragMode = DragType.NotDragging;
						_draggingNodeFieldFrom = null;
						_draggingNodeFieldTo = null;
					}
					else
					{
						base.OnStopDragging(inputEvent, cancelled);
					}
				}

				protected override void OnLeftMouseDown(Event inputEvent)
				{
					//Check for clicking on a link
					NodeEditorField clickedOnNodeFromField = null;
					NodeEditorField clickedOnNodeToField = null;

					for (int i = 0; i < _editableObjects.Count && clickedOnNodeFromField == null; i++)
					{
						NodeEditorGUI node = (NodeEditorGUI)_editableObjects[i];

						if (node.GetOutputField() != null)
						{
							Vector2 toField = inputEvent.mousePosition - node.GetOutputField()._position;

							if (toField.magnitude < kLinkIconWidth * 0.5f)
							{
								clickedOnNodeFromField = node.GetOutputField();
								break;
							}
						}

						foreach (NodeEditorField nodeInputField in node.GetInputFields())
						{
							//If has an link going into it we can drag it away
							int linkNodeId = GetNodeInputFieldLinkNodeId(nodeInputField);

							if (linkNodeId != -1)
							{
								NodeEditorGUI linkedNode = GetEditableObject(linkNodeId);

								if (linkedNode != null)
								{
									Vector2 toField = inputEvent.mousePosition - nodeInputField._position;

									if (toField.magnitude < kLinkIconWidth)
									{
										clickedOnNodeFromField = linkedNode.GetOutputField();
										clickedOnNodeToField = nodeInputField;
										break;
									}
								}
							}
						}
					}

					if (clickedOnNodeFromField != null)
					{
						_draggingNodeFieldFrom = clickedOnNodeFromField;
						_draggingNodeFieldTo = clickedOnNodeToField;
						_dragMode = DragType.Custom;
						_dragPos = inputEvent.mousePosition;
						_dragAreaRect = new Rect(-1.0f, -1.0f, 0.0f, 0.0f);
					}
					//Normal object clicking
					else
					{
						_dragMode = DragType.LeftClick;

						base.OnLeftMouseDown(inputEvent);
					}
				}
				#endregion

				#region Private Functions
				private void InitGUIStyles()
				{
					if (_titleStyle == null)
					{
						_titleStyle = new GUIStyle(EditorStyles.label);
						_titleStyle.richText = true;
						_titleStyle.alignment = TextAnchor.MiddleCenter;
					}

					if (_nodeTitleTextStyle == null)
					{
						_nodeTitleTextStyle = new GUIStyle(EditorUtils.TextStyle);
						_nodeTitleTextStyle.alignment = TextAnchor.MiddleLeft;
						_nodeTitleTextStyle.padding = new RectOffset(0, 0, 0, 0);
						_nodeTitleTextStyle.border = new RectOffset(0, 0, 0, 0);
					}

					if (_nodeTextStyle == null)
					{
						_nodeTextStyle = new GUIStyle(EditorUtils.TextStyleSmall);
						_nodeTextStyle.alignment = TextAnchor.MiddleCenter;
						_nodeTextStyle.fontStyle = FontStyle.Italic;
						_nodeTextStyle.padding = new RectOffset(0, 0, 0, 0);
						_nodeTextStyle.border = new RectOffset(0, 0, 0, 0);
					}

					if (_nodeBoldTextStyle == null)
					{
						_nodeBoldTextStyle = new GUIStyle(_nodeTextStyle);
						_nodeBoldTextStyle.fontStyle = FontStyle.BoldAndItalic;
					}
				}
				
				private void RenderTitleBar(Vector2 windowSize)
				{
					EditorGUILayout.BeginVertical();
					{
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							string titleText = _title + (Asset != null ? " - <b>" + Asset.name : "");

							if (HasChanges())
								titleText += "<b>*</b>";

							EditorGUILayout.LabelField(titleText, _titleStyle);
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							if (GUILayout.Button("New", EditorStyles.toolbarButton))
							{
								New();
							}

							if (GUILayout.Button("Load", EditorStyles.toolbarButton))
							{
								if (ShowOnLoadSaveChangesDialog())
								{
									string fileName = EditorUtility.OpenFilePanel("Open File", Application.dataPath, "asset");
									if (fileName != null && fileName != string.Empty)
									{
										LoadFile(fileName);
									}
								}
							}

							if (GUILayout.Button("Save", EditorStyles.toolbarButton))
							{
								Save();
							}

							if (GUILayout.Button("Save As", EditorStyles.toolbarButton))
							{
								SaveAs();
							}

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							GUILayout.Button("Zoom", EditorStyles.toolbarButton);

							float zoom = EditorGUILayout.Slider(_currentZoom, 0.5f, 1.5f);

							if (GUILayout.Button("Reset Zoom", EditorStyles.toolbarButton))
							{
								zoom = 1.0f;
							}

							SetZoom(zoom);

							if (GUILayout.Button("Center", EditorStyles.toolbarButton))
							{
								CenterCamera();
							}
							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}

				private void SaveEditorPrefs()
				{
					string prefsXml = Serializer.ToString(_editorPrefs);
					ProjectEditorPrefs.SetString(_editorPrefsTag, prefsXml);
				}

				private void CreateViews()
				{
					_currentZoom = _editorPrefs._zoom;

					if (!string.IsNullOrEmpty(_editorPrefs._fileName))
					{
						LoadFile(_editorPrefs._fileName);
					}
				}

				private void LoadFile(string fileName)
				{
					if (Load(AssetUtils.GetAssetPath(fileName)))
					{
						if (_editorPrefs._fileName != fileName)
						{
							_editorPrefs._fileName = fileName;
							SaveEditorPrefs();
						}
					}
					else
					{
						_editorPrefs._fileName = null;
						SaveEditorPrefs();
					}
				}

				private bool ShowOnLoadSaveChangesDialog()
				{
					if (HasChanges())
					{
						int option = EditorUtility.DisplayDialogComplex("Node Machine Has Been Modified", "Do you want to save the changes you made to the node machine:\n\n" + Asset.name + "\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save", "Cancel");

						switch (option)
						{
							//Save
							case 0: Save(); return true;
							//Don't save
							case 1: return true;
							//Cancel load
							default:
							case 2: return false;
						}
					}

					return true;
				}

				private void SetNodeInputFieldLinkNodeID(NodeEditorField nodeField, int nodeId)
				{
					object inputValueInstance = nodeField.GetValue();
					INodeInputField inputField = inputValueInstance as INodeInputField;
					inputField.SetSourceNode(nodeId);
					nodeField.SetValue(inputValueInstance);
					EditorUtility.SetDirty(nodeField._nodeEditorGUI.Asset);
				}
				
				private int GetNodeInputFieldLinkNodeId(NodeEditorField nodeField)
				{
					object inputValueInstance = nodeField.GetValue();
					INodeInputField inputField = inputValueInstance as INodeInputField;
					return inputField.GetSourceNodeId();
				}

				private NodeEditorGUI GetEditableObject(int nodeId)
				{
					foreach (NodeEditorGUI node in _editableObjects)
					{
						if (node.Asset._nodeId == nodeId)
						{
							return node;
						}
					}

					return null;
				}

				private void RenderLinks(float scale, NodeEditorField highlightedField)
				{
					Vector2 mousePosition = Event.current.mousePosition;

					foreach (NodeEditorGUI node in _editableObjects)
					{
						if (node.HasOutput())
						{
							bool highlighted = false;

							if (_dragMode == DragType.NotDragging)
							{
								Vector2 toField = mousePosition - node.GetOutputField()._position;
								highlighted = toField.magnitude < kLinkIconWidth;
								SetNeedsRepaint();
							}

							RenderLinkIcon(node.GetOutputField()._position, kOutputLinkIconColor, scale, highlighted);
						}
						
						foreach (NodeEditorField nodeInputField in node.GetInputFields())
						{
							Color color; 

							if (_dragMode == DragType.Custom &&
								(nodeInputField._type != _draggingNodeFieldFrom._type && !_draggingNodeFieldFrom._type.IsAssignableFrom(nodeInputField._type)))
							{ 
								color = kUnusableLinkIconColor;
							}	
							else
							{
								color = kInputLinkIconColor;
							}

							bool highlighted = nodeInputField == highlightedField;

							if (_dragMode == DragType.NotDragging)
							{
								Vector2 toField = mousePosition - nodeInputField._position;
								highlighted = toField.magnitude < kLinkIconWidth;
								SetNeedsRepaint();
							}

							RenderStaticValueBox(nodeInputField, nodeInputField._position, kUnusableLinkIconColor, scale);
							RenderLinkIcon(nodeInputField._position, color, scale, highlighted);
						}
					}

					foreach (NodeEditorGUI node in _editableObjects)
					{
						foreach (NodeEditorField nodeInputField in node.GetInputFields())
						{
							if (_dragMode != DragType.Custom || _draggingNodeFieldTo != nodeInputField)
							{
								int linkedOutputNodeId = GetNodeInputFieldLinkNodeId(nodeInputField);

								if (linkedOutputNodeId != -1)
								{
									NodeEditorGUI outputNode = GetEditableObject(linkedOutputNodeId);

									if (outputNode != null && outputNode.HasOutput())
									{
										RenderLink(outputNode.GetOutputField()._position, nodeInputField._position, _dragMode == DragType.Custom ? Color.Lerp(kLinkLineColor, Color.black, 0.3f) : kLinkLineColor, scale);
									}
								}
								
							}
						}
					}

					if (_dragMode == DragType.Custom)
					{
						//Instead of rendering current dragging link, render it to mouse position
						RenderLink(_draggingNodeFieldFrom._position, Event.current.mousePosition, kLinkLineColor, scale, true);
					}
				}

				private void RenderLinkIcon(Vector2 position, Color color, float scale, bool highlight)
				{
					Vector3 position3d = new Vector3(position.x, position.y, 0.0f);

					Handles.BeginGUI();

					if (highlight)
					{
						Handles.color = kInputLinkHighlightColor;
						Handles.DrawSolidDisc(position3d, -Vector3.forward, kLinkIconWidth * 0.65f * scale);
					}

					Handles.color = color;
					Handles.DrawSolidDisc(position3d, -Vector3.forward, kLinkIconWidth * 0.5f * scale);
					Handles.color = Color.Lerp(color, Color.black, 0.5f);
					Handles.DrawSolidDisc(position3d, -Vector3.forward, kLinkIconWidth * 0.32f * scale);
					Handles.color = Color.black;
					Handles.DrawSolidDisc(position3d, -Vector3.forward, kLinkIconWidth * 0.24f * scale);

					Handles.EndGUI();
				}

				private void RenderLink(Vector2 fromPos, Vector2 toPos, Color color, float scale, bool looseLink = false)
				{
					float normalDist = Mathf.Min(kLinkLineNormalDist, Math.Abs(toPos.x - fromPos.x) * 0.33f) * scale;

					Vector3 startPos = new Vector3(fromPos.x, fromPos.y, 0.0f);
					Vector3 endPos = new Vector3(toPos.x, toPos.y, 0.0f);
					Vector3 startTangent = startPos + Vector3.right * normalDist * _currentZoom;
					Vector3 endTangent = endPos - Vector3.right * normalDist * _currentZoom;
					Handles.BeginGUI();
					Handles.DrawBezier(startPos + new Vector3(0, 2, 0), endPos + new Vector3(0, 2, 0), startTangent, endTangent, Color.Lerp(color, Color.black, 0.76f), EditorUtils.BezierAATexture, 3f * scale);
					Handles.DrawBezier(startPos + new Vector3(-1, 0, 0), endPos + new Vector3(1, 0, 0), startTangent, endTangent, color, EditorUtils.BezierAATexture, 3f * scale);	
					
					if (looseLink)
					{
						Handles.color = kInputLinkHighlightColor;
						Handles.DrawSolidDisc(endPos, Vector3.forward, 2.0f * scale);
						Handles.DrawWireDisc(endPos, Vector3.forward, 6.0f * scale);
					}

					Handles.EndGUI();
				}

				private void RenderStaticValueBox(NodeEditorField nodeField, Vector2 position, Color color, float scale)
				{
					//Get _value field from the input field and then 
					object inputValueInstance = nodeField.GetValue();

					INodeInputField inputField = inputValueInstance as INodeInputField;

					if (inputField.IsStaticValue())
					{
						object nodeValue = SerializedObjectMemberInfo.GetSerializedFieldInstance(inputValueInstance, "value");

						if (nodeValue != null)
						{
							string labelText = nodeValue + "  ";
							GUIContent labelContent = new GUIContent(labelText);
							Vector2 size = _nodeBoldTextStyle.CalcSize(labelContent);
							size.y = kLinkIconWidth * scale;
							size.x += kLinkIconWidth;

							Rect staticFieldPos = new Rect(position.x - size.x, position.y - size.y * 0.5f, size.x, size.y);

							Handles.BeginGUI();
							Handles.color = color;
							Handles.DrawSolidDisc(new Vector3(staticFieldPos.x + kLinkIconWidth * 0.25f, staticFieldPos.y + kLinkIconWidth * 0.5f * scale, 0.0f), -Vector3.forward, kLinkIconWidth * 0.5f * scale);
							Handles.EndGUI();

							EditorUtils.DrawColoredRoundedBox(staticFieldPos, color);

							GUI.BeginGroup(staticFieldPos);
							{
								GUI.Label(new Rect(0, 0, staticFieldPos.width, staticFieldPos.height), labelContent, _nodeBoldTextStyle);
							}
							GUI.EndGroup();
						}
					}
				}

				private NodeGraph ConvertToNodeGraph()
				{
					NodeGraph nodeGraph = new NodeGraph();
					
					List<Node> nodes = new List<Node>();

					foreach (NodeEditorGUI nodeView in _editableObjects)
					{
						nodes.Add(nodeView.Asset);
					}

					nodeGraph._nodes = nodes.ToArray();

					return nodeGraph;
				}

				private int GenerateNewNodeId()
				{
					int nodeId = 0;
					bool foundNode = false;

					while (!foundNode)
					{
						foundNode = true;

						foreach (NodeEditorGUI node in _editableObjects)
						{
							if (node.Asset._nodeId == nodeId)
							{
								foundNode = false;
								nodeId++;
								break;
							}
						}	
					}

					return nodeId;
				}

				private NodeEditorField GetHighlightedNode(Vector2 mousePosition)
				{
					if (_dragMode == DragType.Custom)
					{
						foreach (NodeEditorGUI node in _editableObjects)
						{
							foreach (NodeEditorField nodeField in node.GetInputFields())
							{
								if (nodeField._type == _draggingNodeFieldFrom._type || _draggingNodeFieldFrom._type.IsAssignableFrom(nodeField._type))
								{
									Vector2 toField = mousePosition - nodeField._position;

									if (toField.magnitude < kLinkIconWidth)
									{
										return nodeField;
									}
								}
							}
						}
					}

					return null;
				}

				private static void BuildNodeMap()
				{
					if (_addInputNodeContextMenu == null || _addOutputNodeContextMenu == null || _addNodeContextMenu == null)
					{
						_addNodeContextMenu = new Dictionary<Type, string>();
						_addOutputNodeContextMenu = new Dictionary<Type, string>();
						_addInputNodeContextMenu = new Dictionary<Type, string>();

						Type[] types = SystemUtils.GetAllSubTypes(typeof(Node));

						foreach (Type type in types)
						{
							NodeCategoryAttribute nodeAttribute = SystemUtils.GetAttribute<NodeCategoryAttribute>(type);
							string nodeName;

							if (nodeAttribute != null)
							{
								if (string.IsNullOrEmpty(nodeAttribute.Category))
									nodeName = StringUtils.FromCamelCase(type.Name);
								else
									nodeName = nodeAttribute.Category + "/" + StringUtils.FromCamelCase(type.Name);
							}
							else
							{
								nodeName = StringUtils.FromCamelCase(type.Name); ;
							}

							if (SystemUtils.IsSubclassOfRawGeneric(typeof(OutputNode<,>), type))
							{
								Type outputType = SystemUtils.GetGenericImplementationType(typeof(OutputNode<,>), type, 1);
								_addOutputNodeContextMenu.Add(type, SystemUtils.GetTypeName(outputType));
							}
							else if (SystemUtils.IsSubclassOfRawGeneric(typeof(InputNode<>), type))
							{
								Type inputType = SystemUtils.GetGenericImplementationType(typeof(InputNode<>), type);
								_addInputNodeContextMenu.Add(type, SystemUtils.GetTypeName(inputType));
							}
							else
							{
								_addNodeContextMenu.Add(type, nodeName);
							}
						}
					}
				}

				private void AddNewNodeMenuCallback(object obj)
				{
					Type nodeType = (Type)obj;

					CreateAndAddNewObject(nodeType);
				}
				#endregion
			}
		}
	}
}