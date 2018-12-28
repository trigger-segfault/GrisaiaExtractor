using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Asmodean {
	public static partial class Exkifint {
		
		private const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020;
		
		[DllImport("kernel32.dll")]
		private extern static IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hFile, uint dwFlags);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.I1)]
		private extern static bool FreeLibrary(IntPtr hLibModule);

		[DllImport("kernel32.dll")]
		private extern static IntPtr FindResource(IntPtr hModule, string lpName, string lpType);

		[DllImport("kernel32.dll", SetLastError = true)]
		private extern static IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

		[DllImport("kernel32.dll", SetLastError = true)]
		private extern static uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

		[DllImport("kernel32.dll")]
		private extern static IntPtr LockResource(IntPtr hGlobal);


		[DllImport("asmodean.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[return: MarshalAs(UnmanagedType.LPStr)]
		private extern static void DecryptVCode2(
			byte[] keyBuffer,
			int keyLength,
			byte[] vcode2Buffer,
			int vcode2Length);

		[DllImport("asmodean.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static void DecryptEntry(
			ref KIFENTRYINFO entry,
			uint fileKey);

		[DllImport("asmodean.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static void DecryptData(
			byte[] buffer,
			int length,
			uint fileKey);
	}
}
