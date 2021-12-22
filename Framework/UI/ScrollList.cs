﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	using Framework.Maths;
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
			void OnRemoved();
			void SetFade(float fade);
		}

		public class ScrollList<T> : IEnumerable<IScrollListItem<T>> where T : IComparable
		{
			#region Public Data
			public delegate GameObject CreatItemTypeFunc();

			public InterpolationType MovementInterpolation { get; set; }

			public float MovementTime
			{
				set
				{
					if (value > 0.0f)
						_movementLerpSpeed = 1.0f / value;
				}
			}

			public float FadeTime
			{
				set
				{
					if (value > 0.0f)
						_fadeLerpSpeed = 1.0f / value;
				}
			}

			public RectOffset Borders { get; set; }
			public Vector2 Spacing { get; set; }		
			public int NumColumns { get; set; }
			#endregion

			#region Private Data
			private const float kDefaultMovementTime = 0.25f;
			private const float kDefaultFadeTime = 0.25f;

			private readonly PrefabInstancePool _itemPool;
			private readonly ScrollRect _scrollArea;
			private float _movementLerpSpeed;
			private float _fadeLerpSpeed;

			private class ScrollListItem
			{
				public IScrollListItem<T> _item;
				public Vector2 _fromPosition;
				public Vector2 _targetPosition;
				public float _movementLerp;
				public float _fade;

				public ScrollListItem(IScrollListItem<T> item)
				{
					_item = item;
				}
			}

			private readonly List<ScrollListItem> _items = new List<ScrollListItem>();
			private readonly List<ScrollListItem> _itemsBeingRemoved = new List<ScrollListItem>();

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
			#endregion

			#region Public Interface
			public static void Create(ref ScrollList<T> scrollList, ScrollRect scrollArea, PrefabInstancePool itemPool, IList<T> items = null,
									RectOffset borders = null, Vector2 spacing = default, int numColumns = 1,
									float movementTime = kDefaultMovementTime, InterpolationType movementInterpolation = InterpolationType.InOutSine,
									float fadeTime = kDefaultFadeTime)
			{
				if (borders == null)
				{
					borders = new RectOffset(0, 0, 0, 0);
				}
					
				if (numColumns <= 0)
				{
					numColumns = 1;
				}
				
				if (scrollList == null)
				{
					scrollList = new ScrollList<T>(scrollArea, itemPool);
				}

				scrollList.MovementInterpolation = movementInterpolation;
				scrollList.MovementTime = movementTime;
				scrollList.FadeTime = fadeTime;
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
					item._item.OnHide();
					_itemsBeingRemoved.Add(item);
				}

				UpdateOrdering();
				UpdateContentHeight();

				//Lerp items to their target positions, lerp fade in
				foreach (ScrollListItem item in _items)
				{
					//Update fading in
					if (item._fade < 1f)
					{
						if (_fadeLerpSpeed > 0f)
						{
							item._fade = Mathf.Clamp01(item._fade + _fadeLerpSpeed * deltaTime);
							item._item.SetFade(item._fade);
						}
						else
						{
							item._fade = 1f;
							item._item.SetFade(1f);
						}
					}

					//Update movment
					if (item._movementLerp < 1f)
					{
						if (_movementLerpSpeed > 0f)
						{
							item._movementLerp = Mathf.Clamp01(item._movementLerp + _movementLerpSpeed * deltaTime);
							item._item.RectTransform.anchoredPosition = MathUtils.Interpolate(MovementInterpolation, item._fromPosition, item._targetPosition, item._movementLerp);
						}
						else
						{
							item._movementLerp = 1f;
							item._item.RectTransform.anchoredPosition = item._targetPosition;
						}
					}
				}

				//Lerp fade out for items being removed, destroy any that are no zero
				for (int i = 0; i < _itemsBeingRemoved.Count;)
				{
					if (_fadeLerpSpeed > 0f)
					{
						_itemsBeingRemoved[i]._fade -= _fadeLerpSpeed * deltaTime;

						if (_itemsBeingRemoved[i]._fade <= 0f)
						{
							_itemsBeingRemoved[i]._item.OnRemoved();
							_itemPool.Destroy(_itemsBeingRemoved[i]._item.RectTransform.gameObject);
							_itemsBeingRemoved.RemoveAt(i);
						}
						else
						{
							_itemsBeingRemoved[i]._item.SetFade(_itemsBeingRemoved[i]._fade);
							i++;
						}
					}
					else
					{
						_itemsBeingRemoved[i]._item.OnRemoved();
						_itemPool.Destroy(_itemsBeingRemoved[i]._item.RectTransform.gameObject);
						_itemsBeingRemoved.RemoveAt(i);
					}
				}
			}

			public float GetContentHeight()
			{
				return _scrollArea.content.sizeDelta.y;
			}
			#endregion

			#region Private Functions
			private ScrollList(ScrollRect scrollArea, PrefabInstancePool itemPool)
			{
				_itemPool = itemPool;
				_scrollArea = scrollArea;
			}

			private void Initialise(IList<T> items = null)
			{
				FindChanges(items, out _, out List<ScrollListItem> toRemove);

				//Destroy items no longer in the list
				foreach (ScrollListItem item in toRemove)
				{
					item._item.OnHide();
					item._item.OnRemoved();
					_itemPool.Destroy(item._item.RectTransform.gameObject);
				}

				//Destroy items currently getting removed
				foreach (ScrollListItem item in _itemsBeingRemoved)
				{
					_itemPool.Destroy(item._item.RectTransform.gameObject);
				}
				_itemsBeingRemoved.Clear();

				//Set position and fade instantly for all items
				foreach (ScrollListItem item in _items)
				{
					RectTransform transform = item._item.RectTransform;
					
					item._fade = 1.0f;
					item._item.SetFade(1.0f);

					item._movementLerp = 1.0f;
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
							item._item.Data = items[i];
							item._item.OnShow();

							//Optionally fade the item in
							if (_fadeLerpSpeed > 0f)
							{
								item._fade = 0f;
								item._item.SetFade(0f);
							}
							else
							{
								item._fade = 1f;
								item._item.SetFade(1f);
							}

							item._movementLerp = 1f;
							item._targetPosition = pos;
							item._fromPosition = pos;
							transform = item._item.RectTransform;
							transform.anchoredPosition = pos;

							
							_items.Add(item);
							toAdd.Add(item);
						}
						//Otherwise update existing
						else
						{
							transform = item._item.RectTransform;
							
							if (item._targetPosition != pos)
							{
								if (_movementLerpSpeed > 0f)
								{
									item._targetPosition = pos;
									item._fromPosition = transform.anchoredPosition;
									item._movementLerp = 0.0f;

								}
								else
								{
									item._targetPosition = pos;
									item._fromPosition = pos;
									item._movementLerp = 1f;
								}
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