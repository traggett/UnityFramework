using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Lights")]
		[Serializable]
		public class LensFlareBrightnessSetterNode : Node
		{
			#region Public Data
			public ComponentNodeInputField<LensFlare> _lensFlare = new ComponentNodeInputField<LensFlare>();
			public NodeInputField<float> _brightness = 0.0f;
			[Tooltip("If set instead of setting an absolute value the value will be multiplied with the original brightness value.")]
			public NodeInputField<bool> _multiplyWithOriginal = false;
			#endregion

			#region Public Data
			private float _origBrightness;
			#endregion

			#region Node
			public override void Init()
			{
				LensFlare lensFlare = _lensFlare;
				if (lensFlare != null)
					_origBrightness = lensFlare.brightness;
			}

			public override void Update(float time, float deltaTime)
			{
				LensFlare lensFlare = _lensFlare;

				if (lensFlare != null)
				{
					if (_multiplyWithOriginal)
						lensFlare.brightness = _origBrightness * _brightness;
					else
						lensFlare.brightness = _brightness;
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