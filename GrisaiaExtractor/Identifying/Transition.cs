using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {
	public class Transition : ImageIdentification {

		public static readonly Regex FormatRegex =
			new Regex(@"^wipe(?'index'\d\d)(?'subtype'[a-z])?");

		public int Index { get; private set; }
		public string Subtype { get; private set; }

		public static void Register() {
			ImageIdentifier.RegisterIdentifier<Transition>("Transition", FormatRegex, false);
		}

		public Transition() { }

		protected override void Setup(Match match) {
			Index = int.Parse(match.Groups["index"].Value);
			Subtype = match.Groups["subtype"].Value;
		}

		public override string OutputDirectory => "Transitions";
	}
}
