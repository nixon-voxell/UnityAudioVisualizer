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

The Original Code is Copyright (C) 2020 Voxell Technologies.
All rights reserved.
*/

using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

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

    private Mesh modifiedSampleMesh;
    private NativeArray<float3> originVertices;
    private NativeArray<float3> normals;
    public NativeArray<int> triangles;
    private NativeArray<float3> vertices;
    private int totalTris;

    private void InitAudioVisualizer()
    {
      totalTris = (int)sampleMesh.GetIndexCount(0)/3;
      audioProfile.bandSize = totalTris;

      audioProcessor = new AudioProcessor(ref audioSource, ref audioProfile);

      MeshUtils.DeepCopyMesh(ref sampleMesh, out modifiedSampleMesh);
      audioVFX.SetMesh(VFXPropertyId.mesh_sampleMesh, modifiedSampleMesh);
      modifiedSampleMesh.MarkDynamic();
      meshFilter.mesh = modifiedSampleMesh;

      // transferring mesh data to native arrays to be processed parallely
      Mesh.MeshDataArray sampleMeshData = Mesh.AcquireReadOnlyMeshData(sampleMesh);
      originVertices = MeshUtils.NativeGetVertices(sampleMeshData[0], Allocator.Persistent);
      originVertices.AsReadOnly();
      normals = MeshUtils.NativeGetNormals(sampleMeshData[0], Allocator.Persistent);
      normals.AsReadOnly();
      triangles = MeshUtils.NativeGetIndices(sampleMeshData[0], Allocator.Persistent);
      triangles.AsReadOnly();
      sampleMeshData.Dispose();
    }

    private void UpdateAudioVisualizer()
    {
      audioProcessor.SampleSpectrum();
      audioProcessor.RescaleSamples();
      audioProcessor.GenerateBands();

      vertices = new NativeArray<float3>(modifiedSampleMesh.vertexCount, Allocator.TempJob);

      AudioMeshVisualizer audioMeshVisualizer = new AudioMeshVisualizer
      {
        originVertices = originVertices,
        normals = normals,
        triangles = triangles,
        bands = audioProcessor.bands,
        vertices = vertices
      };

      JobHandle jobHandle = audioMeshVisualizer.Schedule<AudioMeshVisualizer>(totalTris, batchSize);
      jobHandle.Complete();

      modifiedSampleMesh.SetVertices(vertices);
      vertices.Dispose();
    }

    void OnDisable()
    {
      originVertices.Dispose();
      normals.Dispose();
      triangles.Dispose();
      audioProcessor.Destroy();
    }
  }
}

[BurstCompile(
  CompileSynchronously=true,
  FloatPrecision=FloatPrecision.Medium,
  FloatMode=FloatMode.Fast)]
public struct AudioMeshVisualizer : IJobParallelFor
{
  [ReadOnly] public NativeArray<float3> originVertices;
  [ReadOnly] public NativeArray<float3> normals;
  [ReadOnly] public NativeArray<int> triangles;
  [ReadOnly] public NativeArray<float> bands;
  [NativeDisableContainerSafetyRestriction]
  [WriteOnly] public NativeArray<float3> vertices;

  public void Execute(int index)
  {
    int t0 = triangles[index*3];
    int t1 = triangles[index*3 + 1];
    int t2 = triangles[index*3 + 2];

    float3 normal = normals[t0];
    float3 displacement = normal * bands[index];

    vertices[t0] = originVertices[t0] + displacement;
    vertices[t1] = originVertices[t1] + displacement;
    vertices[t2] = originVertices[t2] + displacement;
  }
}