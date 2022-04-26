using UnityEngine;
using System;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public abstract class ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> : IComparable where TParentAsset : ScriptableObject where TChildAsset : ScriptableObject
			{
				#region Private Data
				private ScriptableObjectHierarchyEditor<TParentAsset, TChildAsset> _editor;
				private TChildAsset _asset;
				#endregion

				#region Public Interfacce
				public TChildAsset Asset
				{
					get
					{
						return _asset;
					}
				}

				public void Init(ScriptableObjectHierarchyEditor<TParentAsset, TChildAsset> editor, TChildAsset obj)
				{
					_editor = editor;

					if (obj == null)
						throw new Exception();

					_asset = obj;
					OnSetObject();
				}

				public ScriptableObjectHierarchyEditor<TParentAsset, TChildAsset> GetEditor()
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
					ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset> editorGUI = obj as ScriptableObjectHierarchyEditorObjectGUI<TParentAsset, TChildAsset>;

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
}