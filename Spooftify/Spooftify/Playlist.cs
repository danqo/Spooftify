using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Playlist
    {
        public IList<Song> Songs { get => mSongs; set => mSongs = value; }
        public string Name { get => mName; set => mName = value; }

        private IList<Song> mSongs;
        private string mName;

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
