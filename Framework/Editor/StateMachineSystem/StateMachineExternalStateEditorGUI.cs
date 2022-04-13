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

				public override void OnDoubleClick()
				{
					StateMachineEditor editor = (StateMachineEditor)GetEditor();
					editor.LoadExternalState(this);
				}

			}
		}
	}
}