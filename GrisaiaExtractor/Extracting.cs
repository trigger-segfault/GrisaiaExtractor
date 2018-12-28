using GrisaiaExtractor.Asmodean;
using GrisaiaExtractor.Extensions;
using GrisaiaExtractor.Identifying;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GrisaiaExtractor {
	/// <summary>How converted pngs should be categorized into folders.</summary>
	[Flags]
	public enum Hg3Sorting {
		/// <summary>No categorization.</summary>
		None = 0,

		/// <summary>Pngs are saved into a single folder.</summary>
		Unsorted = (1 << 0),
		/// <summary>Pngs are saved into categorized folders.</summary>
		Sorted = (1 << 1),

		/// <summary>Pngs are saved into both a single folder and categorized folders.</summary>
		Both = Sorted | Unsorted,
	}
	
	/// <summary>Arguments for .int extraction callbacks.</summary>
	public struct ExkifintArgs {
		/// <summary>The index of the current file being extracted.</summary>
		public int FileIndex { get; set; }
		/// <summary>The total number of files to extract.</summary>
		public int FileCount { get; set; }
		/// <summary>The name of the file without the extension.</summary>
		public string FileName { get; set; }
		/// <summary>The completion percentage.</summary>
		public double Percent { get; set; }
		/// <summary>The time ellapsed since the operation started.</summary>
		public TimeSpan Ellapsed { get; set; }
	}

	/// <summary>Arguments for .hg3 extraction callbacks.</summary>
	public struct Hgx2pngArgs {
		/// <summary>The index of the current file being extracted.</summary>
		public int FileIndex { get; set; }
		/// <summary>The total number of files to extract.</summary>
		public int FileCount { get; set; }
		/// <summary>The full path to the file.</summary>
		public string FilePath { get; set; }
		/// <summary>The name of the file without the extension.</summary>
		public string FileName { get; set; }
		/// <summary>The completion percentage.</summary>
		public double Percent { get; set; }
		/// <summary>The time ellapsed since the operation started.</summary>
		public TimeSpan Ellapsed { get; set; }
		
		/// <summary>The total number of errors that occurred during the operation.</summary>
		public int TotalErrors { get; set; }
	}
	
	/// <summary>A callback while extracting an .int file.</summary>
	public delegate bool ExkifintCallback(ExkifintArgs args);
	/// <summary>A callback while extracting .hg3's.</summary>
	public delegate bool Hgx2pngCallback(Hgx2pngArgs args);
	/// <summary>A callback with an error while extracting .hg3's.</summary>
	public delegate bool Hgx2pngErrorCallback(Exception ex, Hgx2pngArgs args);

	/// <summary>A static class for extracting Grisaia files.</summary>
	public static class Extracting {

		/// <summary>The maximum number of retries for operations.</summary>
		public const int MaxRetries = 5;

		/// <summary>The path to the program used to extract .int files.</summary>
		//public static readonly string Exkifint = Path.Combine(
		//	AppContext.BaseDirectory, "exkifint_v3.exe");
		/// <summary>The path to the program used to extract .int files.</summary>
		//public static readonly string ExkifintOld = Path.Combine(
		//	AppContext.BaseDirectory, "exkifint_v3Old.exe");
		/// <summary>The path to the program used to extract .hg3 files.</summary>
		//public static readonly string Hgx2bmp = Path.Combine(
		//	AppContext.BaseDirectory, "hgx2bmp.exe");

		/// <summary>The regex used to identify the Grisaia executable.</summary>
		//public static readonly Regex GrisaiaExeRegex =
		//	new Regex(@"^Grisaia.*\.exe$", RegexOptions.IgnoreCase);

		/// <summary>Extracts the contents of an .int file.</summary>
		/// <returns>True if exkifint_v3.exe returned without error.</returns>
		public static bool ExtractInt(string inputDir,
			string intFile, string exeFile, string outputDir, ExkifintCallback progress)
		{
			/*if (grisaiaExe == null)
				grisaiaExe = FindGrisaiaExe(inputDir) ??
					throw new GrisaiaExeNotFoundException();
			else
				grisaiaExe = Path.ChangeExtension(grisaiaExe, ".exe");*/
			intFile = Path.ChangeExtension(intFile, ".int");

			Directory.CreateDirectory(outputDir);
			//string exkifint = Path.Combine(outputDir, Path.GetFileName(Exkifint));
			//if (!File.Exists(exkifint))
			//	File.Copy(Exkifint, exkifint);

			intFile = Path.Combine(inputDir, intFile);
			exeFile = Path.Combine(inputDir, exeFile);

			string binFile = Path.ChangeExtension(exeFile, ".bin");
			if (File.Exists(binFile))
				exeFile = binFile;

			Exkifint.Run(intFile, exeFile, outputDir, progress);
			return true;
			/*ProcessStartInfo start = new ProcessStartInfo() {
				FileName = old ? ExkifintOld : Exkifint,
				Arguments = $"\"{intFile}\" \"{exeFile}\"",
				CreateNoWindow = true,
				WorkingDirectory = outputDir,
				UseShellExecute = false,
			};
			DateTime startTime = DateTime.UtcNow;
			Process process = Process.Start(start);
			if (progress != null) {
				do {
					progress(DateTime.UtcNow - startTime);
					Thread.Sleep(1000);
				} while (!process.HasExited);
			}
			else {
				process.WaitForExit();
			}
			return process.ExitCode == 0;*/
		}

		/// <summary>Finds the name of the Grisaia executable in the specified
		/// directory.</summary>
		/*public static string FindGrisaiaExe(string dir) {
			foreach (string file in Directory.GetFiles(dir)) {
				string name = Path.GetFileName(dir);
				if (GrisaiaExeRegex.IsMatch(name))
					return file;
			}
			return null;
		}*/

		/// <summary>Converts the Hg3 to a bitmap and returns all the created images.</summary>
		/// <returns>An array of all output .bmp files. Null if an error occurred.</returns>
		public static string[] Hg3ToBmp(string file, bool expand) {
			Exception exception = null;
			for (int retries = 0; retries < MaxRetries; retries++) {
				// Run the conversion process
				/*ProcessStartInfo start = new ProcessStartInfo() {
					FileName = Hgx2bmp,
					Arguments = $"\"{file}\"",
					CreateNoWindow = true,
					UseShellExecute = false,
				};
				Process process = Process.Start(start);
				process.WaitForExit();
				if (process.ExitCode != 0)
					continue;

				return AnimationHelper.GetFileNames(file, ".bmp");*/

				try {
					string[] pngs = Hgx2png.Run(file, null, expand);
					return pngs.Select(p =>
						Path.ChangeExtension(p, ".png")).ToArray();
				}
				catch (Exception ex) {
					exception = ex;
				}
			}
			throw new ExtractHg3Exception(ExtractHg3Result.Unknown, file, exception);
		}

		/// <summary>Extracts all .hg3 files that meet the pattern into .png's.</summary>
		public static ImageIdentifier ExtractHg3s(string inputDir, string outputDir,
			Hg3Sorting sorting, string pattern,
			Hgx2pngCallback progressCallback, Hgx2pngErrorCallback errorCallback,
			string continueFile = null, string[] remainingFiles = null)
		{
			ImageIdentifier identifier = new ImageIdentifier();
			DateTime startTime = DateTime.UtcNow;
			string unsortedDir = outputDir;
			if (sorting.HasFlag(Hg3Sorting.Both))
				unsortedDir = Path.Combine(unsortedDir, "Unsorted");
			Directory.CreateDirectory(unsortedDir);

			if (string.IsNullOrWhiteSpace(pattern)) {
				pattern = "*.hg3";
			}
			else if (!pattern.EndsWith(".hg3", StringComparison.OrdinalIgnoreCase) &&
					!pattern.EndsWith(".hg3*", StringComparison.OrdinalIgnoreCase)) {
				pattern += ".hg3";
			}
			string[] files = Directory.GetFiles(inputDir, pattern);
			int fileCount = files.Length;
			Hgx2pngArgs args = new Hgx2pngArgs() {
				FileCount = fileCount,
			};

			// Skip files if we're continuing from a later point
			int start = 0;
			if (continueFile != null) {
				for (start = 0; start < fileCount; start++) {
					if (string.Compare(files[start], continueFile, true) >= 0)
						break;
				}
			}

			// Remove non-remaining files.
			// This will be used for retrying files that had errors.
			if (remainingFiles != null) {
				List<string> fileList = new List<string>(files);
				int remainIndex = 0;
				int remainCount = remainingFiles.Length;
				for (int i = 0; i < fileList.Count && remainIndex < remainCount &&
					(continueFile == null || i < start); i++)
				{
					if (PathHelper.IsPathTheSame(
						remainingFiles[remainIndex], fileList[i]))
					{
						fileList.RemoveAt(i);
						if (continueFile != null)
							start--;
						i--;
						remainIndex++;
					}
				}
				files = fileList.ToArray();
			}

			DateTime lastRefresh = DateTime.MinValue;
			Stopwatch writeTime = new Stopwatch();
			TimeSpan refreshTime = TimeSpan.FromMilliseconds(20);
			//Stopwatch processTime = new Stopwatch();
			for (int i = start; i < fileCount; i++) {
				string file = files[i];
				string name = Path.GetFileNameWithoutExtension(file);
				
				args.FileIndex = i;
				args.FileName = name;
				args.FilePath = file;
				args.Ellapsed = DateTime.UtcNow - startTime;
				// Round to nearest hundredth
				args.Percent = Math.Round((double) i / fileCount * 10000) / 100;
				// If true, cancel operation
				TimeSpan sinceRefresh = DateTime.UtcNow - lastRefresh;
				if (sinceRefresh >= refreshTime) {
					lastRefresh = DateTime.UtcNow;
					writeTime.Start();
					if (progressCallback?.Invoke(args) ?? false)
						return identifier;
					writeTime.Stop();
				}

				//processTime.Restart();
				List<Exception> exceptions = new List<Exception>();
				string[] bmps;
				try {
					//bmps = Hg3Convert.UnpackHg3(file, null, true);
					ImageIdentification image = identifier.PreIdentifyImage(file);
					bmps = Extracting.Hg3ToBmp(file, image.ExpandImage);
					image = identifier.IdentifyImage(bmps);
					string sortedDir = Path.Combine(outputDir, image.OutputDirectory);
					if (sorting.HasFlag(Hg3Sorting.Sorted))
						Directory.CreateDirectory(sortedDir);
					for (int j = 0; j < bmps.Length; j++) {
						try {
							BmpToPng(bmps[j], sorting, unsortedDir, sortedDir);
						}
						catch (ExtractHg3Exception ex) {
							exceptions.Add(ex);
						}
					}
					//processTime.Stop();
					//if (processTime.ElapsedMilliseconds >= 500)
					//	Trace.WriteLine($"Large File: {new FileInfo(file).Length / 1024:###,###,###,###}KB [{processTime.ElapsedMilliseconds}ms]");
				}
				catch (ExtractHg3Exception ex) {
					exceptions.Add(ex);
				}
				
				foreach (Exception ex in exceptions) {
					args.TotalErrors++;
					// If true, cancel operation
					if (errorCallback?.Invoke(ex, args) ?? false)
						return identifier;
				}
			}
			args.Ellapsed = DateTime.UtcNow - startTime;
			args.Percent = 100.0;
			progressCallback?.Invoke(args);
			Trace.WriteLine($"Console Write Time: {writeTime.Elapsed:mm\\:ss\\.fff}");
			return identifier;
		}

		public static void ResortPngs(string resortDir,
			Hgx2pngCallback progressCallback, Hgx2pngErrorCallback errorCallback)
		{
			DateTime startTime = DateTime.UtcNow;

			ImageIdentifier identifier = new ImageIdentifier();
			Hgx2pngArgs args = new Hgx2pngArgs();

			string message = "Locating Pngs...";
			Console.WriteLine(message);
			args.FileCount = PathHelper.EnumerateAllFiles(resortDir, "*.png").Count();
			Console.WriteLine($"\r{new string(' ', message.Length)}");

			DateTime lastRefresh = DateTime.MinValue;
			Stopwatch writeTime = new Stopwatch();
			TimeSpan refreshTime = TimeSpan.FromMilliseconds(20);
			int i = 0;
			foreach (string file in PathHelper.EnumerateAllFiles(resortDir, "*.png")) {
				//string file = files[i];
				ImageIdentification image = identifier.PreIdentifyImage(file);
				string path = Path.Combine(resortDir, image.OutputDirectory, Path.GetFileName(file));
				string name = Path.GetFileName(file);

				args.FileIndex = i++;
				args.FileName = name;
				args.FilePath = file;
				args.Ellapsed = DateTime.UtcNow - startTime;
				// Round to nearest hundredth
				args.Percent = Math.Round((double) i / args.FileCount * 10000) / 100;
				// If true, cancel operation
				TimeSpan sinceRefresh = DateTime.UtcNow - lastRefresh;
				if (sinceRefresh >= refreshTime) {
					lastRefresh = DateTime.UtcNow;
					writeTime.Start();
					if (progressCallback?.Invoke(args) ?? false)
						return;
					writeTime.Stop();
				}

				if (PathHelper.IsPathTheSame(path, file))
					continue;

				string sortedDir = Path.Combine(resortDir, image.OutputDirectory);
				Directory.CreateDirectory(sortedDir);
				try {
					BmpToPng(file, Hg3Sorting.Sorted, "", sortedDir);
				}
				catch (ExtractHg3Exception ex) {
					args.TotalErrors++;
					// If true, cancel operation
					if (errorCallback?.Invoke(ex, args) ?? false)
						return;
				}
			}
			args.Ellapsed = DateTime.UtcNow - startTime;
			args.Percent = 100.0;
			progressCallback?.Invoke(args);
			Trace.WriteLine($"Console Write Time: {writeTime.Elapsed:mm\\:ss\\.fff}");
			PathHelper.DeleteAllEmptyDirectories(resortDir);
		}

		/// <summary>Converts all bitmaps with the base name and animation postfixes to pngs.</summary>
		/// <exception cref="ExtractHg3Exception">An error occurred while trying to
		/// convert, save, or delete.</exception>
		public static void BmpToPng(string bmpFile, Hg3Sorting sorting,
			string unsortedDir, string sortedDir)
		{
			Exception exception = null;
			try {
				TryCopyPng(bmpFile, sorting, unsortedDir, sortedDir);
			}
			catch (ExtractHg3Exception ex) {
				exception = ex;
			}

			try {
				TryDeleteBmp(bmpFile);
			}
			catch (ExtractHg3Exception ex) {
				if (exception == null)
					exception = ex;
			}
			if (exception != null)
				throw exception;
		}

		/// <summary>Attempts to convert the .bmp file to a .png up to MaxRetries times.</summary>
		/// <exception cref="ExtractHg3Exception">An error occurred while trying to
		/// convert or save.</exception>
		private static void TryCopyPng(string oldFile, Hg3Sorting sorting,
			string unsortedDir, string sortedDir)
		{
			Exception exception = null;
			for (int retries = 0; retries < MaxRetries; retries++) {
				try {
					// Wait a moment in hopes of file access returning
					if (retries > 0)
						Thread.Sleep(100);
					/*string fileName = Path.GetFileName(bmpFile);
					byte[] B = File.ReadAllBytes(bmpFile);
					GCHandle GCH = GCHandle.Alloc(B, GCHandleType.Pinned);
					try {
					IntPtr Scan0 = (IntPtr) ((int) (GCH.AddrOfPinnedObject()) + 54);
					int W = Marshal.ReadInt32(Scan0, -36);
					int H = Marshal.ReadInt32(Scan0, -32);
					using (Bitmap bitmap = new Bitmap(W, H, 4 * W, PixelFormat.Format32bppArgb, Scan0)) {
						bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
						GCH.Free();
						if (sorting.HasFlag(ExtractHg3Sorting.Unsorted))
							TrySavePng(bitmap, unsortedDir, fileName);
						if (sorting.HasFlag(ExtractHg3Sorting.Sorted))
							TrySavePng(bitmap, sortedDir, fileName);
						return;
					}*/
					string fileName = Path.GetFileName(oldFile);
					if (sorting.HasFlag(Hg3Sorting.Unsorted))
						File.Copy(oldFile, Path.Combine(unsortedDir, fileName), true);
					if (sorting.HasFlag(Hg3Sorting.Sorted))
						File.Copy(oldFile, Path.Combine(sortedDir, fileName), true);
					return;
				}
				catch (ExtractHg3Exception) {
					throw;
				}
				catch (UnauthorizedAccessException ex) {
					exception = ex;
				}
				catch (IOException ex) {
					exception = ex;
				}
				catch (Exception ex) {
					exception = ex;
					break;
				}
			}
			throw new ExtractHg3Exception(
				ExtractHg3Result.BmpConvertFailed, oldFile, exception);
		}

		/// <summary>Attempts to save the bitmap as a .png up to MaxRetries times.</summary>
		/// <exception cref="ExtractHg3Exception">An error occurred while trying to
		/// save.</exception>
		private static void TrySavePng(Bitmap bitmap, string pngDir, string fileName) {
			string pngFile = Path.Combine(
				pngDir, Path.ChangeExtension(fileName, ".png"));

			Exception exception = null;
			for (int retries = 0; retries < MaxRetries; retries++) {
				// Wait a moment in hopes of file access returning
				if (retries > 0)
					Thread.Sleep(100);
				try {
					bitmap.Save(pngFile, ImageFormat.Png);
					return;
				}
				catch (UnauthorizedAccessException ex) {
					exception = ex;
				}
				catch (IOException ex) {
					exception = ex;
				}
				catch (Exception ex) {
					exception = ex;
					break;
				}
			}
			throw new ExtractHg3Exception(
				ExtractHg3Result.PngSaveFailed, pngFile, exception);
		}

		/// <summary>Attempts to delete the leftover .bmp file up to MaxRetries times.</summary>
		/// <exception cref="ExtractHg3Exception">An error occurred while trying to
		/// delete.</exception>
		private static void TryDeleteBmp(string bmpFile) {
			Exception exception = null;
			for (int retries = 0; retries < MaxRetries; retries++) {
				// Wait a moment in hopes of file access returning
				if (retries > 0)
					Thread.Sleep(100);
				try {
					File.Delete(bmpFile);
					return;
				}
				catch (UnauthorizedAccessException ex) {
					exception = ex;
				}
				catch (IOException ex) {
					exception = ex;
				}
				catch (Exception ex) {
					exception = ex;
					break;
				}
			}
			throw new ExtractHg3Exception(
				ExtractHg3Result.BmpDeleteFailed, bmpFile, exception);
		}
	}
}
