using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	using Maths;
	using Utils;
	
	namespace UI
	{
		public interface IAnimatedListItem<T>
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

		public abstract class AnimatedList<T> : MonoBehaviour, IEnumerable<IAnimatedListItem<T>> where T : IComparable
		{
			#region Public Data
			public RectOffset Borders
			{
				get
				{
					return _borders;
				}
				set
				{
					_borders = value;

					//TO DO update positioning!!
				}
			}

			public int NumColumns
			{
				get
				{
					return _numColumns;
				}
				set
				{
					if (value >= 1 && value != _numColumns)
					{
						_numColumns = value;
						UpdateItemLayout();
					}
				}
			}

			public Vector2 ItemSpacing
			{
				get
				{
					return _itemSpacing;
				}
				set
				{
					_itemSpacing = value;
					UpdateItemLayout();
				}
			}

			public int ItemMovementTime
			{
				set
				{
					if (value > 0.0f)
					{
						_movementLerpSpeed = 1.0f / value;
					}
					else
					{
						_movementLerpSpeed = -1f;
					}
				}
			}

			public InterpolationType ItemMovementInterpolation
			{
				get
				{
					return _itemMovementInterpolation;
				}
				set
				{
					_itemMovementInterpolation = value;
				}
			}

			public float ItemFadeTime
			{
				set
				{
					if (value > 0.0f)
					{
						_fadeLerpSpeed = 1.0f / value;
					}
					else
					{
						_fadeLerpSpeed = -1f;
					}
				}
			}

			public RectTransform RectTransform
			{
				get
				{
					return (RectTransform)this.transform;
				}
			}
			#endregion

			#region Serialised Data
			[SerializeField]
			private RectOffset _borders = new RectOffset(0, 0, 0, 0);

			[SerializeField]
			private int _numColumns = 1;

			[SerializeField]
			private Vector2 _itemSpacing = Vector2.zero;

			[SerializeField]
			private InterpolationType _itemMovementInterpolation;

			[SerializeField]
			private PrefabInstancePool _itemPool;

			[SerializeField]
			private float _movementLerpSpeed = 4f;

			[SerializeField]
			private float _fadeLerpSpeed = 2f;
			#endregion

			#region Private Data
			private class ScrollListItem
			{
				public IAnimatedListItem<T> _item;
				public Vector2 _fromPosition;
				public Vector2 _targetPosition;
				public float _movementLerp;
				public float _fade;

				public ScrollListItem(IAnimatedListItem<T> item)
				{
					_item = item;
				}
			}

			private readonly List<ScrollListItem> _items = new List<ScrollListItem>();
			private readonly List<ScrollListItem> _itemsBeingRemoved = new List<ScrollListItem>();
			#endregion

			#region Unity Messages
			private void Update()
			{
				float deltaTime = Time.deltaTime;

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
							item._item.RectTransform.anchoredPosition = MathUtils.Interpolate(ItemMovementInterpolation, item._fromPosition, item._targetPosition, item._movementLerp);
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
			#endregion

			#region Public Interface
			public void Initialise(IList<T> items = null)
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

			public void SetItems(IList<T> items)
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
			}

			public void Clear()
			{
				Initialise(null);
			}
			#endregion

			#region Private Functions
			private void FindChanges(IList<T> items, out List<ScrollListItem> toAdd, out List<ScrollListItem> toRemove)
			{
				toAdd = new List<ScrollListItem>();
				toRemove = new List<ScrollListItem>(_items);
				_items.Clear();

				Vector2 pos = new Vector2(Borders.left, -Borders.top);
				int currentColumn = 0;

				float ColumnWidth = (RectTransform.rect.width - Borders.horizontal) / NumColumns;

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
							GameObject gameObject = _itemPool.Instantiate();
							item = new ScrollListItem(gameObject.GetComponent<IAnimatedListItem<T>>());
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
							pos.x += ColumnWidth + ItemSpacing.x;
						}
						else
						{
							currentColumn = 0;
							pos.x = Borders.left;
							pos.y -= itemHeight + ItemSpacing.y;
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
						if (currentColumn == 0)
						{
							RectTransform transform = _items[i]._item.RectTransform;
							contentHeight += transform.sizeDelta.y;
						}
						
						currentColumn++;

						if (currentColumn >= NumColumns)
						{
							currentColumn = 0;

							if (i != _items.Count - 1)
							{
								contentHeight += ItemSpacing.y;
							}
						}
					}

					RectTransformUtils.SetHeight(RectTransform, contentHeight);
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

			private void UpdateItemLayout()
			{
				//TO DO!
			}
			#endregion

			#region Enumerator Class
			private class ScrollListEnumerator : IEnumerator<IAnimatedListItem<T>>
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

				IAnimatedListItem<T> IEnumerator<IAnimatedListItem<T>>.Current
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

			#region IEnumerator
			public IEnumerator<IAnimatedListItem<T>> GetEnumerator()
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