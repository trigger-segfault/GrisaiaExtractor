using GrisaiaExtractor;
using GrisaiaExtractor.Asmodean;
using GrisaiaExtractor.Extensions;
using GrisaiaExtractor.Identifying;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrisaiaExtractorConsole {
	static partial class Program {

		static UserSettings settings;
		
		static void Main(string[] args) {
			try {
				Run(args);
			}
			catch (Exception ex) {
				WriteError("An unexpected error occurred!");
				Console.WriteLine(ex.ToString());
			}
			Console.ResetColor();
		}

		static private void ResetForegroundColor() {
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		private static void Run(string[] args) {
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Title = "Grisaia Extractor (Ripping written by asmodean)";
			DrawLogo();
			settings = new UserSettings();
			settings.Load();
			if (!PathHelper.IsValidDirectory(settings.Directories.CurrentDirectory)) {
				WriteError("CurrentDirectory ini setting is not valid!");
				settings.Directories.CurrentDirectory = "";
			}
			if (!PathHelper.IsValidPath(settings.Directories.IntDirectory)) {
				WriteError("IntDirectory ini setting is not valid!");
				settings.Directories.IntDirectory = "Raw";
			}
			if (!PathHelper.IsValidPath(settings.Directories.Hg3Directory)) {
				WriteError("IntDirectory ini setting is not valid!");
				settings.Directories.Hg3Directory = "Hg3";
			}
			foreach (var pair in settings.GameLocations.Paths) {
				if (pair.Value == null)
					continue;
				if (!PathHelper.IsValidDirectory(pair.Value)) {
					WriteError($"{pair.Key} ini setting is not valid!");
					Locator.SetPath(null, pair.Key);
				}

			}
			if (!string.IsNullOrWhiteSpace(settings.Directories.CurrentDirectory)) {
				Directory.CreateDirectory(settings.Directories.CurrentDirectory);
				Directory.SetCurrentDirectory(settings.Directories.CurrentDirectory);
			}
			bool parseSuccess;

			bool hg3 = false;
			do {
				Console.Write("What to Extract (int/hg3): ");
				string input = ReadLine();
				parseSuccess = true;
				if (input.Equals2("int", true))
					hg3 = false;
				else if (input.Equals2("hg3", true))
					hg3 = true;
				else {
					WriteError("Invalid choice!");
					parseSuccess = false;
				}
			} while (!parseSuccess);
			Game game = ReadGame(hg3);
			IntArgs intArgs = new IntArgs();
			Hg3Args hg3Args = new Hg3Args();
			bool alsoHg3 = false;
			if (!hg3) {
				intArgs = RequestExtractIntArgs(game);

				if (intArgs.IsImage) {
					do {
						Console.Write("Convert hg3s afterwords (y/n): ");
						alsoHg3 = ReadYesNo(null, out parseSuccess);
					} while (!parseSuccess);
				}
			}
			if (hg3 || alsoHg3) {
				if (alsoHg3)
					hg3Args = RequestConvertHg3Args(game, intArgs);
				else
					hg3Args = RequestConvertHg3Args(game);
			}
			using (LogInfo log = new LogInfo()) {
				if (game != Game.All) {
					RipGame(game, hg3, alsoHg3, intArgs, hg3Args, log);
				}
				else {
					foreach (Game located in Locator.LocateGames(out _)) {
						RipGame(located, hg3, alsoHg3, intArgs, hg3Args, log);
					}
				}

				if (log.OperationsComplete > 1) {
					Console.Clear();
					DrawLogo();
					WriteLog(log, true);
				}
			}


			Console.WriteLine();
			Console.WriteLine("Finished! (Press any key to continue)");
			Console.Beep(1000, 750);
			Console.Read();
		}

		private static void WriteGames(IEnumerable<Game> games) {
			foreach (Game game in games) {
				Console.WriteLine($"  {game.Name()}");
			}
		}

		private static void RipGame(Game game, bool hg3, bool alsoHg3,
			IntArgs intArgs, Hg3Args hg3Args, LogInfo log)
		{
			if (!hg3) {
				intArgs.Game = game;
				// Continue to the next process on success
				if (ExtractIntFile(intArgs, log)) {
					if (alsoHg3)
						hg3 = true;
					else
						log.GamesComplete++;
				}
				log.OperationsComplete++;
			}
			if (hg3) {
				hg3Args.Game = game;
				if (ConvertHg3s(hg3Args, log))
					log.GamesComplete++;
				log.OperationsComplete++;
			}
		}

		

		private static bool ExtractIntFile(IntArgs args, LogInfo log) {
			Console.Clear();
			DrawLogo();
			WriteLog(log, false);
			Console.WriteLine($"Extracting {args.IntFile}:");
			LogMessage(log, $"Extracting {args.IntFile}", args.Game);
			string inputDir = args.InputDir;
			if (string.IsNullOrEmpty(args.InputDir))
				inputDir = args.Game?.Path;
			string outputDir = args.OutputDir;
			if (!string.IsNullOrEmpty(args.OutputDirAfter))
				outputDir = Path.Combine(args.OutputDir,
					args.Game.Name(), args.OutputDirAfter);

			int line = Console.CursorTop;
			int lastLineLength = 0;
			try {
				Extracting.ExtractInt(inputDir, args.IntFile,
					args.Game.GetExtractExe(inputDir), outputDir,
					(a) => {
						WriteProgress(line, ref lastLineLength, a);
						return false;
					});
				LogMessage(log, "Finished!");
				Console.Beep();
				return true;
			}
			catch (Exception ex) {
				if (args.Game != null)
					log.GamesFailed.Add(args.Game);
				WriteError(ex);
				LogMessage(log, $"Error: {ex.Message}", args.Game);
				log.WriteLine(ex.ToString());
				Console.Beep(300, 750);
				Thread.Sleep(1500);
				return false;
			}
		}

		private static bool ConvertHg3s(Hg3Args args, LogInfo log) {
			Console.Clear();
			DrawLogo();
			WriteLog(log, false);
			Console.WriteLine("Converting Hg3s to Pngs:");
			LogMessage(log, "Converting Hg3s to Pngs", args.Game);

			string inputDir = args.InputDir;
			if (!string.IsNullOrEmpty(args.InputDirAfter))
				inputDir = Path.Combine(args.InputDir,
					args.Game.Name(), args.InputDirAfter);
			string outputDir = args.OutputDir;
			if (!string.IsNullOrEmpty(args.OutputDirAfter))
				outputDir = Path.Combine(args.OutputDir,
					args.Game.Name(), args.OutputDirAfter);

			int line = Console.CursorTop;
			int lastLineLength = 0;
			bool error = false;
			Extracting.ExtractHg3s(inputDir, outputDir, args.Sorting,
				args.Pattern,
				(a) => {
					WriteProgress(line, ref lastLineLength, a);
					return false;
				},
				(ex, a) => {
					if (!error) {
						LogMessage(log, $"Error on {a.FileName}: {ex.Message}", args.Game);
						log.WriteLine(ex.ToString());
						if (args.StopOnError) {
							a.TotalErrors = 0;
							WriteProgress(line, ref lastLineLength, a);
							Console.WriteLine();
							WriteError(ex);
							Console.Beep(300, 750);
							Thread.Sleep(1500);
							log.GamesFailed.Add(args.Game);
							LogMessage(log, "Stopping due to error!");
							return true;
						}
						else
							log.GamesWithErrors.Add(args.Game);
					}
					error = true;
					WriteProgress(line, ref lastLineLength, a);
					return false;
				});

			if (!error && !args.StopOnError) {
				LogMessage(log, "Finished!");
				Console.Beep();
			}

			return !error;
		}
	}
}
