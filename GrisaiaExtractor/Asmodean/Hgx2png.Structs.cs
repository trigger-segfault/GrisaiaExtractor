using GrisaiaExtractor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Asmodean {
	public static partial class Hgx2png {
		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 20, CharSet = CharSet.Ansi)]
		private struct HG3HDR {
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
			public char[] SignatureRaw; // "HG-3"
			public int Unknown1;
			public int Unknown2;
			public int Unknown3;
			public int EntryCount;

			public string Signature => SignatureRaw.ToNullTerminatedString();
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16, CharSet = CharSet.Ansi)]
		private struct HG3TAG {
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
			public char[] SignatureRaw;
			public int OffsetNext;
			public int Length;

			public string Signature => SignatureRaw.ToNullTerminatedString();
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 40)]
		private struct HG3STDINFO {
			public int Width;
			public int Height;
			public int DepthBits;
			public int OffsetX;
			public int OffsetY;
			public int TotalWidth;
			public int TotalHeight;
			public int Unknown1;
			public int Unknown2;
			public int Unknown3;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 24)]
		private struct HG3IMG {
			public int Unknown;
			public int Height;
			public int DataLength;
			public int OriginalDataLength;
			public int CmdLength;
			public int OriginalCmdLength;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
		private struct HG3IMGAL {
			public int Length;
			public int OriginalLength;
		};

	}
}
