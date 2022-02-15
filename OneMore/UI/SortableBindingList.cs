//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;


	/// <summary>
	/// Provides a sortable BindingList for automated sorting in a DataGridView...
	/// something that Microsoft should have done!
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class SortableBindingList<T> : BindingList<T> where T : class
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
}
