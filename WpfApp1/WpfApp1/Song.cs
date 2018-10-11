using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Song
    {
        public string mTitle;
        public string mArtist;
        public string mExtension;
        public string mAlbum;
        public string mAlbumArt;
        public string Title { get => mTitle; set => mTitle = value; }
        public string Artist { get => mArtist; set => mArtist = value; }
        public string Extension { get => mExtension; set => mExtension = value; }
        public string Album { get => mAlbum; set => mAlbum = value; }
        public string Directory { get => mArtist + "-" + mTitle + mExtension; }
        public string AlbumArt { get => mAlbumArt; set => mAlbumArt = value; }
        public Song()
        {

        }
        public Song(string title,  string artist)
        {
            mTitle = title;
            mArtist = artist;
            mAlbumArt = "";
        }
        public Song(string title, string artist, string extension)
        {
            mTitle = title;
            mArtist = artist;
            mExtension = extension;
            mAlbumArt = "";
        }
        public override string ToString()
        {
            return String.Format("{0} ({1}) - {2}", mArtist, mAlbum, mTitle);
        }
    }
}
