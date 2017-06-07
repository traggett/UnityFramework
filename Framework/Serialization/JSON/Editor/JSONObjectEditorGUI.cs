using UnityEngine;
using UnityEditor;

using System;

namespace Engine
{
	namespace JSON
	{
		public abstract class JSONObjectEditorGUI<T> : ScriptableObject, IJSONCustomEditable, IComparable where T : class
		{
			#region Private Data
			private bool _dirty;
			private JSONObjectEditor<T> _editor;
			private T _editableObject;

			[SerializeField]
			private string _undoObjectJSON = null;
			#endregion

			#region Public Interfacce
			public JSONObjectEditorGUI(JSONObjectEditor<T> editor, T obj)
			{
				_editor = editor;
				SetEditableObject(obj);
				Undo.undoRedoPerformed += UndoRedoCallback;
			}

			~JSONObjectEditorGUI()
			{
				Undo.undoRedoPerformed -= UndoRedoCallback;
			}

			public JSONObjectEditor<T> GetEditor()
			{
				return _editor;
			}

			public void SetEditableObject(T obj)
			{
				if (obj == null)
					throw new Exception();

				_editableObject = obj;
				OnSetObject();
			}

			public T GetEditableObject()
			{
				if(_editableObject == null)
					throw new Exception();

				return _editableObject;
			}

			public bool IsDirty()
			{
				return _dirty;
			}

			public void MarkAsDirty(bool dirty)
			{
				_dirty = dirty;

				if (_dirty)
					EditorUtility.SetDirty(this);
			}

			public bool IsValid()
			{
				return _editableObject != null && _editor != null;
			}

			public void SaveUndoState()
			{
				_undoObjectJSON = JSONConverter.ToJSONString(_editableObject);
			}

			public void ClearUndoState()
			{
				_undoObjectJSON = null;
			}

			public void RenderProperties()
			{
				if (_editableObject == null)
					throw new Exception();

				//If store an undo command on a temp string representing event, then on undo performed callback recreate event from string.
				string undoObjectJSON = JSONConverter.ToJSONString(_editableObject);
				
				if (RenderObjectProperties(GUIContent.none))
				{
					_undoObjectJSON = undoObjectJSON;
					Undo.RegisterCompleteObjectUndo(this, GetEditableObject().GetType().Name + " changed");
					SaveUndoState();

					GetEditor().SetNeedsRepaint();
					MarkAsDirty(true);
				}
			}
			#endregion

			#region Virtual Interface
			public abstract Rect GetBounds();

			public abstract Vector2 GetPosition();

			public abstract void SetPosition(Vector2 position);

			protected abstract void OnSetObject();
			#endregion

			#region IJSONCustomEditable
			public virtual bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged;
				_editableObject = JSONEditorGUI.ObjectField(_editableObject, label, out dataChanged);
				return dataChanged;
			}
			#endregion

			#region IComparable
			public virtual int CompareTo(object obj)
			{
				JSONObjectEditorGUI<T> editorGUI = obj as JSONObjectEditorGUI<T>;

				if (editorGUI == null)
					return 1;

				if (editorGUI == this)
					return 0;

				return this.GetHashCode().CompareTo(editorGUI.GetHashCode());
			}
			#endregion

			#region Private Functions
			private void UndoRedoCallback()
			{
				if (this != null)
				{
					if (!string.IsNullOrEmpty(_undoObjectJSON) && _editableObject != null)
					{
						_editableObject = (T)JSONConverter.FromJSONString(_editableObject.GetType(), _undoObjectJSON);
						if (_editableObject == null)
							throw new Exception();


						_undoObjectJSON = null;
						GetEditor().SetNeedsRepaint();
						MarkAsDirty(true);
					}
				}
			}
			#endregion
		}
	}
}
