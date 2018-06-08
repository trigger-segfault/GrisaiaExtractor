using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {
	public class Logo : ImageIdentification {

		public static readonly string[] Prefixes = {
			"10thlogo",
			"fwlogo",
			"install",
			"logo",
			"sekai_logo",
			"sys_title",
			"title",
		};

		public static readonly Regex FormatRegex = PrefixesToRegex(Prefixes);


		public static void Register() {
			ImageIdentifier.RegisterIdentifier<Logo>("Logo", FormatRegex, false);
		}

		public Logo() {

		}

		public override string OutputDirectory => "Logos";
		public override bool ExpandImage => false;
	}
}
