#if UNITY_EDITOR

using System;
using System.Collections;

namespace Framework
{
	using UnityEngine;
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachineNote : State
		{
			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				throw new NotImplementedException();
			}

			public override string GetEditorLabel()
			{
				return "Note";
			}

			public override Color GetEditorColor()
			{
				return new Color(224 / 255f, 223 / 255f, 188 / 255f);
			}
		}
	}
}

#endif