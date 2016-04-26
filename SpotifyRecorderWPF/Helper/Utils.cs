using System.IO;
using System.Linq;

namespace SpotifyRecorderWPF.Helper
{
    public class Utils
    {
        public static string GetWavFileName ( DirectoryInfo outputDirectory, string currentSong )
        {
            var cleanSongName = $"{currentSong}.wav";
            Path.GetInvalidFileNameChars().ToList().ForEach(x => cleanSongName = cleanSongName.Replace(x, '_'));
            return Path.Combine ( outputDirectory.FullName, cleanSongName);
        }
    }
}
