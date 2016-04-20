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

                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                
                process.StartInfo.FileName = "lame.exe";
                process.StartInfo.Arguments = $"-b {bitrate} --tt \"{tag.Track}\" --ta \"{tag.Artist}\"  \"{wavFilePath}\" \"{Path.ChangeExtension(wavFilePath, ".mp3")}\"";

                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(typeof(Mp3Converter).Assembly.Location);
                process.Start();
                process.WaitForExit(20000);
                if (!process.HasExited)
                    process.Kill();
                File.Delete(wavFilePath);
            } );
        }
    }
}
