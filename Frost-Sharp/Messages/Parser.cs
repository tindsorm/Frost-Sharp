using NodaTime;
using NodaTime.Text;
using System;
using System.Xml.Linq;
using Frost_Sharp.Utils;

namespace Frost_Sharp.Messages {
	public class Parser {
		public readonly Message message;
		public Parser(string xmlString) {
			XElement xml;

			try {
				xml = XElement.Parse(xmlString);
			} catch { return; }

			message = new Message();
			try {
				message.Sender = (string)xml.Element("From");
			} catch { }

			try {
				message.Subject = (string)xml.Element("Subject");
			} catch { }

			try {
				message.Body = (string)xml.Element("Body");
			} catch { }

			try {
				string date = (string)xml.Element("Date");
				string time = (string)xml.Element("Time");
				ZonedDateTimePattern pattern = ZonedDateTimePattern.CreateWithCurrentCulture("yyyy.M.d HH:mm:ss z", DateTimeZoneProviders.Bcl);
				ZonedDateTime datetime = pattern.Parse(string.Format("{0} {1}", date, time).Replace("GMT", " UTC")).Value;
				message.Timestamp = Instant.FromDateTimeUtc(datetime.ToDateTimeUtc()).Ticks / NodaConstants.TicksPerSecond;
			} catch (Exception e) {
				Log.E("Messages.Parser", e.ToString());
			}

			try {
				var attachments = xml.Element("AttachmentList");
				if (attachments != null) {
					foreach (XElement attachment in attachments.Elements("Attachment")) {
						switch ((string)attachment.Attribute("type")) {
							case "file":
								var file = attachment.Element("File");
								message.Attachments.Add(new Attachment() {
									Type = Attachment.Types.File,
									Name = (string)file.Element("name"),
									Size = (int)file.Element("size"),
									PublicKey = (string)file.Element("key")
								});
								break;
							case "board":
								message.Attachments.Add(new Attachment() {
									Type = Attachment.Types.Board,
									Name = (string)attachment.Element("Name"),
									Description = (string)attachment.Element("description"),
									PublicKey = (string)attachment.Element("pubKey"),
									PrivateKey = (string)attachment.Element("privKey")
								});
								break;
						}
					}
				}
			} catch (Exception e) {
				Log.E("Messages.Parser", e.ToString());
			}
		}
	}
}
