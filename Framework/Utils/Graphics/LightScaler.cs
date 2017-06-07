using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[ExecuteInEditMode()]
		[RequireComponent(typeof(Light))]
		public class LightScaler : MonoBehaviour
		{
			//Scale  range and flips light direction with scale
			public float _range;
			public Vector3 _angles;

			private Light _light;
			private Light TargetLight
			{
				get
				{
					if (_light == null)
					{
						_light = GetComponent<Light>();
					}
					return _light;
				}
			}

			void Update()
			{
				Vector3 scale = this.transform.parent.lossyScale;
				TargetLight.range = _range * Mathf.Abs(scale.z);

				if (TargetLight.type == LightType.Spot || TargetLight.type == LightType.Directional)
				{
					Quaternion rotation = Quaternion.Euler(scale.y > 0.0f ? _angles.x : -_angles.x, scale.x > 0.0f ? _angles.y : -_angles.y, _angles.z);
					this.transform.localRotation = rotation;
				}
			}	
		}
	}
}