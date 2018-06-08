using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Identifying {
	/// <summary>The base class for an image identification.</summary>
	public abstract class ImageIdentification {

		// General:
		/// <summary>The base filename with no extension.</summary>
		public string FileName { get; private set; }

		// Animation:
		/// <summary>True if a filename exists without the animation postfix.</summary>
		public bool HasBase { get; private set; }
		/// <summary>Gets the first A frame in the animation. -1 if not an animation.</summary>
		public int FirstFrameA { get; private set; } = -1;
		/// <summary>Gets the first B frame in the animation. -1 if not an animation.</summary>
		public int FirstFrameB { get; private set; } = -1;
		/// <summary>Gets the last A frame in the animation. -1 if not an animation.</summary>
		public int LastFrameA { get; private set; } = -1;
		/// <summary>Gets the last B frame in the animation. -1 if not an animation.</summary>
		public int LastFrameB { get; private set; } = -1;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		/// <summary>Constructs the base image identification.</summary>
		public ImageIdentification() { }

		/// <summary>Initializes the image identification from the filepath.</summary>
		public void Initialize(string path, Match match) {
			FileName = AnimationHelper.GetBaseFileName(path);

			string[] fileNames = AnimationHelper.GetFileNames(path);
			InitializeFileNames(fileNames);
			Setup(match);
		}

		/// <summary>Initializes the image identification from the filepath.</summary>
		public void Initialize(string[] paths, Match match) {
			FileName = AnimationHelper.GetBaseFileName(paths[0]);

			string[] fileNames = paths.Select(p =>
				Path.GetFileNameWithoutExtension(p)).ToArray();
			InitializeFileNames(fileNames);
			Setup(match);
		}

		/// <summary>Sets up the basic identification information.</summary>
		private void InitializeFileNames(string[] fileNames) {
			// Gather information about the animation:
			// Check the first two files for animations
			int indexA, indexB;
			for (int i = 0; i < 2 && i + 1 < fileNames.Length; i++) {
				if (AnimationHelper.IsAnimation(fileNames[i], out indexA, out indexB)) {
					FirstFrameA = indexA;
					FirstFrameB = indexB;
					// Get the last animation frames
					AnimationHelper.IsAnimation(fileNames[fileNames.Length - 1], out indexA, out indexB);
					LastFrameA = indexA;
					LastFrameB = indexB;
					break;
				}
				else {
					// This should only ever be reached when i == 0
					HasBase = true;
				}
			}
		}

		/// <summary>Sets up the extended image identification.</summary>
		protected virtual void Setup(Match match) { }


		//-----------------------------------------------------------------------------
		// General
		//-----------------------------------------------------------------------------

		/// <summary>Creates a string representation of the identification as a
		/// filename and tags.</summary>
		public override string ToString() {
			string name = FileName;
			if (IsAnimated)
				return name += $" (+{TotalFrameCount})";
			name += $" {string.Join(",", Tags.ToArray())}";
			return name;
		}

		/// <summary>A quick method to create regex from a list of prefixes.</summary>
		protected static Regex PrefixesToRegex(string[] prefixes)
			=> new Regex($"^({string.Join("|", prefixes)})");


		//-----------------------------------------------------------------------------
		// File Paths
		//-----------------------------------------------------------------------------

		/// <summary>Gets the base .png filepath.</summary>
		public string GetPng(string dir) {
			return Path.Combine(dir, OutputDirectory, FileName + ".png");
		}

		/// <summary>Gets the animation .png filepath.</summary>
		public string GetPng(string dir, int indexA, int indexB) {
			return AnimationHelper.GetFileNameQuick(Path.Combine(dir, OutputDirectory), FileName, indexA, indexB, ".png");
		}

		/// <summary>Gets all .png filepaths for the image.</summary>
		public IEnumerable<string> GetPngs(string dir) {
			if (HasBase)
				yield return GetPng(dir);
			for (int a = FirstFrameA; a <= LastFrameA; a++) {
				for (int b = FirstFrameB; b <= LastFrameB; b++) {
					yield return GetPng(dir, a, b);
				}
			}
		}

		/// <summary>Gets the .bmp filepath.</summary>
		public string GetBmp(string dir) {
			return Path.Combine(dir, FileName + ".bmp");
		}

		/// <summary>Gets the animation .bmp filepath.</summary>
		public string GetBmp(string dir, int indexA, int indexB) {
			return AnimationHelper.GetFileNameQuick(dir, FileName, indexA, indexB, ".bmp");
		}

		/// <summary>Gets all .bmp filepaths for the image.</summary>
		public IEnumerable<string> GetBmps(string dir) {
			if (HasBase)
				yield return GetBmp(dir);
			for (int a = FirstFrameA; a <= LastFrameA; a++) {
				for (int b = FirstFrameB; b <= LastFrameB; b++) {
					yield return GetBmp(dir, a, b);
				}
			}
		}

		/// <summary>Gets the .hg3 filepath.</summary>
		public string GetHg3(string dir) {
			return Path.Combine(dir, FileName + ".hg3");
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		/// <summary>Returns true if the image has animation frames.</summary>
		public bool IsAnimated {
			get { return FirstFrameA != -1; }
		}

		/// <summary>Gets the number of A frames in the animation.</summary>
		public int FrameCountA {
			get { return (FirstFrameA == -1 ? 0 : LastFrameA - FirstFrameA + 1); }
		}

		/// <summary>Gets the number of B frames in the animation.</summary>
		public int FrameCountB {
			get { return (FirstFrameB == -1 ? 0 : LastFrameB - FirstFrameB + 1); }
		}

		/// <summary>Gets the total number of frames in the animation.</summary>
		public int TotalFrameCount {
			get { return FrameCountA * FrameCountB; }
		}

		/// <summary>Gets the output directory for the image.</summary>
		public abstract string OutputDirectory { get; }

		/// <summary>Gets the searchable tags for the image.</summary>
		public virtual IEnumerable<string> Tags {
			get { return Enumerable.Empty<string>(); }
		}

		/// <summary>Gets if the image should be expanded to its full size when
		/// extracting.</summary>
		public virtual bool ExpandImage => true;
	}
}
