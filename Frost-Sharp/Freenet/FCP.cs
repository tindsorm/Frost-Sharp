using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;

namespace Frost_Sharp.Freenet {
	static class FCP {
		static private Random rand = new Random();

		// Used to download a message
		// returns the data received,
		// or null on failure.
		static public string Get(string key, object s = null) {
			BackgroundWorker sender = null;
			if (s is BackgroundWorker) {
				sender = s as BackgroundWorker;
			}
			//string key = "KSK@frost|message|news|2017.1.1-freenet-0.xml";
			TcpClient sock;
			try {
				sock = new TcpClient(Settings.FCPHost, Settings.FCPPort);
			} catch (SocketException e) {
				Utils.Log.D("FCP", e.ToString());
				MessageBox.Show(e.ToString());
				return null;
			}
			NetworkStream stream = sock.GetStream();
			StreamWriter writer = new StreamWriter(stream, Encoding.ASCII);
			writer.AutoFlush = true;
			StreamReader reader = new StreamReader(stream, Encoding.ASCII);

			string data = null;
			try {
				string msg;

				msg = "";
				msg += "ClientHello\n";
				msg += "Name=SharpFreenet " + UniqueId(4) + "\n";
				msg += "ExpectedVersion=2.0\n";
				msg += "EndMessage\n";
				writer.Write(msg);

				bool NodeHello = false;
				string line;

				while (true) {
					if (sender != null && sender.CancellationPending) {
						throw new OperationCanceledException();
					}
					line = reader.ReadLine();
					if (String.IsNullOrEmpty(line)) {
						Thread.Sleep(10);
					} else {
						if (line.Equals("NodeHello")) {
							NodeHello = true;
						}
						if (line.Equals("EndMessage") && NodeHello) {
							break;
						}
					}
				}

				msg = "";
				msg += "ClientGet\n";
				msg += "URI=" + key + "\n";
				msg += "Identifier=" + key + "\n";
				msg += "EndMessage\n";
				writer.Write(msg);

				bool bAllData = false;
				bool bData = false;
				bool bGetFailed = false;
				int dataLength = -1;
				var sw = Stopwatch.StartNew();
				while (true) {
					if (sender != null && sender.CancellationPending) {
						throw new OperationCanceledException();
					}
					line = reader.ReadLine();
					if (!String.IsNullOrEmpty(line)) {
						if (line.Equals("GetFailed")) {
							bGetFailed = true;
						}
						if (bGetFailed) {
							if (line.Equals("EndMessage")) {
								Utils.Log.I("FCP", "Get failed");
								break;
							}
							continue;
						}
						if (!bAllData) {
							if (line.Equals("AllData")) {
								bAllData = true;
							}
						} else {
							if (line.StartsWith("DataLength=")) {
								dataLength = Convert.ToInt32(line.Substring(11));
							}
							if (line.Equals("Data") && dataLength != -1) {
								bData = true;
							}
							if (bData) {
								char[] buf = new char[dataLength];
								reader.Read(buf, 0, dataLength);
								data = new string(buf);
								break;
							}
						}
					} else {
						if (sw.ElapsedMilliseconds >= 120000) {
							Utils.Log.D("FCP", "Timeout Elapsed, giving up");
							break;
						}
						Thread.Sleep(10);
					}
				}
				// Give up after 2 minutes
				sw.Stop();
			} catch (OperationCanceledException) { }

			reader.Close();
			writer.Close();
			sock.Close();
			return data;
		}

		static public string UniqueId(int length = 16) {
			byte[] buf = new byte[length];
			rand.NextBytes(buf);
			int id = BitConverter.ToInt32(buf, 0);
			if (id < 0) {
				id *= -1;
			}
			return id.ToString().Substring(0, length);
		}
	}
}