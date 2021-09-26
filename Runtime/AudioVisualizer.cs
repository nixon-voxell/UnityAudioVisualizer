/*
This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software Foundation,
Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.

The Original Code is Copyright (C) 2020 Voxell Technologies and Contributors.
All rights reserved.
*/

using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Voxell.Inspector;
using Voxell.Mathx;

namespace Voxell.Audio
{
  public partial class AudioCore
  {
    [Header("Audio Visualizer Settings")]
    public AudioSource audioSource;
    public AudioProfile audioProfile;
    public AudioProcessor audioProcessor;

    public VisualEffect audioVFX;
    public MeshFilter meshFilter;
    public Mesh sampleMesh;
    [Tooltip("Damping value to multiply the velocity of each triangles each frame.")]
    [Range(0, 1)]
    public float velocityMultiplier = 0.95f;
    public int batchSize = 128;
    [Tooltip("Randomization of triangle index, set to 0 if you don't want to randomize it.")]
    public uint seed;
    [InspectOnly] public int totalTriangles;

    private Mesh modifiedSampleMesh;
    private NativeArray<float3> normals;
    private NativeArray<int> triangles;
    private NativeArray<float> samples;
    private NativeArray<int> bandDistribution;
    private NativeArray<float3> vertices;
    private NativeArray<float> prevBands;
    private NativeArray<float> bandVelocities;

    private void InitAudioVisualizer()
    {
      totalTriangles = (int)sampleMesh.GetIndexCount(0)/3;
      audioProfile.bandSize = totalTriangles;

      audioProcessor = new AudioProcessor(ref audioSource, ref audioProfile);

      MeshUtil.DeepCopyMesh(ref sampleMesh, out modifiedSampleMesh);
      audioVFX.SetMesh(VFXPropertyId.mesh_sampleMesh, modifiedSampleMesh);
      audioVFX.SetInt(VFXPropertyId.int_triangleCount, totalTriangles);
      modifiedSampleMesh.MarkDynamic();
      meshFilter.mesh = modifiedSampleMesh;

      // transferring mesh data to native arrays to be processed parallely
      Mesh.MeshDataArray sampleMeshData = Mesh.AcquireReadOnlyMeshData(sampleMesh);
      normals = MeshUtil.NativeGetNormals(sampleMeshData[0], Allocator.Persistent);
      normals.AsReadOnly();

      triangles = MeshUtil.NativeGetIndices(sampleMeshData[0], Allocator.Persistent);
      triangles.AsReadOnly();

      vertices = MeshUtil.NativeGetVertices(sampleMeshData[0], Allocator.Persistent);

      // audio processing attributes
      samples = new NativeArray<float>(audioProfile.sampleSize, Allocator.Persistent);
      bandDistribution = new NativeArray<int>(audioProfile.bandSize+1, Allocator.Persistent);
      bandDistribution.CopyFrom(audioProcessor.bandDistribution);
      bandDistribution.AsReadOnly();

      prevBands = new NativeArray<float>(totalTriangles, Allocator.Persistent);
      prevBands.CopyFrom(prevBands);

      bandVelocities = new NativeArray<float>(totalTriangles, Allocator.Persistent);
      bandVelocities.CopyFrom(bandVelocities);

      sampleMeshData.Dispose();

      if (seed != 0)
      {
        // if randomized is turned on
        int[] seqArray = MathUtil.GenerateSeqArray(totalTriangles);
        MathUtil.ShuffleArray<int>(ref seqArray, seed);

        // triangle indices
        NativeArray<int> trianglesCopy = new NativeArray<int>(triangles, Allocator.Temp);

        for (int s=0; s < seqArray.Length; s++)
        {
          triangles[s*3] = trianglesCopy[seqArray[s]*3];
          triangles[s*3 + 1] = trianglesCopy[seqArray[s]*3 + 1];
          triangles[s*3 + 2] = trianglesCopy[seqArray[s]*3 + 2];
        }

        trianglesCopy.Dispose();
      }
    }

    private void UpdateAudioVisualizer()
    {
      audioProcessor.SampleSpectrum();

      // copy audio samples to native array samples
      samples.CopyFrom(audioProcessor.samples);

      AudioMeshVisualizer audioMeshVisualizer = new AudioMeshVisualizer
      {
        normals = normals,
        triangles = triangles,
        samples = samples,
        bandDistribution = bandDistribution,
        power = audioProfile.power,
        scale = audioProfile.scale,
        bandAverage = audioProcessor.bandAverage,
        vertices = vertices,
        prevBands = prevBands,
        bandVelocities = bandVelocities,
        velocityMultiplier = velocityMultiplier,
        deltaTime = Time.deltaTime
      };

      JobHandle jobHandle = audioMeshVisualizer.Schedule<AudioMeshVisualizer>(totalTriangles, batchSize);
      jobHandle.Complete();

      modifiedSampleMesh.SetVertices<float3>(vertices);
    }

    void OnDisable()
    {
      NativeUtil.DisposeArray(ref normals);
      NativeUtil.DisposeArray(ref triangles);
      NativeUtil.DisposeArray(ref samples);
      NativeUtil.DisposeArray(ref bandDistribution);
      NativeUtil.DisposeArray(ref vertices);
      NativeUtil.DisposeArray(ref prevBands);
      NativeUtil.DisposeArray(ref bandVelocities);
    }
  }
}

[BurstCompile(
  CompileSynchronously=true,
  FloatPrecision=FloatPrecision.Medium,
  FloatMode=FloatMode.Fast
)]
public struct AudioMeshVisualizer : IJobParallelFor
{
  [ReadOnly] public NativeArray<float3> normals;
  [ReadOnly] public NativeArray<int> triangles;

  [ReadOnly] public NativeArray<float> samples;
  [ReadOnly] public NativeArray<int> bandDistribution;
  public float power;
  public float scale;
  public int bandAverage;

  [NativeDisableContainerSafetyRestriction]
  public NativeArray<float3> vertices;

  public NativeArray<float> prevBands;
  public NativeArray<float> bandVelocities;
  public float velocityMultiplier;
  public float deltaTime;

  public void Execute(int index)
  {
    int t0 = triangles[index*3];
    int t1 = triangles[index*3 + 1];
    int t2 = triangles[index*3 + 2];

    float3 normal = normals[t0];

    float band = CreateBand(bandDistribution[index], bandDistribution[index+1]);
    band = math.pow(math.sqrt(band), power) * scale;

    bandVelocities[index] += band - prevBands[index];
    float magnitude = bandVelocities[index] * deltaTime;
    float3 displacement = normal * magnitude;

    vertices[t0] += displacement;
    vertices[t1] += displacement;
    vertices[t2] += displacement;

    bandVelocities[index] *= velocityMultiplier;
    prevBands[index] += magnitude;
  }

  private float CreateBand(int start, int end)
  {
    float totalFreq = 0.0f;
    for (int b=start; b < end; b++) totalFreq += samples[b]/bandAverage;
    return totalFreq;
  }
}