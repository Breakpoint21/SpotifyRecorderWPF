using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyRecorderWPF
{
    public class SpotifyWatcher
    {
        private Task _watcherTask;
        private bool _cancel = false;

        public EventHandler<SpotifyTrackChanged> SongChanged;

        public void Start ( )
        {
            _cancel = false;
            _watcherTask = Task.Factory.StartNew ( ( ) =>
            {
                string _previousSong = null;

                while ( !_cancel )
                {
                    var process = Process.GetProcessesByName("spotify");
                    if (process.Length > 0)
                    {
                        var song = process.FirstOrDefault ( x => !string.IsNullOrEmpty ( x.MainWindowTitle ) )?.MainWindowTitle?.Replace("Spotify", "");
                        
                        if ( _previousSong != song )
                        {
                            SongChanged?.Invoke(this, new SpotifyTrackChanged(song));
                            _previousSong = song;
                        }
                    }
                    Task.Delay ( 100 ).Wait ( );
                }
            } );
        }

        public async void Stop ( )
        {
            _cancel = true;
            await _watcherTask;
        }
    }

    public class SpotifyTrackChanged : EventArgs
    {
        public SpotifyTrackChanged ( string song )
        {
            Song = song;
        }

        public string Song { get; }

        public bool IsStopped => string.IsNullOrEmpty ( Song );
    }
}
