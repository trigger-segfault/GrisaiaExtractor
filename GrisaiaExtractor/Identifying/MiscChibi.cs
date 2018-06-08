using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {

	public class MiscChibi : ImageIdentification {

		public static readonly Regex FormatRegex =
			new Regex(@"^_?_?sd\d\d\d", RegexOptions.IgnoreCase);


		public static void Register() {
			ImageIdentifier.RegisterIdentifier<MiscChibi>(
				"Misc Chibi", FormatRegex, false);
		}

		public override string OutputDirectory => Path.Combine("Story", "Chibi");
	}
}
