using LiteDB;
using NodaTime;
using System;
using System.Collections.Generic;

namespace Frost_Sharp.Messages {
	public class Message {
		[BsonIndex] public Guid Id { get; set; }

		public string Sender { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string MessageId { get; set; }
		public string Board { get; set; }
		public string[] InReplyTo { get; set; }

		public IList<Attachment> Attachments { get; set; } = new List<Attachment>();

		//public ZonedDateTime DateTime { get; set; }

		public long Timestamp { get; set; }


		[BsonIgnore] public ZonedDateTime Date {
			get {
				// TODO: Setting to show times in local time or UTC
				// if (Settings.showTimeUtc) {
				return Instant.FromSecondsSinceUnixEpoch(Timestamp).InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault());
				// } else {
				// return Instant.FromSecondsSinceUnixEpoch(Timestamp).InUtc();
			//}
			}
		}
	}
}
