using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Asmodean {
	public static partial class Exkifint {
		public unsafe partial class Blowfish {

			private const int MAXKEYBYTES = 56;
			private const int NPASS = 16;

			private uint[] PArray;
			private uint[,] SBoxes;

			public Blowfish() {
				PArray = new uint[18];
				SBoxes = new uint[4,256];
			}

			/*private uint S(aword x, int i) {
				return SBoxes[i, x.bytes[i]];
			}*/


			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private uint bf_F(aword x) {
				return ((SBoxes[0, x.byte0] + SBoxes[1, x.byte1]) ^ SBoxes[2, x.byte2]) + SBoxes[3, x.byte3];
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void ROUND(ref aword a, aword x, int n) {
				//a.dword ^= bf_F(x) ^ PArray[n];
				a.dword ^= (((SBoxes[0, x.byte0] + SBoxes[1, x.byte1]) ^ SBoxes[2, x.byte2]) + SBoxes[3, x.byte3]) ^ PArray[n];
			}

			//[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private uint ROUND(aword x, int n) {
				//a.dword ^= bf_F(x) ^ PArray[n];
				Count++;
				return (((SBoxes[0, x.byte0] + SBoxes[1, x.byte1]) ^ SBoxes[2, x.byte2]) + SBoxes[3, x.byte3]) ^ PArray[n];
			}

			public static int Count = 0;
			private unsafe void Blowfish_encipher(uint* xl, uint* xr) {
				aword Xl = new aword();
				aword Xr = new aword();

				Xl.dword = *xl;
				Xr.dword = *xr;

				Xl.dword ^= PArray[0];
				/*ROUND(ref Xr, Xl, 1);
				ROUND(ref Xl, Xr, 2);
				ROUND(ref Xr, Xl, 3);
				ROUND(ref Xl, Xr, 4);
				ROUND(ref Xr, Xl, 5);
				ROUND(ref Xl, Xr, 6);
				ROUND(ref Xr, Xl, 7);
				ROUND(ref Xl, Xr, 8);
				ROUND(ref Xr, Xl, 9);
				ROUND(ref Xl, Xr, 10);
				ROUND(ref Xr, Xl, 11);
				ROUND(ref Xl, Xr, 12);
				ROUND(ref Xr, Xl, 13);
				ROUND(ref Xl, Xr, 14);
				ROUND(ref Xr, Xl, 15);
				ROUND(ref Xl, Xr, 16);*/
				Xr.dword ^= ROUND(Xl, 1);
				Xl.dword ^= ROUND(Xr, 2);
				Xr.dword ^= ROUND(Xl, 3);
				Xl.dword ^= ROUND(Xr, 4);
				Xr.dword ^= ROUND(Xl, 5);
				Xl.dword ^= ROUND(Xr, 6);
				Xr.dword ^= ROUND(Xl, 7);
				Xl.dword ^= ROUND(Xr, 8);
				Xr.dword ^= ROUND(Xl, 9);
				Xl.dword ^= ROUND(Xr, 10);
				Xr.dword ^= ROUND(Xl, 11);
				Xl.dword ^= ROUND(Xr, 12);
				Xr.dword ^= ROUND(Xl, 13);
				Xl.dword ^= ROUND(Xr, 14);
				Xr.dword ^= ROUND(Xl, 15);
				Xl.dword ^= ROUND(Xr, 16);
				Xr.dword ^= PArray[17];

				*xr = Xl.dword;
				*xl = Xr.dword;
			}

			private unsafe void Blowfish_decipher(uint* xl, uint* xr) {
				aword Xl = new aword();
				aword Xr = new aword();

				Xl.dword = *xl;
				Xr.dword = *xr;

				Xl.dword ^= PArray[17];
				/*ROUND(ref Xr, Xl, 16);
				ROUND(ref Xl, Xr, 15);
				ROUND(ref Xr, Xl, 14);
				ROUND(ref Xl, Xr, 13);
				ROUND(ref Xr, Xl, 12);
				ROUND(ref Xl, Xr, 11);
				ROUND(ref Xr, Xl, 10);
				ROUND(ref Xl, Xr, 9);
				ROUND(ref Xr, Xl, 8);
				ROUND(ref Xl, Xr, 7);
				ROUND(ref Xr, Xl, 6);
				ROUND(ref Xl, Xr, 5);
				ROUND(ref Xr, Xl, 4);
				ROUND(ref Xl, Xr, 3);
				ROUND(ref Xr, Xl, 2);
				ROUND(ref Xl, Xr, 1);*/
				Xr.dword ^= ROUND(Xl, 16);
				Xl.dword ^= ROUND(Xr, 15);
				Xr.dword ^= ROUND(Xl, 14);
				Xl.dword ^= ROUND(Xr, 13);
				Xr.dword ^= ROUND(Xl, 12);
				Xl.dword ^= ROUND(Xr, 11);
				Xr.dword ^= ROUND(Xl, 10);
				Xl.dword ^= ROUND(Xr, 9);
				Xr.dword ^= ROUND(Xl, 8);
				Xl.dword ^= ROUND(Xr, 7);
				Xr.dword ^= ROUND(Xl, 6);
				Xl.dword ^= ROUND(Xr, 5);
				Xr.dword ^= ROUND(Xl, 4);
				Xl.dword ^= ROUND(Xr, 3);
				Xr.dword ^= ROUND(Xl, 2);
				Xl.dword ^= ROUND(Xr, 1);
				Xr.dword ^= PArray[0];

				*xl = Xr.dword;
				*xr = Xl.dword;
			}

			// constructs the enctryption sieve
			public void Initialize(byte* key, int keybytes) {
				int i, j;
				uint data, datal, datar;
				aword temp = new aword();

				// first fill arrays from data tables
				for (i = 0; i < 18; i++)
					PArray[i] = bf_P[i];

				for (i = 0; i < 4; i++) {
					for (j = 0; j < 256; j++)
						SBoxes[i,j] = bf_S[i,j];
				}


				j = 0;
				for (i = 0; i < NPASS + 2; ++i) {
					temp.dword = 0;
					temp.byte0 = key[j];
					temp.byte1 = key[(j + 1) % keybytes];
					temp.byte2 = key[(j + 2) % keybytes];
					temp.byte3 = key[(j + 3) % keybytes];
					data = temp.dword;
					PArray[i] ^= data;
					j = (j + 4) % keybytes;
				}

				datal = 0;
				datar = 0;

				for (i = 0; i < NPASS + 2; i += 2) {
					Blowfish_encipher(&datal, &datar);
					PArray[i] = datal;
					PArray[i + 1] = datar;
				}

				for (i = 0; i < 4; ++i) {
					for (j = 0; j < 256; j += 2) {
						Blowfish_encipher(&datal, &datar);
						SBoxes[i,j] = datal;
						SBoxes[i,j + 1] = datar;
					}
				}
			}
			
			// get output length, which must be even MOD 8
			private int GetOutputLength(int lInputLong) {
				int lVal;

				lVal = lInputLong % 8;  // find out if uneven number of bytes atthe end
				if (lVal != 0)
					return lInputLong + 8 - lVal;
				else
					return lInputLong;
			}

			// Decode pIntput into pOutput.  Input length in lSize.  Inputbuffer and
			// output buffer can be the same, but be sure buffer length is even MOD8.
			public unsafe void Decode(byte[] pInputArray, byte[] pOutputArray, int lSize) {
				uint lCount;
				byte* pi, po;
				int i;
				bool SameDest = pInputArray == pOutputArray;

				fixed (byte* pInputFixed = pInputArray)
				fixed (byte* pOutputFixed = pOutputArray) {
					byte* pInput = pInputFixed;
					byte* pOutput = pOutputFixed;

					for (lCount = 0; lCount < lSize; lCount += 8) {
						if (SameDest)   // if encoded data is being written into inputbuffer
						{
							Blowfish_decipher((uint*) pInput,
								(uint*) (pInput + 4));
							pInput += 8;
						}
						else            // output buffer not equal to inputbuffer
						{               // so copy input to output before decoding
							pi = pInput;
							po = pOutput;
							for (i = 0; i < 8; i++)
								*po++ = *pi++;
							Blowfish_decipher((uint*) pOutput,
								(uint*) (pOutput + 4));
							pInput += 8;
							pOutput += 8;
						}
					}
				}
			}


			public void Set_Key(byte* key, int keybytes) {
				Initialize(key, keybytes);
			}

			public void Decrypt(byte[] pInput, int lSize) {
				if (lSize != GetOutputLength(lSize))
					throw new Exception("Input len != Output len");
				
				byte[] pOutput = new byte[lSize];
				Decode(pInput, pOutput, lSize);

				Array.Copy(pOutput, pInput, lSize);
			}

			[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
			private struct aword {
				[FieldOffset(0)]
				public uint dword;
				[FieldOffset(0)]
				public byte byte3;
				[FieldOffset(1)]
				public byte byte2;
				[FieldOffset(2)]
				public byte byte1;
				[FieldOffset(3)]
				public byte byte0;

				/*public byte byte3 {
					get { return (byte) (dword & 0xFF); }
					set { dword = (dword & 0xFFFFFF00) ^ value; }
				}
				public byte byte2 {
					get { return (byte) ((dword >> 8) & 0xFF); }
					set { dword = (dword & 0xFFFF00FF) ^ (uint) (value << 8); }
				}
				public byte byte1 {
					get { return (byte) ((dword >> 16) & 0xFF); }
					set { dword = (dword & 0xFF00FFFF) ^ (uint) (value << 16); }
				}
				public byte byte0 {
					get { return (byte) ((dword >> 24) & 0xFF); }
					set { dword = (dword & 0x00FFFFFF) ^ (uint) (value << 24); }
				}*/
			}
		}
	}
}
