using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {
	public class UserInterface : ImageIdentification {
		private static readonly string[] Prefixes = {
			"_conf_txt",
			//"10thlogo",
			"activate",
			"award",
			"caution",
			"bgm",
			"cap_mic_change",
			"cg_",
			"cgmode",
			"click",
			"cm001",
			"conf",
			"cursor",
			"dave_select",
			"delt_plane",
			"delta_plane",
			"dl_",
			"eyecatch",
			//"fwlogo",
			"guripe",
			"hamon",
			"half_plane",
			"hist",
			//"install",
			"jumpmes",
			//"logo",
			"moviemode",
			"novel_click",
			"nowloading",
			"progress",
			"s_cnf_",
			"scenemode",
			"scenesel",
			"scnhlp",
			"scnsel",
			"secret",
			"seek",
			//"sekai_logo",
			"sel",
			"scenarioselect",
			"shortcut",
			"sl_",
			"slide_",
			"ss_",
			"sys_(?!title)", // sys_title -> Logo
			//"title",
			"userfont",
			"wpthm",
		};

		public static readonly Regex FormatRegex = PrefixesToRegex(Prefixes);

		public static void Register() {
			ImageIdentifier.RegisterIdentifier<UserInterface>(
				"User Interface", FormatRegex, false);
		}

		public UserInterface() { }

		public override string OutputDirectory => "User Interface";
		public override bool ExpandImage => false;
	}
}
