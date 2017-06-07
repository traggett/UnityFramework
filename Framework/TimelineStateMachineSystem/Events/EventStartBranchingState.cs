using UnityEngine;

namespace Framework
{
	using StateMachineSystem;
	using System;
	using TimelineSystem;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory(EventCategoryAttribute.kCoreEvent)]
		public class EventStartBranchingState : Event, IStateMachineEvent
		{
			#region Public Data
			public Branch[] _branches = new Branch[0];
			public BranchingBackgroundLogic[] _backgroundLogic = new BranchingBackgroundLogic[0];
			#endregion

			#region Event
#if UNITY_EDITOR
			public override bool EndsTimeline()
			{
				return true;
			}

			public override Color GetColor()
			{
				return new Color(90.0f / 255.0f, 140.0f / 255.0f, 227.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return null;
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachine stateMachine)
			{
				IBranch[] branches = new IBranch[_branches.Length + _backgroundLogic.Length];

				Array.Copy(_branches, branches, _branches.Length);
				Array.Copy(_backgroundLogic, 0, branches, _branches.Length, _backgroundLogic.Length);

				stateMachine.GoToBranchingState(branches);

				return eEventTriggerReturn.EventFinishedExitState;
			}

			public eEventTriggerReturn Update(StateMachine stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachine stateMachine) { }

#if UNITY_EDITOR
			public EditorStateLink[] GetEditorLinks()
			{
				EditorStateLink[] links = new EditorStateLink[_branches.Length];

				for (int i=0; i<_branches.Length; i++)
				{
					Branch branch = _branches[i];

					links[i] = new EditorStateLink();
					links[i]._timeline = branch._goToState;
					links[i]._description = branch.GetDescription();
				}

				return links;
			}
#endif
			#endregion
		}
	}
}