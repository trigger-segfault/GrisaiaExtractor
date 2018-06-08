using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor {
	/// <summary>A static class for help with animation file names.</summary>
	public static class AnimationHelper {

		/// <summary>Matches an animation and its indexes.</summary>
		public static Regex AnimationRegex = new Regex(@"^(?'name'.*)(?:\+(?'indexA'\d\d\d)\+(?'indexB'\d\d\d))$");

		/// <summary>Gets the filename without extension or animation postfix.</summary>
		public static string GetBaseFileName(string path) {
			return GetBaseFileName(path, out _, out _);
		}

		/// <summary>Gets the filename without extension or animation postfix.</summary>
		public static string GetBaseFileName(string path, out bool isAnimation) {
			string baseName = GetBaseFileName(path, out int indexA, out int indexB);
			isAnimation = indexA != -1 || indexB != -1;
			return baseName;
		}

		/// <summary>Returns true if the file has an animation postfix.</summary>
		public static bool IsAnimation(string path) {
			return AnimationRegex.IsMatch(Path.GetFileNameWithoutExtension(path));
		}
		
		/// <summary>Returns true if the file has an animation postfix and outputs the indexes.</summary>
		public static bool IsAnimation(string path, out int indexA, out int indexB) {
			string fileNameNoExt = Path.GetFileNameWithoutExtension(path);
			Match match = AnimationRegex.Match(fileNameNoExt);
			if (match.Success) {
				indexA = int.Parse(match.Groups["indexA"].Value);
				indexB = int.Parse(match.Groups["indexB"].Value);
				return true;
			}
			indexA = indexB = -1;
			return false;
		}

		/// <summary>Gets the filename without extension or animation postfix.
		/// Outputs the animation indexes if they exist, otherwise, -1.</summary>
		public static string GetBaseFileName(string path, out int indexA, out int indexB) {
			string fileNameNoExt = Path.GetFileNameWithoutExtension(path);
			Match match = AnimationRegex.Match(fileNameNoExt);
			if (match.Success) {
				indexA = int.Parse(match.Groups["indexA"].Value);
				indexB = int.Parse(match.Groups["indexB"].Value);
				return match.Groups["name"].Value;
			}
			indexA = indexB = -1;
			return fileNameNoExt;
		}

		/// <summary>Gets all filenames associated with this file.</summary>
		public static string[] GetFileNames(string path, string ext = null) {
			List<string> files = new List<string>();
			string dir = Path.GetDirectoryName(path);
			string name = GetBaseFileName(path);
			if (ext == null)
				ext = Path.GetExtension(path);

			// Add the base file if one exists
			string file = Path.Combine(dir, name + ext);
			if (File.Exists(file))
				files.Add(file);

			// Look for files that end with +###+###
			// These are mostly used with animations
			// Check both +000+000 and +001+000 as first possible name
			for (int j = 0; ; j++) {
				file = GetFileNameQuick(dir, name, j, 0, ext);
				if (!File.Exists(file)) {
					if (j == 0)
						continue;
					else
						break;
				}

				files.Add(file);

				for (int k = 1; ; k++) {
					file = GetFileNameQuick(dir, name, j, k, ext);
					if (!File.Exists(file))
						break;
					files.Add(file);
				}
			}
			return files.ToArray();
		}

		/// <summary>Gets the filepath with the animation postfix and extension.
		/// Assumes the filename is just a name with no extension</summary>
		public static string GetFileNameQuick(string dir, string name, int indexA, int indexB, string ext = "") {
			return Path.Combine(dir,
				$"{name}" +
				$"+{indexA.ToString("000")}" +
				$"+{indexB.ToString("000")}" +
				$"{ext}");
		}

		/// <summary>Gets the filepath with the animation postfix and extension.</summary>
		public static string GetFileName(string dir, string file, int indexA, int indexB, string ext = "") {
			return Path.Combine(dir,
				$"{Path.GetFileNameWithoutExtension(file)}" +
				$"+{indexA.ToString("000")}" +
				$"+{indexB.ToString("000")}" +
				$"{ext}");
		}

		/// <summary>Gets the filepath with the animation postfix.</summary>
		public static string GetFileName(string filePath, int indexA, int indexB, string ext = "") {
			return 
				$"{Path.ChangeExtension(filePath, null)}" +
				$"+{indexA.ToString("000")}" +
				$"+{indexB.ToString("000")}" +
				$"{ext}";
		}

	}
}
