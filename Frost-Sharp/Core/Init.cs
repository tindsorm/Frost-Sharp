using Frost_Sharp.Utils;
using LiteDB;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Frost_Sharp.Core {
	public class Init {

		public Init() {
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += worker_Init;
			worker.RunWorkerCompleted += worker_InitCompleted;
			worker.RunWorkerAsync();
		}

		private void worker_Init(object sender, DoWorkEventArgs e) {
			new Utils.Logger();
			if (!Directory.Exists(MainWindow.DatabaseFolder)) {
				Directory.CreateDirectory(MainWindow.DatabaseFolder);
			}

			MainWindow.GetInstance().db = new LiteDatabase(MainWindow.DatabasePath);
			MainWindow.GetInstance().boards = MainWindow.GetInstance().db.GetCollection<Boards.Board>("boards");

			Settings.Init();

			// Add default boards
			// TODO: These should come from an xml file
			//if (Settings.isFirstRun) {
			if (Settings.isFirstRun) {
				Log.I("INIT", "isFirstRun: true");
				Log.I("INIT", "Adding default boards");
				if (MainWindow.GetInstance().boards.FindOne(x => x.Name == "freenet") == null) {
					MainWindow.GetInstance().boards.Insert(new Boards.Board() {
						Name = "freenet",
						Description = "Discussions about freenet"
					});
				}

				if (MainWindow.GetInstance().boards.FindOne(x => x.Name == "frost") == null) {
					MainWindow.GetInstance().boards.Insert(new Boards.Board() { Name = "frost" });
				}

				if (MainWindow.GetInstance().boards.FindOne(x => x.Name == "test") == null) {
					MainWindow.GetInstance().boards.Insert(new Boards.Board() {
						Name = "test",
						Description = "The test board"
					});
				}

				Settings.isFirstRun = false;
			}
		}

		private void worker_InitCompleted(object sender, RunWorkerCompletedEventArgs e) {
			// Set up Click handler for message list columns
			// Can't use a static class in XAML for this
			foreach (GridViewColumn column in MainWindow.GetInstance().MessageListGridView.Columns) {
				(column.Header as GridViewColumnHeader).Click += GUI.MessageListSorter.ColumnHeader_Click;
			}

			// GridSplitter is ignoring the MinWidth of the column to the right of it...
			MainWindow.GetInstance().MainFrame.SizeChanged += (_s, _e) => {
				Grid g = _s as Grid;
				double maxWidth = g.ActualWidth - g.ColumnDefinitions[1].Width.Value - g.ColumnDefinitions[2].MinWidth;
				g.ColumnDefinitions[0].MaxWidth = maxWidth;
			};
			if (!Double.IsNaN(Settings.BoardTreeWidth)) {
				double MaxPossibleWidth = MainWindow.GetInstance().Width - MainWindow.GetInstance().MainFrame.ColumnDefinitions[1].Width.Value - MainWindow.GetInstance().MainFrame.ColumnDefinitions[2].MinWidth;
				MainWindow.GetInstance().MainFrame.ColumnDefinitions[0].Width = new GridLength(Math.Min(Settings.BoardTreeWidth, MaxPossibleWidth), GridUnitType.Pixel);
			} else {
				MainWindow.GetInstance().MainFrame.ColumnDefinitions[0].Width = new GridLength(MainWindow.GetInstance().Width / 8, GridUnitType.Pixel);
			}

			// GridSplitter is ignoring the MinHeight of the row below it...
			MainWindow.GetInstance().MessagePane.SizeChanged += (_s, _e) => {
				Grid g = _s as Grid;
				double maxHeight = g.ActualHeight - g.RowDefinitions[0].Height.Value - g.RowDefinitions[1].MinHeight;
				g.RowDefinitions[0].MaxHeight = maxHeight;
			};
			if (!Double.IsNaN(Settings.MessageListHeight) && Settings.MessageListHeight < MainWindow.GetInstance().Height) {
				MainWindow.GetInstance().MessageGrid.RowDefinitions[0].Height = new GridLength(Settings.MessageListHeight, GridUnitType.Pixel);
			}

			// Fill the board tree
			// TODO: We need to support folders
			/* ex: Root
					┣ Freenet
					┃	┃ Freenet
					┃	┃ Frost
					┗Foo
						┃ FooBoard
						┗ Bar
							┃ Baz
			*/

			Boards.Tree.Init(MainWindow.GetInstance());
			foreach (Boards.Board b in MainWindow.GetInstance().boards.FindAll()) {
				Boards.Tree.AddBoard(b);
			}

			if (Settings.isSplashEnabled) {
				GUI.Splash Splash = GUI.Splash.GetInstance();
				Splash.splashFinished = true;
				Splash.Close();
			} else {
				MainWindow.GetInstance().Show();
			}
		}
	}
}
