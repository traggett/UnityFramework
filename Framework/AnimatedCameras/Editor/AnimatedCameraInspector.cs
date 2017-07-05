using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils.Editor;
	using System.Collections.Generic;
	using Utils;

	namespace AnimationSystem
	{
		[CustomEditor(typeof(AnimatedCamera), true)]
		public class AnimatedCameraInspector : UnityEditor.Editor
		{
			//Things still needed to do
			// - make controls virutal so game animated camera can do cool shit with depth of field (if ness?)
			// - make depth of field controller cool again.
			// it needs a priotiy system - aniamted camera can control it, some code can?
			//things like phone override it

			#region Private Data
			private static readonly int kPreviewScreenResolutionWidth = 1280;
			private static readonly int kPreviewScreenResolutionHeight = 720;
			private static readonly float kMouseLookSpeed = 82.0f;
			private static readonly float kMovementSpeed = 0.16f;

			private RenderTexture _targetTexture;
			private Dictionary<KeyCode, bool> _buttonPressed = new Dictionary<KeyCode, bool>();
			private bool[] _mouseDown = new bool[3];

			private Vector3 _cameraOrigPosition;
			private Quaternion _camerOrigRotation;
			private Vector3 _cameraOrigScale;

			protected AnimatedCameraSnapshot _currentSnapshot;
			#endregion

			private void OnEnable()
			{
				EditorApplication.update += Update;

				AnimatedCamera camera = (AnimatedCamera)target;
				_cameraOrigPosition = camera.transform.position;
				_camerOrigRotation = camera.transform.rotation;
				_cameraOrigScale = camera.transform.localScale;
			}

			private void OnDisable()
			{
				EditorApplication.update -= Update;

				AnimatedCamera camera = (AnimatedCamera)target;

				if (camera != null)
				{
					camera.transform.position = _cameraOrigPosition;
					camera.transform.rotation = _camerOrigRotation;
					camera.transform.localScale = _cameraOrigScale;
				}
			}

			private void Update()
			{
				UpdateKeys();
			}

			#region Editor Calls
			public override void OnInspectorGUI()
			{
				if (!Application.isPlaying)
				{
					DrawSnapshotDropdown();
				}
			}

			public virtual void OnSceneGUI()
			{
				if (Event.current != null)
				{
					switch (Event.current.type)
					{
						case EventType.Repaint: RenderGamePreview(Event.current); break;
						case EventType.KeyDown: HandleKeyDown(Event.current); break;
						case EventType.KeyUp: HandleKeyUp(Event.current); break;
						case EventType.MouseDown: HandleMouseDown(Event.current); break;
						case EventType.MouseUp: HandleMouseUp(Event.current); break;
						case EventType.MouseDrag: HandleMouseDrag(Event.current); break;
					}
				}
			}
			#endregion

			#region Private Functions
			private void HandleMouseDrag(Event evnt)
			{
				if (AllowFreeCam())
				{
					AnimatedCamera camera = (AnimatedCamera)target;
					Vector3 eulerAngles = camera.transform.eulerAngles;
					eulerAngles += new Vector3(evnt.delta.y / Camera.current.pixelRect.height, evnt.delta.x / Camera.current.pixelRect.width, 0.0f) * kMouseLookSpeed;
					camera.transform.eulerAngles = eulerAngles;
					UpdateSnapshotFromCamera();
				}

				Event.current.Use();
			}

			private void HandleKeyDown(Event evnt)
			{
				_buttonPressed[evnt.keyCode] = true;
				evnt.Use();
			}

			private void HandleKeyUp(Event evnt)
			{
				_buttonPressed[evnt.keyCode] = false;
				evnt.Use();
			}

			private void HandleMouseDown(Event evnt)
			{
				_mouseDown[evnt.button] = true;
				evnt.Use();
			}

			private void HandleMouseUp(Event evnt)
			{
				_mouseDown[evnt.button] = false;
				evnt.Use();
			}

			private bool AllowFreeCam()
			{
				return _mouseDown[1];
			}

			private void UpdateKeys()
			{
				if (AllowFreeCam())
				{
					Vector3 movement = Vector3.zero;
					bool held = false;

					if (_buttonPressed.TryGetValue(KeyCode.A, out held) && held)
					{
						movement.x -= 1.0f;
					}
					if (_buttonPressed.TryGetValue(KeyCode.D, out held) && held)
					{
						movement.x += 1.0f;
					}
					if (_buttonPressed.TryGetValue(KeyCode.W, out held) && held)
					{
						if (_buttonPressed.TryGetValue(KeyCode.CapsLock, out held) && held)
							movement.y += 1.0f;
						else
							movement.z += 1.0f;
					}
					if (_buttonPressed.TryGetValue(KeyCode.S, out held) && held)
					{
						if (_buttonPressed.TryGetValue(KeyCode.CapsLock, out held) && held)
							movement.y -= 1.0f;
						else
							movement.z -= 1.0f;
					}

					AnimatedCamera camera = (AnimatedCamera)target;
					camera.transform.Translate(movement * kMovementSpeed, Space.Self);

					UpdateSnapshotFromCamera();
				}		
			}

			private void UpdateSnapshotFromCamera()
			{
				AnimatedCamera camera = (AnimatedCamera)target;

				if (_currentSnapshot != null)
				{
					_currentSnapshot.SetFromCamera(camera);
				}		
			}
			
			private void UpdateCameraFromSnapshot(AnimatedCameraSnapshot snapshot)
			{
				AnimatedCamera camera = (AnimatedCamera)target;

				if (_currentSnapshot != null)
				{
					camera.SetFromSnapshot(_currentSnapshot);
				}
			}

			private void RenderGamePreview(Event evnt)
			{
				if (_currentSnapshot != null)
				{
					AnimatedCamera camera = (AnimatedCamera)target;

					if (_targetTexture == null)
					{
						_targetTexture = new RenderTexture(kPreviewScreenResolutionWidth, kPreviewScreenResolutionHeight, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
						_targetTexture.Create();
					}

					if (_targetTexture.IsCreated())
					{
						//Render scene - need better way of grabbng post fx
						RenderTexture active = RenderTexture.active;
						RenderTexture.active = _targetTexture;
						GL.Clear(true, true, Color.clear);
						RenderTexture.active = active;

						RenderCameras();

						//Render on screen
						Rect sceneViewRect = Camera.current.pixelRect;
						GUI.BeginGroup(sceneViewRect);

						//Clear screen to black
						Graphics.DrawTexture(sceneViewRect, EditorUtils.OnePixelTexture, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, Color.black);
						//Render game texture to screen
						Rect viewRect = new Rect();
						viewRect.width = sceneViewRect.width;
						viewRect.height = sceneViewRect.width * (kPreviewScreenResolutionHeight / (float)kPreviewScreenResolutionWidth);
						viewRect.y = (sceneViewRect.height - viewRect.height) * 0.5f;
						Graphics.DrawTexture(GetFlippedRect(viewRect), _targetTexture);

						GUI.EndGroup();
					}
				}
			}

			private void RenderCameras()
			{
				foreach (Camera camera in Camera.allCameras)
				{
					RenderTexture texture = camera.targetTexture;
					camera.targetTexture = _targetTexture;
					camera.Render();
					camera.targetTexture = texture;
				}
			}

			private Rect GetFlippedRect(Rect rect)
			{
				if (SystemInfo.graphicsUVStartsAtTop)
				{
					return rect;
				}
				else
				{
					Rect targetInView = rect;
					targetInView.y += targetInView.height;
					targetInView.height = -targetInView.height;
					return targetInView;
				}
			}

			private bool DrawSnapshotDropdown()
			{
				AnimatedCamera camera = (AnimatedCamera)target;

				AnimatedCameraSnapshot[] snapshots = SceneUtils.FindAllInScene<AnimatedCameraSnapshot>(camera.gameObject.scene);

				int index = 0;
				int currentIndex = 0;
				
				string[] snapshotNames = new string[snapshots.Length + 2];
				snapshotNames[index++] = "(None)";

				foreach (AnimatedCameraSnapshot snapshot in snapshots)
				{
					snapshotNames[index] = snapshot.gameObject.name;

					if (snapshot == _currentSnapshot)
					{
						currentIndex = index;
					}

					index++;
				}

				snapshotNames[index++] = "(New Snapshot)";

				int newIndex = EditorGUILayout.Popup("Edit Snapshot", currentIndex, snapshotNames);

				if (currentIndex != newIndex)
				{
					if (newIndex == 0)
					{
						_currentSnapshot = null;
					}
					else if (newIndex == snapshotNames.Length-1)
					{
						//Add new snapshot!
						_currentSnapshot = camera.CreateSnapshot("Snapshot" + newIndex);
						UpdateSnapshotFromCamera();
						return true;
					}
					else
					{
						//Select new snapshot
						_currentSnapshot = snapshots[newIndex-1];
						//Set camera position to snapshot
						UpdateCameraFromSnapshot(_currentSnapshot);
					}
				}			

				return false;
			}
			#endregion
		}
	}
}