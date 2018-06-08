using GrisaiaExtractor.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GrisaiaExtractorConsole {
	public class AsciiImage {
		public struct Pixel {
			public static readonly Encoding ConsoleEncoding =
				Encoding.GetEncoding("Windows-1252");

			public char Character;
			public ConsoleColor Foreground;

			public Pixel(char character, ConsoleColor foreground) {
				Character = character;
				Foreground = foreground;
			}

			public Pixel(byte character, byte foreground) {
				Character = ConsoleEncoding.GetChar(character);
				Foreground = (ConsoleColor) foreground;
			}
		}

		public Pixel[,] Pixels { get; private set; }

		public int Width => Pixels.GetLength(0);
		public int Height => Pixels.GetLength(1);

		public static AsciiImage FromStream(Stream stream) {
			AsciiImage image = new AsciiImage();
			BinaryReader reader = new BinaryReader(stream);

			int width = reader.ReadInt32();
			int height = reader.ReadInt32();
			image.Pixels = new Pixel[width, height];
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					image.Pixels[x, y] = new Pixel(
						reader.ReadByte(), reader.ReadByte());
				}
			}

			return image;
		}

		public void Draw() {
			ConsoleColor oldForeground = Console.ForegroundColor;

			// Make sure we start on a brand-spanking-new line
			if (Console.CursorLeft > 0)
				Console.WriteLine();

			// Optimized by writing to the console as few times as possible
			int bufferWidth = Console.BufferWidth;
			string currentLine = "";
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width && x < bufferWidth; x++) {
					if (Pixels[x, y].Foreground != Console.ForegroundColor) {
						// We've reached a few color, gotta write the current line
						// (Check to make sure this isn't the first pixel
						if (currentLine.Length > 0) {
							Console.Write(currentLine);
							currentLine = "";
						}
						// Set the new foreground color for the next line
						Console.ForegroundColor = Pixels[x, y].Foreground;
					}
					currentLine += Pixels[x, y].Character;
				}

				// Keep drawing the current string onto the next line
				// (If we don't automatically go to the next line due to wrapping)
				if ((Console.CursorLeft + currentLine.Length) % bufferWidth != 0)
					currentLine += Environment.NewLine;
			}
			// Write the remaining line
			if (currentLine.Length > 0)
				Console.Write(currentLine);

			Console.ForegroundColor = oldForeground;
		}
	}
}
