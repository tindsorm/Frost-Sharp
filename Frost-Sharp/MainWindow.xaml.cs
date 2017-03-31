using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using LiteDB;
using Frost_Sharp.Utils;

namespace Frost_Sharp {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		static readonly public string MyFolder = AppDomain.CurrentDomain.BaseDirectory;
		static readonly public string DatabaseFolder = System.IO.Path.Combine(MyFolder, "databases");
		static readonly public string DatabasePath = System.IO.Path.Combine(DatabaseFolder, "frost-sharp.db");
		static readonly public string ConfigFolder = System.IO.Path.Combine(MyFolder, "config");

		public bool Destroyed = false;

		public LiteDatabase db;
		public LiteCollection<Boards.Board> boards;

		static private MainWindow _instance;
		public MainWindow() {
			InitializeComponent();
			_instance = this;
			this.Closing += MainWindow_Closing;
			this.Hide(); // We'll show window again after we finish initializing
			GUI.Splash Splash = new GUI.Splash();
			Splash.Show();

			new Core.Init(); // All initialization is done here
			setupKeyBindings();
		}

		static public MainWindow GetInstance() {
			return _instance;
		}

		private void Menu_File_Exit_Click(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e) {
			MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (result == MessageBoxResult.No) {
				e.Cancel = true;
			}

			Log.I("MAIN", "Frost-Sharp is shutting down");
			Destroyed = true;

			// TODO: Tell all background threads to cancel and wait
			db.Dispose();
			Settings.Save();
			Utils.Logger.closeLog();
		}

		private void Menu_Tools_Preferences_Click(object sender, RoutedEventArgs e) {
			e.Handled = true;
			MessageBox.Show("Preferences dialog goes here");
		}

		private void setupKeyBindings() {
			KeyGesture PreferencesKeyGesture = new KeyGesture(
				Key.P,
				ModifierKeys.Control);

			MyCommand PreferencesCmd = new MyCommand(() => { Menu_Tools_Preferences_Click(this, new RoutedEventArgs()); });


			KeyBinding PreferencesCmdKeybinding = new KeyBinding(
							PreferencesCmd,
							PreferencesKeyGesture);

			this.InputBindings.Add(PreferencesCmdKeybinding);
		}

		private void BoardTree_GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
			e.Handled = true;
			if (!e.Canceled) {
				Settings.BoardTreeWidth = MainFrame.ColumnDefinitions[0].Width.Value;
			}
		}

		private void MessageGrid_GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
			e.Handled = true;
			if (!e.Canceled) {
				Settings.MessageListHeight = MessageGrid.RowDefinitions[0].Height.Value;
			}
		}

		private void MessageList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			e.Handled = true;
			var s = sender as ListView;
			if (s.SelectedItems.Count == 1 && s.SelectedItems[0] is Messages.Message) {
				var message = s.SelectedItems[0] as Messages.Message;
				MessageBody.Text = message.Body;
				MessageHeaderContainer.Visibility = Visibility.Visible;
				MessageHeaderFrom.Text = message.Sender;
				MessageHeaderSubject.Text = message.Subject;

				MessageAttachmentsBoards.Items.Clear();
				MessageAttachmentsFiles.Items.Clear();

				if (message.Attachments != null && message.Attachments.Count > 0) {
					MessageAttachmentsPane.Visibility = Visibility.Visible;
					foreach (Messages.Attachment attachment in message.Attachments) {
						switch (attachment.Type) {
							case Messages.Attachment.Types.Board:
								MessageAttachmentsBoards.Items.Add(attachment);
								break;
							case Messages.Attachment.Types.File:
								MessageAttachmentsFiles.Items.Add(attachment);
								break;
						}
					}

					if (MessageAttachmentsBoards.HasItems) {
						MessageAttachmentsBoards.Visibility = Visibility.Visible;
					}

					if (MessageAttachmentsFiles.HasItems) {
						MessageAttachmentsFiles.Visibility = Visibility.Visible;
					}
				} else {
					MessageAttachmentsPane.Visibility = Visibility.Collapsed;
					MessageAttachmentsBoards.Visibility = Visibility.Collapsed;
					MessageAttachmentsFiles.Visibility = Visibility.Collapsed;
				}
			} else {
				MessageBody.Text = "";
				MessageHeaderFrom.Text = "";
				MessageHeaderSubject.Text = "";
				MessageAttachmentsPane.Visibility = Visibility.Collapsed;
				MessageAttachmentsBoards.Visibility = Visibility.Collapsed;
				MessageAttachmentsFiles.Visibility = Visibility.Collapsed;
				MessageHeaderContainer.Visibility = Visibility.Collapsed;
			}
		}
	}
}