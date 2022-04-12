using System;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		[ConditionCategory("")]
		public class ConditionAND : Condition
		{
			#region Public Data
			public Condition[] _conditions = new Condition[0];
			#endregion

			#region Conditional
#if UNITY_EDITOR
			public override string GetDescription()
			{
				string description;

				if (_conditions != null && _conditions.Length > 0 && _conditions[0] != null)
				{
					description = "If (" + _conditions[0].GetDescription();

					for (int i = 1; i < _conditions.Length; i++)
					{
						if (_conditions[i] != null)
						{
							description += ") && (";
							description += _conditions[i].GetDescription();
						}
					}

					description += ")";
				}
				else
				{
					description = "(condition) && (condition)";
				}

				return description;
			}

			public override string GetTakenText()
			{
				return GetDescription();
			}
#endif

			public override void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
				foreach (Condition condition in _conditions)
				{
					condition.OnStartConditionChecking(stateMachine);
				}
			}

			public override bool IsConditionMet(StateMachineComponent stateMachine)
			{
				bool allConditionsMet = true;

				foreach (Condition condition in _conditions)
				{
					if (!condition.IsConditionMet(stateMachine))
					{
						allConditionsMet = false;
					}
				}

				return allConditionsMet;
			}

			public override void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
				foreach (Condition condition in _conditions)
				{
					condition.OnEndConditionChecking(stateMachine);
				}
			}
			#endregion
		}
	}
}