using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace NodeGraphSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(NodeGraphComponent), true)]
			public sealed class NodeGraphComponentInspector : UnityEditor.Editor
			{
				private SerializedProperty _unscaledTime;
				private SerializedProperty _runInEditor;

				private SerializedProperty _nodeGraphRef;
				private bool _nodeGraphRefOut = true;
				private SerializedProperty _nodeGraphRefAsset;

				private SerializedProperty _floatInputs;
				private bool _floatsInputsFoldOut = true;
				private SerializedProperty _intInputs;
				private bool _intInputsFoldOut = true;
				private SerializedProperty _floatRangeInputs;
				private bool _floatRangeInputsFoldOut = true;
				private SerializedProperty _intRangeInputs;
				private bool _intRangeInputsFoldOut = true;
				private SerializedProperty _vector2Inputs;
				private bool _vector2InputsFoldOut = true;
				private SerializedProperty _vector3Inputs;
				private bool _vector3InputsFoldOut = true;
				private SerializedProperty _vector4Inputs;
				private bool _vector4InputsFoldOut = true;
				private SerializedProperty _quaternionInputs;
				private bool _quaternionInputsFoldOut = true;
				private SerializedProperty _colorInputs;
				private bool _colorInputsFoldOut = true;
				private SerializedProperty _stringInputs;
				private bool _stringInputsFoldOut = true;
				private SerializedProperty _boolInputs;
				private bool _boolInputsFoldOut = true;
				private SerializedProperty _gradientInputs;
				private bool _gradientInputsFoldOut = true;
				private SerializedProperty _animationCurveInputs;
				private bool _animationCurveInputsFoldOut = true;
				private SerializedProperty _transformInputs;
				private bool _transformInputsFoldOut = true;
				private SerializedProperty _gameObjectInputs;
				private bool _gameObjectInputsFoldOut = true;
				private SerializedProperty _componentInputs;
				private bool _componentInputsFoldOut = true;
				private SerializedProperty _materialInputs;
				private bool _materialInputsFoldOut = true;
				private SerializedProperty _textureInputs;
				private bool _textureInputsFoldOut = true;


				private NodeGraph _nodeGraph;
				private bool _inputsFoldOut = true;

				void OnEnable()
				{
					NodeGraphComponent nodeGraphComponent = (NodeGraphComponent)target;

					_unscaledTime = serializedObject.FindProperty("_unscaledTime");
					_runInEditor = serializedObject.FindProperty("_runInEditor");
					_nodeGraphRef = serializedObject.FindProperty("_nodeGraphRef");
					_nodeGraphRefAsset = _nodeGraphRef.FindPropertyRelative("_file");

					_floatInputs = serializedObject.FindProperty("_floatInputs");
					_intInputs = serializedObject.FindProperty("_intInputs");
					_floatRangeInputs = serializedObject.FindProperty("_floatRangeInputs");
					_intRangeInputs = serializedObject.FindProperty("_intRangeInputs");
					_vector2Inputs = serializedObject.FindProperty("_vector2Inputs");
					_vector3Inputs = serializedObject.FindProperty("_vector3Inputs");
					_vector4Inputs = serializedObject.FindProperty("_vector4Inputs");
					_quaternionInputs = serializedObject.FindProperty("_quaternionInputs");
					_colorInputs = serializedObject.FindProperty("_colorInputs");
					_stringInputs = serializedObject.FindProperty("_stringInputs");
					_boolInputs = serializedObject.FindProperty("_boolInputs");
					_gradientInputs = serializedObject.FindProperty("_gradientInputs");
					_animationCurveInputs = serializedObject.FindProperty("_animationCurveInputs");
					_transformInputs = serializedObject.FindProperty("_transformInputs");
					_gameObjectInputs = serializedObject.FindProperty("_gameObjectInputs");
					_componentInputs = serializedObject.FindProperty("_componentInputs");
					_materialInputs = serializedObject.FindProperty("_materialInputs");
					_textureInputs = serializedObject.FindProperty("_textureInputs");

					if (nodeGraphComponent.GetNodeGraph() == null)
					{
						nodeGraphComponent.LoadNodeGraph();
					}

					_nodeGraph = nodeGraphComponent.GetNodeGraph();
				}

				public override void OnInspectorGUI()
				{
					NodeGraphComponent nodeGraphComponent = (NodeGraphComponent)target;

					EditorGUILayout.PropertyField(_unscaledTime);

					{
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(_runInEditor);
						if (EditorGUI.EndChangeCheck() && _runInEditor.boolValue)
						{
							nodeGraphComponent.LoadNodeGraph();
							_nodeGraph = nodeGraphComponent.GetNodeGraph();
						}
					}

					{
						_nodeGraphRefOut = EditorGUILayout.Foldout(_nodeGraphRefOut, "Node Graph");

						if (_nodeGraphRefOut)
						{
							int origIndent = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							EditorGUI.BeginChangeCheck();

							EditorGUILayout.PropertyField(_nodeGraphRefAsset);

							if (EditorGUI.EndChangeCheck())
							{
								SyncInputNodes(new Node[0]);
								serializedObject.ApplyModifiedProperties();
								nodeGraphComponent.LoadNodeGraph();
								_nodeGraph = nodeGraphComponent.GetNodeGraph();
							}

							EditorGUILayout.BeginHorizontal();
							{
								EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorGUIUtility.labelWidth - (EditorGUI.indentLevel * 15.0f) + 11));

								if (GUILayout.Button("Edit"))
								{
									NodeGraphEditorWindow.Load(_nodeGraphRefAsset.objectReferenceValue as TextAsset);
								}
								else if (GUILayout.Button("Refresh"))
								{
									nodeGraphComponent.LoadNodeGraph();
									_nodeGraph = nodeGraphComponent.GetNodeGraph();
								}
							}
							EditorGUILayout.EndHorizontal();

							EditorGUI.indentLevel = origIndent;
						}
					}
					
					if (_nodeGraph != null)
					{
						_inputsFoldOut = EditorGUILayout.Foldout(_inputsFoldOut, "Inputs");

						if (_inputsFoldOut)
						{
							int origIndent = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							Node[] inputNodes = _nodeGraph.GetInputNodes();

							//First sync nodes with objects in _inputObjects
							SyncInputNodes(inputNodes);
							
							RenderInputArray(nodeGraphComponent, _floatInputs, ref _floatsInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _intInputs, ref _intInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _floatRangeInputs, ref _floatRangeInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _intRangeInputs, ref _intRangeInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _vector2Inputs, ref _vector2InputsFoldOut);
							RenderInputArray(nodeGraphComponent, _vector3Inputs, ref _vector3InputsFoldOut);
							RenderInputArray(nodeGraphComponent, _vector4Inputs, ref _vector4InputsFoldOut);
							RenderInputArray(nodeGraphComponent, _quaternionInputs, ref _quaternionInputsFoldOut); 
							RenderInputArray(nodeGraphComponent, _colorInputs, ref _colorInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _stringInputs, ref _stringInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _boolInputs, ref _boolInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _gradientInputs, ref _gradientInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _animationCurveInputs, ref _animationCurveInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _transformInputs, ref _transformInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _gameObjectInputs, ref _gameObjectInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _componentInputs, ref _componentInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _materialInputs, ref _materialInputsFoldOut);
							RenderInputArray(nodeGraphComponent, _textureInputs, ref _textureInputsFoldOut); 

							EditorGUI.indentLevel = origIndent;
						}
					}

					serializedObject.ApplyModifiedProperties();
				}


				private void SyncInputNodes(Node[] inputNodes)
				{
					//Remove all nodes in component that are no longer in inputNodes array
					{
						RemoveOldNodes(inputNodes, _floatInputs);
						RemoveOldNodes(inputNodes, _intInputs);
						RemoveOldNodes(inputNodes, _floatRangeInputs);
						RemoveOldNodes(inputNodes, _intRangeInputs);
						RemoveOldNodes(inputNodes, _vector2Inputs);
						RemoveOldNodes(inputNodes, _vector3Inputs);
						RemoveOldNodes(inputNodes, _vector4Inputs);
						RemoveOldNodes(inputNodes, _quaternionInputs); 
						RemoveOldNodes(inputNodes, _colorInputs);
						RemoveOldNodes(inputNodes, _stringInputs);
						RemoveOldNodes(inputNodes, _boolInputs);
						RemoveOldNodes(inputNodes, _animationCurveInputs);
						RemoveOldNodes(inputNodes, _gradientInputs);
						RemoveOldNodes(inputNodes, _transformInputs);
						RemoveOldNodes(inputNodes, _gameObjectInputs);
						RemoveOldNodes(inputNodes, _componentInputs);
						RemoveOldNodes(inputNodes, _materialInputs);
						RemoveOldNodes(inputNodes, _textureInputs);
					}

					//Add any nodes that are in inputNodes array but not in component
					foreach (Node node in inputNodes)
					{
						if (!IsNodeInInputArray(_floatInputs, node._nodeId) &&
							!IsNodeInInputArray(_intInputs, node._nodeId) &&
							!IsNodeInInputArray(_floatRangeInputs, node._nodeId) &&
							!IsNodeInInputArray(_intRangeInputs, node._nodeId) &&
							!IsNodeInInputArray(_vector2Inputs, node._nodeId) &&
							!IsNodeInInputArray(_vector3Inputs, node._nodeId) &&
							!IsNodeInInputArray(_vector4Inputs, node._nodeId) &&
							!IsNodeInInputArray(_quaternionInputs, node._nodeId) &&
							!IsNodeInInputArray(_colorInputs, node._nodeId) &&
							!IsNodeInInputArray(_stringInputs, node._nodeId) &&
							!IsNodeInInputArray(_boolInputs, node._nodeId) &&
							!IsNodeInInputArray(_animationCurveInputs, node._nodeId) &&
							!IsNodeInInputArray(_transformInputs, node._nodeId) &&
							!IsNodeInInputArray(_gradientInputs, node._nodeId) &&
							!IsNodeInInputArray(_gameObjectInputs, node._nodeId) &&
							!IsNodeInInputArray(_componentInputs, node._nodeId) &&
							!IsNodeInInputArray(_materialInputs, node._nodeId) &&
							!IsNodeInInputArray(_textureInputs, node._nodeId))
						{
							AddNewInputNode(node);
						}
					}
				}
				private void AddNewInputNode(Node node)
				{
					if (node is FloatInputNode)
					{
						AddNewInputNode(node, _floatInputs);
					}
					if (node is IntInputNode)
					{
						AddNewInputNode(node, _intInputs);
					}
					else if (node is FloatRangeInputNode)
					{
						AddNewInputNode(node, _floatRangeInputs);
					}
					else if (node is IntRangeInputNode)
					{
						AddNewInputNode(node, _intRangeInputs);
					}
					else if (node is Vector2InputNode)
					{
						AddNewInputNode(node, _vector2Inputs);
					}
					else if (node is Vector3InputNode)
					{
						AddNewInputNode(node, _vector3Inputs);
					}
					else if (node is Vector4InputNode)
					{
						AddNewInputNode(node, _vector4Inputs);
					}
					else if (node is QuaternionInputNode)
					{
						AddNewInputNode(node, _quaternionInputs);
					}
					else if (node is ColorInputNode)
					{
						AddNewInputNode(node, _colorInputs);
					}
					else if (node is StringInputNode)
					{
						AddNewInputNode(node, _stringInputs);
					}
					else if (node is BoolInputNode)
					{
						AddNewInputNode(node, _boolInputs);
					}
					else if (node is AnimationCurveInputNode)
					{
						AddNewInputNode(node, _animationCurveInputs);
					}
					else if (node is GradientInputNode)
					{
						AddNewInputNode(node, _gradientInputs);
					}
					else if (node is TransformInputNode)
					{
						AddNewInputNode(node, _transformInputs);
					}
					else if (node is GameObjectInputNode)
					{
						AddNewInputNode(node, _gameObjectInputs);
					}
					else if (node is ComponentInputNode)
					{
						AddNewInputNode(node, _componentInputs);
					}
					else if (node is MaterialInputNode)
					{
						AddNewInputNode(node, _materialInputs);
					}
					else if (node is TextureInputNode)
					{
						AddNewInputNode(node, _textureInputs);
					}
				}

				private void RenderInputArray(NodeGraphComponent nodeGraphComponent, SerializedProperty inputArrayProperty, ref bool foldout)
				{
					if (inputArrayProperty != null && inputArrayProperty.arraySize > 0)
					{
						foldout = EditorGUILayout.Foldout(foldout, inputArrayProperty.displayName);

						if (foldout)
						{
							EditorGUI.indentLevel++;

							for (int i = 0; i < inputArrayProperty.arraySize; i++)
							{
								SerializedProperty serializedInput = inputArrayProperty.GetArrayElementAtIndex(i);
								SerializedProperty nodeIdProp = serializedInput.FindPropertyRelative("_nodeId");
								Node node = _nodeGraph.GetNode(nodeIdProp.intValue);
								SerializedProperty arrayItem = inputArrayProperty.GetArrayElementAtIndex(i);
								SerializedProperty prop = arrayItem.FindPropertyRelative("_valueSource");
								EditorGUILayout.PropertyField(prop, new GUIContent(node._editorDescription));
							}

							EditorGUI.indentLevel--;
						}
					}
				}

				private bool IsNodeInInputArray(SerializedProperty inputArrayProperty, int nodeId)
				{
					if (inputArrayProperty != null)
					{
						for (int i = 0; i < inputArrayProperty.arraySize; i++)
						{
							SerializedProperty serializedInput = inputArrayProperty.GetArrayElementAtIndex(i);
							SerializedProperty nodeIdProp = serializedInput.FindPropertyRelative("_nodeId");

							if (nodeId == nodeIdProp.intValue)
							{
								return true;
							}
						}
					}

					return false;
				}

				private void AddNewInputNode(Node node, SerializedProperty inputArrayProperty)
				{
					inputArrayProperty.InsertArrayElementAtIndex(inputArrayProperty.arraySize);
					SerializedProperty serializedInput = inputArrayProperty.GetArrayElementAtIndex(inputArrayProperty.arraySize - 1);
					SerializedProperty nodeIdProp = serializedInput.FindPropertyRelative("_nodeId");
					nodeIdProp.intValue = node._nodeId;				
				}

				private void RemoveOldNodes(Node[] inputNodes, SerializedProperty inputArrayProperty)
				{
					if (inputArrayProperty != null)
					{
						for (int i = 0; i < inputArrayProperty.arraySize;)
						{
							bool foundNode = false;

							SerializedProperty serializedInput = inputArrayProperty.GetArrayElementAtIndex(i);
							SerializedProperty nodeId = serializedInput.FindPropertyRelative("_nodeId");

							foreach (Node node in inputNodes)
							{
								if (node._nodeId == nodeId.intValue)
								{
									foundNode = true;
									break;
								}
							}

							if (foundNode)
							{
								i++;
							}
							else
							{
								inputArrayProperty.DeleteArrayElementAtIndex(i);
							}
						}
					}
				}
			}
		}
	}
}