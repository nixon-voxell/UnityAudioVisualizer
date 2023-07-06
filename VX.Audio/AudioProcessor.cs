using UnityEngine;

namespace Voxell.Audio
{
    public class AudioProcessor
    {
        public AudioSource source;
        public AudioProfile profile;

        public float[] samples;
        public int freqSize;
        public int bandAverage;
        public int[] bandDistribution;

        public AudioProcessor(ref AudioSource source, ref AudioProfile profile)
        {
            Debug.Assert(profile.frequencyRange > profile.bandSize, "Number of triangles should not exceed frequency range.");
            this.source = source;
            this.profile = profile;

            samples = new float[profile.sampleSize];

            float freqInterval = AudioSettings.outputSampleRate / profile.sampleSize;
            float currFreq = 0.0f;
            freqSize = 0;

            for (int s = 0; s < profile.sampleSize; s++)
            {
                currFreq += freqInterval;
                if (currFreq < profile.frequencyRange) freqSize++;
                else break;
            }

            bandAverage = Mathf.Max(1, freqSize / profile.bandSize);

            bandDistribution = new int[profile.bandSize + 1];
            bandDistribution[0] = 0;
            for (int b = 0; b < profile.bandSize; b++)
            {
                bandDistribution[b + 1] = bandAverage + b * bandAverage;
            }
        }

        public void SampleSpectrum() => source.GetSpectrumData(samples, profile.channel, profile.window);
    }
}
