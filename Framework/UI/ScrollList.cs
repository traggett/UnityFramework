using System;
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
			T Data
			{
				get;
				set;
			}

			RectTransform RectTransform
			{
				get;
			}

			void OnShow();
			void OnHide();
			void SetFade(float fade);
		}

		public class ScrollList<T> : IEnumerable<IScrollListItem<T>> where T : IComparable
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

			public RectOffset Borders { get; set; }
			public Vector2 Spacing { get; set; }		
			public int NumColumns { get; set; }

			private const float kDefaultLerpTime = 0.25f;

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
				_itemPool = itemPool;
				_scrollArea = scrollArea;
			}

			public static void Create(ref ScrollList<T> scrollList, ScrollRect scrollArea, PrefabInstancePool itemPool, IList<T> items = null,
									RectOffset borders = null, Vector2 spacing = default, int numColumns = 1,
									float movementTime = kDefaultLerpTime, AnimationCurve movementCurve = null)
			{
				if (movementCurve == null)
					movementCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

				if (borders == null)
					borders = new RectOffset(0, 0, 0, 0);

				if (numColumns <= 0)
					numColumns = 1;

				if (scrollList == null)
				{
					scrollList = new ScrollList<T>(scrollArea, itemPool);
				}

				scrollList.MovementCurve = movementCurve;
				scrollList.MovementTime = movementTime;
				scrollList.Borders = borders;
				scrollList.Spacing = spacing;
				scrollList.NumColumns = numColumns;

				scrollList.Initialise(items);
			}

			public void Clear()
			{
				Initialise(null);
			}

			public void Update(IList<T> items, float deltaTime)
			{
				FindChanges(items, out _, out List<ScrollListItem> toRemove);

				//Fade out and destroy old ones
				foreach (ScrollListItem item in toRemove)
				{
					item._lerp = 1.0f;
					item._state = ScrollListItem.State.FadingOut;
					item._item.OnHide();
					_itemsBeingRemoved.Add(item);
				}

				UpdateOrdering();
				UpdateContentHeight();

				//Lerp items to their target positions, lerp fade in
				foreach (ScrollListItem item in _items)
				{
					if (item._lerp < 1.0f)
					{
						item._lerp = Mathf.Clamp01(item._lerp + _lerpSpeed * deltaTime);

						if (item._state == ScrollListItem.State.Moving)
						{
							float curvedLerp = MovementCurve.Evaluate(item._lerp);
							item._item.RectTransform.anchoredPosition = Vector2.Lerp(item._fromPosition, item._targetPosition, curvedLerp);
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
					_itemsBeingRemoved[i]._lerp += _lerpSpeed * deltaTime;

					if (_itemsBeingRemoved[i]._lerp > 1.0f)
					{
						_itemPool.Destroy(_itemsBeingRemoved[i]._item.RectTransform.gameObject);
						_itemsBeingRemoved.RemoveAt(i);
					}
					else
					{
						_itemsBeingRemoved[i]._item.SetFade(1.0f - _itemsBeingRemoved[i]._lerp);
						i++;
					}
				}
			}

			public float GetContentHeight()
			{
				return _scrollArea.content.sizeDelta.y;
			}

			#region Private Functions
			private void Initialise(IList<T> items = null)
			{
				FindChanges(items, out _, out List<ScrollListItem> toRemove);

				//Destroy items no longer in the list
				foreach (ScrollListItem item in toRemove)
				{
					item._item.OnHide();
					_itemPool.Destroy(item._item.RectTransform.gameObject);
				}

				//Destroy items currently getting removed
				foreach (ScrollListItem item in _itemsBeingRemoved)
				{
					_itemPool.Destroy(item._item.RectTransform.gameObject);
				}
				_itemsBeingRemoved.Clear();

				//Set position instantly for all items
				foreach (ScrollListItem item in _items)
				{
					RectTransform transform = item._item.RectTransform;
					item._lerp = 1.0f;
					item._state = ScrollListItem.State.Normal;
					item._item.SetFade(1.0f);
					item._fromPosition = item._targetPosition;
					transform.anchoredPosition = item._targetPosition;
				}

				UpdateOrdering();
				UpdateContentHeight();
			}

			private void FindChanges(IList<T> items, out List<ScrollListItem> toAdd, out List<ScrollListItem> toRemove)
			{
				toAdd = new List<ScrollListItem>();
				toRemove = new List<ScrollListItem>(_items);
				_items.Clear();

				Vector2 pos = new Vector2(Borders.left, -Borders.top);
				int currentColumn = 0;

				float ColumnWidth = (((RectTransform)_scrollArea.content.transform).rect.width - Borders.horizontal) / NumColumns;

				if (items != null)
				{
					for (int i = 0; i < items.Count; ++i)
					{
						ScrollListItem item = null;

						foreach (ScrollListItem button in toRemove)
						{
							if (button._item.Data.Equals(items[i]))
							{
								_items.Add(button);
								toRemove.Remove(button);
								item = button;
								break;
							}
						}

						RectTransform transform;

						//If no item exists for this create a new one
						if (item == null)
						{
							GameObject gameObject = _itemPool.Instantiate(_scrollArea.content.transform);
							item = new ScrollListItem(gameObject.GetComponent<IScrollListItem<T>>());
							item._item.OnShow();
							item._item.Data = items[i];
							item._lerp = 0.0f;
							item._state = ScrollListItem.State.FadingIn;
							item._targetPosition = pos;
							item._fromPosition = pos;
							transform = item._item.RectTransform;
							transform.anchoredPosition = pos;
							item._item.SetFade(0.0f);
							_items.Add(item);
							toAdd.Add(item);
						}
						//Otherwise update existing
						else
						{
							transform = item._item.RectTransform;

							if (item._targetPosition != pos)
							{
								item._targetPosition = pos;
								item._fromPosition = transform.anchoredPosition;
								item._state = ScrollListItem.State.Moving;
								item._lerp = 0.0f;
								item._item.SetFade(1.0f);
							}

							item._item.Data = items[i];
						}

						float itemHeight = RectTransformUtils.GetHeight(transform);

						//Put next item in next column
						currentColumn++;

						if (currentColumn < NumColumns)
						{
							pos.x += ColumnWidth + Spacing.x;
						}
						else
						{
							currentColumn = 0;
							pos.x = Borders.left;
							pos.y -= itemHeight + Spacing.y;
						}			
					}
				}
			}

			private void UpdateContentHeight()
			{
				//TO DO also check items being destroyed
				if (_items != null)
				{
					float contentHeight = Borders.vertical;
					int currentColumn = 0;

					for (int i = 0; i < _items.Count; i++)
					{
						currentColumn++;

						if (currentColumn >= NumColumns)
						{
							RectTransform transform = _items[i]._item.RectTransform;
							contentHeight += transform.sizeDelta.y;

							if (i != _items.Count - 1)
							{
								contentHeight += Spacing.y;
							}
						}
					}

					RectTransformUtils.SetHeight(_scrollArea.content, contentHeight);

					SetScrollAreaEnabled(contentHeight > _scrollArea.viewport.rect.height);
				}
			}

			private void SetScrollAreaEnabled(bool enabled)
			{
				_scrollArea.enabled = enabled;
				
				if (!enabled)
				{
					RectTransformUtils.SetY(_scrollArea.content, 0f);
				}

				RectMask2D mask = _scrollArea.viewport.GetComponent<RectMask2D>();
				
				if (mask != null)
				{
					mask.enabled = enabled;
				}
			}

			private void UpdateOrdering()
			{
				if (_items != null)
				{
					for (int i = 0; i < _items.Count; i++)
					{
						_items[i]._item.RectTransform.SetSiblingIndex(_items.Count - i - 1);
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