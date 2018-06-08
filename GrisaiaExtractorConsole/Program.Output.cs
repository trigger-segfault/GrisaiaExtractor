using GrisaiaExtractor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractorConsole {
	static partial class Program {

		private static void WriteLog(LogInfo log, bool complete) {
			if (complete)
				Console.WriteLine("Ripping Finished!");
			if (log.GamesComplete > 0)
				Console.WriteLine($"Games Completed: {log.GamesComplete}");
			if (log.GamesWithErrors.Any()) {
				Console.WriteLine($"Games Failed: {log.GamesFailed.Count}");
				if (complete)
					WriteGames(log.GamesFailed);
			}
			if (log.GamesWithErrors.Any()) {
				Console.WriteLine($"Games with Errors: {log.GamesWithErrors.Count}");
				if (complete)
					WriteGames(log.GamesWithErrors);
			}
			if (log.OperationsComplete > 0) {
				string operations = "operation";
				if (log.OperationsComplete > 1)
					operations += 's';
				if (complete && log.OperationsComplete > 1)
					Console.Write($"All {log.OperationsComplete} ");
				else if (!complete && log.OperationsComplete == 1)
					Console.Write($"Previous ");
				else if (!complete && log.OperationsComplete > 1)
					Console.Write($"{log.OperationsComplete} previous ");
				Console.WriteLine($"{operations} took: {log.Ellapsed.ToString(@"hh\:mm\:ss")}");
			}
		}

		private static void LogOperation(IntArgs args, LogInfo log) {

		}
		
		private static void LogDateTime(LogInfo log) {
			log.Write($"[{DateTime.Now}] ");
		}

		private static void LogMessage(LogInfo log, string message, Game game = null) {
			log.Write($"[{DateTime.Now}] ");
			log.WriteLine(message);
			if (game != null && game != Game.All)
				log.WriteLine($"Game: {game.Name()}");
		}

		private static string Name(this Game game) =>
			game.Name(settings.General.UseJapaneseNames);


		private static void WriteWatermark(string watermark) {
			int left = Console.CursorLeft;
			int top = Console.CursorTop;
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write(watermark);
			Console.CursorLeft = left;
			Console.CursorTop = top;
			ResetForegroundColor();
		}

		private static void WriteError(string message) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Error: {message}");
			ResetForegroundColor();
		}

		private static void WriteError(Exception ex) {
			Console.ForegroundColor = ConsoleColor.Red;
			string message = ex.Message;
			if (Debugger.IsAttached)
				message = ex.ToString();
			Console.WriteLine($"Error: {message}");
			ResetForegroundColor();
		}

		private static void DrawLogo() {
			Assembly assembly = typeof(AsciiImage).Assembly;
			using (Stream stream = assembly.GetManifestResourceStream(
				$"GrisaiaExtractorConsole.grisaia.ascii"))
			{
				AsciiImage logo = AsciiImage.FromStream(stream);
				if (Console.CursorLeft != 0)
					Console.WriteLine();
				logo.Draw();
			}
		}

		private static void WriteIntProgress(int line, TimeSpan ellapsed) {
			Console.CursorLeft = 0;
			Console.CursorTop = line;
			Console.Write($"[{ellapsed.ToString(@"hh\:mm\:ss")}]");
		}

		private static void WriteProgress(int line, ref int lastLineLength, Hgx2pngArgs args) {
			Console.CursorLeft = 0;
			Console.CursorTop = line;
			string newLine =
				$"[{args.Percent.ToString("00.00")}%]" +
				$"[{Math.Min(args.FileCount, args.FileIndex + 1)}/{args.FileCount}]" +
				$"[{args.Ellapsed.ToString(@"hh\:mm\:ss")}]" +
				$" {args.FileName}";
			if (newLine.Length > Console.BufferWidth)
				newLine = newLine.Substring(0, Console.BufferWidth);
			if (lastLineLength > newLine.Length)
				newLine += new string(' ', lastLineLength - newLine.Length);
			lastLineLength = newLine.Length;
			Console.Write(newLine);

			// Display the number of errors if there are any
			if (args.TotalErrors > 0) {
				Console.CursorLeft = 0;
				Console.CursorTop = line + 1;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write($"Errors: {args.TotalErrors}");
				ResetForegroundColor();
			}
		}

		private static void WriteProgress(int line, ref int lastLineLength, ExkifintArgs args) {
			Console.CursorLeft = 0;
			Console.CursorTop = line;
			string newLine =
				$"[{args.Percent.ToString("00.00")}%]" +
				$"[{Math.Min(args.FileCount, args.FileIndex + 1)}/{args.FileCount}]" +
				$"[{args.Ellapsed.ToString(@"hh\:mm\:ss")}]" +
				$" {args.FileName}";
			if (newLine.Length > Console.BufferWidth)
				newLine = newLine.Substring(0, Console.BufferWidth);
			if (lastLineLength > newLine.Length)
				newLine += new string(' ', lastLineLength - newLine.Length);
			lastLineLength = newLine.Length;
			Console.Write(newLine);
		}

		private static void Beep() {
			if (settings.General.BeepAfterOperation)
				Console.Beep();
		}

		private static void Beep(int frequency, int duration) {
			if (settings.General.BeepAfterOperation)
				Console.Beep(frequency, duration);
		}
	}
}
