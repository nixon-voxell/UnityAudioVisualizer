using UnityEngine;

namespace Voxell.Audio
{
    [System.Serializable]
    public class AudioProfile
    {
        public int channel;
        public FFTWindow window;
        public int sampleSize;
        public float power;
        public float scale;
        public int smoothingIterations;
        [Range(5000, 20000)] public int frequencyRange;
        [HideInInspector] public int bandSize;
    }
}
