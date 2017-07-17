using System;

namespace Framework
{ 
	namespace StateMachineSystem
	{
		[Serializable]
		[ConditionCategory("")]
		public class ConditionDefault : Condition
		{
			#region Conditional
#if UNITY_EDITOR
			public override string GetDescription()
			{
				return "By Default";
			}

			public override string GetTakenText()
			{
				return GetDescription();
			}
#endif

			public override void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
			}

			public override bool IsConditionMet(StateMachineComponent stateMachine)
			{
				return true;
			}

			public override void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
			}
			#endregion
		}
	}
}