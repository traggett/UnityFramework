using System;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public abstract class ToggableCondition : Condition
		{
			#region Public Data
			public bool _not;
			#endregion
		}
	}
}