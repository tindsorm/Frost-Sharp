using System.ComponentModel;
using System.Windows.Controls;

namespace Frost_Sharp.Boards {
	public class BoardTreeViewItem : TreeViewItem {
		public Board Board { set; get; }
		public Types Type { set; get; }

		public BackgroundWorker UpdateThread { set; get; }
		public BoardTreeViewItem() {
			this.Selected += Tree.treeView_Event_Selected;
		}
	}
}
