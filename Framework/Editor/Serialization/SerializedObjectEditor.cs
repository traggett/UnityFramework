using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace Framework
{
	using Utils.Editor;
	
	namespace Serialization
	{
		public abstract class SerializedObjectEditor<T> : ScriptableObject where T : ScriptableObject
		{
			public interface IEditorWindow
			{
				void DoRepaint();
			}

			#region Protected Data
			protected enum DragType
			{
				NotDragging,
				LeftClick,
				MiddleMouseClick,
				Custom,
			}
			protected DragType _dragMode = DragType.NotDragging;
			protected SerializedObjectEditorGUI<T> _draggedObject;
			protected Vector2 _dragPos = Vector2.zero;
			protected Rect _dragAreaRect;

			//Note these should all be SerializedObjectEditorGUI<T> but unity won't serialize them (and thus make them usable in Undo actions) if they are a template class
			[SerializeField]
			protected List<ScriptableObject> _editableObjects = new List<ScriptableObject>();
			[SerializeField]
			protected List<ScriptableObject> _selectedObjects = new List<ScriptableObject>();
			[SerializeField]
			protected List<ScriptableObject> _copiedObjects = new List<ScriptableObject>();
			#endregion

			#region Private Data
			private enum eRightClickOperation
			{
				Copy,
				Paste,
				Cut,
				Remove,
			}

			private class RightClickData
			{
				public eRightClickOperation _operation;
				public SerializedObjectEditorGUI<T> _editableObject;
			}
			[SerializeField]
			private IEditorWindow _editorWindow;
			private int _controlID;
			private bool _needsRepaint;
			private bool _isDirty;
			private List<ScriptableObject> _cachedEditableObjects = new List<ScriptableObject>();
			#endregion

			#region Public Methods
			public SerializedObjectEditor()
			{
				Undo.undoRedoPerformed += UndoRedoCallback;
			}

			~SerializedObjectEditor()
			{
				Undo.undoRedoPerformed -= UndoRedoCallback;
			}

			public void OnEnable()
			{
				_controlID = GUIUtility.GetControlID(FocusType.Passive);
			}

			public bool NeedsRepaint()
			{
				return _needsRepaint;
			}

			public void MarkAsDirty()
			{
				_isDirty = true;
				EditorUtility.SetDirty(this);
			}

			public bool HasChanges()
			{
				if (_isDirty)
					return true;

				foreach (SerializedObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					if (editorGUI.IsDirty())
						return true;
				}

				return false;
			}

			public void ClearDirtyFlag()
			{
				_isDirty = false;

				foreach (SerializedObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					editorGUI.MarkAsDirty(false);
				}
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
			protected void SetEditorWindow(IEditorWindow editorWindow)
			{
				_editorWindow = editorWindow;
			}

			protected void ClearObjects()
			{
				foreach (SerializedObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					Undo.ClearUndo(editorGUI);

					if (Selection.activeObject == editorGUI)
						Selection.activeObject = null;
				}

				_editableObjects.Clear();
				_selectedObjects.Clear();
				_draggedObject = null;

				Undo.ClearUndo(this);

				ClearDirtyFlag();
			}

			protected SerializedObjectEditorGUI<T> AddNewObject(T obj)
			{
				SerializedObjectEditorGUI<T> editorGUI = null;

				if (obj != null)
				{
					editorGUI = CreateObjectEditorGUI(obj);
					editorGUI.SetEditableObject(obj);

					_editableObjects.Add(editorGUI);
					SortObjects();

					UpdateCachedObjectList();
				}

				return editorGUI;
			}

			protected void RemoveObject(SerializedObjectEditorGUI<T> editorGUI)
			{
				if (Selection.activeObject == editorGUI)
					Selection.activeObject = null;

				_editableObjects.Remove(editorGUI);
				_selectedObjects.Remove(editorGUI);
				UpdateCachedObjectList();
			}

			protected void SortObjects()
			{
				_editableObjects.Sort((a, b) => (((SerializedObjectEditorGUI<T>)a).CompareTo((SerializedObjectEditorGUI<T>)b)));
			}
			#endregion

			#region Abstract Interface
			protected abstract void OnCreatedNewObject(T obj);

			protected abstract SerializedObjectEditorGUI<T> CreateObjectEditorGUI(T obj);

			protected abstract T CreateCopyFrom(SerializedObjectEditorGUI<T> editorGUI);

			protected abstract void ZoomEditorView(float amount);

			protected abstract void ScrollEditorView(Vector2 delta);

			protected abstract void DragObjects(Vector2 delta);

			protected abstract void AddContextMenu(GenericMenu menu);

			protected abstract void SetObjectPosition(SerializedObjectEditorGUI<T> obj, Vector2 position);
			#endregion

			#region Virtual Interface
			protected virtual bool CanBeCopied(SerializedObjectEditorGUI<T> editorGUI)
			{
				return true;
			}

			protected virtual bool CanBeDeleted(SerializedObjectEditorGUI<T> editorGUI)
			{
				return true;
			}

			protected virtual void OnLeftMouseDown(Event inputEvent)
			{
				SerializedObjectEditorGUI<T> clickedOnObject = null;

				Vector2 gridPosition = GetEditorPosition(inputEvent.mousePosition);

				for (int i = 0; i < _editableObjects.Count; i++)
				{
					SerializedObjectEditorGUI<T> evnt = (SerializedObjectEditorGUI<T>)_editableObjects[i];
					if (evnt.GetBounds().Contains(gridPosition))
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

				//Dragging
				{
					if (clickedOnObject != null)
					{
						Selection.activeObject = clickedOnObject;
						GUIUtility.keyboardControl = 0;
					}
					else
					{
						Selection.activeObject = null;
					}

					_draggedObject = clickedOnObject;
					_dragMode = DragType.LeftClick;
					_dragPos = inputEvent.mousePosition;
					_dragAreaRect = new Rect(-1.0f, -1.0f, 0.0f, 0.0f);

					//Save state before dragging
					foreach (SerializedObjectEditorGUI<T> evnt in _selectedObjects)
					{
						evnt.CacheUndoStatePreChanges();
					}
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
				SerializedObjectEditorGUI<T> clickedNode = null;
				_dragPos = GetEditorPosition(inputEvent.mousePosition);
				
				//Check clicked on event
				Vector2 gridPosition = GetEditorPosition(inputEvent.mousePosition);

				foreach (SerializedObjectEditorGUI<T> node in _editableObjects)
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

						foreach (SerializedObjectEditorGUI<T> evnt in _selectedObjects)
						{
							evnt.CacheUndoStatePreChanges();
						}
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
							SerializedObjectEditorGUI<T> editorGUI = (SerializedObjectEditorGUI<T>)_editableObjects[i];
							//Bounds need to account for scale
							//when zoom is 0.5 boxes are half size?

							if (editorGUI.GetBounds().Overlaps(gridDragRect))
							{
								_selectedObjects.Add(editorGUI);
							}
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

			private GenericMenu GetRightMouseMenu(SerializedObjectEditorGUI<T> clickedObject)
			{
				GenericMenu menu = new GenericMenu();

				if (clickedObject != null)
				{
					RightClickData copyData = new RightClickData();
					copyData._operation = eRightClickOperation.Copy;
					copyData._editableObject = clickedObject;
					menu.AddItem(new GUIContent("Copy"), false, ContextMenuCallback, copyData);
					RightClickData cutData = new RightClickData();
					cutData._operation = eRightClickOperation.Cut;
					cutData._editableObject = clickedObject;
					menu.AddItem(new GUIContent("Cut"), false, ContextMenuCallback, cutData);
					RightClickData removeData = new RightClickData();
					removeData._operation = eRightClickOperation.Remove;
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
						pasteData._operation = eRightClickOperation.Paste;
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
					case eRightClickOperation.Copy:
						{
							CopySelected();
						}
						break;
					case eRightClickOperation.Paste:
						{
							Paste();
						}
						break;
					case eRightClickOperation.Cut:
						{
							CutSelected();
						}
						break;
					case eRightClickOperation.Remove:
						{
							DeleteSelected();
						}
						break;
				}
			}

			protected void CreateAndAddNewObject(Type type)
			{
				Undo.RegisterCompleteObjectUndo(this, "Create Object");
				T newObject = ScriptableObject.CreateInstance(type) as T;
				SerializedObjectEditorGUI<T> editorGUI = AddNewObject(newObject);
				OnCreatedNewObject(newObject);
				
				Selection.activeObject = editorGUI;
				GUIUtility.keyboardControl = 0;

				_selectedObjects.Clear();
				_selectedObjects.Add(editorGUI);
				SetObjectPosition(editorGUI, _dragPos);
			}

			private void SelectAll()
			{
				_selectedObjects.Clear();
				foreach (SerializedObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					_selectedObjects.Add(editorGUI);
				}
				_needsRepaint = true;
			}

			private void CopySelected()
			{
				_copiedObjects.Clear();
				foreach (SerializedObjectEditorGUI<T> editorGUI in _selectedObjects)
				{
					if (CanBeCopied(editorGUI))
					{
						_copiedObjects.Add(editorGUI);
					}
				}
			}

			private void Paste()
			{
				if (_copiedObjects.Count > 0)
				{
					Undo.RegisterCompleteObjectUndo(this, "Paste Object(s)");

					Vector2 pos = ((SerializedObjectEditorGUI<T>)_copiedObjects[0]).GetPosition();


					foreach (SerializedObjectEditorGUI<T> editorGUI in _copiedObjects)
					{
						T copyObject = CreateCopyFrom(editorGUI);
						SerializedObjectEditorGUI<T> copyEditorGUI = AddNewObject(copyObject);
						OnCreatedNewObject(copyObject);
						SetObjectPosition(copyEditorGUI, _dragPos + editorGUI.GetPosition() - pos);
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

					foreach (SerializedObjectEditorGUI<T> editorGUI in selectedObjects)
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

					foreach (SerializedObjectEditorGUI<T> editorGUI in new List<ScriptableObject>(_selectedObjects))
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

			private void UpdateCachedObjectList()
			{
				_cachedEditableObjects = new List<ScriptableObject>(_editableObjects);
			}

			private void UndoRedoCallback()
			{
				//Need way of knowing when a objects been restored from undo history
				foreach (SerializedObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					if (!_cachedEditableObjects.Contains(editorGUI))
					{
						OnCreatedNewObject(editorGUI.GetEditableObject());
					}
				}
			}
			#endregion
		}
	}
}