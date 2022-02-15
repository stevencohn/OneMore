//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Windows.Forms;

	internal partial class CrawlWebPageDialog : UI.LocalizableForm
	{

		#region SortableBindingList
		private sealed class SortableBindingList<T> : BindingList<T> where T : class
		{
			private bool isSorted;
			private ListSortDirection sortDirection = ListSortDirection.Ascending;
			private PropertyDescriptor sortProperty;

			public SortableBindingList()
			{
			}

			public SortableBindingList(IList<T> list)
				: base(list)
			{
			}

			protected override bool SupportsSortingCore => true;

			protected override bool IsSortedCore => isSorted;

			protected override ListSortDirection SortDirectionCore => sortDirection;

			protected override PropertyDescriptor SortPropertyCore => sortProperty;

			protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
			{
				sortProperty = prop;
				sortDirection = direction;

				if (!(Items is List<T> list)) return;

				list.Sort(Compare);

				isSorted = true;
				OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			}

			private int Compare(T lhs, T rhs)
			{
				var result = OnComparison(lhs, rhs);
				// invert if descending
				if (sortDirection == ListSortDirection.Descending)
					result = -result;
				return result;
			}

			private int OnComparison(T lhs, T rhs)
			{
				object lhsValue = lhs == null ? null : sortProperty.GetValue(lhs);
				object rhsValue = rhs == null ? null : sortProperty.GetValue(rhs);
				if (lhsValue == null)
				{
					return (rhsValue == null) ? 0 : -1; // nulls are equal
				}
				if (rhsValue == null)
				{
					return 1; // first has value, second doesn't
				}
				if (lhsValue is IComparable comparable)
				{
					return comparable.CompareTo(rhsValue);
				}
				if (lhsValue.Equals(rhsValue))
				{
					return 0; // both are the same
				}
				// not comparable, compare ToString
				return lhsValue.ToString().CompareTo(rhsValue.ToString());
			}

			protected override void RemoveSortCore()
			{
				sortDirection = ListSortDirection.Ascending;
				sortProperty = null;
				isSorted = false;
			}

			public void RemoveSort()
			{
				RemoveSortCore();
			}
		}
		#endregion SortableBindingList


		public CrawlWebPageDialog()
		{
			InitializeComponent();
		}


		public CrawlWebPageDialog(List<CrawlHyperlink> links)
			: this()
		{
			gridView.DataSource = new SortableBindingList<CrawlHyperlink>(links);
		}


		public List<CrawlHyperlink> GetSelectedHyperlinks()
		{
			return ((SortableBindingList<CrawlHyperlink>)gridView.DataSource)
				.Where(e => e.Selected)
				.ToList();
		}


		private void DirtyStateChanged(object sender, EventArgs e)
		{
			if (gridView.CurrentCell is DataGridViewCheckBoxCell)
			{
				gridView.CommitEdit(DataGridViewDataErrorContexts.Commit);

				var data = (SortableBindingList<CrawlHyperlink>)gridView.DataSource;
				okButton.Enabled = data.Any(d => d.Selected);
			}
		}
	}
}
