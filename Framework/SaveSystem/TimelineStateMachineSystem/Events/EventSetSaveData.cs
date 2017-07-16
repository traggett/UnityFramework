using System;

using UnityEngine;

namespace Framework
{
	using SaveSystem;
	using StateMachineSystem;
	using TimelineSystem;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory("SaveData")]
		public class EventSetSaveData : Event, IStateMachineEvent
		{
			public SaveDataValueRef<object> _saveDataProperty;
			public object _value;
			
			#region Event
#if UNITY_EDITOR
			public override Color GetColor()
			{
				return new Color(0.9f, 0.83f, 0.15f);
			}

			public override string GetEditorDescription()
			{
				return "Set <b>" + _saveDataProperty + "</b> to <b>" + _value + "</b>";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachine stateMachine) 
			{
				SaveDataBlock saveData = _saveDataProperty.GetSaveData();
				
				if (saveData != null && _value != null)
				{
					saveData.SetValue(_saveDataProperty.GetSaveValueID(), _value);
				}

				return eEventTriggerReturn.EventFinished;
			}

			public eEventTriggerReturn Update(StateMachine stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachine stateMachine) { }
#if UNITY_EDITOR
			public EditorStateLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}