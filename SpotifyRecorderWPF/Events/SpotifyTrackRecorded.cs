using System;
using SpotifyRecorderWPF.ObjectModel;

namespace SpotifyRecorderWPF.Events
{
    public class SpotifyTrackRecorded : EventArgs
    {
        public SpotifyTrackRecorded ( SpotifyWav spotifyWav )
        {
            Song = spotifyWav?.Song;
            File = spotifyWav;
        }

        public string Song { get;}
        public SpotifyWav File { get;  }
    }
}