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


		[BsonIgnore] public string Date {
			get {
				return Instant.FromSecondsSinceUnixEpoch(Timestamp).InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).ToString("yyyy.MM.dd HH:mm:ss x", null);
				//return Instant.FromSecondsSinceUnixEpoch(Timestamp).InZone(NodaTime.TimeZones.BclDateTimeZone.ForSystemDefault()).ToString("yyyy.MM.dd HH:mm:ss x", null);
				
				//NodaTime.Instant time = InstantPattern.GeneralPattern.Parse(DateTime.ToString("yyyy-MM-dd'T'HH:mm:sszzzz")).Value;
				//
				//return time.InZone(zone).ToString("yyyy.MM.dd HH:mm:ss x",);

				//return DateTime.ToString("yyyy.MM.dd HH:mm:ss x", null);

				//return DateTime.WithZone(NodaTime.TimeZones.BclDateTimeZone.ForSystemDefault()).ToString("yyyy.MM.dd HH:mm:ss x", null);
			}
		}
	}
}
