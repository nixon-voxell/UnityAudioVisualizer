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

namespace SmartAssistant.Audio.Visualizer
{
  [RequireComponent(typeof(VisualEffect))]
  [RequireComponent(typeof(AudioSource))]
  public partial class AudioVisualizer : MonoBehaviour
  {
    public VisualEffect audioVFX;
    public AudioSource audioSource;
    private const float epsilon = 0.001f;
    
    #region VFX Property IDs
    private int noisePositionAddition,
    radius,
    // audio1,
    // audio2,
    // audio3,
    // audio4,
    // audio5,
    // audio6,
    // audio7,
    // audio8,
    noiseIntensity;
    #endregion

    #region Editor Stuff
    [HideInInspector]
    public bool drawDefaultInspect;
    #endregion

    void Start()
    {
      InitVFXPropertyIDs();

      InitAgentInteraction();
      // InitAudioVisualizer();
    }

    void Update()
    {
      UpdateAgentInteraction();
      // UpdateAudioVisualizer();
    }

    void InitVFXPropertyIDs()
    {
      // audio1 = Shader.PropertyToID("Audio1");
      // audio2 = Shader.PropertyToID("Audio2");
      // audio3 = Shader.PropertyToID("Audio3");
      // audio4 = Shader.PropertyToID("Audio4");
      // audio5 = Shader.PropertyToID("Audio5");
      // audio6 = Shader.PropertyToID("Audio6");
      // audio7 = Shader.PropertyToID("Audio7");
      // audio8 = Shader.PropertyToID("Audio8");
      radius = Shader.PropertyToID("Radius");
      noisePositionAddition = Shader.PropertyToID("NoisePositionAddition");
      noiseIntensity = Shader.PropertyToID("Intensity");
    }
  }
}