using System;
using System.Collections;

namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public class BranchingState : State
		{
			public Branch[] _branches;
			public BranchingBackgroundLogic[] _backgroundLogic;

			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				if (_branches != null)
				{
					foreach (Branch branch in _branches)
					{
						branch.OnBranchingStarted(stateMachine);
					}

					foreach (BranchingBackgroundLogic branch in _backgroundLogic)
					{
						branch.OnLogicStarted(stateMachine);
					}

					bool branchTaken = false;

					while (!branchTaken)
					{
						foreach (BranchingBackgroundLogic branch in _backgroundLogic)
						{
							branch.UpdateLogic(stateMachine);
						}

						foreach (Branch branch in _branches)
						{
							if (branch.ShouldBranch(stateMachine))
							{
								IEnumerator goToState = branch.GetGoToState(stateMachine);
								if (goToState != null)
								{
									stateMachine.GoToState(goToState);
								}

								branchTaken = true;
								break;
							}
						}

						yield return null;
					}

					foreach (BranchingBackgroundLogic branch in _backgroundLogic)
					{
						branch.OnLogicFinished(stateMachine);
					}

					foreach (Branch branch in _branches)
					{
						branch.OnBranchingFinished(stateMachine);
					}
				}
			}

#if UNITY_EDITOR
			public override string GetDescription()
			{
				return _editorDescription;
			}

			public override StateMachineEditorLink[] GetEditorLinks()
			{
				if (_branches != null)
				{
					StateMachineEditorLink[] links = new StateMachineEditorLink[_branches.Length];

					for (int i = 0; i < _branches.Length; i++)
					{
						Branch branch = _branches[i];

						links[i] = new StateMachineEditorLink();
						links[i]._timeline = branch._goToState;
						links[i]._description = branch.GetDescription();
					}

					return links;
				}

				return null;
			}
#endif
		}
	}
}