using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		public abstract class SerializedObjectGridBasedEditor<T> : SerializedObjectEditor<T> where T : class
		{
			#region Protected Data
			protected float _currentZoom = 1.0f;
			protected Vector2 _cameraPosition = Vector2.zero;
			protected Vector2 _renderOffset = Vector2.zero;
			#endregion

			#region Private Data
			private static readonly float kZoomScrollWheelSpeed = 0.01f;
			private Rect _gridArea;
			private Texture2D _gridBackground;
			private GUIStyle _gridAreaStyle;
			#endregion
			
			#region Public Interfacce
			public void RenderGridView(Rect area)
			{
				InitGUIStyles();

				SetNeedsRepaint(false);

				GUI.BeginGroup(area, GUIContent.none, _gridAreaStyle);
				{
					_gridArea = new Rect(0.0f, 0.0f, area.width, area.height - _gridArea.y);

					_renderOffset = _cameraPosition;
					_renderOffset.x += Mathf.Floor(_gridArea.width * 0.5f * (1.0f / _currentZoom));
					_renderOffset.y += Mathf.Floor(_gridArea.height * 0.5f * (1.0f / _currentZoom));

					//Draw the grid
					{
						float uvWidth = _gridArea.width / (_gridBackground.width * _currentZoom);
						float uvHeight = _gridArea.height / (_gridBackground.height * _currentZoom);
						float uvX = 0.0f - (_renderOffset.x / _gridBackground.width);
						float uvY = uvHeight - (_renderOffset.y / _gridBackground.height);
						GUI.DrawTextureWithTexCoords(_gridArea, _gridBackground, new Rect(uvX, uvY, uvWidth, -uvHeight), true);
					}

					RenderObjectsOnGrid();

					HandleInput();
				}
				GUI.EndGroup();
			}
			#endregion

			#region Protected Functions
			protected void SetZoom(float zoom)
			{
				if (_currentZoom != zoom)
				{
					_currentZoom = zoom;

					OnZoomChanged(_currentZoom);
				}
			}

			protected void CenterCameraOn(SerializedObjectEditorGUI<T> editorGUI)
			{
				_cameraPosition = -editorGUI.GetPosition();
			}

			protected void CenterCamera()
			{
				if (_editableObjects.Length > 0)
				{
					Rect maxBounds = ((SerializedObjectEditorGUI<T>)_editableObjects[0]).GetBounds();

					for (int i = 1; i < _editableObjects.Length; i++)
					{
						SerializedObjectEditorGUI<T> editorGUI = (SerializedObjectEditorGUI<T>)_editableObjects[i];
						maxBounds.xMin = Mathf.Min(maxBounds.xMin, editorGUI.GetBounds().xMin);
						maxBounds.xMax = Mathf.Max(maxBounds.xMax, editorGUI.GetBounds().xMax);
						maxBounds.yMin = Mathf.Min(maxBounds.yMin, editorGUI.GetBounds().yMin);
						maxBounds.yMax = Mathf.Max(maxBounds.yMax, editorGUI.GetBounds().yMax);
					}

					_cameraPosition = -maxBounds.center;
				}
				else
				{
					_cameraPosition = Vector2.zero;
				}
			}

			protected override Rect GetEditorRect(Rect screenRect)
			{
				float invZoom = (1.0f / _currentZoom);
				Rect gridRect = screenRect;
				gridRect.position = invZoom * gridRect.position - _renderOffset;
				gridRect.width *= invZoom;
				gridRect.height *= invZoom;
				return gridRect;
			}

			protected override Vector2 GetEditorPosition(Vector2 screenPosition)
			{
				return (1.0f / _currentZoom) * screenPosition - _renderOffset;
			}

			protected override Rect GetScreenRect(Rect gridRect)
			{
				Rect screenRect = gridRect;
				screenRect.position += _renderOffset;
				screenRect.position *= _currentZoom;
				screenRect.position = new Vector2(Mathf.Round(screenRect.position.x), Mathf.Round(screenRect.position.y));
				return screenRect;
			}
			#endregion

			#region Abstract Interface
			protected abstract void RenderObjectsOnGrid();
			#endregion

			#region Virtual Interface
			protected virtual void OnZoomChanged(float zoom)
			{

			}

			protected override void ZoomEditorView(float amount)
			{
				float zoom = _currentZoom + amount * kZoomScrollWheelSpeed;
				SetZoom(zoom);
			}

			protected override void DragObjects(Vector2 delta)
			{
				delta *= (1.0f / _currentZoom);

				foreach (SerializedObjectEditorGUI<T> editorGUI in _selectedObjects)
				{
					editorGUI.SetPosition(editorGUI.GetPosition() + delta);
				}
			}

			protected override void ScrollEditorView(Vector2 delta)
			{
				//Move camera round by drag amount
				_cameraPosition += delta;
				_cameraPosition.x = Mathf.Round(_cameraPosition.x);
				_cameraPosition.y = Mathf.Round(_cameraPosition.y);
			}
			#endregion

			#region Private Functions
			private void InitGUIStyles()
			{
				if (_gridBackground == null)
					_gridBackground = CreateTilableGridTexture(32, 32, Color.clear, new Color(0.0f, 0.0f, 0.0f, 0.4f), new Color(0.0f, 0.0f, 0.0f, 0.15f));

				if (_gridAreaStyle == null)
					_gridAreaStyle = "flow background";
			}

			private static Texture2D CreateTilableGridTexture(int width, int height, Color backgroundColor, Color majorLineColor, Color minorLineColor)
			{
				Color[] array = new Color[width * height];
				for (int i = 0; i < height * width; i++)
				{
					array[i] = backgroundColor;
				}
				for (int j = 0; j < height; j++)
				{
					array[j * width + Mathf.FloorToInt(width / 4f)] = minorLineColor;
					array[j * width + Mathf.FloorToInt(width / 2f)] = minorLineColor;
					array[j * width + Mathf.FloorToInt(3f * width / 4f)] = minorLineColor;

					array[j * width] = majorLineColor;
				}
				for (int k = 1; k < width; k++)
				{
					array[k + Mathf.FloorToInt((height * width) / 4f)] = minorLineColor;
					array[k + Mathf.FloorToInt((height * width) / 2f)] = minorLineColor;
					array[k + Mathf.FloorToInt((3 * height * width) / 4f)] = minorLineColor;

					array[k] = majorLineColor;
				}
				Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, true);
				texture2D.hideFlags = HideFlags.HideAndDontSave;
				texture2D.filterMode = FilterMode.Bilinear;
				texture2D.anisoLevel = 9;
				texture2D.mipMapBias = -0.5f;
				texture2D.SetPixels(array);
				texture2D.Apply();
				return texture2D;
			}
			#endregion
		}
	}
}