using System.Collections;

namespace Framework
{
	namespace StateMachineSystem
	{
		public interface IBranch
		{
			bool ShouldBranch(StateMachineComponent stateMachine);
			IEnumerator GetGoToState(StateMachineComponent stateMachine);
			void OnBranchingStarted(StateMachineComponent stateMachine);
			void OnBranchingFinished(StateMachineComponent stateMachine);			
		}
	}
}