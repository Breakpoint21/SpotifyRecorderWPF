using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SpotifyRecorderWPF.Events;

namespace SpotifyRecorderWPF.Logic
{
    public class SpotifyWatcher
    {
        private Task _watcherTask;
        private bool _cancel;

        public EventHandler<SpotifyTrackChanged> SongChanged;

        private static SpotifyWatcher _instance;
        public static SpotifyWatcher Instance { get { return _instance ?? ( _instance = new SpotifyWatcher ( ) ); } }

        private SpotifyWatcher() { }

        public void Start ( )
        {
            _cancel = false;
            _watcherTask = Task.Factory.StartNew ( ( ) =>
            {
                string previousSong = null;

                while ( !_cancel )
                {
                    var process = Process.GetProcessesByName("spotify");
                    if (process.Length > 0)
                    {
                        var song = process.FirstOrDefault ( x => !string.IsNullOrEmpty ( x.MainWindowTitle ) )?.MainWindowTitle?.Replace("Spotify", "");
                        
                        if ( previousSong != song )
                        {
                            try
                            {
                                SongChanged?.Invoke(this, new SpotifyTrackChanged(song));
                            }
                            catch ( Exception ) { }
                            
                            previousSong = song;
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
}
