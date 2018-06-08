using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {
	public class Item : ImageIdentification {

		public static readonly Regex FormatRegex = new Regex(@"^item");

		public static void Register() {
			ImageIdentifier.RegisterIdentifier<Item>("Item", FormatRegex, false);
		}

		public override string OutputDirectory => "Items";
	}
}
