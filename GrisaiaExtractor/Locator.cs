using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor {
	public class Game {
		public static readonly Game All =
			new Game("All", "All Located Games", "All Located Games", 0, null, null);

		public string ID { get; }
		public string JPName { get; }
		public string USName { get; }
		public uint SteamID { get; }

		public string RegistryKey { get; }

		public string Executable { get; }

		public string GetExtractExe(string dir) {
			string bin = System.IO.Path.ChangeExtension(Executable, ".bin");
			if (File.Exists(System.IO.Path.Combine(dir, bin)))
				return bin;
			return Executable;
		}

		public string Path { get; set; }

		public Game(string id, string name, string jpName, uint steamID, string registryKey,
			string executable)
		{
			ID = id;
			USName = name;
			JPName = jpName;
			SteamID = steamID;
			RegistryKey = registryKey;
			Executable = executable;
		}

		public string Name(bool japanese) {
			return (japanese ? JPName : USName);
		}

		public void Write(bool japanese = true) {
			Console.Write(Name(japanese));

			if (Path == null) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write(" [NOT FOUND]");
				Console.ResetColor();
			}
		}
		public void WriteLine(bool japanese = true) {
			Write(japanese);
			Console.WriteLine();
		}
	}

	/*public struct LocatedGame {

		public Game Game { get; }
		public string Path { get; }
		public bool IsSteam { get; }

		public LocatedGame(Game game, string path, bool isSteam) {
			Game = game;
			Path = path;
			IsSteam = isSteam;
		}

		public override string ToString() {
			if (Path == null)
				return $"{Game.JPName}: [NOT FOUND]";
			return $"{Game.JPName}";
		}

		public string ToString(bool japanese) {
			if (Path == null)
				return $"{Game.Name(japanese)} [NOT FOUND]";
			return $"{Game.Name(japanese)}";
		}
	}*/

	public static class Locator {

		public static List<Game> Games { get; } = new List<Game>();

		public static Game GetGame(string id) {
			return Games.First(g => g.ID == id);
		}

		public static string GetPath([CallerMemberName] string id = null) {
			return GetGame(id).Path;
		}

		public static void SetPath(string path, [CallerMemberName] string id = null) {
			GetGame(id).Path = (string.IsNullOrWhiteSpace(path) ? null : path);
		}

		private const string SteamKey = @"HKEY_CURRENT_USER\Software\Valve\Steam";
		private const string SteamApps = "steamapps";
		private const string SteamLibraryFolders = "libraryfolders.vdf";
		private const string FrontwingKey = @"HKEY_CURRENT_USER\Software\Frontwing";
		private const string AppManifest = @"appmanifest_{0}.acf";

		static Locator() {
			Games.Add(new Game(
				"Kajitsu",
				"The Fruit of Grisaia",
				"Grisaia no Kajitsu",
				345610,
				"fruit",
				"Grisaia.exe"));
			Games.Add(new Game(
				"Meikyuu",
				"The Labyrintha of Grisaia",
				"Grisaia no Meikyuu",
				345620,
				"labyrinth",
				"Grisaia2.exe"));
			Games.Add(new Game(
				"Rakuen",
				"The Eden of Grisaia",
				"Grisaia no Rakuen",
				345620,
				"eden",
				"Grisaia3.exe"));
			Games.Add(new Game(
				"Yuukan",
				"The Leisure of Grisaia",
				"Grisaia no Yuukan",
				460160,
				"leisure",
				"GrisaiaAno1.exe"));
			Games.Add(new Game(
				"Zankou",
				"The Afterglow of Grisaia",
				"Grisaia no Zankou",
				464490,
				"afterglow",
				"GrisaiaAno2.exe"));
			Games.Add(new Game(
				"Senritsu",
				"The Melody of Grisaia",
				"Grisaia no Senritsu",
				464500,
				"melody",
				"GrisaiaAno3.exe"));
			Games.Add(new Game(
				"IdolMahouFull",
				"Idol Magical Girl Chiru Chiru Michiru (Full)",
				"Idol Mahou Shoujo Chiruchiru Michiru (Full)",
				0,
				null,
				"GrisaiaEx.exe"));
			Games.Add(new Game(
				"IdolMahouPart1",
				"Idol Magical Girl Chiru Chiru Michiru (Part 1)",
				"Idol Mahou Shoujo Chiruchiru Michiru (Part 1)",
				377710,
				null,
				"GrisaiaEx.exe"));
			Games.Add(new Game(
				"IdolMahouPart2",
				"Idol Magical Girl Chiru Chiru Michiru (Part 2)",
				"Idol Mahou Shoujo Chiruchiru Michiru (Part 2)",
				377720,
				null,
				"GrisaiaEx.exe"));
		}
		
		public static string[] LocateSteamFolders() {
			List<string> steamPaths = new List<string>();
			try {
				string primary = (string) Registry.GetValue(SteamKey, "SteamPath", null);
				if (primary != null && PathHelper.IsValidDirectory(primary)) {
					primary = Path.Combine(primary, SteamApps).Replace('/', '\\');
					steamPaths.Add(
						PathHelper.GetProperDirectoryCapitalization(primary));
					string libraryFolders = Path.Combine(primary, SteamLibraryFolders);
					/*if (File.Exists(libraryFolders)) {
						try {
							string text;
							using (Stream stream = File.OpenRead(libraryFolders)) {
								StreamReader reader = new StreamReader(stream);
								text = reader.ReadToEnd();
							}
							ReadLibraryFolders(steamPaths, text);
						}
						catch (Exception) { }
					}*/
					var collection = ParseValveFile(libraryFolders);
					if (collection != null) {
						int index = 1;
						while (collection.TryGetValue(
							$"LibraryFolders/{index}", out string path))
						{
							if (PathHelper.IsValidDirectory(path) &&
								Directory.Exists(path))
								steamPaths.Add(
									PathHelper.GetProperDirectoryCapitalization(path));
							index++;
						}
					}
				}
			}
			catch (Exception) { }
			return steamPaths.ToArray();
		}

		public static List<Game> LocateGames(out bool newPaths, bool includeMissing = false) {
			List<Game> gamesLeft = new List<Game>(Games.Where(g => g.Path == null));
			List<Game> located = new List<Game>(Games.Where(g => g.Path != null));
			newPaths = false;
			// Check for Steam Install Paths:
			string[] steamPaths = LocateSteamFolders();
			foreach (string steamapps in steamPaths) {
				for (int i = 0; i < gamesLeft.Count; i++) {
					Game game = gamesLeft[i];
					string path = CheckAppManifest(steamapps, game);
					if (path != null) {
						located.Add(game);
						game.Path = path;
						newPaths = true;
						gamesLeft.RemoveAt(i);
						i--;
					}
				}
			}

			// Check for Registry Install Paths:
			for (int i = 0; i < gamesLeft.Count; i++) {
				Game game = gamesLeft[i];
				string path = CheckRegistry(game);
				if (path != null) {
					located.Add(game);
					game.Path = path;
					newPaths = true;
					gamesLeft.RemoveAt(i);
					i--;
				}
			}

			if (includeMissing) {
				foreach (Game game in gamesLeft)
					located.Add(game);
			}
			located.Sort((a, b) => Games.IndexOf(a) - Games.IndexOf(b));
			return located;
		}

		private static string CheckAppManifest(string steamapps, Game game) {
			if (game.SteamID == 0)
				return null;
			string appManifest = Path.Combine(steamapps,
				string.Format(AppManifest, game.SteamID));
			var collection = ParseValveFile(appManifest);
			if (collection == null)
				return null;
			if (!collection.TryGetValue("AppState/installdir", out string installDir))
				return null;
			installDir = Path.Combine(steamapps, "common", installDir);
			if (!PathHelper.IsValidDirectory(installDir))
				return null;
			if (!Directory.Exists(installDir))
				return null;
			return installDir;
		}

		private static string CheckRegistry(Game game) {
			if (game.RegistryKey == null)
				return null;
			return Registry.GetValue($"{FrontwingKey}\\{game.RegistryKey}",
				"InstallPath", null) as string;
		}
		
		private static Dictionary<string, string> ParseValveFile(string file) {
			if (!File.Exists(file))
				return null;
			string text = File.ReadAllText(file);
			Dictionary<string, string> collection =
				new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			int level = 0;
			string path = "";
			string lastToken = "";
			bool isName = false;
			bool isValue = false;
			bool building = false;
			string token = "";
			bool escape = false;

			for (int i = 0; i < text.Length; i++) {
				char c = text[i];
				if (!building) {
					if (char.IsWhiteSpace(c)) {
						continue;
					}
					else if (c == '{') {
						if (isName) { // Was the last token a name?
							if (level > 0)
								path += '/';
							path += lastToken;
							level++;
							isName = false;
							isValue = false;
						}
						else {
							// Error
							return null;
						}
					}
					else if (c == '}') {
						if (level > 0 && !isName) { // Are we high enough and not defining something?
							level--;
							if (level == 0)
								path = "";
							else
								path = path.Substring(0, path.LastIndexOf('/'));
							isName = false;
							isValue = false;
						}
						else {
							// Error
							return null;
						}
					}
					else if (c == '"') {
						if (isName) {
							isName = false;
							isValue = true;
						}
						else {
							isValue = false;
							isName = true;
						}
						building = true;
					}
					else {
						// Error: Unknown character
						return null;
					}
				}
				else if (escape) {
					token += c;
					escape = false;
				}
				else if (c == '\\') {
					escape = true;
				}
				else if (c == '"') {
					if (isValue)
						collection.Add($"{path}/{lastToken}", token);
					lastToken = token;
					token = "";
					building = false;
				}
				else {
					token += c;
				}
			}
			if (building || isName || level != 0) {
				// Error: Unexpected end of file
				return null;
			}
			return collection;
		}

		
		private static void ReadLibraryFolders(List<string> steamPaths, string text) {
			int level = 0;
			string path = "";
			string lastToken = "";
			bool isName = false;
			bool isValue = false;
			bool building = false;
			string token = "";
			bool escape = false;

			for (int i = 0; i < text.Length; i++) {
				char c = text[i];
				if (!building) {
					if (char.IsWhiteSpace(c)) {
						continue;
					}
					else if (c == '{') {
						if (isName) { // Was the last token a name?
							level++;
							path += $"/{lastToken}";
							isName = false;
							isValue = false;
						}
						else {
							// Error
							return;
						}
					}
					else if (c == '}') {
						if (level > 0 && !isName) { // Are we high enough and not defining something?
							level--;
							path = path.Substring(0, path.LastIndexOf('/'));
							isName = false;
							isValue = false;
						}
						else {
							// Error
							return;
						}
					}
					else if (c == '"') {
						if (isName) {
							isName = false;
							isValue = true;
						}
						else {
							isValue = false;
							isName = true;
						}
						building = true;
					}
					else {
						// Error: Unknown character
						return;
					}
				}
				else if (escape) {
					token += c;
					escape = false;
				}
				else if (c == '\\') {
					escape = true;
				}
				else if (c == '"') {
					if (isValue && path == "/LibraryFolders" &&
						int.TryParse(lastToken, out _) &&
						PathHelper.IsValidDirectory(token)) {
						steamPaths.Add(Path.Combine(token, SteamApps));
					}
					lastToken = token;
					token = "";
					building = false;
				}
				else {
					token += c;
				}
			}
			if (building || isName || level != 0) {
				// Error: Nothing we can do now
			}
		}
	}
}
