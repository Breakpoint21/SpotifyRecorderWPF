using System;
using System.IO;
using NAudio.CoreAudioApi;
using SpotifyRecorderWPF.Events;
using SpotifyRecorderWPF.Helper;
using SpotifyRecorderWPF.ObjectModel;

namespace SpotifyRecorderWPF.Logic
{
    public class SpotifyRecorder
    {
        private SoundCardRecorder _recorder;
        private string _recordingDeviceId;
        private bool _skipCommertials;
        private int _bitrate;
        private readonly MMDeviceEnumerator _deviceEnum = new MMDeviceEnumerator();

        public bool IsRecording { get; private set; }

        public EventHandler< SpotifyTrackRecorded > TrackRecorded;

        public SpotifyRecorder ( )
        {
            SpotifyWatcher.Instance.SongChanged += SongChanged;
        }

        private void SongChanged ( object sender, SpotifyTrackChanged spotifyTrackChanged )
        {
            if ( IsRecording && _recorder != null )
            {
                var spotifyWav = _recorder?.Stop();
                RecordTrack ( spotifyTrackChanged );
                ConvertAndSave ( spotifyWav );
            }
        }

        private void RecordTrack ( SpotifyTrackChanged spotifyTrackChanged )
        {
            if ( !spotifyTrackChanged.IsStopped )
            {
                _recorder.Start ( spotifyTrackChanged.Song, _deviceEnum.GetDevice ( _recordingDeviceId ) );
            }
        }

        private void ConvertAndSave ( SpotifyWav spotifyWav )
        {
            if ( spotifyWav != null && ( spotifyWav.Duration > TimeSpan.FromSeconds ( 30 ) || !_skipCommertials ) )
            {
                TrackRecorded?.Invoke ( this, new SpotifyTrackRecorded ( spotifyWav ) );
                Mp3Converter.ConvertToMp3 ( spotifyWav.WavFile.FullName, _bitrate, Mp3Tag.Parse ( spotifyWav.Song ) );
            }
            else
            {
                try
                {
                    spotifyWav?.WavFile.Delete ( );
                }
                catch ( Exception e )
                {
                    Console.WriteLine ( e );
                }
            }
        }

        public void StartRecoding ( string recordingDeviceId, string outputFolderPath, bool skipCommertials, int bitrate )
        {
            if ( !IsRecording )
            {
                _recordingDeviceId = recordingDeviceId;
                _skipCommertials = skipCommertials;
                _bitrate = bitrate;

                _recorder = new SoundCardRecorder(new DirectoryInfo(outputFolderPath));
                IsRecording = true;
            }
            
        }

        public void StopRecording ( )
        {
            if (IsRecording)
            {
                
                var file = _recorder?.Stop();
                _recorder?.Dispose();
                _recorder = null;

                //Delete current recording on cancellation
                if (file != null && file.WavFile.Exists)
                {
                    file.WavFile.Delete();
                }
                IsRecording = false;
            }
        }
    }
}
