using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Lights")]
		[Serializable]
		public class LightIntensitySetterNode : Node
		{
			#region Public Data
			public ComponentNodeInputField<Light> _light;
			public NodeInputField<float> _intensity = 1.0f;
			[Tooltip("If set instead of setting an absolute value the value will be multiplied with the original intensity value.")]
			public NodeInputField<bool> _multiplyWithOriginal = false;
			#endregion

			#region Public Data
			private float _origIntensity;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				Light light = _light;

				if (light != null)
				{
					if (IsFirstUpdate())
					{
						_origIntensity = light.intensity;
					}

					if (_multiplyWithOriginal)
						light.intensity = _origIntensity * _intensity;
					else
						light.intensity = _intensity;
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