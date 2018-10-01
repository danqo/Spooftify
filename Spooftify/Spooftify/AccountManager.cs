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
        public Playlist CurrentPlaylist { get => currentPlaylist; set => currentPlaylist = value; }

        private Account acct;
        private Playlist currentPlaylist;

        public AccountManager()
        {
            instance = this;
        }

        public void LoadAccount(String username)
        {
            string acctJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("UserJson\\{0}.json", username)));
            acct = JsonConvert.DeserializeObject<Account>(acctJson);
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
