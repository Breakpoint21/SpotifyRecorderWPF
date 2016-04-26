using System;

namespace SpotifyRecorderWPF.Events
{
    public class SpotifyTrackChanged : EventArgs
    {
        public SpotifyTrackChanged ( string song )
        {
            Song = song?.Trim();
        }

        public string Song { get; }

        public bool IsStopped => string.IsNullOrEmpty ( Song );
    }
}