using LiteDB;
using System;

namespace Frost_Sharp.Boards {
	public class Board {
		public int Id { get; set; }
		[BsonIndex] public string Name { get; set; }
		public string pubKey { get; set; }
		public string privKey { get; set; }
		public string Description { get; set; }
		public int ParentFolder { get; set; }
		public Settings Settings { get; set; } = new Settings();

		public string internalName {
			get {
				return Utils.Utils.sanitizeFilename(Name);
			}
		}
	}
	public class Settings {
		public bool? AutomaticUpdate { get; set; }
		public int? DownloadDaysBackwards { get; set; }
		public DateTime? LastFullDownload { get; set; }
		public DateTime? LastDayDownloaded { get; set; }

		[BsonIgnore]
		public bool isUpdating = false;
		[BsonIgnore]
		public string BoardUpdateProgress { get; set; }
	}
}
