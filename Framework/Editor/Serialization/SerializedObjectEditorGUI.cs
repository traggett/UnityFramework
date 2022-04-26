using UnityEngine;
using System;

namespace Framework
{
	namespace Serialization
	{
		public abstract class SerializedObjectEditorGUI<TAsset, TSubAsset> : ScriptableObject, IComparable where TAsset : ScriptableObject where TSubAsset : ScriptableObject
		{
			#region Private Data
			private SerializedObjectEditor<TAsset, TSubAsset> _editor;
			private TSubAsset _asset;

			[SerializeField]
			private string _undoObjectSerialized = null;
			#endregion

			#region Public Interfacce
			public TSubAsset Asset
			{
				get
				{
					return _asset;
				}
			}

			public void Init(SerializedObjectEditor<TAsset, TSubAsset> editor, TSubAsset obj)
			{
				_editor = editor;

				if (obj == null)
					throw new Exception();

				_asset = obj;
				OnSetObject();
			}

			public SerializedObjectEditor<TAsset, TSubAsset> GetEditor()
			{
				return _editor;
			}
			#endregion

			#region Virtual Interface
			public abstract Rect GetBounds();

			public abstract Vector2 GetPosition();

			public abstract void SetPosition(Vector2 position);

			protected abstract void OnSetObject();
			#endregion

			#region IComparable
			public virtual int CompareTo(object obj)
			{
				SerializedObjectEditorGUI<TAsset, TSubAsset> editorGUI = obj as SerializedObjectEditorGUI<TAsset, TSubAsset>;

				if (editorGUI == null)
					return 1;

				if (editorGUI == this)
					return 0;

				return this.GetHashCode().CompareTo(editorGUI.GetHashCode());
			}
			#endregion
		}
	}
}
