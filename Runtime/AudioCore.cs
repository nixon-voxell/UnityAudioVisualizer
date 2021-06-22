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

namespace Voxell.Audio
{
  [RequireComponent(typeof(VisualEffect))]
  [RequireComponent(typeof(AudioSource))]
  public partial class AudioCore : MonoBehaviour
  {
    private const float EPSILON = 0.001f;

    void Start()
    {
      InitAgentInteraction();
      InitAudioVisualizer();
    }

    void Update()
    {
      UpdateAgentInteraction();
      UpdateAudioVisualizer();
    }

    internal static class ShaderPropertyId
    {
      public static readonly int sampleCount = Shader.PropertyToID("sampleCount");
      public static readonly int transform = Shader.PropertyToID("transform");
      public static readonly int oldTransform = Shader.PropertyToID("oldTransform");
      public static readonly int framRate = Shader.PropertyToID("framRate");
    }

    internal static class ShaderBufferId
    {
      public static readonly int cb_samplePoints = Shader.PropertyToID("cb_samplePoints");
      public static readonly int cb_position = Shader.PropertyToID("cb_position");
      public static readonly int cb_oldPosition = Shader.PropertyToID("cb_oldPosition");
      public static readonly int cb_normal = Shader.PropertyToID("cb_normal;");
      public static readonly int tex_positionMap = Shader.PropertyToID("tex_positionMap");
      public static readonly int tex_velocityMap = Shader.PropertyToID("tex_velocityMap");
      public static readonly int tex_normalMap = Shader.PropertyToID("tex_normalMap");
    }

    internal static class VFXPropertyId
    {
      public static readonly int mesh_sampleMesh = Shader.PropertyToID("SampleMesh");
      public static readonly int float_forceMultiplier = Shader.PropertyToID("ForceMultiplier");
      public static readonly int int_triangleCount = Shader.PropertyToID("TriangleCount");
      public static readonly int float_idleNoiseSpeed = Shader.PropertyToID("IdleNoiseSpeed");
      public static readonly int float_noiseIntensity = Shader.PropertyToID("NoiseIntensity");
      public static readonly int float3_noiseScale = Shader.PropertyToID("NoiseScale");
    }
  }
}