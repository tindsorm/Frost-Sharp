using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Frost_Sharp.Utils {
	public class Logger {
		private const string LOG_DIR = "logs";
		private static string newLog = Path.Combine(LOG_DIR, "Frost_Sharp.log");
		private static string oldLog = Path.Combine(LOG_DIR, "Frost_Sharp.log.old");
		private static StreamWriter logFile;
		public enum LogLevel {
			None,
			Critical,
			Error,
			Warning,
			Notice,
			Info,
			Debug
		}

		static internal void Write(LogLevel level, string tag, string msg) {
			if (level > Settings.LogLevel) {
				return;
			}
			string line = string.Format("[ {0} ] [ {1,-8} ] [ {2,-12} ] {3}", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss UTC"), level.ToString().ToUpper(), tag, msg);
			Debug.WriteLine(line);
			logFile.WriteLine(line);
		}

		public static void closeLog() {
			Log.I("LOGGER", "Closing log file");
			logFile.Flush();
			logFile.Close();
			if (File.Exists(newLog) && new FileInfo(newLog).Length == 0) {
				File.Delete(newLog);  // No point keeping empty log files
			}
		}

		public Logger() {
			if (!Directory.Exists(Path.Combine(MainWindow.MyFolder, LOG_DIR))) {
				Directory.CreateDirectory(LOG_DIR);
			}

			if (File.Exists(newLog)) {
				if (File.Exists(oldLog)) {
					int i = 1;
					string tmp = string.Format("{0}.{1}", oldLog, i);
					while (File.Exists(tmp)) {
						tmp = string.Format("{0}.{1}", oldLog, i);
						i++;
					}
					oldLog = tmp;
				}
				File.Move(newLog, oldLog);
			}
			logFile = File.AppendText(newLog);
			logFile.AutoFlush = true;
			Log.I("STARTUP", "This is Frost-Sharp version " + Assembly.GetEntryAssembly().GetName().Version.ToString());
		}
	}

	public static class Log {
		public static void C(string tag, string msg) {
			Logger.Write(Logger.LogLevel.Critical, tag, msg);
		}

		public static void E(string tag, string msg) {
			Logger.Write(Logger.LogLevel.Error, tag, msg);
		}

		public static void W(string tag, string msg) {
			Logger.Write(Logger.LogLevel.Warning, tag, msg);
		}

		public static void N(string tag, string msg) {
			Logger.Write(Logger.LogLevel.Notice, tag, msg);
		}

		public static void I(string tag, string msg) {
			Logger.Write(Logger.LogLevel.Info, tag, msg);
		}

		public static void D(string tag, string msg) {
			Logger.Write(Logger.LogLevel.Debug, tag, msg);
		}
	}
}
