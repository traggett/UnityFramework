using System;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[RequireComponent(typeof(Camera))]
		[ExecuteInEditMode]
		public class CameraEvents : MonoBehaviour
		{
			#region Public Data
			public class CameraEvent : EventArgs
			{
				public CameraEvent(Camera val)
				{
					Camera = val;
				}

				public Camera Camera { get; set; }
			}

			public event EventHandler<CameraEvent> _onPreCull = delegate { };
			public event EventHandler<CameraEvent> _onPreRender = delegate { };
			public event EventHandler<CameraEvent> _onPostRender = delegate { };
			#endregion

			#region Private Data
			private Camera _camera;
			#endregion

			#region MonoBehaviour Calls
			void Awake()
			{
				_camera = GetComponent<Camera>();
			}

			public void OnPreCull()
			{
				_onPreCull(this, new CameraEvent(_camera));
			}

			public void OnPreRender()
			{
				_onPreRender(this, new CameraEvent(_camera));
			}

			public void OnPostRender()
			{
				_onPostRender(this, new CameraEvent(_camera));
			}
			#endregion
		}
	}
}
