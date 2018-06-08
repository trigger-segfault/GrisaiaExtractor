using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrisaiaExtractor.Asmodean {
	public static partial class Exkifint {
		private static class MersenneTwister {
			private const int N = 624;
			private const int M = 397;
			private const uint MATRIX_A = 0x9908b0df;
			private const uint UPPER_MASK = 0x80000000;
			private const uint LOWER_MASK = 0x7fffffff;

			private const uint TEMPERING_MASK_B = 0x9d2c5680;
			private const uint TEMPERING_MASK_C = 0xefc60000;
			
			private static uint dummy = 0;
			private static uint[] mt = new uint[N];
			private static uint mti = N + 1;

			private static uint TEMPERING_SHIFT_U(uint y) => y >> 11;
			private static uint TEMPERING_SHIFT_S(uint y) => y << 7;
			private static uint TEMPERING_SHIFT_T(uint y) => y << 15;
			private static uint TEMPERING_SHIFT_L(uint y) => y >> 18;
			
			public static void Seed(int seed) {
				unchecked {
					Seed((uint) seed);
				}
			}

			public static void Seed(uint seed) {
				for (int i = 0; i < N; i++) {
					mt[i] = seed & 0xffff0000;
					seed = 69069 * seed + 1;
					mt[i] |= (seed & 0xffff0000) >> 16;
					seed = 69069 * seed + 1;
				}
				mti = N;
				dummy = mti;
			}

			public static uint GenRand() {
				uint y;
				uint[] mag01 = { 0x0, MATRIX_A };
				/* mag01[x] = x * MATRIX_A  for x=0,1 */

				mti = dummy;

				if (mti >= N) { /* generate N words at one time */
					int kk;

					if (mti == N + 1)   /* if sgenrand() has not been called, */
						Seed(4357); /* a default initial seed is used   */

					for (kk = 0; kk < N - M; kk++) {
						y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
						mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
					}
					for (; kk < N - 1; kk++) {
						y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
						mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1];
					}
					y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
					mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];

					mti = 0;
				}

				y = mt[mti++];
				y ^= TEMPERING_SHIFT_U(y);
				y ^= TEMPERING_SHIFT_S(y) & TEMPERING_MASK_B;
				y ^= TEMPERING_SHIFT_T(y) & TEMPERING_MASK_C;
				y ^= TEMPERING_SHIFT_L(y);
				dummy = mti;

				return y;
			}
		}
	}
}
