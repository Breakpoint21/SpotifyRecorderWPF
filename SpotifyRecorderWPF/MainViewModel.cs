using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NAudio.CoreAudioApi;
using SpotifyRecorderWPF.Annotations;
using System.Collections.ObjectModel;
using System.Windows.Data;
using SpotifyRecorderWPF.Commands;

namespace SpotifyRecorderWPF
{
    public class MainViewModel: INotifyPropertyChanged
    {
        public List<MMDevice> MmDevices { get;  }
        public List<int> Bitrates { get; }
        public ObservableCollection<SpotifyWav> RecordedFiles { get; }

        private int _selectedBitrate;
        public int SelectedBitrate
        {
            get { return _selectedBitrate; }
            set
            {
                if (value == _selectedBitrate) return;
                _selectedBitrate = value;
                OnPropertyChanged();
            }
        }

        private string _selectedMmDeviceId;
        public string SelectedMmDeviceId
        {
            get { return _selectedMmDeviceId; }
            set
            {
                if (value == _selectedMmDeviceId) return;
                _selectedMmDeviceId = value;
                OnPropertyChanged();
            }
        }

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

        public string OutputFolder
        {
            get { return _outputFolder; }
            set
            {
                if ( value == _outputFolder ) return;
                _outputFolder = value;
                OnPropertyChanged ( );
            }
        }

        public bool RecordingStarted
        {
            get { return _recordingStarted; }
            set
            {
                if ( value == _recordingStarted ) return;
                _recordingStarted = value;
                OnPropertyChanged ( );
            }
        }

        public bool IsSkipCommertials
        {
            get { return _isSkipCommertials; }
            set
            {
                if ( value == _isSkipCommertials ) return;
                _isSkipCommertials = value;
                OnPropertyChanged ( );
            }
        }

        public ICommand SelectFolderCommand
        {
            get { return _selectFolderCommand ?? (_selectFolderCommand = new SelectFolderCommand(this)); }
        }
        public ICommand StartRecordingCommand
        {
            get { return _startRecordingCommand ?? ( _startRecordingCommand = new RelayCommand ( StartRecording, ( ) => !RecordingStarted ) ); }
        }
        public ICommand StopRecordingCommand
        {
            get { return _stopRecordingCommand ?? (_stopRecordingCommand = new RelayCommand ( StopRecording, ( ) => RecordingStarted ) ); }
        }

        private readonly SpotifyWatcher _watcher = new SpotifyWatcher();
        private readonly MMDeviceEnumerator _deviceEnum = new MMDeviceEnumerator();
        private SoundCardRecorder _recorder;
        private string _outputFolder;
        private bool _recordingStarted;
        private bool _isSkipCommertials;
        private ICommand _startRecordingCommand;
        private ICommand _stopRecordingCommand;
        private ICommand _selectFolderCommand;

        private object _lock = new object();

        public MainViewModel ( )
        {
            
            RecordedFiles = new ObservableCollection< SpotifyWav > ();
            BindingOperations.EnableCollectionSynchronization(RecordedFiles, _lock);
            Bitrates = new List< int > { 96, 128, 160, 192, 320 };
            
            MmDevices = _deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            _watcher.SongChanged += SongChanged;
            _watcher.Start();
            
            LoadSettings();
        }

        private void SongChanged ( object sender, SpotifyTrackChanged spotifyTrackChanged )
        {
            CurrentSong = spotifyTrackChanged.Song;
            
            if (RecordingStarted)
            {
                var spotifyWav = _recorder?.Stop ( );
                if ( !spotifyTrackChanged.IsStopped )
                {
                    _recorder.Start(spotifyTrackChanged.Song, SelectedMmDevice);
                }
                if ( spotifyWav?.Duration > TimeSpan.FromSeconds(30) || !IsSkipCommertials)
                {
                    RecordedFiles.Add(spotifyWav);
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
            _recorder = new SoundCardRecorder(new DirectoryInfo(OutputFolder));

            RecordingStarted = true;
        }

        public void StopRecording ( )
        {
            if ( RecordingStarted )
            {
                RecordingStarted = false;
                var file = _recorder?.Stop();
                _recorder?.Dispose();
                _recorder = null;

                if (file != null && file.WavFile.Exists)
                {
                    file.WavFile.Delete();
                }
            }
        }

        public void OnClose()
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.SelectedBitrate = SelectedBitrate;
            Properties.Settings.Default.DeviceName = SelectedMmDevice.FriendlyName;
            Properties.Settings.Default.OutputPath = OutputFolder;
            Properties.Settings.Default.SkipCommertials = IsSkipCommertials;

            Properties.Settings.Default.Save();
        }

        private void LoadSettings()
        {
            SelectedBitrate = Bitrates.Contains(Properties.Settings.Default.SelectedBitrate) ? Properties.Settings.Default.SelectedBitrate : 128;
            SelectedMmDeviceId = MmDevices.FirstOrDefault(x => x.FriendlyName == Properties.Settings.Default.DeviceName)?.ID;
            OutputFolder = Directory.Exists(Properties.Settings.Default.OutputPath) ? Properties.Settings.Default.OutputPath : null;
            IsSkipCommertials = Properties.Settings.Default.SkipCommertials;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [ NotifyPropertyChangedInvocator ]
        protected virtual void OnPropertyChanged ( [ CallerMemberName ] string propertyName = null )
        {
            PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
        }
    }
}
