using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spooftify
{
    public class Playlist
    {
        public ObservableCollection<Song> Songs { get => mSongs; set => mSongs = value; }
        public string Name { get => mName; set => mName = value; }

        private ObservableCollection<Song> mSongs;
        private string mName;

        public Playlist(string name)
        {
            mName = name;
            mSongs = new ObservableCollection<Song>();
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
