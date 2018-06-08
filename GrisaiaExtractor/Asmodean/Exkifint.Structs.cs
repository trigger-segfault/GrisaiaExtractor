using GrisaiaExtractor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Asmodean {
	public static partial class Exkifint {

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8, CharSet = CharSet.Ansi)]
		private struct KIFHDR {
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
			public char[] SignatureRaw;
			public int EntryCount;
			
			public string Signature => SignatureRaw.ToNullTerminatedString();
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 72, CharSet = CharSet.Ansi)]
		private struct KIFENTRY {
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
			public char[] FileNameRaw;
			public uint Offset;
			public int Length;

			public string FileName => FileNameRaw.ToNullTerminatedString();
		}
	}
}
