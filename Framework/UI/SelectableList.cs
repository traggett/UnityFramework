using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	using Maths;
	using Utils;

	namespace UI
	{
		public abstract class SelectableList<T> : MonoBehaviour where T : IComparable
		{
			#region Public Data
			public delegate void OnItemsSelected(T[] t);

			public ScrollRect _scrollArea;
			public PrefabInstancePool _itemPool;

			public RectOffset _contentPadding;
			public Vector2 _contentItemSpacing;
			public int _numColumns = 1;

			public float _contentShiftTime;
			public InterpolationType _contentShiftInterpolation;
			public float _contentFadeTime;
			#endregion

			#region Private Data
			private AnimatedList<T> _itemsList;
			private T[] _items = new T[0];
			private List<T> _selected = new List<T>();
			private int _numOfTargetsToChoose;
			private OnItemsSelected _onTargetsSelected;
			#endregion

			#region Unity Messages
			void Update()
			{
				if (_itemsList != null)
					_itemsList.Update(_items, Time.deltaTime);
			}
			#endregion

			#region Public Interface
			public void Show(T[] items, OnItemsSelected targetsSelected, int numTargetsToChoose, params T[] selected)
			{
				if (_itemsList != null)
					_itemsList.Clear();

				_items = items;
				AnimatedList<T>.Create(ref _itemsList, _scrollArea.content, _itemPool, _items,
									_contentPadding, _contentItemSpacing, _numColumns,
									_contentShiftTime, _contentShiftInterpolation, _contentFadeTime);

				_numOfTargetsToChoose = numTargetsToChoose;
				_onTargetsSelected = targetsSelected;

				SetSelected(selected);

				this.gameObject.SetActive(true);
			}

			public void Hide()
			{
				if (_itemsList != null)
					_itemsList.Clear();

				_items = null;

				this.gameObject.SetActive(false);
			}

			public void SetSelected(T[] selected)
			{
				if (_itemsList != null)
				{
					_selected.Clear();

					if (selected != null)
					{
						for (int i = 0; i < selected.Length; i++)
						{
							foreach (SelectableListItem<T> item in _itemsList)
							{
								if (item.Data.Equals(selected[i]))
								{
									_selected.Add(selected[i]);

									if (_selected.Count >= _numOfTargetsToChoose)
									{
										break;
									}
								}
							}
						}
					}

					HighlightSelected();
					_onTargetsSelected.Invoke(_selected.ToArray());
				}
			}

			public void SetAlpha(float alpha)
			{
				//TO DO - fade all items somehow???
			}

			public void ToggleItemSelection(SelectableListItem<T> item)
			{
				T data = item.Data;

				if (_selected.Contains(data))
				{
					_selected.Remove(data);
				}
				else
				{
					_selected.Add(data);

					if (_selected.Count > _numOfTargetsToChoose)
					{
						_selected.RemoveAt(0);
					}
				}

				HighlightSelected();
				_onTargetsSelected.Invoke(_selected.ToArray());
			}

			public float GetContentHeight()
			{
				return _itemsList.GetContentHeight();
			}
			#endregion

			#region Private Functions
			private void HighlightSelected()
			{
				foreach (SelectableListItem<T> item in _itemsList)
				{
					item.SetSelected(IsSelected(item), false);
				}
			}

			private bool IsSelected(SelectableListItem<T> item)
			{
				foreach (T data in _selected)
				{
					if (item.Data.Equals(data))
					{
						return true;
					}
				}

				return false;
			}
			#endregion
		}
	}
}