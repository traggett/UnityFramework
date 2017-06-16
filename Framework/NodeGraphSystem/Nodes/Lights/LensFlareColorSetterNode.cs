using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Lights")]
		[Serializable]
		public class LensFlareColorSetterNode : Node
		{
			#region Public Data
			public ComponentNodeInputField<LensFlare> _lensFlare = new ComponentNodeInputField<LensFlare>();
			public NodeInputField<Color> _color = Color.white;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				LensFlare lensFlare = _lensFlare;

				if (lensFlare != null)
					lensFlare.color = _color;
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