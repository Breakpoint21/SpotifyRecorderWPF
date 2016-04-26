using System;
using System.IO;

namespace SpotifyRecorderWPF.ObjectModel
{
    public class SpotifyWav
    {
        public SpotifyWav ( TimeSpan duration, FileInfo wavFile, string song )
        {
            Duration = duration;
            WavFile = wavFile;
            Song = song;
        }

        public TimeSpan Duration { get; }
        public FileInfo WavFile { get; }
        public string Song { get; }

    }
}