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
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using SmartAssistant.Core.Inspector;

namespace SmartAssistant.Audio
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
    public int batchSize = 100;
    [InspectOnly] public int totalTris;

    private Mesh modifiedSampleMesh;
    private NativeArray<float3> originVertices;
    private NativeArray<float3> normals;
    private NativeArray<int> triangles;
    private NativeArray<float3> vertices;
    private NativeArray<float> samples;
    private NativeArray<int> bandDistribution;

    private void InitAudioVisualizer()
    {
      totalTris = (int)sampleMesh.GetIndexCount(0)/3;
      audioProfile.bandSize = totalTris;

      audioProcessor = new AudioProcessor(ref audioSource, ref audioProfile);

      MeshUtil.DeepCopyMesh(ref sampleMesh, out modifiedSampleMesh);
      audioVFX.SetMesh(VFXPropertyId.mesh_sampleMesh, modifiedSampleMesh);
      audioVFX.SetInt(VFXPropertyId.int_triangleCount, totalTris);
      modifiedSampleMesh.MarkDynamic();
      meshFilter.mesh = modifiedSampleMesh;

      // transferring mesh data to native arrays to be processed parallely
      Mesh.MeshDataArray sampleMeshData = Mesh.AcquireReadOnlyMeshData(sampleMesh);
      originVertices = MeshUtil.NativeGetVertices(sampleMeshData[0], Allocator.Persistent);
      originVertices.AsReadOnly();
      normals = MeshUtil.NativeGetNormals(sampleMeshData[0], Allocator.Persistent);
      normals.AsReadOnly();
      triangles = MeshUtil.NativeGetIndices(sampleMeshData[0], Allocator.Persistent);
      triangles.AsReadOnly();
      vertices = MeshUtil.NativeGetVertices(sampleMeshData[0], Allocator.Persistent);

      // audio processing attributes
      samples = new NativeArray<float>(audioProfile.sampleSize, Allocator.Persistent);
      bandDistribution = new NativeArray<int>(audioProfile.bandSize+1, Allocator.Persistent);
      MathUtil.CopyToNativeArray<int>(ref audioProcessor.bandDistribution, ref bandDistribution);

      sampleMeshData.Dispose();
    }

    private void UpdateAudioVisualizer()
    {
      audioProcessor.SampleSpectrum();

      // copy audio samples to native array samples
      MathUtil.CopyToNativeArray<float>(ref audioProcessor.samples, ref samples);

      AudioMeshVisualizer audioMeshVisualizer = new AudioMeshVisualizer
      {
        originVertices = originVertices,
        normals = normals,
        triangles = triangles,
        samples = samples,
        bandDistribution = bandDistribution,
        power = audioProfile.power,
        scale = audioProfile.scale,
        bandAverage = audioProcessor.bandAverage,
        vertices = vertices
      };

      JobHandle jobHandle = audioMeshVisualizer.Schedule<AudioMeshVisualizer>(totalTris, batchSize);
      jobHandle.Complete();

      modifiedSampleMesh.SetVertices(vertices);
    }

    void OnDisable()
    {
      originVertices.Dispose();
      normals.Dispose();
      triangles.Dispose();
      vertices.Dispose();
      samples.Dispose();
      bandDistribution.Dispose();
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
  [ReadOnly] public NativeArray<float3> originVertices;
  [ReadOnly] public NativeArray<float3> normals;
  [ReadOnly] public NativeArray<int> triangles;

  [ReadOnly] public NativeArray<float> samples;
  [ReadOnly] public NativeArray<int> bandDistribution;
  public float power;
  public float scale;
  public int bandAverage;
  // public int maxBuffer;
  // private NativeArray<float> buffer;

  // [ReadOnly] public NativeArray<float> bands;
  [NativeDisableContainerSafetyRestriction]
  [WriteOnly] public NativeArray<float3> vertices;

  public void Execute(int index)
  {
    int t0 = triangles[index*3];
    int t1 = triangles[index*3 + 1];
    int t2 = triangles[index*3 + 2];

    float3 normal = normals[t0];

    float band = CreateBand(bandDistribution[index], bandDistribution[index+1]);;
    band = math.pow(math.sqrt(band), power) * scale;
    float3 displacement = normal * band;

    vertices[t0] = originVertices[t0] + displacement;
    vertices[t1] = originVertices[t1] + displacement;
    vertices[t2] = originVertices[t2] + displacement;
  }

  private float CreateBand(int start, int end)
  {
    float totalFreq = 0.0f;
    for (int b=start; b < end; b++) totalFreq += samples[b]/bandAverage;
    return totalFreq;
  }
}