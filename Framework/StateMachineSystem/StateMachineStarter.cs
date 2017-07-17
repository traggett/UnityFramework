using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
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