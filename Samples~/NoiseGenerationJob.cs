// --------------------------------------------------------------------------------------------------
//  NoiseGenerationJob.cs
//  =======================
//  A Burst-compatible IJobParallelFor implementation for generating OpenSimplex2V noise.
//  Used in sample workflows to evaluate multithreaded procedural generation. There are many 
//  approaches to generating OpenSimplex2V noise depending on the use case, and this is just one example.
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
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace OpenSimplex2V.Samples
{
	[BurstCompile]
	public unsafe struct NoiseGenerationJob : IJobParallelFor
	{
		public int GridSize;
		public int GridSizeSquared;
		public float NoiseScale;
		public long Seed;

		public NativeArray<float> Noise;

		[NativeDisableUnsafePtrRestriction]
		public float* GradientPtr;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Execute(int index)
		{
			int z = index / GridSizeSquared;
			int y = (index / GridSize) % GridSize;
			int x = index % GridSize;

			float3 pos = new float3(x, y, z) * NoiseScale;

			float noiseValue = OpenSimplex2V.Noise3_Fallback(Seed, pos.x, pos.y, pos.z, GradientPtr);

			Noise[index] = noiseValue;
		}
	}
}