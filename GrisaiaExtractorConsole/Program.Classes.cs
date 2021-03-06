﻿using GrisaiaExtractor;
using GrisaiaExtractor.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractorConsole {
	static partial class Program {
		public struct IntArgs {
			public string InputDir { get; set; }
			public string OutputDir { get; set; }
			public string OutputDirAfter { get; set; }
			public string IntFile { get; set; }
			public Game Game { get; set; }

			public bool IsImage {
				get {
					return Path.GetFileNameWithoutExtension(IntFile).ToLower()
						.StartsWith("image");
				}
			}
		}

		public struct Hg3Args {
			public string InputDir { get; set; }
			public string InputDirAfter { get; set; }
			public string OutputDir { get; set; }
			public string OutputDirAfter { get; set; }
			public string Pattern { get; set; }
			public Hg3Sorting Sorting { get; set; }
			public bool StopOnError { get; set; }
			public string ContinueFile { get; set; }
			public Game Game { get; set; }
		}

		public struct PngArgs {
			public string ResortDir { get; set; }
			public string ResortDirAfter { get; set; }
			public Game Game { get; set; }
		}

		private class LogInfo : IDisposable {
			public DateTime StartTime { get; } = DateTime.UtcNow;
			public int GamesComplete { get; set; }
			public HashSet<Game> GamesWithErrors { get; } = new HashSet<Game>();
			public HashSet<Game> GamesFailed { get; } = new HashSet<Game>();

			public int OperationsComplete { get; set; }


			public TimeSpan Ellapsed => DateTime.UtcNow - StartTime;
			
			public StreamWriter LogWriter { get; }
			public int LastOperationLogged { get; set; }

			public void WriteLine(string line) => LogWriter.WriteLine(line);
			public void WriteLine() => LogWriter.WriteLine();
			public void Write(string text) => LogWriter.Write(text);

			public LogInfo() {
				string path = PathHelper.CombineExecutable($"{PathHelper.ExeName}.log");
				var stream = new FileStream(path, FileMode.Append);
				LogWriter = new StreamWriter(stream) {
					AutoFlush = true,
				};
			}

			public void Dispose() {
				LogWriter.Close();
			}
		}
	}
}
