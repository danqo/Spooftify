using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Spooftify
{
    public class AccountManager
    {
        /// <summary>
        /// AccountManager holds the session information for the currently logged user
        /// </summary>
        public static AccountManager instance;

        public Account Acct { get => acct; set => acct = value; }
        public Playlist AllSongs { get => allSongs; set => allSongs = value; }
        public Playlist CurrentPlaylist { get => currentPlaylist; set => currentPlaylist = value; }

        private Account acct;
        private Playlist allSongs;
        private Playlist currentPlaylist;

        /// <summary>
        /// Constructor for the AccountManager
        /// </summary>
        public AccountManager()
        {
            instance = this;
        }

        /// <summary>
        /// Saves the account object sent from the server
        /// </summary>
        /// <param name="x">Account information sent back from the server</param>
        public void LoadAccount(Account x)
        {
            acct = x;
        }

        /// <summary>
        /// Used when the user has chosen to log out to remove all session information
        /// </summary>
        public void Clear()
        {
            acct = null;
            currentPlaylist = null;
        }
    }
}
