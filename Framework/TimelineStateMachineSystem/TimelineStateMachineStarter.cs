using UnityEngine;

namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		[RequireComponent(typeof(StateMachine))]
		public class TimelineStateMachineStarter : MonoBehaviour
		{
			#region Public Data
			public TimelineStateRefProperty _initialState;
			#endregion

			#region MonoBehaviour
			void Start()
			{
				StateMachine stateMachine = GetComponent<StateMachine>();
				stateMachine.GoToState(TimelineStateMachine.Run(stateMachine, _initialState));
			}
			#endregion
		}
	}
}