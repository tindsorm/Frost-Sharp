using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Frost_Sharp.Utils {
	public static class Utils {
		static private string[] invalidChars = { "/", "\\", "?", "*", "<", ">", "\"", ":", "|" };
		static public string sanitizeFilename(string name) {
			string ret = string.Empty;

			if (name.StartsWith(".")) {
				ret += "_";
			}

			ret += name;

			foreach (string c in invalidChars) {
				ret.Replace(c, "_");
			}

			return ret.ToLower();
		}

		static public string SHA256Sum(string content) {
			SHA256 sha256 = SHA256.Create();
			byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < data.Length; i++) {
				sb.Append(data[i].ToString("x2"));
			}

			return sb.ToString(); ;
		}

		static public MemoryStream StringAsStream(string content) {
			MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
			ms.Seek(0, SeekOrigin.Begin);
			return ms;
		}

		static public string FormatSize(double bytes) {
			string[] suffixes = { "B", "kB", "MB", "GB", "TB" };
			int pos = 0;
			while (bytes >= 1000 && pos < suffixes.Length - 1) {
				pos++;
				bytes /= 1024;
			}
			return string.Format("{0:0.##} {1}", bytes, suffixes[pos]);
		}
	}
}
