using GrisaiaExtractor.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Asmodean {
	public static partial class Exkifint {

		private static uint GenTocSeed(string s) {
			const uint magic = 0x4C11DB7;
			uint seed = uint.MaxValue;

			for (int i = 0; i < s.Length; i++) {
				seed ^= ((uint)s[i]) << 24;

				for (int j = 0; j < 8; j++) {
					if ((seed & 0x80000000) != 0) {
						seed *= 2;
						seed ^= magic;
					}
					else {
						seed *= 2;
					}
				}

				seed = ~seed;
			}

			return seed;
		}


		private static void UnobfuscateFileName(char[] s, uint seed) {
			const string FWD = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			const string REV = "zyxwvutsrqponmlkjihgfedcbaZYXWVUTSRQPONMLKJIHGFEDCBA";

			MersenneTwister.Seed(seed);
			uint key = MersenneTwister.GenRand();
			int shift = (byte)((key >> 24) + (key >> 16) + (key >> 8) + key);

			for (int i = 0; i < s.Length; i++) {
				char c = s[i];
				int index = 0;
				int index2 = shift;

				while (REV[index2 % 0x34] != c) {
					if (REV[(shift + index + 1) % 0x34] == c) {
						index += 1;
						break;
					}

					if (REV[(shift + index + 2) % 0x34] == c) {
						index += 2;
						break;
					}

					if (REV[(shift + index + 3) % 0x34] == c) {
						index += 3;
						break;
					}

					index += 4;
					index2 += 4;

					if (index > 0x34) {
						break;
					}
				}

				if (index < 0x34) {
					s[i] = FWD[index];
				}

				shift++;
			}

			return;
		}

		private static void CopyResource(IntPtr h,
			string name, string type, out byte[] buffer, out int length)
		{
			IntPtr r = FindResource(h, name, type);
			if (r == IntPtr.Zero)
				throw new ResourceException(name, type, "find");

			IntPtr g = LoadResource(h, r);
			if (g == IntPtr.Zero)
				throw new ResourceException(name, type, "load");

			length = (int) SizeofResource(h, r);
			buffer = new byte[(length + 7) & ~7];

			IntPtr lockPtr = LockResource(g);
			if (lockPtr == IntPtr.Zero)
				throw new ResourceException(name, type, "lock");

			Marshal.Copy(lockPtr, buffer, 0, length);
		}

		private static string FindVCode2(string exeFile) {
			IntPtr h = LoadLibraryEx(exeFile, IntPtr.Zero,
				LOAD_LIBRARY_AS_IMAGE_RESOURCE);
			if (h == IntPtr.Zero)
				throw new LoadLibraryException(exeFile);
			
			CopyResource(h, "KEY", "KEY_CODE", out byte[] key, out int keyLength);

			for (int i = 0; i < key.Length; i++)
				key[i] ^= 0xCD;
			
			CopyResource(h, "DATA", "V_CODE2", out byte[] vcode2, out int vcode2Length);

			/*Blowfish bf = new Blowfish();
			fixed (byte* key_buff_ptr = keyBuffer)
				bf.Set_Key(key_buff_ptr, keyLength);
			bf.Decrypt(vcode2Buffer, (vcode2Length + 7) & ~7);
			string vcode2 = Encoding.ASCII.GetString(vcode2Buffer, 0, vcode2Length).NullTerminate();*/
			
			DecryptVCode2(key, keyLength, vcode2, vcode2Length);

			string result = Encoding.ASCII.GetString(vcode2).NullTerminate();

			FreeLibrary(h);

			return result;
		}

		public static void Run(string intFile, string exeFile, string outputDir,
			ExkifintCallback progress = null)
		{
			using (Stream stream = File.OpenRead(intFile))
				Run(stream, intFile, exeFile, outputDir, progress);
		}

		private static void Run(Stream stream, string intFile, string exeFile,
			string outputDir, ExkifintCallback progress = null)
		{
			Stopwatch watch = Stopwatch.StartNew();
			DateTime startTime = DateTime.UtcNow;
			string gameId = FindVCode2(exeFile);

			BinaryReader reader = new BinaryReader(stream);
			KIFHDR hdr = reader.ReadStruct<KIFHDR>();

			if (hdr.Signature != "KIF") // It's really a KIF INT file
				throw new InvalidFileException(Path.GetFileName(intFile), "INT");
			
			KIFENTRY[] entries = reader.ReadStructArray<KIFENTRY>(hdr.EntryCount);

			uint tocSeed = GenTocSeed(gameId);
			uint fileKey = 0;
			bool decrypt = false;

			ExkifintArgs args = new ExkifintArgs();
			for (int i = 0; i < hdr.EntryCount; i++) {
				if (entries[i].FileName == "__key__.dat") {
					if (!decrypt) {
						MersenneTwister.Seed(entries[i].Length);
						fileKey = MersenneTwister.GenRand();
						decrypt = true;
					}
				}
				else  {
					args.FileCount++;
				}
			}

			DateTime lastRefresh = DateTime.MinValue;
			Stopwatch writeTime = new Stopwatch();
			TimeSpan refreshTime = TimeSpan.FromMilliseconds(20);
			//Stopwatch processTime = new Stopwatch();
			for (uint i = 0; i < hdr.EntryCount; i++) {
				if (entries[i].FileName == "__key__.dat")
					continue;

				if (decrypt) {
					UnobfuscateFileName(entries[i].FileNameRaw, tocSeed + i);

					entries[i].Offset += i;

					DecryptEntry(ref entries[i], fileKey);

					/*Blowfish bf = new Blowfish();
					bf.Set_Key((byte*) &file_key, 4);
					byte[] entry_buff = entries[i].bytes;
					bf.Decrypt(entry_buff, 8);
					entries[i].bytes = entry_buff;*/
				}

				args.Ellapsed = DateTime.UtcNow - startTime;
				// Round to nearest hundredth
				args.Percent = Math.Round((double) args.FileIndex / args.FileCount * 10000) / 100;
				args.FileName = entries[i].FileName;
				TimeSpan sinceRefresh = DateTime.UtcNow - lastRefresh;
				if (sinceRefresh >= refreshTime) {
					lastRefresh = DateTime.UtcNow;
					writeTime.Start();
					progress?.Invoke(args);
					writeTime.Stop();
				}

				//processTime.Restart();
				stream.Position = entries[i].Offset;
				byte[] buffer = reader.ReadBytes(entries[i].Length);

				if (decrypt) {
					DecryptData(buffer, entries[i].Length, fileKey);
					/*Blowfish bf = new Blowfish();
					bf.Set_Key((byte*)&file_key, 4);
					bf.Decrypt(buff, (len / 8) * 8);*/
				}

				string path = Path.Combine(outputDir, entries[i].FileName);
				File.WriteAllBytes(path, buffer);
				args.FileIndex++;
				//processTime.Stop();
				//if (processTime.ElapsedMilliseconds >= 500)
				//	Trace.WriteLine($"Large File: {buffer.Length / 1024:###,###,###,###}KB [{processTime.ElapsedMilliseconds}ms]");
			}

			args.Ellapsed = DateTime.UtcNow - startTime;
			args.Percent = 100.0;
			progress?.Invoke(args);
			Trace.WriteLine($"Console Write Time: {writeTime.Elapsed:mm\\:ss\\.fff}");
		}
	}
}
