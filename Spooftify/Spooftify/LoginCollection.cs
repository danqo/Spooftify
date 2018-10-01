using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spooftify
{
    public class LoginCollection
    {
        public class LoginInfo
        {
            public string Username { get => username; set => username = value; }
            public string Password { get => password; set => password = value; }

            private string username = String.Empty;
            private string password = String.Empty;
        }

        public IList<LoginInfo> LoginList { get => loginList; set => loginList = value; }
        private IList<LoginInfo> loginList;
    }
}
