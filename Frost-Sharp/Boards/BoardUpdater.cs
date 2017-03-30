using Frost_Sharp.Freenet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Frost_Sharp.Boards {
	class Updater {
		private Board _Board;
		private MainWindow MainWindow;

		public Updater(MainWindow mW, Board board, object s) {
			BackgroundWorker sender = s as BackgroundWorker;
			_Board = board;
			MainWindow = mW;

			DateTime dt = DateTime.UtcNow;

			string boardName = Utils.Utils.sanitizeFilename(board.Name);
			var db = MainWindow.db;

			var messages = db.GetCollection<Messages.Message>("messages_" + boardName);

			int DownloadDaysBackwards = (int)Frost_Sharp.Settings.DaysDownloadBackwards;
			if (board.Settings.DownloadDaysBackwards.HasValue) {
				DownloadDaysBackwards = board.Settings.DownloadDaysBackwards.Value;
			}

			Utils.Log.D("BoardUpdater", "Will attempt to download " + DownloadDaysBackwards.ToString() + " days");
			int failures = 0;
			for (int currentDay = 1; currentDay <= DownloadDaysBackwards; currentDay++) {
				if (sender.CancellationPending) {
					return;
				}

				sender.ReportProgress(0, new Dictionary<string, object>() {
					{ "day", currentDay },
					{ "date", dt.ToString("yyyy/MM/dd") },
					{ "downloadBackwards", DownloadDaysBackwards },
				});

				string date = dt.ToString("yyyy.M.d");
				failures = 0;
				for (int index = 0; ; index++) {
					if (sender.CancellationPending) {
						return;
					}
					// TODO: Handle keyed boards
					string uri = "KSK@frost|message|news|" + date + "-" + boardName + "-" + index.ToString() + ".xml";
					Utils.Log.D("FCP", uri);
					string identifier = Utils.Utils.SHA256Sum(uri);

					// Already in database
					if (sender.CancellationPending) {
						return;
					}
					if (db.FileStorage.FindById(identifier) != null) {
						Utils.Log.D("FCP", "Already in database");
						continue;
					}

					string data = FCP.Get(uri, s);
					if (sender.CancellationPending) {
						return;
					}
					if (string.IsNullOrEmpty(data)) {
						Utils.Log.D("FCP", "No data");
						failures++;
						if (failures >= 5) {
							Utils.Log.D("FCP", "End of day, going back another");
							break;
						} else {
							continue;
						}
					}
					failures = 0;
					Utils.Log.D("FCP", "Downloaded " + date + "-" + index.ToString());

					db.FileStorage.Upload(identifier, identifier, Utils.Utils.StringAsStream(data));

					// TODO: Start background thread to parse message
					BackgroundWorker bw = new BackgroundWorker();
					bw.DoWork += (_s, _e) => {
						var parser = new Messages.Parser(data);
						if (parser.message != null) {
							messages.Insert(parser.message);
							_e.Result = parser.message;
						}
					};
					bw.RunWorkerCompleted += (_s, _e) => {
						sender.ReportProgress(0, new Dictionary<string, object>() {
							{"message", _e.Result as Messages.Message },
						});
					};
					bw.RunWorkerAsync();
				}

				dt = dt.AddDays(-1);
			}
		}
	}
}
