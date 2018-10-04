using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spooftify
{
    public class Account
    {
        public IList<Playlist> Playlists { get => playlists; set => playlists = value; }
        public string Username { get => username; set => username = value; }
        public string Name { get => name; set => name = value; }
        public string Email { get => email; set => email = value; }
        public string Birthday { get => birthday; set => birthday = value; }
        public string AvatarURI { get => avatarURI; set => avatarURI = value; }

        private IList<Playlist> playlists;
        private string username;
        private string name;
        private string email;
        private string birthday;
        private string avatarURI;

        public Account()
        {
            username = "";
            name = "";
            email = "";
            birthday = "";
            avatarURI = "";
            playlists = new List<Playlist>();
        }

        public Playlist FindPlaylist(String playlistName)
        {
            return playlists.FirstOrDefault(x => x.Name.Equals(playlistName));
        }
    }
}
