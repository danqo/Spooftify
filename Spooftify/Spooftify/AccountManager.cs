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
        public static AccountManager instance;

        public Account Acct { get => acct; set => acct = value; }
        public Playlist AllSongs { get => allSongs; set => allSongs = value; }
        public Playlist CurrentPlaylist { get => currentPlaylist; set => currentPlaylist = value; }

        private Account acct;
        private Playlist allSongs;
        private Playlist currentPlaylist;

        public AccountManager()
        {
            instance = this;
        }

        public void LoadAccount(Account x)
        {
            acct = x;
        }

        public void SaveAccount()
        {
            if(acct != null)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("UserJson\\{0}.json", acct.Username)), JsonConvert.SerializeObject(acct));
            }
        }

        public void Clear()
        {
            acct = null;
            currentPlaylist = null;
        }
    }
}
