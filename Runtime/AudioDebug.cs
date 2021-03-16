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

namespace SmartAssistant.Audio.Debug
{
  public class AudioDebug : MonoBehaviour
  {
    public AudioSource audioSource;
    public AudioProfile audioProfile;
    public AudioProcessor audioProcessor;
    public SpectrumSmoother spectrumSmoother;

    public float width;
    public float xOffset;

    int freqRange;
    int bandSize;

    void Awake()
    {
      audioProcessor = new AudioProcessor(ref audioSource, ref audioProfile);
      spectrumSmoother = new SpectrumSmoother(ref audioProcessor.freqSize, ref audioProfile.smoothingIterations);
      bandSize = audioProfile.bandSize;
    }

    // Update is called once per frame
    void Update()
    {
      if (audioProfile.freqRange != freqRange)
      {
        audioProcessor.Init();
        freqRange = audioProfile.freqRange;
      }
      if (audioProfile.bandSize != bandSize)
      {
        audioProcessor.Init();
        bandSize = audioProfile.bandSize;
      }

      audioProcessor.SampleSpectrum();
      audioProcessor.RescaleSamples(Time.deltaTime);
    }

    void OnDrawGizmos()
    {
      if (Application.isPlaying)
      {
        Gizmos.color = Color.cyan;
        Gizmos.color *= new Color(1, 1, 1, 0.5f);
        for (int s=0; s < audioProcessor.band.Length; s++)
          Gizmos.DrawCube(
            transform.position + new Vector3(width*s + xOffset, 0, 0),
            new Vector3(width, audioProcessor.freq[s], width));
      }
    }
  }
}