using UnityEngine;

namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		[RequireComponent(typeof(StateMachineComponent))]
		public class StateMachineStarter : MonoBehaviour
		{
			#region Public Data
			public StateRefProperty _initialState;
			#endregion

			#region MonoBehaviour
			void Start()
			{
				StateMachineComponent stateMachine = GetComponent<StateMachineComponent>();
				stateMachine.GoToState(StateMachine.Run(stateMachine, _initialState));
			}
			#endregion
		}
	}
}