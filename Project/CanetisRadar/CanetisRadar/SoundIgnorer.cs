using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;

namespace CanetisRadar
{
    /// <summary>
    /// Loads recorded sounds and allows ignoring them based on RMS energy.
    /// </summary>
    public class SoundIgnorer
    {
        private readonly List<float> _rmsValues = new List<float>();
        private readonly float _tolerance;

        public SoundIgnorer(string folder, float tolerance = 0.05f)
        {
            _tolerance = tolerance;
            if (Directory.Exists(folder))
            {
                foreach (var file in Directory.GetFiles(folder, "*.wav"))
                {
                    try
                    {
                        using var reader = new AudioFileReader(file);
                        float[] buffer = new float[reader.WaveFormat.SampleRate];
                        int read;
                        double sum = 0;
                        long count = 0;
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < read; i++)
                            {
                                sum += buffer[i] * buffer[i];
                            }
                            count += read;
                        }
                        if (count > 0)
                        {
                            float rms = (float)Math.Sqrt(sum / count);
                            _rmsValues.Add(rms);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore malformed files
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the current RMS value matches any loaded sound within tolerance.
        /// </summary>
        public bool ShouldIgnore(float currentRms)
        {
            foreach (float rms in _rmsValues)
            {
                if (Math.Abs(rms - currentRms) <= _tolerance)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
