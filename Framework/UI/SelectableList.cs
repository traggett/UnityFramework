using System;
using System.Collections.Generic;

namespace Framework
{
	namespace UI
	{
		public abstract class SelectableList<T> : AnimatedList<T> where T : IComparable
		{
			#region Public Data
			public delegate void OnItemsSelected(T[] t);
			#endregion

			#region Private Data
			private List<T> _selected = new List<T>();
			private int _numOfItemsToChoose;
			private OnItemsSelected _onItemsSelected;
			#endregion

			#region Public Interface
			public void Show(T[] items, OnItemsSelected callback, int numItemsToChoose, params T[] selected)
			{
				Initialise(items);

				_numOfItemsToChoose = numItemsToChoose;
				_onItemsSelected = callback;

				SetSelected(selected);

				this.gameObject.SetActive(true);
			}

			public void Hide()
			{
				Clear();
				this.gameObject.SetActive(false);
			}

			public void SetSelected(T[] selected)
			{
				_selected.Clear();

				if (selected != null)
				{
					for (int i = 0; i < selected.Length; i++)
					{
						foreach (SelectableListItem<T> item in this)
						{
							if (item.Data.Equals(selected[i]))
							{
								_selected.Add(selected[i]);

								if (_selected.Count >= _numOfItemsToChoose)
								{
									break;
								}
							}
						}
					}
				}

				HighlightSelected();
				_onItemsSelected.Invoke(_selected.ToArray());
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

					if (_selected.Count > _numOfItemsToChoose)
					{
						_selected.RemoveAt(0);
					}
				}

				HighlightSelected();
				_onItemsSelected.Invoke(_selected.ToArray());
			}
			#endregion

			#region Private Functions
			private void HighlightSelected()
			{
				foreach (SelectableListItem<T> item in this)
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