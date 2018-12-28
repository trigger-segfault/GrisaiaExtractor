using GrisaiaExtractor.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Asmodean {
	public static partial class Hgx2png {

		public static string[] Run(string hg3File, string outputDir,
			bool expand)
		{
			using (Stream stream = File.OpenRead(hg3File))
				return Run(stream, hg3File, outputDir ?? Path.GetDirectoryName(hg3File),
					Path.GetFileNameWithoutExtension(hg3File), expand);
		}

		public static string[] Run(Stream stream, string hg3File, string outputDir,
			string fileName, bool expand)
		{
			BinaryReader reader = new BinaryReader(stream);
			HG3HDR hdr = reader.ReadStruct<HG3HDR>();

			if (hdr.Signature != "HG-3")
				throw new InvalidFileException(Path.GetFileName(hg3File), "HG3");

			List<string> files = new List<string>();
			int backtrack = Marshal.SizeOf<HG3TAG>() - 1;
			for (int i = 0; true; i++) {
				HG3TAG tag = reader.ReadStruct<HG3TAG>();

				// NEW METHOD: Keep searching for the next stdinfo
				// This way we don't miss any images
				while (!tag.Signature.StartsWith("stdinfo")) {
					if (stream.IsEndOfStream())
						break;
					stream.Position -= backtrack;
					tag = reader.ReadStruct<HG3TAG>();
				}
				if (stream.IsEndOfStream())
					break;

				// OLD METHOD: Missed entries in a few files
				//if (!tag.signature.StartsWith(StdInfoSignature))
				//	break;

				HG3STDINFO stdInfo = reader.ReadStruct<HG3STDINFO>();

				int imgIndex = 0;

				while (tag.OffsetNext != 0) {
					tag = reader.ReadStruct<HG3TAG>();

					string pngFile = Path.Combine(outputDir, MakeFileName(
						fileName, i > 0 || hdr.EntryCount > 0, i, imgIndex++));

					/*if (tag.Signature.StartsWith("img_al")) {
						// Skip this tag
						stream.Position += tag.Length;
						HG3IMGAL imghdr = reader.ReadStruct<HG3IMGAL>();

						int length = imghdr.length;
						byte[] buffer = reader.ReadBytes(length);

						int outLength = imghdr.original_length;
						byte[] outBuffer = reader.ReadBytes(outLength);

						Uncompress(outBuffer, ref outLength, buffer, length);

						files.Add(pngFile);
						WritePng(pngFile,
							outBuffer,
							stdInfo.width,
							stdInfo.height,
							1,
							true);
					}
					else if (tag.Signature.StartsWith("img_jpg")) {
						// Skip this tag
						stream.Position += tag.Length;
					}
					else if (tag.Signature == "imgmode") {
						// Skip this tag
						stream.Position += tag.Length;
					}
					else */if (Regex.IsMatch(tag.Signature, @"img\d+")) {
						HG3IMG imghdr = reader.ReadStruct<HG3IMG>();

						files.Add(pngFile);
						ProcessImage(reader,
							pngFile,
							stdInfo,
							imghdr,
							expand);
							/*stdInfo.Width,
							stdInfo.Height,
							stdInfo.DepthBits / 8,
							stdInfo.TotalWidth,
							stdInfo.TotalHeight,
							stdInfo.OffsetX,
							stdInfo.OffsetY,
							imghdr.DataLength,
							imghdr.OriginalDataLength,
							imghdr.CmdLength,
							imghdr.OriginalCmdLength);*/
					}
					else {
						// Skip this tag
						stream.Position += tag.Length;
					}
				}

				stream.Position += 8;
			}

			return files.ToArray();
		}

		private static string MakeFileName(string prefix, bool useIndex, int index, int subIndex) {
			string fileName = prefix;
			if (useIndex)
				fileName += $"+{index.ToString("000")}";
			if (useIndex || subIndex != 0)
				fileName += $"+{subIndex.ToString("000")}";
			return fileName + ".png";
		}

		#region Unused Code
		// This encoding tries to optimize for lots of zeros. I think. :)
		/*private static byte UnpackVal(byte c) {
			byte z = (byte) ((c & 1) != 0 ? 0xFF : 0);
			return (byte) ((c >> 1) ^ z);
		}

		private static unsafe void UndeltaFilter(byte[] buffer,
			//int length,
			byte[] outBuffer,
			int width,
			int height,
			int depthBytes)
		{
			uint[] table1 = new uint[256];
			uint[] table2 = new uint[256];
			uint[] table3 = new uint[256];
			uint[] table4 = new uint[256];

			for (uint i = 0; i < 256; i++) {
				uint val = i & 0xC0;

				val <<= 6;
				val |= i & 0x30;

				val <<= 6;
				val |= i & 0x0C;

				val <<= 6;
				val |= i & 0x03;

				table4[i] = val;
				table3[i] = val << 2;
				table2[i] = val << 4;
				table1[i] = val << 6;
			}

			uint sectLength = (uint) buffer.Length / 4;

			fixed (byte* pOutBuffer = outBuffer)
			fixed (byte* pBuffer = buffer) {
				byte* sect1 = pBuffer;
				byte* sect2 = sect1 + sectLength;
				byte* sect3 = sect2 + sectLength;
				byte* sect4 = sect3 + sectLength;

				byte* outP = pOutBuffer;
				byte* outEnd = pOutBuffer + buffer.Length;

				while (outP < outEnd) {
					uint val = table1[*sect1++] | table2[*sect2++] | table3[*sect3++] | table4[*sect4++];

					*outP++ = UnpackVal((byte) (val >> 0));
					*outP++ = UnpackVal((byte) (val >> 8));
					*outP++ = UnpackVal((byte) (val >> 16));
					*outP++ = UnpackVal((byte) (val >> 24));
				}

				int stride = width * depthBytes;

				for (int x = depthBytes; x < stride; x++) {
					outBuffer[x] += outBuffer[x - depthBytes];
				}

				for (uint y = 1; y < height; y++) {
					byte* line = pOutBuffer + y * stride;
					byte* prev = pOutBuffer + (y - 1) * stride;

					for (uint x = 0; x < stride; x++) {
						line[x] += prev[x];
					}
				}
			}
		}

		private static void Unrle(byte[] buffer,
				byte[] cmdBuffer,
				out byte[] outBuffer)
		{
			BitBuffer cmdBits = new BitBuffer(cmdBuffer);

			bool copyFlag = cmdBits.GetBit();

			int outLength = (int) cmdBits.GetEliasGammaValue();
			outBuffer = new byte[outLength];

			int n = 0;
			int index = 0;
			for (int i = 0; i < outLength; i += n) {
				n = (int) cmdBits.GetEliasGammaValue();

				if (copyFlag) {
					Array.Copy(buffer, index, outBuffer, i, n);
					index += n;
				}
				else {
					Array.Clear(outBuffer, i, n);
				}

				copyFlag = !copyFlag;
			}
		}*/
		#endregion

		private static void ProcessImage(BinaryReader reader,
			string file, HG3STDINFO std, HG3IMG img, bool expand = true)
		{
			int depthBytes = (std.DepthBits + 7) / 8;
			int stride = (std.Width * depthBytes + 3) / 4 * 4;

			#region Old Code
			/*byte[] buffer = new byte[img.OriginalDataLength];
			{
				byte[] temp = reader.ReadBytes(img.DataLength);
				Uncompress(buffer, ref img.OriginalDataLength,
					temp, img.DataLength);
			}

			byte[] cmdBuffer = new byte[img.OriginalCmdLength];
			{
				byte[] temp = reader.ReadBytes(img.CmdLength);
				Uncompress(cmdBuffer, ref img.OriginalCmdLength,
					temp, img.CmdLength);
			}
			
			Unrle(buffer, cmdBuffer, out byte[] outBuffer);//, ref outLength);

			byte[] rgbaBuffer = new byte[outBuffer.Length];
			UndeltaFilter(outBuffer, rgbaBuffer, std.Width, std.Height, depthBytes);
			UnrleDeltaFilter(
				buffer, buffer.Length,
				cmdBuffer, cmdBuffer.Length,
				out IntPtr pRgbaBuffer, out int rgbaLength,
				std.Width,
				std.Height,
				depthBytes);*/

			/*byte[] bufferTmp = reader.ReadBytes(img.DataLength);
			byte[] cmdBufferTmp = reader.ReadBytes(img.CmdLength);

			ProcessImage(
				bufferTmp,
				img.DataLength,
				img.OriginalDataLength,
				cmdBufferTmp,
				img.CmdLength,
				img.OriginalCmdLength,
				out IntPtr pRgbaBuffer,
				out int rgbaLength,
				std.Width,
				std.Height,
				depthBytes);

			byte[] rgbaBuffer = new byte[rgbaLength];
			Marshal.Copy(pRgbaBuffer, rgbaBuffer, 0, rgbaLength);
			Marshal.FreeCoTaskMem(pRgbaBuffer);*/
			#endregion

			ProcessImageInternal(reader, std, img, out byte[] rgbaBuffer);
			//int depthBytes = std.DepthBits / 8;

			/*byte[] bufferTmp = reader.ReadBytes(img.DataLength);
			byte[] cmdBufferTmp = reader.ReadBytes(img.CmdLength);

			int rgbaLength;
			byte[] rgbaBuffer = new byte[std.Height * std.Width * 4];
			ProcessImage(
				bufferTmp,
				img.DataLength,
				img.OriginalDataLength,
				cmdBufferTmp,
				img.CmdLength,
				img.OriginalCmdLength,
				rgbaBuffer,
				//out IntPtr pRgbaBuffer,
				out rgbaLength,
				std.Width,
				std.Height,
				depthBytes);
			if (rgbaBuffer.Length != rgbaLength) {
				//Console.WriteLine();
				Console.WriteLine($"\n{rgbaLength - rgbaBuffer.Length}                ");
				Console.Beep(500, 2000);
				return;
			}*/

			if (expand) {
				int offsetXBytes = std.OffsetX * depthBytes;
				//int offsetYVert = std.TotalHeight - std.OffsetY - std.Height;
				
				int expStride = (std.TotalWidth * depthBytes + 3) / 4 * 4;
				byte[] expBuffer = new byte[std.TotalHeight * expStride];

				for (int y = 0; y < std.Height; y++) {
					int src = y * stride;
					//int dst = (y + offsetYVert) * expStride + offsetXBytes;
					int dst = (std.Height - (y + 1) + std.OffsetY) * expStride + offsetXBytes;
					Array.Copy(rgbaBuffer, src, expBuffer, dst, stride);
				}

				std.Width = std.TotalWidth;
				std.Height = std.TotalHeight;
				rgbaBuffer = expBuffer;
			}
			else {
				byte[] flipBuffer = new byte[rgbaBuffer.Length];
				for (int y = 0; y < std.Height; y++) {
					int src = y * stride;
					int dst = (std.Height - (y + 1)) * stride;
					Array.Copy(rgbaBuffer, src, flipBuffer, dst, stride);
				}
				rgbaBuffer = flipBuffer;
			}

			WritePng(file, rgbaBuffer, std.Width, std.Height, std.DepthBits);
		}

		private static void WritePng(string file, byte[] buffer, int width,
			int height, int depthBits)
		{
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			try {
				IntPtr scan0 = handle.AddrOfPinnedObject();
				int depthBytes = (depthBits + 7) / 8;
				int stride = (width * depthBytes + 3) / 4 * 4;
				PixelFormat format;
				switch (depthBits) {
				case 32: format = PixelFormat.Format32bppArgb; break;
				case 24: format = PixelFormat.Format24bppRgb;  break;
				default: throw new Exception($"Unsupported depth bits {depthBits}!");
				}
				using (Bitmap bitmap = new Bitmap(width, height, stride, format, scan0)) {
					//if (flip)
					//	bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
					bitmap.Save(file, ImageFormat.Png);
				}
			}
			finally {
				handle.Free();
			}
		}

		private static byte[] ProcessImageInternal(BinaryReader reader, HG3STDINFO std, HG3IMG img,
			out byte[] rgbaBuffer)
		{
			int depthBytes = (std.DepthBits + 7) / 8;

			byte[] bufferTmp = reader.ReadBytes(img.DataLength);
			byte[] cmdBufferTmp = reader.ReadBytes(img.CmdLength);

			rgbaBuffer = new byte[std.Height * std.Width * 4];
			ProcessImage(
				bufferTmp,
				img.DataLength,
				img.OriginalDataLength,
				cmdBufferTmp,
				img.CmdLength,
				img.OriginalCmdLength,
				out IntPtr pRgbaBuffer,
				out int rgbaLength,
				std.Width,
				std.Height,
				depthBytes);

			rgbaBuffer = new byte[rgbaLength];
			Marshal.Copy(pRgbaBuffer, rgbaBuffer, 0, rgbaLength);
			Marshal.FreeHGlobal(pRgbaBuffer);
			return rgbaBuffer;
		}
	}
}
