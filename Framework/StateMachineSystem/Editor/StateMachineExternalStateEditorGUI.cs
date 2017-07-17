namespace Framework
{
	using StateMachineSystem.Editor;
	using UnityEditor;
	using UnityEngine;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(StateMachineExternalState))]
			public class StateMachineExternalStateEditorGUI : StateEditorGUI
			{
				public StateRef ExternalStateRef
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
					StateRef externalStateRef = ExternalStateRef;
					externalStateRef._editorExternalLinkPosition = position;
					ExternalStateRef = externalStateRef;

					MarkAsDirty(true);
				}

				public override Vector2 GetPosition()
				{
					return ExternalStateRef._editorExternalLinkPosition;
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