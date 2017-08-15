using System;
using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public abstract class ToggableCondition : Condition
		{
			#region Public Data
			[HideInInspector]
			public bool _not;
			#endregion
		}
	}
}