using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Lights")]
		[Serializable]
		public class LightColorSetterNode : Node
		{
			#region Public Data
			public ComponentNodeInputField<Light> _light = new ComponentNodeInputField<Light>();
			public NodeInputField<Color> _color = Color.white;
			#endregion
			
			#region Node
			public override void Update(float time, float deltaTime)
			{
				Light light = _light;

				if (light != null)
				{
					light.color = _color;
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return LightNodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}