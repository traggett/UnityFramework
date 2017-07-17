using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(CoroutineState))]
			public class CoroutineStateEditorGUI : StateEditorGUI
			{
				public override bool RenderObjectProperties(GUIContent label)
				{
					bool dataChanged = false;

					CoroutineState conditionalState = (CoroutineState)GetEditableObject();
					
					dataChanged |= RenderStateDescriptionField();
					dataChanged |= RenderStateColorField();

					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("Coroutine:", EditorStyles.boldLabel);
					EditorGUILayout.Separator();

					Serialization.SerializationEditorGUILayout.ObjectField(GetEditableObject(), GUIContent.none, ref dataChanged);

					return dataChanged;
				}

				public override Color GetColor(StateMachineEditorStyle style)
				{
					return style._coroutineStateColor;
				}
			}
		}
	}
}