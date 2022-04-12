using System;
using System.Collections;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public class ConditionalState : State
		{
			public ConditionalStateBranch[] _branches;
			public ConditionalStateBackgroundLogic[] _backgroundLogic;

			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				if (_branches != null)
				{
					foreach (ConditionalStateBranch branch in _branches)
					{
						branch.OnBranchingStarted(stateMachine);
					}

					foreach (ConditionalStateBackgroundLogic branch in _backgroundLogic)
					{
						branch.OnLogicStarted(stateMachine);
					}

					bool branchTaken = false;

					while (!branchTaken)
					{
						foreach (ConditionalStateBackgroundLogic branch in _backgroundLogic)
						{
							branch.UpdateLogic(stateMachine);
						}

						foreach (ConditionalStateBranch branch in _branches)
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

					foreach (ConditionalStateBackgroundLogic branch in _backgroundLogic)
					{
						branch.OnLogicFinished(stateMachine);
					}

					foreach (ConditionalStateBranch branch in _branches)
					{
						branch.OnBranchingFinished(stateMachine);
					}
				}
			}

#if UNITY_EDITOR
			public override StateMachineEditorLink[] GetEditorLinks()
			{
				if (_branches != null)
				{
					StateMachineEditorLink[] links = new StateMachineEditorLink[_branches.Length];

					for (int i = 0; i < _branches.Length; i++)
					{
						ConditionalStateBranch branch = _branches[i];
						links[i] = new StateMachineEditorLink(branch, "goToState", branch.GetTakenText());
					}

					return links;
				}

				return null;
			}

			public override string GetAutoDescription()
			{
				string label = "";
				bool firstBranch = true;

				if (_branches != null)
				{
					foreach (ConditionalStateBranch branch in _branches)
					{
						if (!firstBranch)
							label += "\nOr ";

						label += branch.GetDescription();

						firstBranch = false;
					}
				}			

				if (_backgroundLogic != null && _backgroundLogic.Length > 0)
				{
					label += "\n\nDuring State:";
					
					foreach (ConditionalStateBackgroundLogic backgroundLogic in _backgroundLogic)
					{
						label += "\n" + backgroundLogic.GetDescription();
					}
				}

				return label;
			}

			public override string GetStateIdLabel()
			{
				return "Conditional (State" + _stateId.ToString("00") + ")";
			}
#endif
		}
	}
}