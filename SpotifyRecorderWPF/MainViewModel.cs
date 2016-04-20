using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
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


        public ICommand StartRecordingCommand
        {
            get { return _startRecordingCommand ?? ( _startRecordingCommand = new SimpleCommand ( StartRecording, ( ) => !RecordingStarted ) ); }
        }
        public ICommand StopRecordingCommand
        {
            get { return _stopRecordingCommand ?? (_stopRecordingCommand = new SimpleCommand ( StopRecording, ( ) => RecordingStarted ) ); }
        }

        public List<SpotifyWav> RecordedFiles { get; set; }

        public ICommand SelectFolderCommand { get; }

        private readonly SpotifyWatcher _watcher = new SpotifyWatcher();
        private readonly MMDeviceEnumerator _deviceEnum = new MMDeviceEnumerator();

        private SoundCardRecorder _recorder = null;
        private string _outputFolder;
        private bool _recordingStarted;
        private bool _isSkipCommertials;
        private ICommand _startRecordingCommand;
        private ICommand _stopRecordingCommand;


        public MainViewModel ( )
        {
            RecordedFiles = new List< SpotifyWav > ();
            Bitrates = new List< int > { 96, 128, 160, 192, 320 };
            
            MmDevices = _deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            _watcher.SongChanged += SongChanged;
            _watcher.Start();

            SelectFolderCommand = new SelectFolderCommand(this);
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
                _recorder?.Stop();
                _recorder?.Dispose();
                _recorder = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [ NotifyPropertyChangedInvocator ]
        protected virtual void OnPropertyChanged ( [ CallerMemberName ] string propertyName = null )
        {
            PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
        }
    }

    public class SimpleCommand : ICommand
    {
        private readonly Action _execAction;
        private readonly Func<bool> _canExecute;

        public SimpleCommand (Action execAction, Func<bool> canExecute )
        {
            _execAction = execAction;
            _canExecute = canExecute;
        }

        public bool CanExecute ( object parameter )
        {
            return _canExecute ( );
        }

        public void Execute ( object parameter )
        {
            _execAction ( );
        }

        public event EventHandler CanExecuteChanged;
    }

    public class SelectFolderCommand : ICommand
    {
        private readonly MainViewModel _viewModel;
        private readonly FolderBrowserDialog _folderBrowserDialog;

        public SelectFolderCommand ( MainViewModel vm )
        {
            _viewModel = vm;
            _folderBrowserDialog = new FolderBrowserDialog ( );
        }

        public bool CanExecute ( object parameter )
        {
            return !_viewModel.RecordingStarted;
        }

        public void Execute ( object parameter )
        {
            var dialogResult = _folderBrowserDialog.ShowDialog();
            if ( dialogResult == DialogResult.OK )
            {
                _viewModel.OutputFolder = _folderBrowserDialog.SelectedPath;
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
