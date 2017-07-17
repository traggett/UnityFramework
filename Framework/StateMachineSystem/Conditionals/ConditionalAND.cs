using System;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		[ConditionalCategory("")]
		public class ConditionalAND : IConditional
		{
			#region Public Data
			public Condition[] _conditions = new Condition[0];
			#endregion

#if UNITY_EDITOR
			private bool[] _editorFoldout;
#endif

			#region IConditional
#if UNITY_EDITOR
			public string GetEditorDescription()
			{
				string description;

				if (_conditions != null && _conditions.Length > 0 && _conditions[0]._conditional != null)
				{
					description = "(" + _conditions[0]._conditional.GetEditorDescription();

					for (int i = 1; i < _conditions.Length; i++)
					{
						if (_conditions[i]._conditional != null)
						{
							description += ") <b>&&</b> (";
							description += _conditions[i]._conditional.GetEditorDescription();
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

			public bool AllowInverseVariant()
			{
				return false;
			}
#endif

			public void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
				foreach (Condition condition in _conditions)
				{
					condition._conditional.OnStartConditionChecking(stateMachine);
				}
			}

			public bool IsConditionMet(StateMachineComponent stateMachine)
			{
				bool allConditionsMet = true;

				foreach (Condition condition in _conditions)
				{
					if (!condition._conditional.IsConditionMet(stateMachine))
					{
						allConditionsMet = false;
					}
				}

				return allConditionsMet;
			}

			public void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
				foreach (Condition condition in _conditions)
				{
					condition._conditional.OnEndConditionChecking(stateMachine);
				}
			}
			#endregion
		}
	}
}