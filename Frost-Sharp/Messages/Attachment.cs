using LiteDB;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost_Sharp.Messages {
	[Serializable]
	public class Attachment {

		public enum Types {
			File,
			Board
		}
		public Types Type { get; set; }
		public string PublicKey { get; set; }
		public string PrivateKey { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Size { get; set; }

		public string Access {
			get {
				if (string.IsNullOrEmpty(PublicKey)) {
					return "Public";
				}
				if (string.IsNullOrEmpty(PrivateKey)) {
					return "Read-only";
				}
				return "Read/write";
			}
		}
	}
}
