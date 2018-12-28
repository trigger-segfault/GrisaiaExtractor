using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {
	public class StoryCGChibi : ImageIdentification {

		public static readonly Dictionary<string, string> Categories =
			new Dictionary<string, string>()
		{
			{ "op", "Opening" },
			{ "pro", "Prologue" },
			{ "bla", "Main Story" },
			{ "aft", "After Story" },
			{ "mag", "Story" }, // Idol Mahou Shoujo
			{ "ev", "Story" }, // Yuukan
			{ "evt", "Story" }, // Zankou
		};

		/*public static readonly string[] RegexNames = {
			"pro(?!title)",
		};

		public const string NameRegex

		private static string[] GetNames() {
			string[] names = new string[Categories.Count];
			int i = 0;
			foreach (string name in Categories.Keys) {
				names[i] = name + @"";
				foreach (string nameRegex in RegexNames) {
					if (new Regex(nameRegex).IsMatch(name)) {
						names[i] = nameRegex;
						break;
					}
				}
				i++;
			}
			return names;
		}*/


		public static readonly Regex FormatRegex = new Regex(
			$"^(?'chibi'_?_?sd)?(?'category'{string.Join("|", Categories.Keys.ToArray())})(?:[^a-zA-Z]|$)", RegexOptions.IgnoreCase);

		public static void Register() {
			ImageIdentifier.RegisterIdentifier<StoryCGChibi>(
				"CG/Chibi", FormatRegex, false);
		}


		public StoryCGChibi() {

		}

		protected override void Setup(Match match) {
			if (string.IsNullOrEmpty(match.Groups["chibi"].Value))
				Type = "CG";
			else
				Type = "Chibi";
			Code = match.Groups["category"].Value.ToLower();
			Category = Categories[Code];
		}

		public string Type { get; private set; }

		public string Code { get; private set; }
		public string Category { get; private set; }



		public override string OutputDirectory => Path.Combine(Category, Type);
	}
}
