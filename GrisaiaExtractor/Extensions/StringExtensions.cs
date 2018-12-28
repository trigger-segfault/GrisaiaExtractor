using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Extensions {
	public static partial class StringExtensions {

		/// <summary>Returns true if the strings are equal, allows for ignore case.</summary>
		public static bool Equals2(this string a, string b, bool ignoreCase) {
			return a.Equals(b, ignoreCase ?
				StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		}

		/*public static string Surround(this string str, string open, string close) {

		}*/

		/// <summary>Removes surrounding quotes from a string.</summary>
		public static string RemoveQuotes(this string str) {
			if (str.StartsWith("\"") && str.EndsWith("\"")) {
				if (str.Length >= 2)
					return str.Substring(1, str.Length - 2);
				return "";
			}
			return str;
		}

		public static string ReplaceAt(this string str, int index, string value) {
			return str.Substring(0, index) + value + str.Substring(index + value.Length);
		}

		public static string ReplaceAt(this string str, int index, char c) {
			return str.Substring(0, index) + c + str.Substring(index + 1);
		}

		public static string NullTerminate(this string str) {
			int index = str.IndexOf('\0');
			return (index != -1 ? str.Substring(0, index) : str);
		}

		public static int IndexOfNullTerminator(this string str) {
			int index = str.IndexOf('\0');
			return (index != -1 ? index : str.Length);
		}

		public static int IndexOfNullTerminator(this char[] chars) {
			for (int i = 0; i < chars.Length; i++) {
				if (chars[i] == '\0')
					return i;
			}
			return chars.Length;
		}

		public static int IndexOfNullTerminator(this byte[] chars) {
			for (int i = 0; i < chars.Length; i++) {
				if (chars[i] == '\0')
					return i;
			}
			return chars.Length;
		}

		public static string ToNullTerminatedString(this byte[] chars) {
			return Encoding.ASCII.GetString(chars, 0, chars.IndexOfNullTerminator());
		}
		public static string ToNullTerminatedString(this byte[] chars, Encoding encoding) {
			return encoding.GetString(chars, 0, chars.IndexOfNullTerminator());
		}

		public static string ToNullTerminatedString(this char[] chars) {
			return new string(chars, 0, chars.IndexOfNullTerminator());
		}

		public static string GetNullTerminated(this Encoding encoding, byte[] bytes) {
			return encoding.GetString(bytes).NullTerminate();
		}

		/// <summary>Designed for use with ASCII encoding to easily get a single char
		/// from a single byte.</summary>
		public static char GetChar(this Encoding encoding, byte singleByte) {
			return encoding.GetChars(new byte[] { singleByte })[0];
		}
	}
}
