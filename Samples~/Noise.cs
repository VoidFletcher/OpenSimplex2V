// --------------------------------------------------------------------------------------------------
//  Noise.cs
//  =========
//  An example for allocating and generating 3D OpenSimplex2V noise data.
//  Used for sampling, benchmarking, and integration testing in the Unity editor.
//
//  Authored by Dylan Engelbrecht (VoidFletcher):
//      https://github.com/VoidFletcher/OpenSimplex2V
//
//  Based on the OpenSimplex2 algorithm by KdotJPG:
//      https://github.com/KdotJPG/OpenSimplex2
//
//  See LICENSE file in the repository root for license information.
// --------------------------------------------------------------------------------------------------

using System;
using Unity.Collections;
using Unity.Jobs;

namespace OpenSimplex2V.Samples
{
	public class Noise : IDisposable
	{
		private NativeArray<float> value;
		public NativeArray<float> Value => value;
		
		public Noise(int gridSize, float noiseScale, long seed)
		{
			var arrayLength = gridSize * gridSize * gridSize;
			value = new NativeArray<float>(arrayLength, Allocator.Persistent);
			GenerateNoise(gridSize, noiseScale, seed);
		}
		
		public unsafe void GenerateNoise(int gridSize, float noiseScale, long seed)
		{
			var arrayLength = gridSize * gridSize * gridSize;

			LookupTable.Gradients.Prepare();
			var job = new NoiseGenerationJob
			{
				GridSize = gridSize,
				GridSizeSquared = gridSize * gridSize,
				NoiseScale = noiseScale,
				Noise = value,
				Seed = seed,
				GradientPtr = LookupTable.Gradients.GetPointer()
			};

			var handle = job.Schedule(arrayLength, 128);
			handle.Complete();
		}
		
		public void Dispose()
		{
			if (value.IsCreated)
			{
				
				value.Dispose();
			}
		}
	}
}