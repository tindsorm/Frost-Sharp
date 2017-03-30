using System;
using System.IO;
using System.Xml.Linq;
using Frost_Sharp.Utils;
using System.Reflection;

namespace Frost_Sharp {
	static public class Settings {
		static private string ConfigPath = MainWindow.ConfigFolder + @"\frost-sharp.xml";
		static private XElement xml;

		// These are our default settings
		static private bool _isFirstRun = true;
		static private bool _isSplashEnabled = true;
		static private string _FCPHost = "127.0.0.1";
		static private ushort _FCPPort = 9481;
		static private string _FProxyHost = "127.0.0.1";
		static private ushort _FProxyPort = 8888;
		static private double _BoardTreeWidth = double.NaN;
		static private double _MessageListHeight = double.NaN;
		static private bool _AutomaticBoardUpdate = true;
		static private uint _DaysDownloadBackwards = 60;
#if DEBUG
		static private Logger.LogLevel _LogLevel = Logger.LogLevel.Debug;
#else
		static private Logger.LogLevel _LogLevel = Logger.LogLevel.Error;
#endif

		static public bool isFirstRun {
			get { return _isFirstRun; }
			set {
				_isFirstRun = value;
				Set("isFirstRun", _isFirstRun);
			}
		}

		static public bool isSplashEnabled {
			get { return _isSplashEnabled; }
			set {
				_isSplashEnabled = value;
				Set("isSplashEnabled", _isSplashEnabled);
			}
		}

		static public string FCPHost {
			get { return _FCPHost; }
			set {
				_FCPHost = value;
				Set("FCPHost", _FCPHost);
			}
		}

		static public ushort FCPPort {
			get { return _FCPPort; }
			set {
				_FCPPort = value;
				Set("FCPPort", _FCPPort);
			}
		}

		static public string FProxyHost {
			get { return _FProxyHost; }
			set {
				_FProxyHost = value;
				Set("FProxyHost", _FProxyHost);
			}
		}

		static public ushort FProxyPort {
			get { return _FProxyPort; }
			set {
				_FProxyPort = value;
				Set("FProxyPort", _FProxyPort);
			}
		}

		static public double BoardTreeWidth {
			get { return _BoardTreeWidth; }
			set {
				_BoardTreeWidth = value;
				Set("BoardTreeWidth", _BoardTreeWidth);
			}
		}

		static public double MessageListHeight {
			get { return _MessageListHeight; }
			set {
				_MessageListHeight = value;
				Set("MessageListHeight", _MessageListHeight);
			}
		}

		static public bool AutomaticBoardUpdate {
			get { return _AutomaticBoardUpdate; }
			set {
				_AutomaticBoardUpdate = value;
				Set("AutomaticBoardUpdate", _AutomaticBoardUpdate);
			}
		}

		static public uint DaysDownloadBackwards {
			get { return _DaysDownloadBackwards; }
			set {
				_DaysDownloadBackwards = value;
				Set("DaysDownloadBackwards", _DaysDownloadBackwards);
			}
		}

		static public Logger.LogLevel LogLevel {
			get { return _LogLevel; }
			set {
				_LogLevel = value;
				Set("LogLevel", _LogLevel);
			}
		}
		static public void Init() {
			if (!Directory.Exists(MainWindow.ConfigFolder)) {
				Directory.CreateDirectory(MainWindow.ConfigFolder);
			}

			Load();

		}

		static public void Save() {
			xml.Save(ConfigPath);
		}

		static public void Load() {
			try {
				xml = XElement.Load(ConfigPath);
				// We don't care about missing options, we'll just use the default
				Log.D("SETTINGS", "Loading settings");
				try { isFirstRun = (bool)xml.Element("isFirstRun"); } catch { }
				try { isSplashEnabled = (bool)xml.Element("isSplashEnabled"); } catch { }

				try { FCPHost = xml.Element("FCPHost").Value; } catch { }
				try { FCPPort = Convert.ToUInt16(xml.Element("FCPPort").Value); } catch { }

				try { FProxyHost = xml.Element("FProxyHost").Value; } catch { }
				try { FProxyPort = Convert.ToUInt16(xml.Element("FProxyPort").Value); } catch { }

				try { BoardTreeWidth = (double)xml.Element("BoardTreeWidth"); } catch { }
				try { MessageListHeight = (double)xml.Element("MessageListHeight"); } catch { }

				try { AutomaticBoardUpdate = (bool)xml.Element("AutomaticBoardUpdate"); } catch { }
				try { DaysDownloadBackwards = (uint)xml.Element("DaysDownloadBackwards"); } catch { }

				try { LogLevel = (Logger.LogLevel)Enum.Parse(typeof(Logger.LogLevel), (string)xml.Element("LogLevel")); } catch { }


				foreach (PropertyInfo property in typeof(Settings).GetProperties()) {
					try {
						Log.D("SETTINGS", string.Format("{0}={1}", property.Name, property.GetValue(typeof(Settings), null)));
					} catch (Exception e) {
						Log.W("SETTINGS", e.ToString());
					}
				}
			} catch (FileNotFoundException) {
				// No settings file, so we'll just create an empty one
				xml = new XElement("Settings");
			}
		}

		static private void Set(string name, object content) {
			Log.D("SETTINGS", string.Format("Set: {0}={1}", name, content.ToString()));
			XElement elem = xml.Element(name);
			if (elem == null) {
				xml.Add(new XElement(name, content));
			} else {
				elem.Value = content.ToString();
			}
		}
	}
}
