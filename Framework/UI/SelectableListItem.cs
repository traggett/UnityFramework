using System;
using UnityEngine;

namespace Framework
{
	using Utils;

	namespace UI
	{
		public abstract class SelectableListItem<T> : MonoBehaviour, IAnimatedListItem<T> where T : IComparable
		{
			#region Public Data
			public SelectableList<T> List
			{
				get
				{
					return _list;
				}
			}
			#endregion

			#region Private Data
			private SelectableList<T> _list;
			#endregion

			#region IAnimatedListItem
			public abstract T Data
			{
				get;
				set;
			}

			public RectTransform RectTransform
			{
				get
				{
					return (RectTransform)this.transform;
				}
			}

			public virtual void OnShow()
			{
				if (_list == null)
				{
					_list = GameObjectUtils.GetComponentInParents<SelectableList<T>>(this.gameObject, true);
				}

				SetSelected(false, true);
			}

			public virtual void OnHide()
			{

			}

			public virtual void OnRemoved()
			{

			}

			public virtual void SetFade(float fade)
			{

			}
			#endregion

			#region Virtual Interface
			public abstract void SetSelected(bool selected, bool instant);
			#endregion
		}
	}
}