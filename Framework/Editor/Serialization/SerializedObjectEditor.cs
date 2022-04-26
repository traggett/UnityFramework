using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace Framework
{
	using Utils;
	using Utils.Editor;
	
	namespace Serialization
	{
		public abstract class SerializedObjectEditor<TAsset, TSubAsset> : ScriptableObject where TAsset : ScriptableObject where TSubAsset : ScriptableObject
		{
			public interface IEditorWindow
			{
				void DoRepaint();
			}

			#region Protected Data
			protected static readonly float kDoubleClickTime = 0.32f;

			protected TAsset Asset
			{
				get
				{
					return _currentAsset;
				}
			}
			
			protected enum DragType
			{
				NotDragging,
				LeftClick,
				MiddleMouseClick,
				Custom,
			}
			protected DragType _dragMode = DragType.NotDragging;
			protected SerializedObjectEditorGUI<TAsset, TSubAsset> _draggedObject;
			protected Vector2 _dragPos = Vector2.zero;
			protected Rect _dragAreaRect;

			//Note these should all be SerializedObjectEditorGUI<TAsset, TSubAsset> but unity won't serialize them (and thus make them usable in Undo actions) if they are a template class
			[SerializeField]
			protected List<ScriptableObject> _editableObjects = new List<ScriptableObject>();
			[SerializeField]
			protected List<ScriptableObject> _selectedObjects = new List<ScriptableObject>();
			[SerializeField]
			protected List<ScriptableObject> _copiedObjects = new List<ScriptableObject>();
			#endregion

			#region Private Data
			private enum RightClickOperation
			{
				Copy,
				Paste,
				Cut,
				Remove,
			}

			private class RightClickData
			{
				public RightClickOperation _operation;
				public SerializedObjectEditorGUI<TAsset, TSubAsset> _editableObject;
			}
			[SerializeField]
			private IEditorWindow _editorWindow;
			private TAsset _currentAsset;
			private int _controlID;
			private bool _needsRepaint;
			#endregion

			#region Public Methods
			public void OnEnable()
			{
				_controlID = GUIUtility.GetControlID(FocusType.Passive);
			}

			public void MarkAsDirty()
			{
				EditorUtility.SetDirty(_currentAsset);
			}

			public bool HasChanges()
			{
				if (EditorUtility.IsDirty(_currentAsset))
					return true;

				return false;
			}

			public void ClearDirtyFlag()
			{
				EditorUtility.ClearDirty(_currentAsset);
			}

			public bool NeedsRepaint()
			{
				return _needsRepaint;
			}

			public void SetNeedsRepaint(bool needsRepaint = true)
			{
				_needsRepaint = needsRepaint;
			}

			public IEditorWindow GetEditorWindow()
			{
				return _editorWindow;
			}
			#endregion

			#region Protected Functions
			public void ClearAsset()
			{
				_editableObjects.Clear();
				_selectedObjects.Clear();
				_draggedObject = null;

				_currentAsset = null;

				GetEditorWindow().DoRepaint();
			}

			public void Save()
			{
				//Save to file
				AssetDatabase.SaveAssets();

				ClearDirtyFlag();

				GetEditorWindow().DoRepaint();
			}

			public void SaveAs(string path)
			{
				string assetPath = AssetUtils.GetAssetPath(path);

				AssetDatabase.DeleteAsset(assetPath);

				//Save to file
				AssetDatabase.CreateAsset(Asset, assetPath);
				AssetDatabase.SaveAssets();

				//Then load new asset
				Load(AssetDatabase.LoadMainAssetAtPath(assetPath) as TAsset);
			}

			public void Create(string path)
			{
				string assetPath = AssetUtils.GetAssetPath(path);

				AssetDatabase.DeleteAsset(assetPath);

				//Create new assset
				AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TAsset>(), assetPath);
				AssetDatabase.SaveAssets();

				//Then load new asset
				Load(AssetDatabase.LoadMainAssetAtPath(assetPath) as TAsset);
			}

			public void Load(TAsset asset)
			{
				_editableObjects.Clear();
				_selectedObjects.Clear();
				_draggedObject = null;

				_currentAsset = asset;

				//Find child assets and add them
				UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));

				foreach (UnityEngine.Object subAsset in assets)
				{
					if (subAsset is TSubAsset subAsset1)
					{
						CreateObjectGUI(subAsset1);
					}
				}
				
				OnLoadAsset(asset);

				GetEditorWindow().DoRepaint();
			}

			protected void SetEditorWindow(IEditorWindow editorWindow)
			{
				_editorWindow = editorWindow;
			}

			protected void SortObjects()
			{
				_editableObjects.Sort((a, b) => (((SerializedObjectEditorGUI<TAsset, TSubAsset>)a).CompareTo((SerializedObjectEditorGUI<TAsset, TSubAsset>)b)));
			}
			#endregion

			#region Abstract Interface
			protected abstract void OnLoadAsset(TAsset asset);

			protected abstract void OnCreatedNewObject(TSubAsset obj);

			protected abstract SerializedObjectEditorGUI<TAsset, TSubAsset> CreateObjectEditorGUI(TSubAsset obj);

			protected abstract void ZoomEditorView(float amount);

			protected abstract void ScrollEditorView(Vector2 delta);

			protected abstract void DragObjects(Vector2 delta);

			protected abstract void AddContextMenu(GenericMenu menu);
			#endregion

			#region Virtual Interface
			protected virtual bool CanBeCopied(SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI)
			{
				return true;
			}

			protected virtual bool CanBeDeleted(SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI)
			{
				return true;
			}

			protected virtual void OnLeftMouseDown(Event inputEvent)
			{
				SerializedObjectEditorGUI<TAsset, TSubAsset> clickedOnObject = null;

				Vector2 mousePosition = GetEditorPosition(inputEvent.mousePosition);

				for (int i = 0; i < _editableObjects.Count; i++)
				{
					SerializedObjectEditorGUI<TAsset, TSubAsset> evnt = (SerializedObjectEditorGUI<TAsset, TSubAsset>)_editableObjects[i];
					
					if (evnt.GetBounds().Contains(mousePosition))
					{
						clickedOnObject = evnt;
						break;
					}
				}

				if (inputEvent.shift || inputEvent.control)
				{
					if (clickedOnObject != null)
					{
						if (_selectedObjects.Contains(clickedOnObject))
						{
							_selectedObjects.Remove(clickedOnObject);
							clickedOnObject = null;
						}
						else
						{
							_selectedObjects.Add(clickedOnObject);
						}
					}
				}
				else if (clickedOnObject == null)
				{
					_selectedObjects.Clear();
				}
				else if (!_selectedObjects.Contains(clickedOnObject))
				{
					_selectedObjects = new List<ScriptableObject>() { clickedOnObject };
				}

				//Update selection
				if (clickedOnObject == null)
				{
					Selection.activeObject = null;
				}
				else
				{
					Selection.activeObject = clickedOnObject.Asset;
				}

				//Dragging
				{
					_draggedObject = clickedOnObject;
					_dragMode = DragType.LeftClick;
					_dragPos = inputEvent.mousePosition;
					_dragAreaRect = new Rect(-1.0f, -1.0f, 0.0f, 0.0f);
				}
			}

			protected virtual void OnMiddleMouseDown(Event inputEvent)
			{
				_dragMode = DragType.MiddleMouseClick;
				_dragPos = inputEvent.mousePosition;
				_dragAreaRect = new Rect(-1.0f, -1.0f, 0.0f, 0.0f);
			}

			protected virtual void OnRightMouseDown(Event inputEvent)
			{
				SerializedObjectEditorGUI<TAsset, TSubAsset> clickedNode = null;
				_dragPos = GetEditorPosition(inputEvent.mousePosition);

				if (Asset != null)
				{
					//Check clicked on event
					Vector2 gridPosition = GetEditorPosition(inputEvent.mousePosition);

					foreach (SerializedObjectEditorGUI<TAsset, TSubAsset> node in _editableObjects)
					{
						if (node.GetBounds().Contains(gridPosition))
						{
							clickedNode = node;
							break;
						}
					}

					if (clickedNode != null)
					{
						if (!_selectedObjects.Contains(clickedNode))
							_selectedObjects = new List<ScriptableObject>() { clickedNode };

						GetEditorWindow().DoRepaint();
					}

					// Now create the menu, add items and show it
					GenericMenu menu = GetRightMouseMenu(clickedNode);
					menu.ShowAsContext();
				}			
			}

			protected virtual Vector2 GetEditorPosition(Vector2 screenPosition)
			{
				return screenPosition;
			}

			protected virtual Rect GetEditorRect(Rect screenRect)
			{
				return screenRect;
			}

			protected virtual Rect GetScreenRect(Rect editorRect)
			{
				return editorRect;
			}

			protected virtual void OnStopDragging(Event inputEvent, bool cancelled)
			{
				if (_dragMode != DragType.NotDragging)
				{
					if (_dragMode == DragType.LeftClick)
					{
						Undo.RegisterCompleteObjectUndo(_selectedObjects.ToArray(), "Move Objects(s)");
					}

					SortObjects();
					inputEvent.Use();
					_dragMode = DragType.NotDragging;
					_needsRepaint = true;
				}
			}

			protected virtual void OnDragging(Event inputEvent)
			{
				Vector2 currentPos = inputEvent.mousePosition;

				if (_dragMode == DragType.LeftClick)
				{
					_needsRepaint = true;

					inputEvent.Use();

					if (_draggedObject != null)
					{
						Vector2 delta = currentPos - _dragPos;
						_dragPos = currentPos;

						DragObjects(delta);
					}
					else
					{
						_dragAreaRect.x = Math.Min(currentPos.x, _dragPos.x);
						_dragAreaRect.y = Math.Min(currentPos.y, _dragPos.y);
						_dragAreaRect.height = Math.Abs(currentPos.y - _dragPos.y);
						_dragAreaRect.width = Math.Abs(currentPos.x - _dragPos.x);

						_selectedObjects.Clear();

						Rect gridDragRect = GetEditorRect(_dragAreaRect);

						for (int i = 0; i < _editableObjects.Count; i++)
						{
							SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI = (SerializedObjectEditorGUI<TAsset, TSubAsset>)_editableObjects[i];
							//Bounds need to account for scale
							//when zoom is 0.5 boxes are half size?

							if (editorGUI.GetBounds().Overlaps(gridDragRect))
							{
								_selectedObjects.Add(editorGUI);
							}
						}

						//Update selection
						{
							UnityEngine.Object[] selectedAssets = new TSubAsset[_selectedObjects.Count];

							for (int i = 0; i < selectedAssets.Length; i++)
							{
								selectedAssets[i] = ((SerializedObjectEditorGUI<TAsset, TSubAsset>)_selectedObjects[i]).Asset;
							}

							Selection.objects = selectedAssets;
						}
					}
				}
				else if (_dragMode == DragType.MiddleMouseClick)
				{
					Vector2 delta = currentPos - _dragPos;
					_dragPos = currentPos;

					ScrollEditorView(delta);

					SetNeedsRepaint();
				}
			}
			#endregion

			#region Private Functions
			private SerializedObjectEditorGUI<TAsset, TSubAsset> CreateObjectGUI(TSubAsset obj)
			{
				SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI = null;

				if (obj != null)
				{
					editorGUI = CreateObjectEditorGUI(obj);
					
					_editableObjects.Add(editorGUI);
					SortObjects();
				}

				return editorGUI;
			}

			protected void HandleInput()
			{
				if (_editorWindow == null)
				{
					throw new Exception("Editor window not set!");
				}

				Event inputEvent = Event.current;

				if (inputEvent == null)
					return;
				
				EventType controlEventType = inputEvent.GetTypeForControl(_controlID);
				
				if (_dragMode != DragType.NotDragging && inputEvent.rawType == EventType.MouseUp)
				{
					OnStopDragging(inputEvent, false);
					_needsRepaint = true;
				}

				switch (controlEventType)
				{
					case EventType.MouseDown:
						{
							OnMouseDown(inputEvent);
						}
						break;

					case EventType.MouseUp:
						{
							OnStopDragging(inputEvent, false);
							_needsRepaint = true;
						}
						break;

					case EventType.ContextClick:
						{
							_needsRepaint = true;
							inputEvent.Use();
							OnStopDragging(inputEvent, false);
							OnRightMouseDown(inputEvent);
						}
						break;

					case EventType.MouseDrag:
						{
							OnDragging(inputEvent);
						}
						break;

					case EventType.ScrollWheel:
						{
							#region Zooming via Mouse Wheel
							float zoomDelta = -inputEvent.delta.y;
							ZoomEditorView(zoomDelta);
							_needsRepaint = true;
							#endregion
						}
						break;

					case EventType.KeyDown:
						{
							if (inputEvent.keyCode == KeyCode.Escape)
							{
								OnStopDragging(inputEvent, true);
								_needsRepaint = true;
							}
						}
						break;

					case EventType.ValidateCommand:
						{
							if (inputEvent.commandName == "SoftDelete")
							{
								DeleteSelected();
							}
							else if (inputEvent.commandName == "SelectAll")
							{
								SelectAll();
							}
							else if (inputEvent.commandName == "Cut")
							{
								CutSelected();
							}
							else if (inputEvent.commandName == "Copy")
							{
								CopySelected();
							}
							else if (inputEvent.commandName == "Paste")
							{
								Paste();
							}
							else if (inputEvent.commandName == "Duplicate")
							{
								CopySelected();
								Paste();
							}
							else if (inputEvent.commandName == "UndoRedoPerformed")
							{
								_needsRepaint = true;
							}
						}
						break;
					case EventType.Repaint:
						{
							#region Dragging Rect
							if (_dragMode != DragType.NotDragging && _draggedObject == null)
							{
								EditorUtils.DrawSelectionRect(_dragAreaRect);
							}
							#endregion
						}
						break;
				}
			}

			private void OnMouseDown(Event inputEvent)
			{
				if (inputEvent.button == 0)
				{
					inputEvent.Use();
					_needsRepaint = true;

					OnLeftMouseDown(inputEvent);
				}
				else if (inputEvent.button == 1)
				{
					inputEvent.Use();
					OnRightMouseDown(inputEvent);
				}
				else if (inputEvent.button == 2)
				{
					inputEvent.Use();
					_needsRepaint = true;

					OnMiddleMouseDown(inputEvent);
				}
				else
				{
					_dragMode = DragType.NotDragging;
				}
			}

			private GenericMenu GetRightMouseMenu(SerializedObjectEditorGUI<TAsset, TSubAsset> clickedObject)
			{
				GenericMenu menu = new GenericMenu();

				if (clickedObject != null)
				{
					RightClickData copyData = new RightClickData();
					copyData._operation = RightClickOperation.Copy;
					copyData._editableObject = clickedObject;
					menu.AddItem(new GUIContent("Copy"), false, ContextMenuCallback, copyData);
					RightClickData cutData = new RightClickData();
					cutData._operation = RightClickOperation.Cut;
					cutData._editableObject = clickedObject;
					menu.AddItem(new GUIContent("Cut"), false, ContextMenuCallback, cutData);
					RightClickData removeData = new RightClickData();
					removeData._operation = RightClickOperation.Remove;
					removeData._editableObject = clickedObject;
					menu.AddItem(new GUIContent("Remove"), false, ContextMenuCallback, removeData);
				}
				else
				{
					AddContextMenu(menu);

					RightClickData pasteData = null;
					if (_copiedObjects.Count > 0)
					{
						pasteData = new RightClickData();
						pasteData._operation = RightClickOperation.Paste;
						pasteData._editableObject = clickedObject;
						menu.AddItem(new GUIContent(_copiedObjects.Count == 1 ? "Paste" : "Paste"), false, ContextMenuCallback, pasteData);
					}
				}

				return menu;
			}

			private void ContextMenuCallback(object obj)
			{
				RightClickData data = obj as RightClickData;

				if (data == null)
				{
					return;
				}

				switch (data._operation)
				{
					case RightClickOperation.Copy:
						{
							CopySelected();
						}
						break;
					case RightClickOperation.Paste:
						{
							Paste();
						}
						break;
					case RightClickOperation.Cut:
						{
							CutSelected();
						}
						break;
					case RightClickOperation.Remove:
						{
							DeleteSelected();
						}
						break;
				}
			}

			protected void CreateAndAddNewObject(Type type)
			{
				Undo.RegisterCompleteObjectUndo(this, "Create Object");

				TSubAsset newObject = ScriptableObject.CreateInstance(type) as TSubAsset;

				if (newObject != null)
				{
					AssetDatabase.AddObjectToAsset(newObject, _currentAsset);
					SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI = CreateObjectGUI(newObject);
					OnCreatedNewObject(newObject);

					_selectedObjects.Clear();
					_selectedObjects.Add(editorGUI);

					editorGUI.SetPosition(_dragPos);
				}		
			}

			protected T CreateAndAddNewObject<T>() where T : TSubAsset
			{
				T newObject = ScriptableObject.CreateInstance<T>();

				AssetDatabase.AddObjectToAsset(newObject, _currentAsset);
				CreateObjectGUI(newObject);
				OnCreatedNewObject(newObject);

				return newObject;
			}

			protected void RemoveObject(SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI)
			{
				TSubAsset asset = editorGUI.Asset;

				if (Selection.activeObject == editorGUI.Asset)
					Selection.activeObject = null;

				_editableObjects.Remove(editorGUI);
				_selectedObjects.Remove(editorGUI);
				
				AssetDatabase.RemoveObjectFromAsset(asset);
				DestroyImmediate(asset);
			}

			private void SelectAll()
			{
				_selectedObjects.Clear();
				foreach (SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI in _editableObjects)
				{
					_selectedObjects.Add(editorGUI);
				}
				_needsRepaint = true;
			}

			private void CopySelected()
			{
				_copiedObjects.Clear();
				foreach (SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI in _selectedObjects)
				{
					if (CanBeCopied(editorGUI))
					{
						_copiedObjects.Add(editorGUI);
					}
				}
			}

			private void Paste()
			{
				if (Asset != null && _copiedObjects.Count > 0)
				{
					Undo.RegisterCompleteObjectUndo(this, "Paste Object(s)");

					Vector2 pos = ((SerializedObjectEditorGUI<TAsset, TSubAsset>)_copiedObjects[0]).GetPosition();

					foreach (SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI in _copiedObjects)
					{
						//Create a copy and add to asset
						TSubAsset copyObject = ScriptableObject.Instantiate(editorGUI.Asset);
						AssetDatabase.AddObjectToAsset(copyObject, _currentAsset);

						SerializedObjectEditorGUI<TAsset, TSubAsset> copyEditorGUI = CreateObjectGUI(copyObject);
						OnCreatedNewObject(copyObject);
						
						copyEditorGUI.SetPosition(_dragPos + editorGUI.GetPosition() - pos);
					}

					MarkAsDirty();

					_needsRepaint = true;
				}
			}

			private void CutSelected()
			{
				if (_selectedObjects.Count > 0)
				{
					Undo.RegisterCompleteObjectUndo(this, "Cut Object(s)");

					_copiedObjects.Clear();

					List<ScriptableObject> selectedObjects = new List<ScriptableObject>(_selectedObjects);

					foreach (SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI in selectedObjects)
					{
						if (CanBeCopied(editorGUI))
						{
							_copiedObjects.Add(editorGUI);

							if (CanBeDeleted(editorGUI))
							{
								RemoveObject(editorGUI);
							}
						}				
					}
					_selectedObjects.Clear();

					MarkAsDirty();

					_needsRepaint = true;
				}
			}

			private void DeleteSelected()
			{
				if (_selectedObjects.Count > 0)
				{
					Undo.RegisterCompleteObjectUndo(this, "Remove Object(s)");

					foreach (SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI in new List<ScriptableObject>(_selectedObjects))
					{
						if (CanBeDeleted(editorGUI))
						{
							RemoveObject(editorGUI);
						}
					}
					_selectedObjects.Clear();

					MarkAsDirty();

					_needsRepaint = true;
				}
			}
			#endregion
		}
	}
}