using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	using Utils;

	namespace UI
	{
		public interface IScrollListItem<T>
		{
			void Init(T item);
			bool Matches(T item);
			void SetItem(T item);
			RectTransform GetTransform();
			void SetFade(float fade);
		}

		public class ScrollList<T> : IEnumerable<IScrollListItem<T>>
		{
			public delegate GameObject CreatItemTypeFunc();

			public AnimationCurve MovementCurve { get; set; }

			public float MovementTime
			{
				set
				{
					if (value > 0.0f)
						_lerpSpeed = 1.0f / value;
				}
			}

			public float StartPadding { get; set; }
			public float EndPadding { get; set; }

			private static readonly float kDefaultLerpTime = 0.25f;

			private readonly PrefabInstancePool _itemPool;
			private readonly ScrollRect _scrollArea;
			private float _lerpSpeed;

			private class ScrollListItem
			{
				public enum State
				{
					Normal,
					FadingIn,
					FadingOut,
					Moving,
				}

				public IScrollListItem<T> _item;
				public Vector2 _fromPosition;
				public Vector2 _targetPosition;
				public float _lerp;
				public State _state;

				public ScrollListItem(IScrollListItem<T> item)
				{
					_item = item;
				}
			}

			private List<ScrollListItem> _items = new List<ScrollListItem>();
			private List<ScrollListItem> _itemsBeingRemoved = new List<ScrollListItem>();

			private class ScrollListEnumerator : IEnumerator<IScrollListItem<T>>
			{
				private readonly List<ScrollListItem> _items = new List<ScrollListItem>();
				private int _index;

				public ScrollListEnumerator(List<ScrollListItem> items)
				{
					_items = items;
					_index = -1;
				}

				#region IEnumerator
				public object Current
				{
					get
					{
						return _items[_index]._item;
					}
				}

				IScrollListItem<T> IEnumerator<IScrollListItem<T>>.Current
				{
					get
					{
						return _items[_index]._item;
					}
				}

				public void Dispose()
				{

				}

				public bool MoveNext()
				{
					_index++;

					if (_index >= _items.Count)
						return false;
					else
						return true;
				}

				public void Reset()
				{
					_index = -1;
				}
				#endregion
			}

			private ScrollList(ScrollRect scrollArea, PrefabInstancePool itemPool)
			{
				MovementTime = kDefaultLerpTime;
				MovementCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
				_itemPool = itemPool;
				_scrollArea = scrollArea;
				Initialise();
			}

			public static void Create(ref ScrollList<T> scrollList, ScrollRect scrollArea, PrefabInstancePool itemPool, IList<T> items = null)
			{
				if (scrollList == null)
				{
					scrollList = new ScrollList<T>(scrollArea, itemPool);
				}

				scrollList.Initialise(items);
			}

			public void Clear()
			{
				Initialise(null);
			}

			public void Update(IList<T> items)
			{
				FindChanges(items, out _, out List<ScrollListItem> toRemove);

				//Fade out and destroy old ones
				foreach (ScrollListItem item in toRemove)
				{
					item._lerp = 1.0f;
					item._state = ScrollListItem.State.FadingOut;
					_itemsBeingRemoved.Add(item);
				}

				UpdateOrdering();
				UpdateContentSize();

				//Lerp items to their target positions, lerp fade in
				foreach (ScrollListItem item in _items)
				{
					if (item._lerp < 1.0f)
					{
						item._lerp = Mathf.Clamp01(item._lerp + _lerpSpeed * Time.deltaTime);

						if (item._state == ScrollListItem.State.Moving)
						{
							float curvedLerp = MovementCurve.Evaluate(item._lerp);
							item._item.GetTransform().anchoredPosition = Vector2.Lerp(item._fromPosition, item._targetPosition, curvedLerp);
						}
						else if (item._state == ScrollListItem.State.FadingIn)
						{
							item._item.SetFade(item._lerp);
						}

						if (item._lerp >= 1.0f)
						{
							item._state = ScrollListItem.State.Normal;
						}
					}
				}

				//Lerp fade out for items being removed, destroy any that are no zero
				for (int i = 0; i < _itemsBeingRemoved.Count;)
				{
					_itemsBeingRemoved[i]._lerp += _lerpSpeed * Time.deltaTime;

					if (_itemsBeingRemoved[i]._lerp > 1.0f)
					{
						_itemPool.Destroy(_itemsBeingRemoved[i]._item.GetTransform().gameObject);
						_itemsBeingRemoved.RemoveAt(i);
					}
					else
					{
						_itemsBeingRemoved[i]._item.SetFade(1.0f - _itemsBeingRemoved[i]._lerp);
						i++;
					}
				}
			}

			#region Private Functions
			private void Initialise(IList<T> items = null)
			{
				FindChanges(items, out _, out List<ScrollListItem> toRemove);

				//Destroy items no longer in the list
				foreach (ScrollListItem item in toRemove)
				{
					_itemPool.Destroy(item._item.GetTransform().gameObject);
				}

				//Destroy items currently getting removed
				foreach (ScrollListItem item in _itemsBeingRemoved)
				{
					_itemPool.Destroy(item._item.GetTransform().gameObject);
				}
				_itemsBeingRemoved.Clear();

				//Set position instantly for all items
				foreach (ScrollListItem item in _items)
				{
					RectTransform transform = item._item.GetTransform();
					item._lerp = 1.0f;
					item._state = ScrollListItem.State.Normal;
					item._item.SetFade(1.0f);
					item._fromPosition = item._targetPosition;
					transform.anchoredPosition = item._targetPosition;
				}

				UpdateOrdering();
				UpdateContentSize();
			}

			private ScrollListItem CreatItemType(T item)
			{
				GameObject gameObject = _itemPool.Instantiate(_scrollArea.content.transform);
				IScrollListItem<T> newItem = gameObject.GetComponent<IScrollListItem<T>>();
				newItem.Init(item);
				return new ScrollListItem(newItem);
			}

			private void FindChanges(IList<T> items, out List<ScrollListItem> toAdd, out List<ScrollListItem> toRemove)
			{
				toAdd = new List<ScrollListItem>();
				toRemove = new List<ScrollListItem>(_items);
				_items.Clear();

				Vector2 pos = new Vector2(0f, -StartPadding);

				if (items != null)
				{
					for (int i = 0; i < items.Count; ++i)
					{
						ScrollListItem item = null;

						foreach (ScrollListItem button in toRemove)
						{
							if (button._item.Matches(items[i]))
							{
								button._item.SetItem(items[i]);
								_items.Add(button);
								toRemove.Remove(button);
								item = button;
								break;
							}
						}

						//If no item exists for this create a new one
						bool newItem = item == null;
						RectTransform transform;

						if (newItem)
						{
							item = CreatItemType(items[i]);
							item._lerp = 0.0f;
							item._state = ScrollListItem.State.FadingIn;
							item._targetPosition = pos;
							item._fromPosition = pos;
							transform = item._item.GetTransform();
							transform.anchoredPosition = pos;
							item._item.SetFade(0.0f);
							_items.Add(item);
							toAdd.Add(item);
						}

						//Update position and content size
						else
						{
							transform = item._item.GetTransform();

							if (item._targetPosition != pos)
							{
								item._targetPosition = pos;
								item._fromPosition = transform.anchoredPosition;
								item._state = ScrollListItem.State.Moving;
								item._lerp = 0.0f;
								item._item.SetFade(1.0f);
							}
						}

						pos.y -= transform.sizeDelta.y;
					}
				}
			}

			private void UpdateContentSize()
			{
				//TO DO also check items being destroyed
				if (_items != null)
				{
					float size = StartPadding;

					for (int i = 0; i < _items.Count; i++)
					{
						RectTransform transform = _items[i]._item.GetTransform();
						size += transform.sizeDelta.y;
					}

					size += EndPadding;

					Vector2 scrollAreaSize = _scrollArea.content.sizeDelta;
					scrollAreaSize.y = size;
					_scrollArea.content.sizeDelta = scrollAreaSize;
				}
			}

			private void UpdateOrdering()
			{
				if (_items != null)
				{
					for (int i = 0; i < _items.Count; i++)
					{
						_items[i]._item.GetTransform().SetSiblingIndex(_items.Count - i - 1);
					}
				}

			}
			#endregion

			#region IEnumerator
			public IEnumerator<IScrollListItem<T>> GetEnumerator()
			{
				return new ScrollListEnumerator(_items);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return (IEnumerator)new ScrollListEnumerator(_items);
			}
			#endregion
		}
	}
}