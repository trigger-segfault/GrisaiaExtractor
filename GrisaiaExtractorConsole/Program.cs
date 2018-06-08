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
		
		static int Main(string[] args) {
			try {
				bool parseSuccess;
				bool another;
				do {
					Run(args);
					if (Console.CursorLeft != 0)
						Console.WriteLine();
					//Console.WriteLine();
					if (settings.General.BeepOnCompletion)
						Console.Beep(1000, 750);
					do {
						Console.Write("Finished! Perform another task (y/n): ");
						WriteWatermark("no");
						another = ReadYesNo(false, out parseSuccess);
					} while (!parseSuccess);
				} while (another);
				Console.ResetColor();
				return 0;
			}
			catch (Exception ex) {
				WriteError("An unexpected error occurred!");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex.ToString());

				Console.WriteLine();
				Console.WriteLine("Task Stopped! (Press any key to exit)");
				if (settings.General.BeepOnCompletion)
					Console.Beep(200, 750);
				Console.ResetColor();
				Console.Read();
				return -1;
			}
		}

		static private void ResetForegroundColor() {
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		private static void Run(string[] args) {
			Console.Clear();
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Title = "Grisaia Extract (Ripping written by asmodean)";
			DrawLogo();
			settings = new UserSettings();
			settings.Load();
			if (!string.IsNullOrWhiteSpace(settings.Directories.CurrentDirectory) &&
				!PathHelper.IsValidDirectory(settings.Directories.CurrentDirectory))
			{
				WriteError("CurrentDirectory ini setting is not valid!");
				settings.Directories.CurrentDirectory = "";
			}
			if (!PathHelper.IsValidRelativePath(settings.Directories.IntDirectory)) {
				WriteError("IntDirectory ini setting is not valid!");
				settings.Directories.IntDirectory = "Raw";
			}
			if (!PathHelper.IsValidRelativePath(settings.Directories.Hg3Directory)) {
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
			bool resort = false;
			do {
				Console.Write("What to Extract (int/hg3/resort): ");
				string input = ReadLine();
				parseSuccess = true;
				if (input.Equals2("int", true))
					hg3 = false;
				else if (input.Equals2("hg3", true))
					hg3 = true;
				else if (input.Equals2("resort", true))
					resort = true;
				else {
					WriteError("Invalid choice!");
					parseSuccess = false;
				}
			} while (!parseSuccess);
			Game game = ReadGame(hg3 || resort);
			IntArgs intArgs = new IntArgs();
			Hg3Args hg3Args = new Hg3Args();
			PngArgs pngArgs = new PngArgs();
			bool alsoHg3 = false;
			if (resort) {
				pngArgs = RequestResortPngArgs(game);
			}
			else if (!hg3) {
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
					RipGame(game, hg3, alsoHg3, resort, intArgs, hg3Args, pngArgs, log);
				}
				else {
					foreach (Game located in Locator.LocateGames(out _)) {
						RipGame(located, hg3, alsoHg3, resort, intArgs, hg3Args, pngArgs, log);
					}
				}

				if (log.OperationsComplete > 1) {
					Console.Clear();
					DrawLogo();
					WriteLog(log, true);
				}
			}

		}

		private static void WriteGames(IEnumerable<Game> games) {
			foreach (Game game in games) {
				Console.WriteLine($"  {game.Name()}");
			}
		}

		private static void RipGame(Game game, bool hg3, bool alsoHg3, bool resort,
			IntArgs intArgs, Hg3Args hg3Args, PngArgs pngArgs, LogInfo log)
		{
			if (resort) {
				pngArgs.Game = game;
				ResortPngs(pngArgs, log);
				log.OperationsComplete++;
				log.GamesComplete++;
			}
			else if (!hg3) {
				intArgs.Game = game;
				// Continue to the next process on success
				string inputDir = intArgs.InputDir;
				if (string.IsNullOrEmpty(intArgs.InputDir))
					inputDir = intArgs.Game?.Path;
				string[] intFiles = Directory.GetFiles(inputDir, intArgs.IntFile);
				if (intFiles.Length == 0) {
					LogMessage(log, $"No int files found matching `{intArgs.IntFile}`!", game);
					WriteError($"No int files found matching `{intArgs.IntFile}`!");
					Beep(300, 750);
					Thread.Sleep(1500);
					log.GamesFailed.Add(game);
					log.OperationsComplete++;
				}
				else {
					bool error = false;
					for (int i = 0; i < intFiles.Length; i++) {
						string intFile = Path.GetFileName(intFiles[i]);
						intArgs.IntFile = intFile;
						if (!ExtractIntFile(intArgs, log))
							error = true;
						log.OperationsComplete++;
					}
					if (!error) {
						if (alsoHg3)
							hg3 = true;
						else
							log.GamesComplete++;
					}
				}
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
				Beep();
				return true;
			}
			catch (Exception ex) {
				if (args.Game != null)
					log.GamesFailed.Add(args.Game);
				WriteError(ex);
				LogMessage(log, $"Error: {ex.Message}", args.Game);
				log.WriteLine(ex.ToString());
				Beep(300, 750);
				Thread.Sleep(1500);
				return false;
			}
		}

		private static bool ConvertHg3s(Hg3Args args, LogInfo log) {
			Console.Clear();
			DrawLogo();
			WriteLog(log, false);
			if (args.Game != null)
				Console.WriteLine($"Converting {args.Game.Name()} Hg3s to Pngs:");
			else
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
							Beep(300, 750);
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
				Beep();
			}

			return !error;
		}

		private static bool ResortPngs(PngArgs args, LogInfo log) {
			Console.Clear();
			DrawLogo();
			WriteLog(log, false);
			if (args.Game != null)
				Console.WriteLine($"Resorting {args.Game.Name()} Pngs:");
			else
				Console.WriteLine("Resorting Pngs:");
			LogMessage(log, "Resorting Pngs", args.Game);

			string resortDir = args.ResortDir;
			if (!string.IsNullOrEmpty(args.ResortDirAfter))
				resortDir = Path.Combine(args.ResortDir,
					args.Game.Name(), args.ResortDirAfter);

			int line = Console.CursorTop;
			int lastLineLength = 0;
			bool error = false;
			Extracting.ResortPngs(resortDir,
				(a) => {
					WriteProgress(line, ref lastLineLength, a);
					return false;
				},
				(ex, a) => {
					if (!error) {
						LogMessage(log, $"Error on {a.FileName}: {ex.Message}", args.Game);
						log.WriteLine(ex.ToString());
						/*if (args.StopOnError) {
							a.TotalErrors = 0;
							WriteProgress(line, ref lastLineLength, a);
							Console.WriteLine();
							WriteError(ex);
							Beep(300, 750);
							Thread.Sleep(1500);
							log.GamesFailed.Add(args.Game);
							LogMessage(log, "Stopping due to error!");
							return true;
						}
						else*/
							log.GamesWithErrors.Add(args.Game);
					}
					error = true;
					WriteProgress(line, ref lastLineLength, a);
					return false;
				});

			if (!error/* && !args.StopOnError*/) {
				LogMessage(log, "Finished!");
				Beep();
			}

			return !error;
		}
	}
}
