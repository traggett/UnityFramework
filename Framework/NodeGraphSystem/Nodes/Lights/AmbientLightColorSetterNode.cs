using UnityEngine;
using System;

namespace Framework
{
	using Graphics;

	namespace NodeGraphSystem
	{
		[NodeCategory("Lights")]
		[Serializable]
		public class AmbientLightColorSetterNode : Node
		{
			#region Public Data
			public ComponentNodeInputField<AmbientLightSetter> _ambientLight = new ComponentNodeInputField<AmbientLightSetter>();
			public NodeInputField<Color> _color = Color.black;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				AmbientLightSetter ambientLight = _ambientLight;

				if (ambientLight != null)
				{
					ambientLight.SetColor(_color);
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