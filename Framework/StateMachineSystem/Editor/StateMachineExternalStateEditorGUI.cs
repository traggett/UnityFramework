using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(StateMachineExternalState))]
			public class StateMachineExternalStateEditorGUI : StateEditorGUI
			{
				//need to point at an object's StateRef
				//So like a object pointer and a fieldinfo?
				//then can grab it from the object and set it.


				public StateMachineEditorLink ExternalStateRef
				{
					get
					{
						return ((StateMachineExternalState)GetEditableObject())._externalStateRef;
					}

					set
					{
						((StateMachineExternalState)GetEditableObject())._externalStateRef = value;
					}
				}
				public bool ExternalHasRendered
				{
					get
					{
						return _externalHasRendered;
					}

					set
					{
						_externalHasRendered = value;
					}
				}
				private bool _externalHasRendered;

				public override void SetPosition(Vector2 position)
				{
					StateRef externalStateRef = ExternalStateRef.GetStateRef();
					externalStateRef._editorExternalLinkPosition = position;
					ExternalStateRef.SetStateRef(externalStateRef);

					MarkAsDirty(true);
				}

				public override Vector2 GetPosition()
				{
					return ExternalStateRef.GetStateRef()._editorExternalLinkPosition;
				}

				public override bool RenderObjectProperties(GUIContent label)
				{
					EditorGUI.BeginChangeCheck();
					
					GUILayout.Label("External State Link: " + ExternalStateRef, EditorStyles.centeredGreyMiniLabel);

					EditorGUILayout.Separator();

					if (GUILayout.Button("Open State machine"))
					{
						StateMachineEditor timelineStateMachineEditor = (StateMachineEditor)GetEditor();
						timelineStateMachineEditor.LoadExternalState(this);
					}

					return EditorGUI.EndChangeCheck();
				}

				public override void OnDoubleClick()
				{
					StateMachineEditor editor = (StateMachineEditor)GetEditor();
					editor.LoadExternalState(this);
				}

				protected override string GetStateIdLabel()
				{
					return "External State";
				}

				public override Color GetColor(StateMachineEditorStyle style)
				{
					return style._externalStateColor;
				}
			}
		}
	}
}