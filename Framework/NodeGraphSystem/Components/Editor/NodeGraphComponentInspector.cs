using UnityEngine;
using UnityEditor;

using System;

namespace Framework
{
	using Serialization;
	using Maths;
	using Utils;
	using Utils.Editor;
	using Framework.DynamicValueSystem;

	namespace NodeGraphSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(NodeGraphComponent), true)]
			[CanEditMultipleObjects]
			public sealed class NodeGraphComponentInspector : UnityEditor.Editor
			{
				private SerializedProperty _unscaledTime;

				private bool _nodeGraphRefFoldOut = true;
				private SerializedProperty _nodeGraphRef;
				private SerializedProperty _nodeGraphRefAsset;

				private SerializedProperty _floatInputs;
				private SerializedProperty _intInputs;
				private SerializedProperty _floatRangeInputs;
				private SerializedProperty _intRangeInputs;
				private SerializedProperty _vector2Inputs;
				private SerializedProperty _vector3Inputs;
				private SerializedProperty _vector4Inputs;
				private SerializedProperty _quaternionInputs;
				private SerializedProperty _colorInputs;
				private SerializedProperty _stringInputs;
				private SerializedProperty _boolInputs;
				private SerializedProperty _gradientInputs;
				private SerializedProperty _animationCurveInputs;
				private SerializedProperty _transformInputs;
				private SerializedProperty _gameObjectInputs;
				private SerializedProperty _componentInputs;
				private SerializedProperty _materialInputs;
				private SerializedProperty _textureInputs;

				private NodeGraph _nodeGraph;
				private bool _inputsFoldOut = true;
				private bool _outputsFoldOut = true;

				void OnEnable()
				{
					_unscaledTime = serializedObject.FindProperty("_unscaledTime");
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

					ReloadNodeGraph();
				}

				public override void OnInspectorGUI()
				{
					EditorGUILayout.PropertyField(_unscaledTime);
					{
						_nodeGraphRefFoldOut = EditorGUILayout.Foldout(_nodeGraphRefFoldOut, "Node Graph");

						if (_nodeGraphRefFoldOut)
						{
							int origIndent = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							EditorGUI.BeginChangeCheck();

							EditorGUILayout.PropertyField(_nodeGraphRefAsset);

							if (EditorGUI.EndChangeCheck())
							{
								SyncInputNodes(new Node[0]);
								serializedObject.ApplyModifiedProperties();
								ReloadNodeGraph();
							}

							if (!_nodeGraphRefAsset.hasMultipleDifferentValues)
							{
								EditorGUILayout.BeginHorizontal();
								{
									EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorUtils.GetLabelWidth()));

									if (GUILayout.Button("Edit"))
									{
										NodeGraphEditorWindow.Load(_nodeGraphRefAsset.objectReferenceValue as TextAsset);
									}
									else if (GUILayout.Button("Refresh"))
									{
										ReloadNodeGraph();
									}
								}
								EditorGUILayout.EndHorizontal();
							}

							EditorGUI.indentLevel = origIndent;
						}
					}
					
					if (_nodeGraph != null && !_nodeGraphRefAsset.hasMultipleDifferentValues)
					{
						_inputsFoldOut = EditorGUILayout.Foldout(_inputsFoldOut, "Inputs");

						if (_inputsFoldOut)
						{
							int origIndent = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							Node[] inputNodes = _nodeGraph.GetInputNodes();
							SyncInputNodes(inputNodes);

							foreach (Node inputNode in inputNodes)
							{
								RenderInputNode(inputNode);
							}

							EditorGUI.indentLevel = origIndent;
						}

						_outputsFoldOut = EditorGUILayout.Foldout(_outputsFoldOut, "Outputs");

						if (_outputsFoldOut)
						{
							int origIndent = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							Node[] outputNodes;
							NodeGraphComponent nodeGraphComponent = (NodeGraphComponent)target;

							//Is the application is running then get the output node directly from the component so can see the live value
							if (Application.isPlaying && nodeGraphComponent.GetOutputNodes() != null)
							{
								outputNodes = nodeGraphComponent.GetOutputNodes();
							}
							else
							{
								outputNodes = _nodeGraph.GetOutputNodes();
							}

							foreach (Node outputNode in outputNodes)
							{
								RenderOutputNode(outputNode);
							}

							EditorGUI.indentLevel = origIndent;
						}
					}

					serializedObject.ApplyModifiedProperties();
				}

				private void ReloadNodeGraph()
				{
					NodeGraphComponent nodeGraphComponent = (NodeGraphComponent)target;
					_nodeGraph = nodeGraphComponent._nodeGraphRef.LoadNodeGraph();
				}

				private void SyncInputNodes(Node[] inputNodes)
				{
					//Remove all nodes in component that are no longer in inputNodes array
					{
						RemoveOldInputNodes(inputNodes, _floatInputs);
						RemoveOldInputNodes(inputNodes, _intInputs);
						RemoveOldInputNodes(inputNodes, _floatRangeInputs);
						RemoveOldInputNodes(inputNodes, _intRangeInputs);
						RemoveOldInputNodes(inputNodes, _vector2Inputs);
						RemoveOldInputNodes(inputNodes, _vector3Inputs);
						RemoveOldInputNodes(inputNodes, _vector4Inputs);
						RemoveOldInputNodes(inputNodes, _quaternionInputs);
						RemoveOldInputNodes(inputNodes, _colorInputs);
						RemoveOldInputNodes(inputNodes, _stringInputs);
						RemoveOldInputNodes(inputNodes, _boolInputs);
						RemoveOldInputNodes(inputNodes, _animationCurveInputs);
						RemoveOldInputNodes(inputNodes, _gradientInputs);
						RemoveOldInputNodes(inputNodes, _transformInputs);
						RemoveOldInputNodes(inputNodes, _gameObjectInputs);
						RemoveOldInputNodes(inputNodes, _componentInputs);
						RemoveOldInputNodes(inputNodes, _materialInputs);
						RemoveOldInputNodes(inputNodes, _textureInputs);
					}

					//Add any nodes that are in inputNodes array but not in component
					foreach (Node inputNode in inputNodes)
					{
						SerializedProperty inputArrayProperty = GetInputArrayForNode(inputNode);
						if (inputArrayProperty != null && !IsNodeInInputArray(inputArrayProperty, inputNode._nodeId))
						{
							AddNewInputNode(inputNode, inputArrayProperty);
						}
					}
				}

				private SerializedProperty GetInputArrayForNode(Node node)
				{
					Type inputNodeType = SystemUtils.GetGenericImplementationType(typeof(InputNode<>), node.GetType());

					if (inputNodeType == typeof(float))
						return _floatInputs;
					else if (inputNodeType == typeof(int))
						return _intInputs;
					else if (inputNodeType == typeof(FloatRange))
						return _floatRangeInputs;
					else if (inputNodeType == typeof(IntRange))
						return _intRangeInputs;
					else if (inputNodeType == typeof(Vector2))
						return _vector2Inputs;
					else if (inputNodeType == typeof(Vector3))
						return _vector3Inputs;
					else if (inputNodeType == typeof(Vector4))
						return _vector4Inputs;
					else if (inputNodeType == typeof(Quaternion))
						return _quaternionInputs;
					else if (inputNodeType == typeof(Color))
						return _colorInputs;
					else if (inputNodeType == typeof(string))
						return _stringInputs;
					else if (inputNodeType == typeof(bool))
						return _boolInputs;
					else if (inputNodeType == typeof(AnimationCurve))
						return _animationCurveInputs;
					else if (inputNodeType == typeof(Gradient))
						return _gradientInputs;
					else if (inputNodeType == typeof(Transform))
						return _transformInputs;
					else if (inputNodeType == typeof(GameObject))
						return _gameObjectInputs;
					else if (inputNodeType == typeof(Component))
						return _componentInputs;
					else if (inputNodeType == typeof(Material))
						return _materialInputs;
					else if (inputNodeType == typeof(Texture))
						return _textureInputs;
					else
						return null;
				}

				private void RenderInputNode(Node node)
				{
					SerializedProperty inputArrayProperty = GetInputArrayForNode(node);

					Type inputNodeType = SystemUtils.GetGenericImplementationType(typeof(InputNode<>), node.GetType());

					if (inputNodeType != null)
					{
						for (int i = 0; i < inputArrayProperty.arraySize; i++)
						{
							SerializedProperty arrayItem = inputArrayProperty.GetArrayElementAtIndex(i);
							SerializedProperty nodeIdProp = arrayItem.FindPropertyRelative("_nodeId");

							if (nodeIdProp != null && nodeIdProp.intValue == node._nodeId)
							{
								SerializedProperty prop = arrayItem.FindPropertyRelative("_valueSource");
								if (prop != null)
									EditorGUILayout.PropertyField(prop, new GUIContent(node._editorDescription + " (" + SystemUtils.GetTypeName(inputNodeType) + ")"));
							}
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
					serializedObject.ApplyModifiedProperties();

					SetDefaultValue(serializedInput);
				}

				private void RemoveOldInputNodes(Node[] inputNodes, SerializedProperty inputArrayProperty)
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

				private void RenderOutputNode(Node node)
				{
					Type outputNodeType = SystemUtils.GetGenericImplementationType(typeof(OutputNode<,>), node.GetType(), 1);

					if (outputNodeType != null)
					{
						object value = node.GetType().GetMethod("GetValue").Invoke(node, new object[] { });
						bool guiEnabled = GUI.enabled;
						GUI.enabled = false;
						SerializationEditorGUILayout.ObjectField(value, new GUIContent(node._editorDescription + " (" + SystemUtils.GetTypeName(outputNodeType) + ")"));
						GUI.enabled = guiEnabled;
					}
				}

				private void SetDefaultValue(SerializedProperty serializedInput)
				{
					Type nodeInputType = SerializedPropertyUtils.GetSerializedPropertyType(serializedInput);
					NodeGraphComponent nodeGraphComponent = (NodeGraphComponent)target;

					//Default Transform nodes to this GameObject's Transform
					if (nodeInputType == typeof(NodeGraphComponent.TransformInput))
					{
						DynamicTransformRef transformRef = nodeGraphComponent.transform;
						SerializedProperty valueProp = serializedInput.FindPropertyRelative("_valueSource");
						SerializedPropertyUtils.SetSerializedPropertyValue(valueProp, transformRef);
					}
					//Default Material nodes to this GameObject's Renderer's material
					else if (nodeInputType == typeof(NodeGraphComponent.MaterialInput))
					{
						Renderer renderer = nodeGraphComponent.GetComponent<Renderer>();

						if (renderer != null)
						{
							DynamicMaterialRef materialRef = DynamicMaterialRef.InstancedMaterialRef(renderer); 
							SerializedProperty valueProp = serializedInput.FindPropertyRelative("_valueSource");
							SerializedPropertyUtils.SetSerializedPropertyValue(valueProp, materialRef);
						}
					}
					//Default GameObject nodes to this GameObject
					else if (nodeInputType == typeof(NodeGraphComponent.GameObjectInput))
					{
						DynamicGameObjectRef gameObjectRef = nodeGraphComponent.gameObject;
						SerializedProperty valueProp = serializedInput.FindPropertyRelative("_valueSource");
						SerializedPropertyUtils.SetSerializedPropertyValue(valueProp, gameObjectRef);
					}
				}
			}
		}
	}
}