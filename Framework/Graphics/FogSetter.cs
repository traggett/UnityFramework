using UnityEngine;

namespace Framework
{
	namespace Graphics
	{
		[ExecuteInEditMode()]
		public class FogSetter : MonoBehaviour
		{
			public bool _enableFog;
			public Color _fogColor;
			public float _fogDensity;
			public float _fogEndDistance;
			public FogMode _fogMode = FogMode.Exponential;
			public float _fogStartDistance;

#if UNITY_EDITOR
			void OnEnable()
			{
				if (!Application.isPlaying)
					SetFog();
			}
#endif

			public void SetFog()
			{
				if (_enableFog)
				{
					RenderSettings.fog = true;
					RenderSettings.fogColor = _fogColor;
					RenderSettings.fogMode = _fogMode;
					RenderSettings.fogDensity = _fogDensity;
					RenderSettings.fogStartDistance = _fogStartDistance;
					RenderSettings.fogEndDistance = _fogEndDistance;
				}
				else
				{
					RenderSettings.fog = false;
					RenderSettings.fogColor = Color.black;
				}
			}

			public static void ClearFog()
			{
				RenderSettings.fog = false;
				RenderSettings.fogColor = Color.black;
				RenderSettings.fogDensity = 0.0f;
			}
		}
	}
}