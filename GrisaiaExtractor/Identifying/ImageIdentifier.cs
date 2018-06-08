using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {

	/// <summary>A handler for matching and creating an identification.</summary>
	public class ImageIdentifierType {
		// Information:
		/// <summary>The type created from this identifier.</summary>
		public Type Type { get; }
		/// <summary>The name of the type created from this identifier.</summary>
		public string Name { get; }

		// Matching:
		/// <summary>The regex used to match with this type.</summary>
		public Regex Regex { get; }
		/// <summary>True if this type requires an animation.</summary>
		public bool RequiresAnimation { get; }

		// Functions:
		/// <summary>Called after the identification has been created.</summary>
		public Action<ImageIdentifier, ImageIdentification> PostAdd { get; }

		/// <summary>Constructs the identifier type.</summary>
		public ImageIdentifierType(Type type, string name, Regex regex,
			bool requiresAnimation,
			Action<ImageIdentifier, ImageIdentification> postAdd = null)
		{
			Type = type;
			Name = name;
			Regex = regex;
			RequiresAnimation = requiresAnimation;
			PostAdd = postAdd;
		}

		/// <summary>Creates an empty image identification from the type.</summary>
		public ImageIdentification Create() {
			return (ImageIdentification) Activator.CreateInstance(Type);
		}
	}

	/// <summary>Identifies .</summary>
	public class ImageIdentifier {
		
		private static List<ImageIdentifierType> identifiers =
			new List<ImageIdentifierType>();

		static ImageIdentifier() {
			Background.Register();
			MiscBackground.Register();
			BackgroundI.Register();
			//BackgroundSpecialAnimation.Register();
			Effect.Register();
			Logo.Register();
			StoryCGChibi.Register();
			Character.Register();
			MiscChibi.Register();
			UserInterface.Register();
			TmbIcon.Register();
			Transition.Register();
			Item.Register();
		}

		public static void RegisterIdentifier<IType>(
			string name, Regex regex, bool requiresAnimation,
			Action<ImageIdentifier, ImageIdentification> postAdd = null)
			where IType : ImageIdentification
		{
			identifiers.Add(new ImageIdentifierType(
				typeof(IType), name, regex, requiresAnimation, postAdd));
		}


		private Dictionary<string, ImageIdentification> images;

		public ImageIdentifier() {
			images = new Dictionary<string, ImageIdentification>();
		}

		public void AddImage(ImageIdentification image) {
			images.Add(image.FileName, image);
		}

		public bool TryGetImage(string name, out ImageIdentification result) {
			return images.TryGetValue(name, out result);
		}

		public ImageIdentifierType GetIdentifier(string path, out Match match) {
			string name = AnimationHelper.GetBaseFileName(path, out bool isAnimation);
			foreach (ImageIdentifierType identifier in identifiers) {
				if (identifier.RequiresAnimation && !isAnimation)
					continue;
				match = identifier.Regex.Match(name);
				if (match.Success)
					return identifier;
			}
			match = null;
			return null;
		}

		public ImageIdentification PreIdentifyImage(string path) {
			ImageIdentifierType identifier = GetIdentifier(path, out Match match);
			ImageIdentification image = identifier?.Create() ?? new Unidentified();
			image.Initialize(path, match, false);
			//AddImage(image);
			//identifier?.PostAdd?.Invoke(this, image);
			return image;
		}

		public ImageIdentification IdentifyImage(string[] paths) {
			ImageIdentifierType identifier = GetIdentifier(paths[0], out Match match);
			ImageIdentification image = identifier?.Create() ?? new Unidentified();
			image.Initialize(paths, match);
			AddImage(image);
			identifier?.PostAdd?.Invoke(this, image);
			return image;
		}
	}
}
