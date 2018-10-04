using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spooftify
{
    public class Song
    {
        public string Title { get => mTitle; set => mTitle = value; }
        public string Artist { get => mArtist; set => mArtist = value; }
        public string Extension { get => mExtension; set => mExtension = value; }
        public string Album { get => mAlbum; set => mAlbum = value; }
        public string Directory { get => mArtist + "-" + mTitle + mExtension; }
        public string Display { get => ToString(); }

        private string mTitle;
        private string mArtist;
        private string mExtension;
        private string mAlbum;

        public Song()
        {

        }
        public Song(string title,  string artist)
        {
            mTitle = title;
            mArtist = artist;
        }

        public Song(string title, string artist, string extension)
        {
            mTitle = title;
            mArtist = artist;
            mExtension = extension;
        }

        public Song(Song s)
        {
            mTitle = s.Title;
            mArtist = s.Artist;
            mExtension = s.Extension;
            mAlbum = s.Album;
        }

        public override string ToString()
        {
            return String.Format("{0} ({1}) - {2}", mArtist, mAlbum, mTitle);
        }

        public bool IsSame(Song s)
        {
            return mTitle.Equals(s.Title) && mArtist.Equals(s.Artist) && mAlbum.Equals(s.Album);
        }
    }
}
