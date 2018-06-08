using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Extensions {
	public static class BinaryExtensions {
		public static TStruct ReadStruct<TStruct>(this BinaryReader reader)
			where TStruct : struct {
			byte[] buffer = reader.ReadBytes(Marshal.SizeOf<TStruct>());

			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

			TStruct result = Marshal.PtrToStructure<TStruct>(
				handle.AddrOfPinnedObject());
			handle.Free();
			return result;
		}

		public static TStruct[] ReadStructArray<TStruct>(this BinaryReader reader, int length)
			where TStruct : struct {
			TStruct[] result = new TStruct[length];
			int size = Marshal.SizeOf<TStruct>();
			byte[] buffer = reader.ReadBytes(size * length);

			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IntPtr ptr = handle.AddrOfPinnedObject();

			for (int i = 0; i < length; i++) {
				IntPtr ins = new IntPtr(ptr.ToInt32() + i * size);
				result[i] = Marshal.PtrToStructure<TStruct>(ins);
			}

			handle.Free();
			return result;
		}

		public static void WriteStruct<TStruct>(this BinaryWriter writer, TStruct value)
			where TStruct : struct {
			byte[] buffer = new byte[Marshal.SizeOf<TStruct>()];
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

			Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), true);
			writer.Write(buffer);
			handle.Free();
		}

		public static string ReadString(this BinaryReader reader, int length) {
			return new string(reader.ReadChars(length));
		}

		public static bool IsEndOfStream(this Stream stream) {
			return stream.Position >= stream.Length;
		}
	}
}
