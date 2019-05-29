using UnityEditor;
using System.Collections;

namespace Framework
{
	namespace Editor
	{
		public class UpdatedEditorWindow : EditorWindow
		{
			#region Private Data
			private enum eState
			{
				NotRunning,
				Running,
				Paused,
			}
			private eState _state = eState.NotRunning;
			private IEnumerator _current;
			private IEnumerator _next;
			#endregion

			#region EditorWindow
			private void OnEnable()
			{
				EditorApplication.update += Update;
			}

			void OnDisable()
			{
				EditorApplication.update -= Update;
			}
			#endregion

			#region Proteced Functions
			protected virtual void Update()
			{
				UpdateProcess();
			}

			protected void Run(IEnumerator process)
			{
				if (_state == eState.NotRunning)
				{
					_current = process;
				}
				else if (_state == eState.Running)
				{
					_next = process;
				}
			}

			protected void StopCoroutines()
			{
				_state = eState.NotRunning;
			}

			protected void UpdateProcess()
			{
				while (_current != null)
				{
					_state = eState.Running;
					while (_current.MoveNext())
					{
						return;
					}

					_current = _next;
					_next = null;
				}

				_state = eState.NotRunning;
			}
			#endregion
		}
	}
}
