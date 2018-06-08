using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {

	[Flags]
	public enum BackgroundFlags {

		[Name("Default"), Code("")]
		Default = 0,

		// Special
		[Name("Alternate"), Code("t")]
		[Description("Some alternates are no different from default")]
		Alternate = (1 << 0),

		// Coloration
		[Name("Dark"), Code("d")]
		Dark = (1 << 1),
		[Name("Evening"), Code("e")]
		Evening = (1 << 2),
		[Name("Night"), Code("n")]
		[Description("Dark but with lights on")]
		Night = (1 << 3),
		[Name("Sepia"), Code("s")]
		Sepia = (1 << 4),

		// Weather
		[Name("Cloudy"), Code("c")]
		Cloudy = (1 << 5),
		[Name("Rain"), Code("r")]
		Rain = (1 << 6),
		[Name("Heavy Rain"), Code("r2")]
		HeavyRain = (1 << 7),

		// Meta
		[Name("Large"), Code("L")]
		Large = (1 << 8),
	}

	public enum BackgroundScale {
		[Name("Full"), Code("")]
		Full = 0,

		[Name("Large"), Code("l")]
		Large,
		[Name("Medium"), Code("m")]
		Medium,
	}

	[Flags]
	public enum BackgroundOffset {
		[Name("No Offset"), Code("")]
		NoOffset = 0,

		[Name("Center"), Code("c")]
		Center = (1 << 0),

		[Name("Left"), Code("l")]
		Left = (1 << 1),

		[Name("Right"), Code("r")]
		Right = (1 << 2),

		[Name("Up"), Code("u")]
		Up = (1 << 3),

		[Name("Down"), Code("d")]
		Down = (1 << 4),
	}

	public class BackgroundI : ImageIdentification {

		public static readonly Regex FormatRegex =
			new Regex(@"^(bgi_|bgmitei)(?'name'.*)");

		public string Name { get; private set; }

		public static void Register() {
			ImageIdentifier.RegisterIdentifier<BackgroundI>(
				"BackgroundI", FormatRegex, false);
		}

		public BackgroundI() { }

		protected override void Setup(Match match) {
			Name = match.Groups["name"].Value;
		}

		public override string OutputDirectory => "Backgrounds";
	}

	public class MiscBackground : ImageIdentification {

		public static readonly string[] Prefixes = {
			"bg_etc",
			"bgdave",
			"b‚‡", // See: Grisaia no Meikyuu 'b‚‡62t.png'
		};

		public static readonly Regex FormatRegex = PrefixesToRegex(Prefixes);

		public static void Register() {
			ImageIdentifier.RegisterIdentifier<MiscBackground>(
				"Misc Backgrounds", FormatRegex, false);
		}

		public override string OutputDirectory => "Backgrounds";

	}
	
	public abstract class BackgroundBase : ImageIdentification {

		/// <summary>The index ID of the background.</summary>
		public int Index { get; private set; }
		/// <summary>The flags describing the background.</summary>
		public BackgroundFlags Flags { get; private set; }
		/// <summary>The unidentified background flags.</summary>
		public string UnknownFlags { get; private set; }

		public BackgroundBase() { }
		
		/// <summary>Sets up the background base identification information.</summary>
		protected override void Setup(Match match) {
			Index = int.Parse(match.Groups["index"].Value);

			Flags = AttributeHelper.ParseCode<BackgroundFlags>(
				match.Groups["flags"].Value, out string unknownFlags);
			UnknownFlags = unknownFlags;
		}
	}

	public class Background : BackgroundBase {
		public static readonly Regex FormatRegex =
			new Regex(@"^bg(e|r)?(?'index'\d\d)(?'flags'[a-zA-Z0-9]*)(?:_(?'scale'[a-zA-Z0-9])(?'offset'[a-zA-Z0-9])?)?(?'offsetIndex'\d\d)?(?'leftover'.*)?$");
		
		public static void Register() {
			ImageIdentifier.RegisterIdentifier<Background>(
				"Background", FormatRegex, false);
		}

		public BackgroundScale Scale { get; private set; }
		public string UnknownScale { get; private set; }
		public BackgroundOffset Offset { get; private set; }
		public string UnknownOffset { get; private set; }
		public List<BackgroundSpecialAnimation> SpecialAnimations { get; }

		public Background() { }

		/// <summary>Sets up the background identification information.</summary>
		protected override void Setup(Match match) {
			Scale = AttributeHelper.ParseCode<BackgroundScale>(
				match.Groups["scale"].Value, out string unknownScale);
			UnknownScale = unknownScale;

			Offset = AttributeHelper.ParseCode<BackgroundOffset>(
				match.Groups["offset"].Value, out string unknownOffset);
			UnknownOffset = unknownOffset;
		}

		public override string OutputDirectory => "Backgrounds";
		public override IEnumerable<string> Tags {
			get {
				List<string> tags = new List<string>();
				tags.Add(Index.ToString());
				tags.AddRange(AttributeHelper.GetNames(Flags));
				tags.AddRange(AttributeHelper.GetNames(Scale));
				tags.AddRange(AttributeHelper.GetNames(Offset));
				return tags;
			}
		}
	}

	public class BackgroundSpecialAnimation : BackgroundBase {
		public static readonly Regex FormatRegex =
			new Regex(@"^(?'parent'bg(e|r)?(?'index'\d\d)(?'flags'[a-zA-Z0-9]*))_(?'name'[a-zA-Z0-9]+)$");

		public static void Register() {
			ImageIdentifier.RegisterIdentifier<BackgroundSpecialAnimation>(
				"Background Special Animation", FormatRegex, true,
				PostAdd);
		}

		public string AnimationName { get; private set; }
		public string Parent { get; private set; }

		public BackgroundSpecialAnimation() { }

		/// <summary>Sets up the background identification information.</summary>
		protected override void Setup(Match match) {
			AnimationName = match.Groups["name"].Value;
			Parent = match.Groups["parent"].Value;
		}

		public static void PostAdd(ImageIdentifier identifier, ImageIdentification selfBase) {
			BackgroundSpecialAnimation self = (BackgroundSpecialAnimation) selfBase;
			if (identifier.TryGetImage(self.AnimationName, out var image)) {
				if (image is Background bg) {
					bg.SpecialAnimations.Add(self);
				}
			}
		}
		public override string OutputDirectory => "Backgrounds";
		public override IEnumerable<string> Tags {
			get {
				List<string> tags = new List<string>();
				tags.Add(Index.ToString());
				tags.AddRange(AttributeHelper.GetNames(Flags));
				tags.Add(AnimationName);
				return tags;
			}
		}
	}
}
