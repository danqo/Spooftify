using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spooftify
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
            Song s = new Song(newSong);
            mSongs.Add(s);
        }

        public void deleteSong(Song song)
        {
            mSongs.Remove(song);
        }

        public override string ToString()
        {
            return mName;
        }
    }
}
