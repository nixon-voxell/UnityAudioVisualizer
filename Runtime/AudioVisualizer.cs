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
using Unity.Collections;

namespace SmartAssistant.Audio
{
  public partial class AudioCore
  {
    public AudioSource audioSource;
    public AudioProfile audioProfile;
    public AudioProcessor audioProcessor;

    public VisualEffect audioVFX;
    public MeshFilter meshFilter;
    public Mesh sampleMesh;
    public float audioEffectIntensity = 5.0f;

    private Mesh modifiedSampleMesh;
    private Vector3[] vertices;
    private Vector3[] velocites;

    void InitAudioVisualizer()
    {
      audioProfile.bandSize = sampleMesh.vertexCount;
      audioProcessor = new AudioProcessor(ref audioSource, ref audioProfile);

      vertices = sampleMesh.vertices;
      velocites = new Vector3[sampleMesh.vertexCount];
      MathUtils.SetArray<Vector3>(ref velocites, Vector3.zero);

      MeshUtils.CopyMesh(in sampleMesh, out modifiedSampleMesh);
      audioVFX.SetMesh(VFXPropertyId.mesh_sampleMesh, modifiedSampleMesh);
      modifiedSampleMesh.MarkDynamic();
      meshFilter.mesh = modifiedSampleMesh;
    }

    void UpdateAudioVisualizer()
    {
      audioProcessor.SampleSpectrum();
      audioProcessor.RescaleSamples();
      audioProcessor.GenerateBands();

      for (int v=0; v < modifiedSampleMesh.vertexCount; v++)
      {
        Vector3 targetPosition = sampleMesh.vertices[v] +
          sampleMesh.normals[v] * audioProcessor.band[v] * audioEffectIntensity;
        vertices[v] = targetPosition;
      }

      modifiedSampleMesh.SetVertices(vertices);
    }
  }
}