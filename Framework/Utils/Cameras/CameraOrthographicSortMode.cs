using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[RequireComponent(typeof(Camera))]
		[ExecuteInEditMode]
		public class CameraOrthographicSortMode : MonoBehaviour
		{
			public TransparencySortMode _sortMode;
			
			void OnEnable()
			{
				Camera camera = GetComponent<Camera>();
				camera.transparencySortMode = _sortMode;
			}
		}
	}
}