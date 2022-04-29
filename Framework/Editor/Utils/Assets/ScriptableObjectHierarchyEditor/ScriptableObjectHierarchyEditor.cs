using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public abstract class ScriptableObjectHierarchyEditor<TParentAsset, TChildAsset> : ScriptableObject where TParentAsset : ScriptableObject where TChildAsset : ScriptableObject
			{
				public interface IEditorWindow
				{
					void DoRepaint();
				}

				#region Protected Data
				protected static readonly float kDoubleClickTime = 0.32f;

				protected TParentAsset Asset
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
				protected ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> _draggedObject;
				protected Vector2 _dragPos = Vector2.zero;
				protected Rect _dragAreaRect;

				protected List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>> _editableObjects = new List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>>();
				protected List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>> _selectedObjects = new List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>>();
				protected List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>> _copiedObjects = new List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>>();
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
					public ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> _editableObject;
				}
				[SerializeField]
				private IEditorWindow _editorWindow;
				private TParentAsset _currentAsset;
				private int _controlID;
				private bool _needsRepaint;
				private bool _hasChanges;
				#endregion

				#region Public Methods
				public void OnEnable()
				{
					_controlID = GUIUtility.GetControlID(FocusType.Passive);
				}

				public void MarkAsDirty()
				{
					_hasChanges = true;
					EditorUtility.SetDirty(_currentAsset);
				}

				public bool HasChanges()
				{
					if (_hasChanges || EditorUtility.IsDirty(_currentAsset))
						return true;

					return false;
				}

				public void ClearDirtyFlag()
				{
					_hasChanges = false;
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

					//If not the same file, delete current first
					if (AssetDatabase.LoadMainAssetAtPath(assetPath) != Asset)
					{
						AssetDatabase.DeleteAsset(assetPath);
					}

					//Save to file
					AssetDatabase.CreateAsset(Asset, assetPath);
					AssetDatabase.SaveAssets();

					//Then load new asset
					Load(AssetDatabase.LoadMainAssetAtPath(assetPath) as TParentAsset);
				}

				public void Create(string path)
				{
					string assetPath = AssetUtils.GetAssetPath(path);

					AssetDatabase.DeleteAsset(assetPath);

					//Create new assset
					AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TParentAsset>(), assetPath);
					AssetDatabase.SaveAssets();

					//Then load new asset
					Load(AssetDatabase.LoadMainAssetAtPath(assetPath) as TParentAsset);
				}

				public void Load(TParentAsset asset)
				{
					_editableObjects.Clear();
					_selectedObjects.Clear();
					_draggedObject = null;

					_currentAsset = asset;

					//Find child assets and add them
					UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));

					foreach (UnityEngine.Object subAsset in assets)
					{
						if (subAsset is TChildAsset subAsset1)
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
					_editableObjects.Sort((a, b) => a.CompareTo(b));
				}
				#endregion

				#region Abstract Interface
				protected abstract void OnLoadAsset(TParentAsset asset);

				protected abstract void OnCreatedNewObject(TChildAsset obj);

				protected abstract ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> CreateObjectEditorGUI(TChildAsset obj);

				protected abstract void ZoomEditorView(float amount);

				protected abstract void ScrollEditorView(Vector2 delta);

				protected abstract void DragObjects(Vector2 delta);

				protected abstract void AddContextMenu(GenericMenu menu);
				#endregion

				#region Virtual Interface
				protected virtual bool CanBeCopied(ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI)
				{
					return true;
				}

				protected virtual bool CanBeDeleted(ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI)
				{
					return true;
				}

				protected virtual void OnLeftMouseDown(Event inputEvent)
				{
					ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> clickedOnObject = null;

					Vector2 mousePosition = GetEditorPosition(inputEvent.mousePosition);

					for (int i = 0; i < _editableObjects.Count; i++)
					{
						ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> evnt = (ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>)_editableObjects[i];

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
						_selectedObjects = new List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>>() { clickedOnObject };
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
					ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> clickedNode = null;
					_dragPos = GetEditorPosition(inputEvent.mousePosition);

					if (Asset != null)
					{
						//Check clicked on event
						Vector2 gridPosition = GetEditorPosition(inputEvent.mousePosition);

						foreach (ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> node in _editableObjects)
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
								_selectedObjects = new List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>>() { clickedNode };

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

				protected virtual Vector2 GetScreenPosition(Vector2 editorPos)
				{
					return editorPos;
				}

				protected virtual Rect GetScreenRect(Rect editorRect)
				{
					return editorRect;
				}

				protected virtual void OnStopDragging(Event inputEvent, bool cancelled)
				{
					if (_dragMode != DragType.NotDragging)
					{
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
								ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI = _editableObjects[i];

								//Bounds need to account for scale
								//when zoom is 0.5 boxes are half size?

								if (editorGUI.GetBounds().Overlaps(gridDragRect))
								{
									_selectedObjects.Add(editorGUI);
								}
							}

							//Update selection
							{
								UnityEngine.Object[] selectedAssets = new TChildAsset[_selectedObjects.Count];

								for (int i = 0; i < selectedAssets.Length; i++)
								{
									selectedAssets[i] = _selectedObjects[i].Asset;
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
				private ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> CreateObjectGUI(TChildAsset obj)
				{
					ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI = null;

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

				private GenericMenu GetRightMouseMenu(ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> clickedObject)
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

					TChildAsset newObject = ScriptableObject.CreateInstance(type) as TChildAsset;

					if (newObject != null)
					{
						AssetDatabase.AddObjectToAsset(newObject, _currentAsset);
						ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI = CreateObjectGUI(newObject);
						OnCreatedNewObject(newObject);

						_selectedObjects.Clear();
						_selectedObjects.Add(editorGUI);

						editorGUI.SetPosition(_dragPos);
					}
				}

				protected T CreateAndAddNewObject<T>() where T : TChildAsset
				{
					T newObject = ScriptableObject.CreateInstance<T>();

					AssetDatabase.AddObjectToAsset(newObject, _currentAsset);
					CreateObjectGUI(newObject);
					OnCreatedNewObject(newObject);

					return newObject;
				}

				protected void RemoveObject(ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI)
				{
					TChildAsset asset = editorGUI.Asset;

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
					foreach (ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI in _editableObjects)
					{
						_selectedObjects.Add(editorGUI);
					}
					_needsRepaint = true;
				}

				private void CopySelected()
				{
					_copiedObjects.Clear();
					foreach (ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI in _selectedObjects)
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

						Vector2 pos = ((ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>)_copiedObjects[0]).GetPosition();

						foreach (ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI in _copiedObjects)
						{
							//Create a copy and add to asset
							TChildAsset copyObject = ScriptableObject.Instantiate(editorGUI.Asset);
							AssetDatabase.AddObjectToAsset(copyObject, _currentAsset);

							ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> copyEditorGUI = CreateObjectGUI(copyObject);
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

						List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>> selectedObjects = new List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>>(_selectedObjects);

						foreach (ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI in selectedObjects)
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

						foreach (ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI in new List<ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>>(_selectedObjects))
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
}