using System.Collections;

namespace Framework
{
	namespace StateMachineSystem
	{
		public interface IBranch
		{
			bool ShouldBranch(StateMachine stateMachine);
			IEnumerator GetGoToState(StateMachine stateMachine);
			void OnBranchingStarted(StateMachine stateMachine);
			void OnBranchingFinished(StateMachine stateMachine);			
		}
	}
}