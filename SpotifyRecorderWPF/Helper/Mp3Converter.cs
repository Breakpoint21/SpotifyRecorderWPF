using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SpotifyRecorderWPF.ObjectModel;

namespace SpotifyRecorderWPF.Helper
{
    public class Mp3Converter
    {
        public static async void ConvertToMp3(string wavFilePath, int bitrate, Mp3Tag tag)
        {
            await Task.Factory.StartNew ( ( ) =>
            {
                if (!File.Exists(wavFilePath))
                    return;

                Process process = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "lame.exe",
                        Arguments = $"-b {bitrate} --tt \"{tag.Track}\" --ta \"{tag.Artist}\"  \"{wavFilePath}\" \"{Path.ChangeExtension ( wavFilePath, ".mp3" )}\"",
                        WorkingDirectory = Path.GetDirectoryName ( typeof ( Mp3Converter ).Assembly.Location )
                    }
                };
                
                process.Start();
                process.WaitForExit(200000);
                if (!process.HasExited)
                    process.Kill();

                try
                {
                    File.Delete(wavFilePath);
                }
                catch ( Exception e )
                {
                    Console.WriteLine ( e );
                }
            } );
        }
    }
}
