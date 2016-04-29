using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using NAudio.CoreAudioApi;
using SpotifyRecorderWPF.Annotations;
using SpotifyRecorderWPF.Commands;
using SpotifyRecorderWPF.Helper;
using SpotifyRecorderWPF.Logic;
using SpotifyRecorderWPF.ObjectModel;

namespace SpotifyRecorderWPF.ViewModels
{
    public class MainViewModel: BaseViewModel, INotifyPropertyChanged
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
                Validate();
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
                Validate();
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
        
        private readonly MMDeviceEnumerator _deviceEnum = new MMDeviceEnumerator();
        private readonly SpotifyRecorder _spotifyRecorder = new SpotifyRecorder();
        
        private string _outputFolder;
        private bool _recordingStarted;
        private bool _isSkipCommertials;
        private ICommand _startRecordingCommand;
        private ICommand _stopRecordingCommand;
        private ICommand _selectFolderCommand;

        private readonly object _lock = new object();

        public MainViewModel ( )
        {
            RecordedFiles = new ObservableCollection< SpotifyWav > ();
            BindingOperations.EnableCollectionSynchronization(RecordedFiles, _lock);
            Bitrates = new List< int > { 96, 128, 160, 192, 320 };
            
            MmDevices = _deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            SpotifyWatcher.Instance.SongChanged += ( sender, e ) => { CurrentSong = e.Song; };
            SpotifyWatcher.Instance.Start();
            
            _spotifyRecorder.TrackRecorded += ( sender, e ) => { RecordedFiles.Add ( e.File ); };
            
            Settings.Load(this);

            InitializeValidation ( );
        }

        private void InitializeValidation ( )
        {
            Rules.Add(new ValidationRule(nameof(OutputFolder), "Output Folder can not be empty!", () => string.IsNullOrEmpty(OutputFolder)));
            Rules.Add(new ValidationRule(nameof(OutputFolder), "Output Folder is not a valid Folder!", () => !Directory.Exists(OutputFolder)));
            Rules.Add(new ValidationRule(nameof(SelectedMmDeviceId), "Recording Device must be selected!", () => string.IsNullOrEmpty(SelectedMmDeviceId)));
        }

        public void StartRecording ( )
        {
            Validate();

            if ( !HasErrors )
            {
                _spotifyRecorder.StartRecoding(SelectedMmDeviceId, OutputFolder, IsSkipCommertials, SelectedBitrate);
                RecordingStarted = _spotifyRecorder.IsRecording;
            }
            
        }

        public void StopRecording ( )
        {
            _spotifyRecorder.StopRecording();
            RecordingStarted = _spotifyRecorder.IsRecording;
        }

        public void OnClose()
        {
            _spotifyRecorder.StopRecording();
            Settings.Save(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [ NotifyPropertyChangedInvocator ]
        protected virtual void OnPropertyChanged ( [ CallerMemberName ] string propertyName = null )
        {
            PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
        }
    }
}
