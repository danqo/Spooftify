using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Playlist
    {
        public string mName;
        public IList<Song> mSongs;

        public Playlist(string name)
        {
            mName = name;
            mSongs = new List<Song>();
        }

        public void addSong(Song newSong)
        {
           
            mSongs.Add(newSong);
        }

        public void deleteSong(Song song)
        {
            mSongs.Remove(song);
        }
    }
}
