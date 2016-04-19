using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyRecorderWPF
{
    public class Mp3Tag
    {
        public string Artist { get; }
        public string Track { get; }

        public Mp3Tag ( string artist, string track )
        {
            Artist = artist;
            Track = track;
        }

        public static Mp3Tag Parse ( string songString )
        {
            if ( !string.IsNullOrEmpty(songString) )
            {
                var split = songString.Split ( new [ ] { '-' }, 2 );
                if ( split.Length == 2 )
                {
                    return new Mp3Tag(split[0], split[1]);
                }
                return new Mp3Tag(string.Empty, songString);
            }
            return new Mp3Tag(string.Empty, string.Empty);
        }
    }
}
