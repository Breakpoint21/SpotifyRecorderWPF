using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyRecorderWPF
{
    public class Utils
    {
        public static string GetWavFileName ( DirectoryInfo outputDirectory, string currentSong )
        {
            var cleanSongName = Path.Combine(currentSong, ".wav");
            Path.GetInvalidFileNameChars().ToList().ForEach(x => cleanSongName = cleanSongName.Replace(x, '_'));
            return Path.Combine ( outputDirectory.FullName, cleanSongName);
        }
    }
}
