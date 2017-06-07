using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;
	using InputSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("User Input")]
		[Serializable]
		public class ButtonIsHeldNode : Node, IValueSource<bool>
		{
			#region Public Data
			public int _buttonMappingId = -1;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return UserInputNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<bool>
			public bool GetValue()
			{
				return Input.IsButtonHeld(_buttonMappingId);
			}
			#endregion
		}
	}
}