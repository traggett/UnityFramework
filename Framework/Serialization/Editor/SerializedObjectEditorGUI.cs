using UnityEngine;
using UnityEditor;

using System;

namespace Framework
{
	namespace Serialization
	{
		public abstract class SerializedObjectEditorGUI<T> : ScriptableObject, ICustomEditorInspector, IComparable where T : class
		{
			#region Private Data
			private bool _dirty;
			private SerializedObjectEditor<T> _editor;
			private T _editableObject;
			[SerializeField]
			private string _undoObjectSerialized;
			private SerializedObject _undoObject;
			private SerializedProperty _undoProperty;
			#endregion

			#region Public Interfacce
			public SerializedObjectEditorGUI()
			{
				Undo.undoRedoPerformed += UndoRedoCallback;
			}

			~SerializedObjectEditorGUI()
			{
				Undo.undoRedoPerformed -= UndoRedoCallback;
			}

			public void Init(SerializedObjectEditor<T> editor, T obj)
			{
				_editor = editor;
				SetEditableObject(obj);
				_undoObject = new SerializedObject(this);
				_undoProperty = _undoObject.FindProperty("_undoObjectSerialized");
			}

			public SerializedObjectEditor<T> GetEditor()
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
			}

			public bool IsValid()
			{
				return _editableObject != null && _editor != null;
			}

			public void SaveUndoState()
			{
				_undoProperty.stringValue = Serializer.ToString(_editableObject);
				_undoObject.ApplyModifiedProperties();
			}

			public void ClearUndoState()
			{
				_undoProperty.stringValue = null;
				_undoObject.ApplyModifiedPropertiesWithoutUndo();
			}

			public void RenderProperties()
			{
				if (_editableObject == null)
					throw new Exception();

				//If store an undo command on a temp string representing event, then on undo performed callback recreate event from string.
				string undoObjectXml = Serializer.ToString(_editableObject);

				if (RenderObjectProperties(GUIContent.none))
				{
					//Update undo string to be the object before properties were changed..
					_undoProperty.stringValue = undoObjectXml;
					_undoObject.ApplyModifiedPropertiesWithoutUndo();
					//Then save the new object with modified properties (this will add the changes to the undo stack)
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

			#region ICustomEditable
			public virtual bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;
				_editableObject = SerializationEditorGUILayout.ObjectField(_editableObject, label, ref dataChanged);
				return dataChanged;
			}
			#endregion

			#region IComparable
			public virtual int CompareTo(object obj)
			{
				SerializedObjectEditorGUI<T> editorGUI = obj as SerializedObjectEditorGUI<T>;

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
					if (!string.IsNullOrEmpty(_undoObjectSerialized) && _editableObject != null)
					{
						_editableObject = (T)Serializer.FromString(_editableObject.GetType(), _undoObjectSerialized);
						if (_editableObject == null)
							throw new Exception();

						ClearUndoState();
						GetEditor().SetNeedsRepaint();
						MarkAsDirty(true);
					}
				}
			}
			#endregion
		}
	}
}
