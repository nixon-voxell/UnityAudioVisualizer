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

using System.Collections.Generic;
using System.Linq;


namespace SmartAssistant.AudioVisualizer
{
  public class SpectrumSmoother
  {
    private long _frameCount;

    private readonly int _spectrumSize;
    private readonly int _smoothingIterations;
    private readonly float[] _smoothedSpectrum;
    private readonly List<float[]> _spectrumHistory = new List<float[]>();

    public SpectrumSmoother(ref int spectrumSize, ref int smoothingIterations)
    {
      _spectrumSize = spectrumSize;
      _smoothingIterations = smoothingIterations;

      _smoothedSpectrum = new float[_spectrumSize];

      for (int i = 0; i < _spectrumSize; i++)
      {
        _spectrumHistory.Add(new float[_smoothingIterations]);
      }
    }

    public void AdvanceFrame()
    {
      _frameCount++;
    }

    public float[] GetSpectrumData(float[] spectrum)
    {
      // Record and average last N frames
      for (var i = 0; i < _spectrumSize; i++)
      {
        var historyIndex = _frameCount % _smoothingIterations;

        var audioData = spectrum[i];
        _spectrumHistory[i][historyIndex] = audioData;

        _smoothedSpectrum[i] = _spectrumHistory[i].Average();
      }

      return _smoothedSpectrum;
    }
  }
}