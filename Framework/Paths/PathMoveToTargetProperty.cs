using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Framework.Maths;
using Framework.Serialization;

namespace Framework
{
	namespace Paths
	{
		[Serializable]
		public sealed class PathMoveToTargetProperty : ICustomEditorInspector
		{
			#region Public Data
			public PathPositionRef _pathPosition = new PathPositionRef();
			public float _speed = 0.5f;
			public bool _moveThrough = false;
			public Direction1D _faceDirection = Direction1D.Forwards;
			#endregion

#if UNITY_EDITOR
			private bool _editorFoldout = true;
#endif

			public static implicit operator string(PathMoveToTargetProperty property)
			{
				if (property != null)
					return property._pathPosition;

				return string.Empty;
			}

			public static implicit operator PathMoveToTarget(PathMoveToTargetProperty property)
			{
				return property.GetMoveToTarget();
			}

			public PathMoveToTarget GetMoveToTarget()
			{
				PathMoveToTarget target = new PathMoveToTarget();

				target._position = _pathPosition;
				target._speed = _speed;
				target._moveThrough = _moveThrough;
				target._finalFaceDirection = _faceDirection;

				return target;
			}

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

					_pathPosition = SerializationEditorGUILayout.ObjectField(_pathPosition, new GUIContent("Path Position"), ref dataChanged);

					EditorGUI.BeginChangeCheck();
					_speed = EditorGUILayout.FloatField("Speed", _speed);
					dataChanged |= EditorGUI.EndChangeCheck();

					EditorGUI.BeginChangeCheck();
					_moveThrough = EditorGUILayout.Toggle("Move Through", _moveThrough);
					dataChanged |= EditorGUI.EndChangeCheck();

					_faceDirection = SerializationEditorGUILayout.ObjectField(_faceDirection, "Final Face Direction", ref dataChanged);

					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion
		}
	}
}