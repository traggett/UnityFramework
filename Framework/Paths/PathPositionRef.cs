using System;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Utils;
	using Serialization;

	namespace Paths
	{
		[Serializable]
		public sealed class PathPositionRef : ISerializationCallbackReceiver, ICustomEditorInspector
		{
			#region Public Data
			public ComponentRef<PathNode> _pathNode;
			public ComponentRef<Path> _path;
			public float _pathT = 0.0f;
			#endregion

#if UNITY_EDITOR
			private bool _editorFoldout = true;
			private enum EditorType
			{
				Node,
				PathT,
			}
			private EditorType _editorType;
#endif

			public static implicit operator string(PathPositionRef property)
			{
				if (!string.IsNullOrEmpty(property._pathNode.GetGameObjectRef().GetGameObjectName()))
				{
					return property._pathNode.GetGameObjectRef().GetGameObjectName();
				}
				else
				{
					return typeof(PathPositionRef).Name;
				}
			}

			public static implicit operator PathPosition(PathPositionRef property)
			{
				return property.GetPathPosition();
			}

			public PathPosition GetPathPosition()
			{
				Path path = _path.GetComponent();
				PathNode pathNode = _pathNode.GetComponent();

				if (path != null)
				{
					if (pathNode == null)
					{

						return path.GetPoint(_pathT);
					}
					else
					{
						return path.GetPoint(path.GetPathT(pathNode));
					}
				}

				return default;
			}

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
#if UNITY_EDITOR
				_editorType = _pathNode.GetComponent() != null ? EditorType.Node : EditorType.PathT;
#endif
			}
			#endregion

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				if (label == null)
					label = new GUIContent();

				label.text += " (" + this + ")";

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);

				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;
					
					EditorType editorType = (EditorType)EditorGUILayout.EnumPopup("Position Type", _editorType);

					if (_editorType != editorType)
					{
						_editorType = editorType;
						_pathNode = new ComponentRef<PathNode>();
						_path = new ComponentRef<Path>();
						_pathT = 0.0f;
						dataChanged = true;
					}

					switch (_editorType)
					{
						case EditorType.Node:
							{
								_pathNode = SerializationEditorGUILayout.ObjectField(_pathNode, new GUIContent("Path Node"), ref dataChanged);
							}
							break;
						case EditorType.PathT:
							{
								_path = SerializationEditorGUILayout.ObjectField(_path, new GUIContent("Path"), ref dataChanged);

								EditorGUI.BeginChangeCheck();
								_pathT = EditorGUILayout.Slider(_pathT, 0.0f, 1.0f);
								if (EditorGUI.EndChangeCheck())
								{
									dataChanged = true;
								}
							}
							break;
					}

					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion
		}
	}
}