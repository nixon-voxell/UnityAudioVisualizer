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

namespace Voxell.Audio
{
  public class AudioProcessor
  {
    public AudioSource source;
    public AudioProfile profile;
    public AudioProcessor(ref AudioSource source, ref AudioProfile profile)
    {
      this.source = source;
      this.profile = profile;

      Init();
    }

    public float[] samples;
    public int freqSize;
    public int bandAverage;
    public int[] bandDistribution;

    public void Init()
    {
      samples = new float[profile.sampleSize];

      float freqInterval = AudioSettings.outputSampleRate/profile.sampleSize;
      float currFreq = 0.0f;
      freqSize = 0;

      for (int s=0; s < profile.sampleSize; s++)
      {
        currFreq += freqInterval;
        if (currFreq < profile.freqRange) freqSize ++;
        else break;
      }

      bandAverage = freqSize/profile.bandSize;

      bandDistribution = new int[profile.bandSize+1];
      bandDistribution[0] = 0;
      for (int b=1; b < profile.bandSize+1; b++)
        bandDistribution[b] = bandAverage + b*bandAverage;
    }

    public void SampleSpectrum() => source.GetSpectrumData(samples, profile.channel, profile.window);
  }
}