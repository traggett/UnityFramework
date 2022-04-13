using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		public abstract class SerializedObjectEditorGUIInspector<T> : UnityEditor.Editor where T : ScriptableObject
		{
			private UnityEditor.Editor _editor;

			protected override void OnHeaderGUI()
			{
				CacheEditor();

				_editor.DrawHeader();
			}

			public override void OnInspectorGUI()
			{
				CacheEditor();

				EditorGUI.BeginChangeCheck();

				_editor.DrawDefaultInspector();

				if (EditorGUI.EndChangeCheck())
				{
					SerializedObjectEditorGUI<T> editorGUI = (SerializedObjectEditorGUI<T>)target;
					editorGUI.GetEditor().MarkAsDirty();
					editorGUI.GetEditor().SetNeedsRepaint();
				}
			}

			private void CacheEditor()
			{
				if (_editor == null)
				{
					SerializedObjectEditorGUI<T> editorGUI = (SerializedObjectEditorGUI<T>)target;
					_editor = UnityEditor.Editor.CreateEditor(editorGUI.GetEditableObject());
				}
			}

		}
	}
}