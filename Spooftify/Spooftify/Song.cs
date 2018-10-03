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
        public override string ToString()
        {
            return mArtist + " - " + mTitle + " - " + Album;
           // return mArtist + " - " + mTitle + String.Format("({})", mAlbum) + (mExtension == ".mp3"? " - Music" : " - Music Video");
        }
    }
}
