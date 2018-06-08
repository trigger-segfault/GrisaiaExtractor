using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {
	public class TmbIcon : ImageIdentification {

		public static readonly Regex FormatRegex = new Regex(@"^tmbicon");

		public static void Register() {
			ImageIdentifier.RegisterIdentifier<TmbIcon>("Icon", FormatRegex, false);
		}

		public TmbIcon() { }

		protected override void Setup(Match match) {

		}

		public override string OutputDirectory => "Icons";
	}
}
