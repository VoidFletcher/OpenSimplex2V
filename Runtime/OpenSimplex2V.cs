// --------------------------------------------------------------------------------------------------
//  OpenSimplex2V.cs
//  =================
//  High-performance OpenSimplex2V 3D noise implementation for Unity,
//  optimized for Burst and Unity Jobs.
//
//  Authored by Dylan Engelbrecht (VoidFletcher):
//      https://github.com/VoidFletcher/OpenSimplex2V
//
//  Based on the OpenSimplex2 algorithm by KdotJPG:
//      https://github.com/KdotJPG/OpenSimplex2
//
//  See LICENSE file in the repository root for license information.
// --------------------------------------------------------------------------------------------------

using System.Runtime.CompilerServices;
using Unity.Burst;

namespace OpenSimplex2V
{
	[BurstCompile]
	public static class OpenSimplex2V
	{
		private const long PrimeX = 0x5205402B9270C86FL;
		private const long PrimeY = 0x598CD327003817B5L;
		private const long PrimeZ = 0x5BCC226E9FA0BACBL;

		private const long HashMultiplier = 0x53A3F72DEEC546F5L;
		private const float FallbackRotate3 = 2.0f / 3.0f;

		private const int NGrads3DExponent = 8;
		private const int NGrads3D = 1 << NGrads3DExponent;
		private const float RSquared3D = 3.0f / 4.0f;
		private const int Grad3DMask = (NGrads3D - 1) << 1;
		private const int Grad3DHashShift = 64 - NGrads3DExponent + 2;

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe float Noise3_Fallback(long seed, float x, float y, float z, float* gradientPtr)
		{
			// Re-orient the cubic lattices via rotation, to produce a familiar look.
			// Orthonormal rotation. Not a skew transform.
			float r = FallbackRotate3 * (x + y + z);
			float xr = r - x, yr = r - y, zr = r - z;

			// Evaluate both lattices to form a BCC lattice.
			return Noise3UnrotatedBase(seed, xr, yr, zr, gradientPtr);
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static unsafe float Noise3UnrotatedBase(long seed, float xr, float yr, float zr, float* gradientPtr)
		{
			// Get base points and offsets.
			int xrb = FastFloor(xr), yrb = FastFloor(yr), zrb = FastFloor(zr);
			float xi = (xr - xrb), yi = (yr - yrb), zi = (zr - zrb);

			// Prime pre-multiplication for hash. Also flip seed for second lattice copy.
			long xrbp = xrb * PrimeX, yrbp = yrb * PrimeY, zrbp = zrb * PrimeZ;
			long seed2 = seed ^ -0x52D547B2E96ED629L;

			// -1 if positive, 0 if negative.
			int xNMask = (int)(-0.5f - xi), yNMask = (int)(-0.5f - yi), zNMask = (int)(-0.5f - zi);

			// First vertex.
			float x0 = xi + xNMask, y0 = yi + yNMask, z0 = zi + zNMask;
			float a0 = RSquared3D - Pow2(x0) - Pow2(y0) - Pow2(z0);
			long xhp = xrbp + (xNMask & PrimeX), yhp = yrbp + (yNMask & PrimeY), zhp = zrbp + (zNMask & PrimeZ);
			long xhpFlip = xrbp + (~xNMask & PrimeX), yhpFlip = yrbp + (~yNMask & PrimeY), zhpFlip = zrbp + (~zNMask & PrimeZ);
			float sample = Pow4(a0) * Gradient(seed, xhp, yhp, zhp, x0, y0, z0, gradientPtr);

			// Second vertex.
			float x1 = xi - 0.5f, y1 = yi - 0.5f, z1 = zi - 0.5f;
			float a1 = RSquared3D - Pow2(x1) - Pow2(y1) - Pow2(z1);
			sample += (a1 * a1) * (a1 * a1) * Gradient(seed2, xrbp + PrimeX, yrbp + PrimeY, zrbp + PrimeZ, x1, y1, z1, gradientPtr);

			// Shortcuts for building the remaining falloffs.
			// Derived by subtracting the polynomials with the offsets plugged in.
			int xMaskFlip = xNMask | 1;
			int yMaskFlip = yNMask | 1;
			int zMaskFlip = zNMask | 1;
			float xAFlipMask0 = (xMaskFlip << 1) * x1;
			float yAFlipMask0 = (yMaskFlip << 1) * y1;
			float zAFlipMask0 = (zMaskFlip << 1) * z1;
			float xAFlipMask1 = (-2 - (xNMask << 2)) * x1 - 1.0f;
			float yAFlipMask1 = (-2 - (yNMask << 2)) * y1 - 1.0f;
			float zAFlipMask1 = (-2 - (zNMask << 2)) * z1 - 1.0f;

			bool skip5 = false;
			float a2 = xAFlipMask0 + a0;
			if (a2 > 0)
			{
				float x2 = x0 - xMaskFlip, y2 = y0, z2 = z0;
				sample += Pow4(a2) * Gradient(seed, xhpFlip, yhp, zhp, x2, y2, z2,gradientPtr);
			}
			else
			{
				float a3 = yAFlipMask0 + zAFlipMask0 + a0;
				if (a3 > 0)
				{
					float x3 = x0, y3 = y0 - yMaskFlip, z3 = z0 - zMaskFlip;
					sample += Pow4(a3) * Gradient(seed, xhp, yhpFlip, zhpFlip, x3, y3, z3, gradientPtr);
				}

				float a4 = xAFlipMask1 + a1;
				if (a4 > 0)
				{
					float x4 = xMaskFlip + x1, y4 = y1, z4 = z1;
					sample += Pow4(a4) * Gradient(seed2, xrbp + (xNMask & unchecked(PrimeX * 2)), yrbp + PrimeY, zrbp + PrimeZ, x4, y4, z4, gradientPtr);
					skip5 = true;
				}
			}

			bool skip9 = false;
			float a6 = yAFlipMask0 + a0;
			if (a6 > 0)
			{
				float x6 = x0, y6 = y0 - yMaskFlip, z6 = z0;
				sample += Pow4(a6) * Gradient(seed, xhp, yhpFlip, zhp, x6, y6, z6, gradientPtr);
			}
			else
			{
				float a7 = xAFlipMask0 + zAFlipMask0 + a0;
				if (a7 > 0)
				{
					float x7 = x0 - xMaskFlip, y7 = y0, z7 = z0 - zMaskFlip;
					sample += Pow4(a7) * Gradient(seed, xhp, yhp, zhpFlip, x7, y7, z7,gradientPtr);
				}

				float a8 = yAFlipMask1 + a1;
				if (a8 > 0)
				{
					float x8 = x1, y8 = yMaskFlip + y1, z8 = z1;
					sample += Pow4(a8) * Gradient(seed2, xrbp + PrimeX, yrbp + (yNMask & (PrimeY << 1)), zrbp + PrimeZ, x8, y8, z8, gradientPtr);
					skip9 = true;
				}
			}

			bool skipD = false;
			float aA = zAFlipMask0 + a0;
			if (aA > 0)
			{
				float xA = x0, yA = y0, zA = z0 - zMaskFlip;
				sample += Pow4(aA) * Gradient(seed, xhp, yhp, zhpFlip, xA, yA, zA, gradientPtr);
			}
			else
			{
				float aB = xAFlipMask0 + yAFlipMask0 + a0;
				if (aB > 0)
				{
					float xB = x0 - xMaskFlip, yB = y0 - yMaskFlip, zB = z0;
					sample += Pow4(aB) * Gradient(seed, xhp, yhpFlip, zhp, xB, yB, zB, gradientPtr);
				}

				float aC = zAFlipMask1 + a1;
				if (aC > 0)
				{
					float xC = x1, yC = y1, zC = zMaskFlip + z1;
					sample += Pow4(aC) * Gradient(seed2, xrbp + PrimeX, yrbp + PrimeY, zrbp + (zNMask & (PrimeZ << 1)), xC, yC, zC, gradientPtr);
					skipD = true;
				}
			}

			if (!skip5)
			{
				float a5 = yAFlipMask1 + zAFlipMask1 + a1;
				if (a5 > 0)
				{
					float x5 = x1, y5 = yMaskFlip + y1, z5 = zMaskFlip + z1;
					sample += Pow4(a5) * Gradient(seed2, xrbp + PrimeX, yrbp + (yNMask & (PrimeY << 1)), zrbp + (zNMask & (PrimeZ << 1)), x5, y5, z5, gradientPtr);
				}
			}

			if (!skip9)
			{
				float a9 = xAFlipMask1 + zAFlipMask1 + a1;
				if (a9 > 0)
				{
					float x9 = xMaskFlip + x1, y9 = y1, z9 = zMaskFlip + z1;
					sample += Pow4(a9) * Gradient(seed2, xrbp + (xNMask & unchecked(PrimeX * 2)), yrbp + PrimeY, zrbp + (zNMask & (PrimeZ << 1)), x9, y9, z9, gradientPtr);
				}
			}

			if (!skipD)
			{
				float aD = xAFlipMask1 + yAFlipMask1 + a1;
				if (aD > 0)
				{
					float xD = xMaskFlip + x1, yD = yMaskFlip + y1, zD = z1;
					sample += Pow4(aD) * Gradient(seed2, xrbp + (xNMask & (PrimeX << 1)), yrbp + (yNMask & (PrimeY << 1)), zrbp + PrimeZ, xD, yD, zD, gradientPtr);
				}
			}

			return sample;
		}
		
		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Pow2(float a)
		{
			return a * a;
		}
		
		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Pow4(float a)
		{
			float sq = a * a;
			return sq * sq;
		}
		
		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static unsafe float Gradient(long seed, long xrvp, long yrvp, long zrvp, float dx, float dy, float dz, float* gradientPtr)
		{
			long hash = ((seed ^ xrvp) ^ (yrvp ^ zrvp)) * HashMultiplier;
			hash ^= hash >> Grad3DHashShift;
			int gi = (int)hash & Grad3DMask;

			return gradientPtr[gi] * dx + gradientPtr[gi | 1] * dy + gradientPtr[gi | 2] * dz;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int FastFloor(float x)
		{
			int xi = (int)x;
			return x < xi ? xi - 1 : xi;
		}
	}
}