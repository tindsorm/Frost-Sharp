using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Frost_Sharp.Boards {
	static public class Tree {
		static private MainWindow MainWindow;
		static private TreeView treeView;
		static private ContextMenu contextMenu;
		static public void Init(MainWindow mW) {
			MainWindow = mW;
			treeView = MainWindow.treeView;

			// Create context menu
			contextMenu = new ContextMenu();

			// Update board
			MenuItem updateItem = new MenuItem();
			updateItem.Header = "Update Board";
			updateItem.Click += treeView_Menu_Update_Board_Click;
			contextMenu.Items.Add(updateItem);

			// Remove board
			MenuItem removeItem = new MenuItem();
			removeItem.Header = "Remove";
			removeItem.Click += treeView_Menu_Remove_Click;
			contextMenu.Items.Add(removeItem);



			// Clear treeView first
			treeView.Items.Clear();

			// Root node
			BoardTreeViewItem root = new BoardTreeViewItem();
			root.Header = BoardTreeItem("Frost", "mail.png");
			root.Type = Types.Folder;
			root.IsExpanded = true;
			root.Collapsed += treeView_Event_Collapse_Prevent;

			BoardTreeViewItem outbox = new BoardTreeViewItem();
			outbox.Header = BoardTreeItem("[Outbox]");
			outbox.Type = Types.Outbox;
			root.Items.Add(outbox);

			BoardTreeViewItem sent = new BoardTreeViewItem();
			sent.Header = BoardTreeItem("[Sent]");
			sent.Type = Types.Sent;
			root.Items.Add(sent);

			treeView.Items.Add(root);
		}


		static private object BoardTreeItem(string Header, string ImageSource = null) {
			if (String.IsNullOrEmpty(ImageSource)) {
				return Header;
			}

			StackPanel stack = new StackPanel() { Orientation = Orientation.Horizontal };

			Image img = new Image();
			img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/" + ImageSource));
			img.Margin = new Thickness(0, 0, 2, 0);

			TextBlock text = new TextBlock() { Text = Header };

			stack.Children.Add(img);
			stack.Children.Add(text);

			return stack;
		}

		static public void AddBoard(Board board) {
			BoardTreeViewItem item = new BoardTreeViewItem();
			item.Header = BoardTreeItem(board.Name); //, "board.png");
			item.Board = board;
			if (!string.IsNullOrEmpty(board.Description)) {
				item.ToolTip = new TextBlock() { Text = board.Description };
			}
			item.Type = Types.Board;
			(treeView.Items[0] as BoardTreeViewItem).Items.Add(item);

		}

		static public void treeView_Event_CollapseExpand(object sender, RoutedEventArgs e) {
			BoardTreeViewItem source = e.Source as BoardTreeViewItem;

			if (source.Header is StackPanel) {
				StackPanel stack = source.Header as StackPanel;
				Image img = stack.Children[0] as Image;
				if (source.IsExpanded) {
					img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/folder-open.png"));
				} else {
					img.Source = new BitmapImage(new Uri("pack://application:,,,/Images/folder.png"));
				}
			}
			e.Handled = true;
		}
		static public void treeView_Event_Collapse_Prevent(object sender, RoutedEventArgs e) {
			BoardTreeViewItem source = e.Source as BoardTreeViewItem;
			source.IsExpanded = true;
			e.Handled = true;
		}
		static public void treeView_Event_Selected(object sender, RoutedEventArgs e) {
			e.Handled = true;
			BoardTreeViewItem source = e.Source as BoardTreeViewItem;
			if (source == treeView.Items[0]) {
				treeView.ContextMenu = null;
			} else {
				treeView.ContextMenu = contextMenu;
			}

			if ((source.Type & Types.Board) != 0 && source.Board is Board) {
				if (source.Board.Settings.isUpdating) {
					MainWindow.BoardUpdateProgress.Text = source.Board.Settings.BoardUpdateProgress;
				} else {
					MainWindow.BoardUpdateProgress.Text = "";
				}

				var messages = MainWindow.db.GetCollection<Messages.Message>("messages_" + source.Board.internalName);
				MainWindow.MessageList.Items.Clear();

				if (messages != null) {
					var allMessages = messages.FindAll();
					if (allMessages != null) {
						foreach (Messages.Message message in allMessages) {
							MainWindow.MessageList.Items.Add(message);
						}
					}
				}
			}

			/*			switch (source.Type) {
							case Utils.Consts.BOARD_TYPE_FOLDER:
								// Selecting a folder leaves the last selected board visible
								break;
							case Utils.Consts.BOARD_TYPE_OUTBOX:
								treeView.ContextMenu = null;
								break;
							case Utils.Consts.BOARD_TYPE_SENT:
								treeView.ContextMenu = null;
								break;
							default:
								break;
						}*/
		}

		static private void treeView_Menu_Remove_Click(object sender, RoutedEventArgs e) {
			e.Handled = true;
			BoardTreeViewItem item = treeView.SelectedItem as BoardTreeViewItem;
			if (item != null) {
				(item.Parent as TreeViewItem).Items.Remove(item);
				if (item.Board != null) {
					MainWindow.boards.Delete(item.Board.Id);
				}
			}
		}

		static private void treeView_Menu_Update_Board_Click(object sender, RoutedEventArgs e) {
			e.Handled = true;
			BoardTreeViewItem item = treeView.SelectedItem as BoardTreeViewItem;
			if (item != null && (item.Type & Types.Board) != 0) {
				if (item.Board.Settings.isUpdating) {
					if (!item.UpdateThread.CancellationPending) {
						item.UpdateThread.CancelAsync();
					}
				} else {
					item.UpdateThread = new BackgroundWorker();
					item.UpdateThread.WorkerSupportsCancellation = true;
					item.UpdateThread.WorkerReportsProgress = true;
					item.UpdateThread.DoWork += (_s, _e) => {
						item.Board.Settings.isUpdating = true;
						new Updater(MainWindow, item.Board, _s);
					};
					item.UpdateThread.ProgressChanged += (_s, _e) => {
						Dictionary<string, object> state = _e.UserState as Dictionary<string, object>;
						foreach (string key in state.Keys) {
							switch (key) {
								case "message":
									if (item.IsSelected) {
										MainWindow.MessageList.Items.Add(state["message"] as Messages.Message);
									}
									break;
							}
						}
						if (state.ContainsKey("day") && state.ContainsKey("date") && state.ContainsKey("downloadBackwards")) {
							item.Board.Settings.BoardUpdateProgress = string.Format("Downloading day {0} of {1} ({2})", state["day"], state["downloadBackwards"], state["date"]);
							if (item.IsSelected) {
								MainWindow.BoardUpdateProgress.Text = item.Board.Settings.BoardUpdateProgress;
							}
						}
					};
					item.UpdateThread.RunWorkerCompleted += (_s, _e) => {
						item.Board.Settings.isUpdating = false;
						if (item.IsSelected) {
							MainWindow.BoardUpdateProgress.Text = "";
						}
						item.UpdateThread = null;
					};
					item.UpdateThread.RunWorkerAsync();
				}
			}
		}
	}
}
