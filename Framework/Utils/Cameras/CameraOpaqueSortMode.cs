using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
	namespace Utils
	{
		[RequireComponent(typeof(Camera))]
		[ExecuteInEditMode]
		public class CameraOpaqueSortMode : MonoBehaviour
		{
			public OpaqueSortMode _sortMode;

			void OnEnable()
			{
				Camera camera = GetComponent<Camera>();
				camera.opaqueSortMode = _sortMode;
			}
		}
	}
}