using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Frost_Sharp.GUI {
	static public class MessageListSorter {
		static GridViewColumnHeader SortingColumn;
		static ListSortDirection SortingDirection;
		static ListView listView = MainWindow.GetInstance().MessageList;

		static public void ColumnHeader_Click(object sender, RoutedEventArgs e) {
			GridViewColumnHeader column = sender as GridViewColumnHeader;

			if (SortingColumn != null) {
				listView.Items.SortDescriptions.Clear();
			}

			ListSortDirection NewDirection = ListSortDirection.Ascending;
			if (SortingColumn == column && SortingDirection == NewDirection) {
				NewDirection = ListSortDirection.Descending;
			}

			SetSorting(column, NewDirection);
		}

		static public void SetSorting(GridViewColumnHeader column, ListSortDirection NewDirection) {
			SortingColumn = column;
			SortingDirection = NewDirection;
			listView.Items.SortDescriptions.Add(new SortDescription(column.Tag.ToString(), NewDirection));
		}
	}
}
