using System;
using Random = UnityEngine.Random;

namespace Framework
{
	using TimelineStateMachineSystem;

	namespace StateMachineSystem
	{
		[Serializable]
		[ConditionCategory("Time")]
		public class ConditionAfterTime : Condition
		{
			#region Public Data
			public float _timeoutMin = 1.0f;
			public float _timeoutMax = 10.0f;
			#endregion

			private float _time;

			#region Conditional
#if UNITY_EDITOR
			public override string GetDescription()
			{
				return "After (" + _timeoutMin + "-" + _timeoutMax + ") secs";
			}

			public override string GetTakenText()
			{
				return GetDescription();
			}
#endif

			public override void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
				_time = Random.Range(_timeoutMin, _timeoutMax);
			}

			public override bool IsConditionMet(StateMachineComponent stateMachine)
			{
				ITimelineStateTimer timer = TimelineState.GetTimer(stateMachine.gameObject);
				_time -= timer.GetDeltaTime();

				return _time <= 0.0f;
			}

			public override void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
			}
			#endregion
		}
	}
}