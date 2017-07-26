using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using DynamicValueSystem;
	using Utils;
	using UnityEngine;
	using Serialization;

	namespace NodeGraphSystem
	{
		public enum eSourceType
		{
			Static,
			Node,
		}

		public interface INodeInputField
		{
			void SetParentNodeGraph(NodeGraph nodeGraph);
#if UNITY_EDITOR
			void SetSourceNode(int nodeId);
			int GetSourceNodeId();
			bool IsStaticValue();
#endif
		}

		public abstract class NodeInputFieldBase<T> : IValueSource<T>, INodeInputField, ICustomEditorInspector
		{
			[HideInInspector]
			public eSourceType _sourceType = eSourceType.Static;
			[HideInInspector]
			public int _sourceNodeId = -1;

			#region Private Data
			private NodeGraph _nodeGraph;
			private IValueSource<T> _sourceObject;
#if UNITY_EDITOR
			private bool _editorFoldout = true;
#endif
			#endregion
			
			public static implicit operator T(NodeInputFieldBase<T> nodeInput)
			{
				if (nodeInput != null)
					return nodeInput.GetValue();

				return default(T);
			}

			#region INodeInputField
			public void SetParentNodeGraph(NodeGraph nodeGraph)
			{
				_nodeGraph = nodeGraph;

				switch (_sourceType)
				{
					case eSourceType.Node:
						_sourceObject = _nodeGraph.GetNode(_sourceNodeId) as IValueSource<T>;
						break;
				}
			}

#if UNITY_EDITOR
			public void SetSourceNode(int nodeId)
			{
				ClearStaticValue();
				_sourceNodeId = nodeId;
				_sourceType = nodeId != -1 ? eSourceType.Node : eSourceType.Static;
			}

			public int GetSourceNodeId()
			{
				return _sourceNodeId;
			}

			public bool IsStaticValue()
			{
				return _sourceType == eSourceType.Static;
			}
#endif
			#endregion

			protected abstract T GetStaticValue();
#if UNITY_EDITOR
			protected abstract void ClearStaticValue();
#endif

			#region IValueSource<T>
			public T GetValue()
			{
				switch (_sourceType)
				{
					case eSourceType.Node:
						if (_sourceObject != null)
							return _sourceObject.GetValue();
						break;
					default:
					case eSourceType.Static:
						return GetStaticValue();
				}

				return default(T);
			}
			#endregion

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				if (label == null)
					label = new GUIContent();

				label.text += " (" + (typeof(T) == typeof(float) ? "float" : typeof(T).Name) + ")";

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);
				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					_sourceType = SerializationEditorGUILayout.ObjectField(_sourceType, new GUIContent("Input Type", "Static: Value will be constant.\nNode: Value read from other nodes output."), ref dataChanged);
					if (dataChanged)
					{
						ClearStaticValue();
						_sourceNodeId = -1;
					}

					switch (_sourceType)
					{
						case eSourceType.Static:
							{
								SerializationEditorGUILayout.RenderObjectMemebers(this, GetType(), ref dataChanged);
							}
							break;
						case eSourceType.Node:
							{
								dataChanged |= DrawNodeNamePopUps();
							}
							break;
					}

					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion

#if UNITY_EDITOR
			private bool DrawNodeNamePopUps()
			{
				if (_nodeGraph != null)
				{
					if (_nodeGraph._nodes != null && _nodeGraph._nodes.Length > 0)
					{
						List<string> nodeNames = new List<string>();
						List<int> nodeIDs = new List<int>();
						int index = 0;
						nodeNames.Add("<none>");
						nodeIDs.Add(-1);

						for (int i = 0; i < _nodeGraph._nodes.Length; i++)
						{
							Type DynamicValueType = SystemUtils.GetGenericImplementationType(typeof(IValueSource<>), _nodeGraph._nodes[i].GetType());

							if (DynamicValueType != null && DynamicValueType.IsAssignableFrom(typeof(T)))
							{
								if (_nodeGraph._nodes[i]._nodeId == _sourceNodeId)
								{
									index = nodeIDs.Count;
								}

								nodeNames.Add(StringUtils.GetFirstLine(_nodeGraph._nodes[i]._editorDescription) + " (" + StringUtils.FromCamelCase(_nodeGraph._nodes[i].GetType().Name) + ")");
								nodeIDs.Add(_nodeGraph._nodes[i]._nodeId);
							}
						}

						EditorGUI.BeginChangeCheck();

						index = EditorGUILayout.Popup("Node", index, nodeNames.ToArray());

						if (EditorGUI.EndChangeCheck())
						{
							_sourceNodeId = nodeIDs[index];
							return true;
						}
					}
				}

				return false;
			}
#endif
		}
	}
}