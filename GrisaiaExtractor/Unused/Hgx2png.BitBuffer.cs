using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Asmodean {
	public static partial class Hgx2png {
		private class BitBuffer {

			private byte[] buffer;
			private int index;

			public BitBuffer(byte[] buffer) {
				this.buffer = buffer;
				this.index = 0;
			}

			public bool GetBit() {
				return ((buffer[index / 8] >> (index++ % 8)) & 1) == 1;
			}

			// Didn't expect to see this in the wild...
			public uint GetEliasGammaValue() {
				uint value = 0;
				int digits = 0;

				while (!GetBit())
					digits++;

				value = 1U << digits;

				while (digits-- != 0) {
					if (GetBit())
						value |= 1U << digits;
				}

				return value;
			}
		}
	}
}
