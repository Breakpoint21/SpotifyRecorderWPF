using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using SpotifyRecorderWPF.Annotations;

namespace SpotifyRecorderWPF
{
    public class MainViewModel: INotifyPropertyChanged
    {
        public List<MMDevice> MmDevices { get; set; }
        public List<int> Bitrates { get; set; }
        public int SelectedBitrate { get; set; }

        public string SelectedMmDeviceId { get; set; }

        public MMDevice SelectedMmDevice
        {
            get
            {
                if ( !string.IsNullOrEmpty(SelectedMmDeviceId) )
                {
                    return _deviceEnum.GetDevice ( SelectedMmDeviceId );
                }
                return null;
            }
        }

        private string _currentSong;
        public string CurrentSong
        {
            get { return _currentSong; }
            set
            {
                if ( value == _currentSong ) return;
                _currentSong = value;
                OnPropertyChanged();
            }
        }

        private readonly SpotifyWatcher _watcher = new SpotifyWatcher();
        private readonly MMDeviceEnumerator _deviceEnum = new MMDeviceEnumerator();

        private SoundCardRecorder _recorder = null;
        private bool _recordingEnabled;

        public MainViewModel ( )
        {
            Bitrates = new List< int > { 96, 128, 160, 192, 320 };
            
            MmDevices = _deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            _watcher.SongChanged += SongChanged;
            _watcher.Start();
        }

        private void SongChanged ( object sender, SpotifyTrackChanged spotifyTrackChanged )
        {
            CurrentSong = spotifyTrackChanged.Song;
            
            if (_recordingEnabled)
            {
                var spotifyWav = _recorder?.Stop ( );
                if ( !spotifyTrackChanged.IsStopped )
                {
                    _recorder.Start(spotifyTrackChanged.Song, SelectedMmDevice);
                }
                if ( spotifyWav?.Duration > TimeSpan.FromSeconds(30) )
                {
                    Mp3Converter.ConvertToMp3(spotifyWav.WavFile.FullName, SelectedBitrate, Mp3Tag.Parse(spotifyWav.Song));
                }
                else
                {
                    spotifyWav?.WavFile.Delete();
                }
            }
        }


        public void StartRecording ( )
        {
            _recorder = new SoundCardRecorder(SelectedMmDevice, new DirectoryInfo(@"C:\Users\Pascal\Downloads"));
            _recordingEnabled = true;
        }

        public void StopRecording ( )
        {
            _recordingEnabled = false;
            _recorder?.Stop ( );
            _recorder?.Dispose();
            _recorder = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [ NotifyPropertyChangedInvocator ]
        protected virtual void OnPropertyChanged ( [ CallerMemberName ] string propertyName = null )
        {
            PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
        }
    }
}
