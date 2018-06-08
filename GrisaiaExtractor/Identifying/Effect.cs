using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {
	public class Effect : ImageIdentification {
		public static readonly string[] Prefixes = {
			"anm_",
			"bom_",
			"glass_anim",
			"manpu_",
			"mask_",
			"parts_",
			"slash",
			"yuge",
		};

		public static readonly Regex FormatRegex = PrefixesToRegex(Prefixes);

		public static void Register() {
			ImageIdentifier.RegisterIdentifier<Effect>(
				"Effect", FormatRegex, false);
		}

		public Effect() { }

		public override string OutputDirectory => "Effects";
	}
}
