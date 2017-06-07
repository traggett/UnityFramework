using UnityEditor;

namespace Engine
{
	using Utils.Editor;
	using Utils;

	namespace JSON
	{
		public class JSONObjectEditorGUIInspector<T> : Editor where T : class
		{
			protected override void OnHeaderGUI()
			{
				JSONObjectEditorGUI<T> editorGUI = (JSONObjectEditorGUI<T>)target;

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
				JSONObjectEditorGUI<T> editorGUI = (JSONObjectEditorGUI<T>)target;

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