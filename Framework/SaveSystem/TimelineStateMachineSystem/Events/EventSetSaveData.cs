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
		public class EventSetSaveData : Event, ITimelineStateEvent
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
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine) 
			{
				SaveDataBlock saveData = _saveDataProperty.GetSaveData();
				
				if (saveData != null && _value != null)
				{
					saveData.SetValue(_saveDataProperty.GetSaveValueID(), _value);
				}

				return eEventTriggerReturn.EventFinished;
			}

			public eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachineComponent stateMachine) { }
#if UNITY_EDITOR
			public StateMachineEditorLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}