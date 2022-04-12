using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		using Editor;

		namespace Timelines
		{
			namespace Editor
			{
				[StateCustomEditorGUI(typeof(TimelineState))]
				public class TimeLineStateEditorGUI : StateEditorGUI
				{
					#region StateEditorGUI		
					public override void OnDoubleClick()
					{
						StateMachineEditor editor = (StateMachineEditor)GetEditor();
						editor.ShowStateDetails(GetStateId());
					}

					public override bool RenderObjectProperties(GUIContent label)
					{
						bool dataChanged = false;

						dataChanged |= RenderStateDescriptionField();
						dataChanged |= RenderStateColorField();

						if (GUILayout.Button("Edit Timeline"))
						{
							StateMachineEditor timelineStateMachineEditor = (StateMachineEditor)GetEditor();
							timelineStateMachineEditor.ShowStateDetails(GetEditableObject()._stateId);
						}

						Serialization.SerializationEditorGUILayout.ObjectField(GetEditableObject(), GUIContent.none, ref dataChanged);

						return dataChanged;
					}
					#endregion
				}
			}
		}
	}
}