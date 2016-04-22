using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SpotifyRecorderWPF
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
            
            _currentSong = song;
            _waveIn = new WasapiCapture(device);
            _writer = new WaveFileWriter(Utils.GetWavFileName(_outputDirectory, _currentSong), _waveIn.WaveFormat);
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.StartRecording();
            _stopwatch.Reset();
            _stopwatch.Start();
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

    public class SpotifyWav
    {
        public SpotifyWav ( TimeSpan duration, FileInfo wavFile, string song )
        {
            Duration = duration;
            WavFile = wavFile;
            Song = song;
        }

        public TimeSpan Duration { get; }
        public FileInfo WavFile { get; }
        public string Song { get; }

    }
}
