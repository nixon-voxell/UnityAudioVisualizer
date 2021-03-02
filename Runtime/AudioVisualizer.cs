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

namespace SmartAssistant.AudioVisualizer
{
  public class Visualizer
  {
    public AudioSource source;
    public AudioProfile profile;
    public Visualizer(ref AudioSource source, ref AudioProfile profile)
    {
      this.source = source;
      this.profile = profile;

      Init();
    }

    public float[] samples;
    public float[][] sampleBuffers;

    public float[] freq;
    public int freqSize;

    private int bandAverage;
    public int[] bandDistribution;
    public float[] band;

    public void Init()
    {
      samples = new float[profile.sampleSize];
      sampleBuffers = new float[profile.sampleSize][];

      float freqInterval = AudioSettings.outputSampleRate/profile.sampleSize;
      float currFreq = 0.0f;
      freqSize = 0;

      for (int s=0; s < profile.sampleSize; s++)
      {
        currFreq += freqInterval;
        if (currFreq < profile.freqRange)
          freqSize ++;
        else break;
      }
      freq = new float[freqSize];

      bandAverage = Mathf.FloorToInt(freqSize/profile.bufferSize); 

      band = new float[profile.bandSize];
      bandDistribution = new int[profile.bandSize];
      for (int b=0; b < profile.bandSize - 1; b++)
        bandDistribution[b] = bandAverage + b*bandAverage;
      
      bandDistribution[profile.bandSize - 1] = freqSize;

      for (int s=0; s < profile.sampleSize; s++)
        sampleBuffers[s] = new float[profile.bufferSize];
    }

    public void SampleSpectrum() => source.GetSpectrumData(samples, profile.channel, profile.window);

    public void RescaleSamples(float deltaTime)
    {
      for (int f=1; f < freqSize; f++)
      {
        // samples[f] = Mathf.Sqrt(Mathf.Pow(samples[f], profile.power)) * profile.scale;
        samples[f] = Mathf.Pow(Mathf.Sqrt(samples[f]), profile.power) * profile.scale;

        // populate buffers
        for (int b=1; b < profile.bufferSize; b++)
          sampleBuffers[f][b] = sampleBuffers[f][b-1];

        // push in newest sample into buffer
        sampleBuffers[f][profile.bufferSize-1] = samples[f];
        float minBuffer = Mathf.Min(sampleBuffers[f]);
        float maxBuffer = Mathf.Max(sampleBuffers[f]);

        float average = (maxBuffer - minBuffer)/2;

        freq[f-1] = Mathf.Lerp(average, samples[f], profile.sensitivity);
      }
    }

    public void GenerateBands()
    {
      CreateBand(0, bandDistribution[0], ref band[0]);
      for (int b=1; b < profile.bandSize; b++)
        CreateBand(bandDistribution[b-1], bandDistribution[b], ref band[b]);
    }

    private void CreateBand(int start, int end, ref float totalFreq)
    {
      totalFreq = 0.0f;
      for (int f=start; f < end; f++)
        totalFreq += freq[f]/bandAverage;
    }

  }
}