// --------------------------------------------------------------------------------------------------
//  NoiseBehaviour.cs
//  ==================
//  A MonoBehaviour for benchmarking OpenSimplex2V noise generation in Unity.
//  Measures sample time and throughput (in MSamples/s)..
//
//  Authored by Dylan Engelbrecht (VoidFletcher):
//      https://github.com/VoidFletcher/OpenSimplex2V
//
//  Based on the OpenSimplex2 algorithm by KdotJPG:
//      https://github.com/KdotJPG/OpenSimplex2
//
//  See LICENSE file in the repository root for license information.
// --------------------------------------------------------------------------------------------------

using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OpenSimplex2V.Samples
{
	public class NoiseBehaviour : MonoBehaviour
	{
		[field: SerializeField]
		public int NumberOfNoiseGenerations { get; private set; } = 10;

		[field: SerializeField]
		public int GridSize { get; private set; }  = 64;

		[field: SerializeField]
		public float NoiseScale { get; private set; } = 0.1f;
		
		[field: SerializeField]
		public long Seed { get; private set; } = 123456789;
		
		private void Start()
		{
			Warm();
			
			for (int i = 0; i < NumberOfNoiseGenerations; i++)
			{
				var stopwatch = Stopwatch.StartNew();
				using var noise = new Noise(GridSize, NoiseScale, Seed);
				stopwatch.Stop();

				int sampleCount = noise.Value.Length;
				double totalMs = stopwatch.Elapsed.TotalMilliseconds;
				double timePerSampleNs = (totalMs / sampleCount) * 1_000_000;
				double samplesPerSecond = 1_000_000_000.0 / timePerSampleNs;

				double msamplesPerSecond = samplesPerSecond / 1_000_000.0;

				Debug.Log(
					$"Generated {sampleCount} noise samples in {totalMs:F3} ms. \n" +
					$"Time per sample: {timePerSampleNs:F1} ns. \n" +
					$"Samples per second: {msamplesPerSecond:F2} MSamples/s");
			}
		}

		private void Warm()
		{
			var stopwatch = Stopwatch.StartNew();
			using var noise = new Noise(GridSize, NoiseScale, Seed);
			stopwatch.Stop();
			Debug.Log($"Warmed noise job in {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
		}
	}
}