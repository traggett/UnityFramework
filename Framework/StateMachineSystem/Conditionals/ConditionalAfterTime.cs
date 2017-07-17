using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[ConditionalCategory("Time")]
		public class ConditionalAfterTime : IConditional
		{
			#region Public Data
			public float _timeoutMin = 1.0f;
			public float _timeoutMax = 10.0f;
			#endregion

			private float _time;

			#region IConditional
#if UNITY_EDITOR
			public string GetEditorDescription()
			{
				return "After (<b>" + _timeoutMin + "-" + _timeoutMax + "</b>) secs:";
			}

			public bool AllowInverseVariant()
			{
				return false;
			}
#endif

			public void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
				_time = Random.Range(_timeoutMin, _timeoutMax);
			}

			public bool IsConditionMet(StateMachineComponent stateMachine)
			{
				ITimelineStateTimer timer = TimelineState.GetTimer(stateMachine.gameObject);
				_time -= timer.GetDeltaTime();

				return _time <= 0.0f;
			}

			public void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
			}
			#endregion
		}
	}
}