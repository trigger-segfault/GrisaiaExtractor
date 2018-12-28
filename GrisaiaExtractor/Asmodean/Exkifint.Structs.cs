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
			/// <summary>
			/// The raw character array signature of the file.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
			public char[] SignatureRaw;
			/// <summary>
			/// The number of <see cref="KIFENTRY"/>s in the KIFINT archive.
			/// </summary>
			public int EntryCount;

			/// <summary>
			/// Gets the signature of the file.
			/// </summary>
			public string Signature => SignatureRaw.ToNullTerminatedString();
		}
		
		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 72, CharSet = CharSet.Ansi)]
		private struct KIFENTRY {
			/// <summary>
			/// We use this to preserve the developer naming fuckups such as the full-width 'ｇ' in
			/// Meikyuu's "bｇ62t.hg3".
			/// </summary>
			private static readonly Encoding JapaneseEncoding = Encoding.GetEncoding(932);
			
			/// <summary>
			/// The raw character array filename of the entry.
			/// </summary>
			[FieldOffset(0)]
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
			public byte[] FileNameRaw;
			/// <summary>
			/// We don't need to pass the <see cref="FileNameRaw"/> during P/Invoke, so we have this info
			/// structure.
			/// </summary>
			[FieldOffset(64)]
			public KIFENTRYINFO Info;
			/// <summary>
			/// The file offset to the entry's data.
			/// </summary>
			[FieldOffset(64)]
			public uint Offset;
			/// <summary>
			/// The file length to the entry's data.
			/// </summary>
			[FieldOffset(68)]
			public int Length;

			/// <summary>
			/// Gets the filename of the entry.
			/// </summary>
			public string FileName => FileNameRaw.ToNullTerminatedString(JapaneseEncoding);
		}
		/// <summary>
		/// We don't need to pass the <see cref="KIFENTRY.FileNameRaw"/> during P/Invoke, so we have this
		/// info structure.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
		private struct KIFENTRYINFO {
			/// <summary>
			/// The file offset to the entry's data.
			/// </summary>
			public uint Offset;
			/// <summary>
			/// The file length to the entry's data.
			/// </summary>
			public int Length;
		}
	}
}
