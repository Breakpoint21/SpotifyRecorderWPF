using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpotifyRecorderWPF
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
                process.WaitForExit(20000);
                if (!process.HasExited)
                    process.Kill();
                File.Delete(wavFilePath);
            } );
        }
    }
}
