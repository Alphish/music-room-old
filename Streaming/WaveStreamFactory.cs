using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;
using NVorbis.NAudioSupport;

namespace Alphicsh.Audio.Streaming
{
    /// <summary>
    /// A helper class for creating wave streams from files.
    /// </summary>
    public static class WaveStreamFactory
    {
        /// <summary>
        /// Returns the most appropriate wave stream for a given file.
        /// </summary>
        /// <param name="filename">The name of the file to load.</param>
        /// <returns>The wave stream for the given file.</returns>
        public static WaveStream Create(String filename)
        {
            var ext = Path.GetExtension(filename);
            switch (ext)
            {
                case ".mp3":
                    return new Mp3FileReader(filename);
                case ".ogg":
                    return new VorbisWaveReader(filename);
                case ".wav":
                    return new WaveFileReader(filename);
                default:
                    throw new NotSupportedException("Cannot read " + ext + " files.");
            }
        }
    }
}
