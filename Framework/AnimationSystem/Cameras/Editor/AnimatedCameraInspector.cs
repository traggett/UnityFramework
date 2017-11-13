using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

namespace Framework
{
	using Utils.Editor;
	using Utils;

	namespace AnimationSystem
	{
		[CustomEditor(typeof(AnimatedCamera), true)]
		public class AnimatedCameraInspector : UnityEditor.Editor
		{
			public static float kAspectRatio = 16f / 9f;

			#region Private Data
			private static readonly float kMouseLookSpeed = 82.0f;
			private static readonly float kMovementSpeed = 0.075f;
			private static readonly float kFastMovementSpeed = 0.16f;

			private RenderTexture _targetTexture;
			private Dictionary<KeyCode, bool> _buttonPressed = new Dictionary<KeyCode, bool>();
			private bool[] _mouseDown = new bool[3];

			private AnimatedCameraState _cameraOrigState;
			protected static IAnimatedCameraStateSource _currentSnapshot;
			#endregion

			private void OnEnable()
			{
				EditorApplication.update += Update;
				CacheCameraState();
			}

			private void OnDisable()
			{
				EditorApplication.update -= Update;

				ResetCameraState();
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

					if (_currentSnapshot != null)
						UpdateSnapshotFromCamera(_currentSnapshot);
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

					float speed = kMovementSpeed;
					
					if (_buttonPressed.TryGetValue(KeyCode.Space, out held) && held)
						speed = kFastMovementSpeed;

					camera.transform.Translate(movement * speed, Space.Self);

					UpdateSnapshotFromCamera(_currentSnapshot);
				}

				if (_currentSnapshot == null)
				{
					CacheCameraState();
				}
			}

			private void UpdateSnapshotFromCamera(IAnimatedCameraStateSource snapshot)
			{
				AnimatedCamera camera = (AnimatedCamera)target;

				if (snapshot != null)
				{
					snapshot.SetState(camera.GetState());
				}		
			}
			
			private void UpdateCameraFromSnapshot()
			{
				AnimatedCamera camera = (AnimatedCamera)target;

				if (_currentSnapshot != null)
				{
					camera.SetState(_currentSnapshot.GetState());
				}
			}

			private void RenderGamePreview(Event evnt)
			{
				if (_currentSnapshot != null)
				{
					AnimatedCamera camera = (AnimatedCamera)target;

					Rect sceneViewRect = Camera.current.pixelRect;
					int viewWidth = (int)sceneViewRect.width;
					int viewHeight = (int)sceneViewRect.height;

					//If at this height the width is to big, need to make height less
					if (viewHeight * kAspectRatio > viewWidth)
					{
						viewHeight = (int)(sceneViewRect.width * (1f / kAspectRatio));
					}
					//If at this height the height is to big, need to make width less
					if (viewWidth * (1f / kAspectRatio) > viewHeight)
					{
						viewWidth = (int)(sceneViewRect.height * kAspectRatio);
					}

					if (_targetTexture == null || viewWidth != _targetTexture.width || viewHeight != _targetTexture.height)
					{
						if (_targetTexture == null)
						{
							_targetTexture = new RenderTexture(viewWidth, viewHeight, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
						}
						else
						{
							_targetTexture.Release();
						}

						_targetTexture.width = viewWidth;
						_targetTexture.height = viewHeight;
						_targetTexture.antiAliasing = 1;
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
						GUI.BeginGroup(sceneViewRect);

						//Clear screen to black
						Color guiColor = GUI.color;
						GUI.color = Color.black;
						GUI.DrawTexture(sceneViewRect, EditorUtils.OnePixelTexture);
						GUI.color = guiColor;

						//Render game texture to screen
						Rect viewRect = new Rect();
						viewRect.width = viewWidth;
						viewRect.height = viewHeight;
						viewRect.x = (sceneViewRect.width - viewRect.width) * 0.5f;
						viewRect.y = (sceneViewRect.height - viewRect.height) * 0.5f;
						GUI.DrawTexture(GetFlippedRect(viewRect), _targetTexture, ScaleMode.StretchToFill, false);

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

				IAnimatedCameraStateSource[] snapshots = SceneUtils.FindAllComponentInferfacesInScene<IAnimatedCameraStateSource>(camera.gameObject.scene);

				int index = 0;
				int currentIndex = 0;
				
				string[] snapshotNames = new string[snapshots.Length + 2];
				snapshotNames[index++] = "(None)";

				foreach (IAnimatedCameraStateSource snapshot in snapshots)
				{
					snapshotNames[index] = snapshot.GetName();

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
						SetCurrentSnapshot(null);
					}
					else if (newIndex == snapshotNames.Length-1)
					{
						//Add new snapshot!
						GameObject newObj = new GameObject("Snapshot" + newIndex);
						newObj.transform.parent = camera.transform.parent;
						newObj.transform.position = camera.transform.position;
						newObj.transform.rotation = camera.transform.rotation;

						IAnimatedCameraStateSource newSnapShot = newObj.AddComponent(camera.GetAnimatedCameraStateSourceType()) as IAnimatedCameraStateSource;
						if (newSnapShot != null)
						{
							UpdateSnapshotFromCamera(newSnapShot);
							SetCurrentSnapshot(newSnapShot);
						}
						
						return true;
					}
					else
					{
						SetCurrentSnapshot(snapshots[newIndex - 1]);
					}
				}

				DrawCameraProperties();

				return false;
			}

			protected void DrawCameraProperties()
			{
				AnimatedCamera camera = (AnimatedCamera)target;

				EditorGUI.BeginChangeCheck();
				float fieldOfView = EditorGUILayout.Slider("Field of View", camera.GetCamera().fieldOfView, 1.0f, 180.0f);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(camera.GetCamera(), "Changed Field of View");
					camera.GetCamera().fieldOfView = fieldOfView;
				}

				EditorGUI.BeginChangeCheck();
				Rect rect = EditorGUILayout.RectField("Viewport Rect", camera.GetCamera().rect);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(camera.GetCamera(), "Changed Viewport Rect");
					camera.GetCamera().rect = rect;
				}
			}

			protected virtual void SetCurrentSnapshot(IAnimatedCameraStateSource snapshot)
			{
				//Select new snapshot
				_currentSnapshot = snapshot;

				if (_currentSnapshot != null)
				{
					//Set camera position to snapshot
					UpdateCameraFromSnapshot();
				}
				else
				{
					ResetCameraState();
				}
			}

			private void CacheCameraState()
			{
				AnimatedCamera camera = (AnimatedCamera)target;
				_cameraOrigState = camera.GetState();
			}

			private void ResetCameraState()
			{
				AnimatedCamera camera = (AnimatedCamera)target;

				if (camera != null)
				{
					camera.SetState(_cameraOrigState);
				}
			}
			#endregion
		}
	}
}