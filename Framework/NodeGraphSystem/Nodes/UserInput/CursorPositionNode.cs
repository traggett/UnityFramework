using UnityEngine;
using System;

namespace Framework
{
	using UI;
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("User Input")]
		[Serializable]
		public class CursorPositionNode : Node, IValueSource<Vector2>
		{
			#region Private Data 
			private Vector2 _value;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return UserInputNodes.kNodeColor;
			}
#endif

			public override void Update(float deltaTime)
			{
				UIInputEvent inputEvent = UIInput.GetCursorEvent();

				if (inputEvent != null && inputEvent.State != UIInputEvent.eState.Invalid)
				{
					_value = inputEvent.Position;
				}
				else
				{
					_value = new Vector2(0.5f, 0.5f);
				}
			}
			#endregion

			#region IValueSource<Vector2>
			public Vector2 GetValue()
			{
				return _value;
			}
			#endregion
		}
	}
}