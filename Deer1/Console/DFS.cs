using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDeer1
{
    public static class DFS
    {
        public static void Clear(Mapper mapper)
        {
            mapper.clear();
        }
        public static void Print(Mapper mapper)
        {
            mapper.printMap();
        }
        public static void Reduce(Mapper mapper)
        {
            mapper.emitMapReduce();
        }
        public static void Map(string type, Mapper mapper)
        {
            if (type == "title")
            {
                System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\Songs.txt"));
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    string[] words = line.Split(',');
                    mapper.emitMap(words[2], words[0] + "-" + words[1]);
                }
                file.Close();
            }
            else if (type == "artist")
            {
                System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\Songs.txt"));
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    string[] words = line.Split(',');
                    mapper.emitMap(words[0], words[1] + "-" + words[2]);
                }
                file.Close();
            }
            else if (type == "album")
            {
                System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\Songs.txt"));
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    string[] words = line.Split(',');
                    mapper.emitMap(words[1], words[0] + "-" + words[2]);
                }
                file.Close();
            }
        }
    }
}
