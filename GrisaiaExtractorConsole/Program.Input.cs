using GrisaiaExtractor;
using GrisaiaExtractor.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractorConsole {
	static partial class Program {
		private static Game ReadGame(bool allowNone = false) {
			do {
				List<Game> games = Locator.LocateGames(out bool newPaths, true);
				if (newPaths) {
					settings.Save();
				}
				int index = 1;
				foreach (Game game in games) {
					Console.Write($"{index}) ");
					game.WriteLine(settings.General.UseJapaneseNames);
					/*Console.Write($"{index}) {game.Game.JPName}");
					if (game.Path == null)
						Console.Write(": [NOT FOUND]");
					Console.WriteLine();*/
					index++;
				}
				Console.WriteLine($"{index}) {Game.All.Name()}");
				if (allowNone)
					Console.WriteLine("0) None");
				Console.Write("Choice: ");
				if (!int.TryParse(ReadLine(), out index) || index < 0)
					WriteError("Input is not a valid index!");
				else if ((!allowNone && index == 0) || index - 1 > games.Count)
					WriteError("Input index is out of bounds!");
				else if (index == 0)
					return null;
				else if (index - 1 == games.Count) {
					Console.WriteLine();
					Console.WriteLine("All located games will be ripped and each " +
						"game will use a directory with its name.");
					Console.WriteLine();
					return Game.All;
				}
				else
					return games[index - 1];
			} while (true);
		}

		private static IntArgs RequestExtractIntArgs(Game game) {
			IntArgs args = new IntArgs() {
				Game = game,
			};
			if (game != null)
				Console.WriteLine($"Extract {game.Name()} Int Archive:");
			else
				Console.WriteLine($"Extract Int Archive:");

			bool parseSuccess;

			if (game != Game.All) {
				do {
					Console.Write("Input Directory: ");
					if (game.Path != null)
						WriteWatermark("<located path>");
					args.InputDir = ReadDirectory(game.Path, out parseSuccess);
				} while (!parseSuccess);
			}

			string defOutputDir = Path.Combine(game.Name(), settings.Directories.IntDirectory);
			do {
				if (game == Game.All) {
					defOutputDir = ".";
					Console.Write("Output Directory (Before game name): ");
					WriteWatermark("<current directory>");
				}
				else {
					Console.Write("Output Directory: ");
					WriteWatermark(defOutputDir);
				}
				args.OutputDir = ReadDirectory(defOutputDir, out parseSuccess);
			} while (!parseSuccess);

			if (game == Game.All) {
				defOutputDir = settings.Directories.IntDirectory;
				do {
					Console.Write("Output Directory (After game name): ");
					WriteWatermark(defOutputDir);
					args.OutputDirAfter = ReadRelativePath(defOutputDir, out parseSuccess);
				} while (!parseSuccess);
			}

			do {
				Console.Write("Int File: ");
				WriteWatermark("image.int");
				args.IntFile = ReadRelativePath("image.int", out parseSuccess);
			} while (!parseSuccess);

			return args;
		}


		private static Hg3Args RequestConvertHg3Args(Game game, IntArgs? intArgs = null) {
			Hg3Args args = new Hg3Args() {
				Game = game,
			};
			if (game != null)
				Console.WriteLine($"Convert {game.Name()} Hg3s to Pngs:");
			else
				Console.WriteLine($"Convert Hg3s to Pngs:");

			bool parseSuccess;

			string defInputDir = settings.Directories.IntDirectory;
			string defOutputDir = settings.Directories.Hg3Directory;
			if (game == Game.All) {
				defInputDir = ".";
			}
			else if (game != null) {
				defInputDir = Path.Combine(game.Name(), defInputDir);
				defOutputDir = Path.Combine(game.Name(), defOutputDir);
			}
			if (!intArgs.HasValue) {
				do {
					if (game == Game.All) {
						defInputDir = ".";
						Console.Write("Input Directory (Before game name): ");
						WriteWatermark("<current directory>");
					}
					else {
						Console.Write("Input Directory: ");
						WriteWatermark(defInputDir);
					}
					args.InputDir = ReadDirectory(defInputDir, out parseSuccess);
				} while (!parseSuccess);

				if (game == Game.All) {
					defInputDir = settings.Directories.IntDirectory;
					do {
						Console.Write("Input Directory (After game name): ");
						WriteWatermark(defInputDir);
						args.InputDirAfter = ReadRelativePath(defInputDir, out parseSuccess);
					} while (!parseSuccess);
				}
			}
			else {
				args.InputDir = intArgs.Value.OutputDir;
				args.InputDirAfter = intArgs.Value.OutputDirAfter;
			}

			/*do {
				Console.Write("Output Directory: ");
				WriteWatermark(defOutputDir);
				args.OutputDir = ReadDirectory(defOutputDir, out parseSuccess);
			} while (!parseSuccess);*/

			do {
				if (game == Game.All) {
					defOutputDir = ".";
					Console.Write("Output Directory (Before game name): ");
					WriteWatermark("<current directory>");
				}
				else {
					Console.Write("Output Directory: ");
					WriteWatermark(defOutputDir);
				}
				args.OutputDir = ReadDirectory(defOutputDir, out parseSuccess);
			} while (!parseSuccess);

			if (game == Game.All) {
				defOutputDir = settings.Directories.Hg3Directory;
				do {
					Console.Write("Output Directory (After game name): ");
					WriteWatermark(defOutputDir);
					args.OutputDirAfter = ReadRelativePath(defOutputDir, out parseSuccess);
				} while (!parseSuccess);
			}

			do {
				Console.Write("Search Pattern: ");
				WriteWatermark("(none)");
				args.Pattern = ReadPattern("", out parseSuccess);
			} while (!parseSuccess);

			do {
				Console.Write("Sorting (sorted/unsorted/both): ");
				WriteWatermark("sorted");
				args.Sorting = ReadSorting(Hg3Sorting.Sorted, out parseSuccess);
			} while (!parseSuccess);

			do {
				Console.Write("Stop on Error (y/n): ");
				WriteWatermark("no");
				args.StopOnError = ReadYesNo(false, out parseSuccess);
			} while (!parseSuccess);


			return args;
		}

		private static string ReadLine() {
			Console.ForegroundColor = ConsoleColor.White;
			string line = Console.ReadLine();
			ResetForegroundColor();
			return line;
		}

		private static Hg3Sorting ReadSorting(Hg3Sorting? defaultValue, out bool parseSuccess) {
			string input = ReadLine().Trim();
			parseSuccess = true;
			if (string.IsNullOrWhiteSpace(input)) {
				if (defaultValue.HasValue)
					return defaultValue.Value;
				WriteError("Input cannot be empty!");
			}
			else {
				if (input.Equals2("sorted", true) ||
					input.Equals2("sort", true) ||
					input.Equals2("s", true))
					return Hg3Sorting.Sorted;
				else if (input.Equals2("unsorted", true) ||
					input.Equals2("unsort", true) ||
					input.Equals2("u", true))
					return Hg3Sorting.Unsorted;
				else if (input.Equals2("both", true) ||
					input.Equals2("b", true))
					return Hg3Sorting.Both;
				WriteError("Input is not in 'sorted/unsorted/both' format!");
			}
			parseSuccess = false;
			return Hg3Sorting.None;
		}

		private static string ReadPattern(string defaultValue, out bool parseSuccess) {
			string input = ReadLine().Trim().RemoveQuotes();
			parseSuccess = true;
			if (string.IsNullOrWhiteSpace(input)) {
				if (defaultValue != null)
					return defaultValue;
				WriteError("Input cannot be empty!");
			}
			else {
				if (PathHelper.IsValidNamePattern(input))
					return input;
				WriteError("Input is not in 'yes/no' format!");
			}
			parseSuccess = false;
			return "";
		}

		private static string ReadRelativePath(string defaultValue, out bool parseSuccess) {
			string input = ReadLine().Trim().RemoveQuotes().Trim();
			parseSuccess = true;
			if (string.IsNullOrWhiteSpace(input)) {
				if (defaultValue != null)
					return defaultValue;
				WriteError("Input cannot be empty!");
			}
			else {
				if (PathHelper.IsValidPathPattern(input))
					return input;
				WriteError("Input is not in yes/no format!");
			}
			parseSuccess = false;
			return "";
		}

		private static string ReadDirectory(string defaultValue, out bool parseSuccess) {
			string input = ReadLine().Trim().RemoveQuotes().Trim();
			parseSuccess = true;
			if (string.IsNullOrWhiteSpace(input)) {
				if (defaultValue != null)
					return defaultValue;
				WriteError("Input cannot be empty!");
			}
			else {
				if (PathHelper.IsValidDirectory(input))
					return input;
				WriteError("Input is not in yes/no format!");
			}
			parseSuccess = false;
			return "";
		}

		private static bool ReadYesNo(bool? defaultValue, out bool parseSuccess) {
			string input = ReadLine().Trim();
			parseSuccess = true;
			if (string.IsNullOrWhiteSpace(input)) {
				if (defaultValue.HasValue)
					return defaultValue.Value;
				WriteError("Input cannot be empty!");
			}
			else {
				if (input.Equals2("yes", true) || input.Equals2("y", true))
					return true;
				else if (input.Equals2("no", true) || input.Equals2("n", true))
					return false;
				WriteError("Input is not in yes/no format!");
			}
			parseSuccess = false;
			return false;
		}
	}
}
