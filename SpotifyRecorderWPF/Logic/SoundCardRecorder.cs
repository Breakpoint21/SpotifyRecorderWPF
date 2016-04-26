using System;
using System.Diagnostics;
using System.IO;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SpotifyRecorderWPF.Helper;
using SpotifyRecorderWPF.ObjectModel;

namespace SpotifyRecorderWPF.Logic
{
    public class SoundCardRecorder
    {
        private IWaveIn _waveIn;
        private WaveFileWriter _writer;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private string _currentSong;
        private readonly DirectoryInfo _outputDirectory;
        
        public SoundCardRecorder(DirectoryInfo outputDirectory)
        {
            _outputDirectory = outputDirectory;
        }

        public void Dispose()
        {
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _waveIn.DataAvailable -= OnDataAvailable;
                _waveIn.Dispose();
            }
            _writer?.Close();
        }
        
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            _writer.Write(e.Buffer, 0, e.BytesRecorded);
        }

        public void Start ( string song, MMDevice device )
        {
            if ( _waveIn != null ) Stop ( );

            try
            {
                _currentSong = song;
                _waveIn = new WasapiCapture(device);
                _writer = new WaveFileWriter(Utils.GetWavFileName(_outputDirectory, _currentSong), _waveIn.WaveFormat);
                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.StartRecording();
                _stopwatch.Reset();
                _stopwatch.Start();
            }
            catch ( Exception )
            {
                
                throw;
            }
        }

        public SpotifyWav Stop ( )
        {
            if ( _waveIn == null || _writer == null ) return null;

            _stopwatch.Stop();
            _waveIn.StopRecording();
            _waveIn.DataAvailable -= OnDataAvailable;

            var result = new SpotifyWav ( _stopwatch.Elapsed, new FileInfo ( _writer.Filename ), _currentSong );

            _waveIn.Dispose();
            _writer.Close();
            _writer.Dispose();

            _currentSong = null;
            _waveIn = null;
            _writer = null;

            return result;
        }
    }
}
