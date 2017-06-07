using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Lights")]
		[Serializable]
		public class GetLensFlareBrightnessNode : Node, IValueSource<float>
		{
			#region Public Data
			public ComponentNodeInputField<LensFlare> _lensFlare = new ComponentNodeInputField<LensFlare>();
			#endregion
			
			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return LightNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<float>
			public float GetValue()
			{
				LensFlare lensFlare = _lensFlare;

				if (lensFlare != null)
				{
					return lensFlare.brightness;
				}

				return 0.0f;
			}
			#endregion
		}
	}
}