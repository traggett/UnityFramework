using UnityEditor;

namespace Framework
{
	using Utils.Editor;
	using Utils;

	namespace Serialization
	{
		public abstract class SerializedObjectEditorGUIInspector<T> : UnityEditor.Editor where T : class
		{
			protected override void OnHeaderGUI()
			{
				SerializedObjectEditorGUI<T> editorGUI = (SerializedObjectEditorGUI<T>)target;

				if (editorGUI != null && editorGUI.IsValid())
				{
					EditorUtils.DrawSimpleInspectorHeader(StringUtils.FromCamelCase(editorGUI.GetEditableObject().GetType().Name));
				}
				else
				{
					Selection.activeObject = null;
				}
			}

			public override void OnInspectorGUI()
			{
				SerializedObjectEditorGUI<T> editorGUI = (SerializedObjectEditorGUI<T>)target;

				if (editorGUI != null && editorGUI.IsValid())
				{
					EditorGUILayout.Separator();
					EditorGUILayout.Separator();

					EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);

					editorGUI.RenderProperties();

					if (editorGUI.GetEditor().NeedsRepaint())
					{
						editorGUI.GetEditor().GetEditorWindow().DoRepaint();
					}

					EditorGUILayout.EndVertical();
				}
				else
				{
					Selection.activeObject = null;
				}
			}
		}
	}
}