using System;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		[Serializable]
		public class SceneLoadingInfo
		{
			#region Public Data		
			public SceneRef _scene;
			public GameObjectLoader.Ref[] _additionalLoadingObjects;
			#endregion

#if UNITY_EDITOR
			private bool _editorFoldout = true;
#endif

			public SceneLoadingInfo()
			{
				_scene = new SceneRef();
				_additionalLoadingObjects = null;
			}

			public SceneLoadingInfo(string scenePath)
			{
				_scene = new SceneRef();
				_scene._scenePath = scenePath;
				_additionalLoadingObjects = null;
			}

			public SceneLoadingInfo(SceneRef scene, GameObjectLoader.Ref[] additionalLoadingObjects)
			{
				_scene = scene;
				_additionalLoadingObjects = additionalLoadingObjects;
			}

			public SceneLoadingInfo(SceneRef scene)
			{
				_scene = scene;
				_additionalLoadingObjects = null;
			}

			public static implicit operator string(SceneLoadingInfo property)
			{
				if (property != null)
					return property._scene;

				return "(Scene)";
			}

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderProperty(GUIContent label)
			{
				bool dataChanged = false;

				if (label == null)
					label = new GUIContent();

				label.text += " (" + this;

				if (_additionalLoadingObjects != null && _additionalLoadingObjects.Length > 0)
					label.text += " with " + GetAdditionalLoadingObjectsDescription();

				label.text += ")";

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);

				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;


					_scene = SerializationEditorGUILayout.ObjectField(_scene, "Scene", ref dataChanged);
					_additionalLoadingObjects = SerializationEditorGUILayout.ObjectField(_additionalLoadingObjects, "Additional Loading Objects", ref dataChanged);

					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion

			private string GetAdditionalLoadingObjectsDescription()
			{
				string longName = string.Empty;

				for (int i = 0; i < _additionalLoadingObjects.Length; i++)
				{
					if (i > 0 && !string.IsNullOrEmpty(longName))
						longName += ", ";

					longName += _additionalLoadingObjects[i];
				}

				return longName;
			}
		}
	}
}
