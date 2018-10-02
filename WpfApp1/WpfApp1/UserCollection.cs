using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class UserCollection
    {
        private List<User> users;
        public List<User> Users { get => users; set => users = value; }
    }
}
