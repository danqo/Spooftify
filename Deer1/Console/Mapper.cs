using Spooftify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDeer1
{
    public class Mapper : MapInterface, ReduceInterface
    {
        public static SortedDictionary<string, List<string>> dict = new SortedDictionary<string, List<string>>();
        public void emitMap(String key, String value)
        {
            if (!dict.ContainsKey(key))
            {
                List<string> g = new List<string>();
                dict[key] = g;
                g.Add(value);
            }
            else
            {
                dict[key].Add(value);
            }

        }
        public void clear()
        {
            dict.Clear();
        }
        public void printMap()
        {
            foreach (var b in dict)
            {
                Console.WriteLine(b.Key);
                foreach (var f in dict[b.Key])
                {

                    Console.WriteLine("\t\t\t" + f.ToString());
                }

            }
        }
        public void emitMapReduce()
        {
            SortedDictionary<string, SortedSet<string>> dictReduce = new SortedDictionary<string, SortedSet<string>>();
            foreach (var b in dict)
            {
                foreach (var f in dict[b.Key])
                {

                    if (!dictReduce.ContainsKey(b.Key))
                    {
                        SortedSet<string> g = new SortedSet<string>();
                        dictReduce[b.Key] = g;
                        g.Add(f.ToString());
                    }
                    else
                    {
                        dictReduce[b.Key].Add(f.ToString());
                    }
                }
            }

            dict.Clear();

            using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(@"C:\Users\nhan\Desktop\Spring2018\CECS491A\Spooftify\Deer1\Console\UserJson\Reduced.txt", false))
            {
                foreach (var b in dictReduce)
                {
                    foreach (var f in dictReduce[b.Key])
                    {
                        file.WriteLine(b.Key + "," + f);
                    }
                }
            }

            foreach (var b in dictReduce)
            {
                foreach (var f in dictReduce[b.Key])
                {

                    if (!dict.ContainsKey(b.Key))
                    {
                        List<string> g = new List<string>();
                        dict[b.Key] = g;
                        g.Add(f.ToString());
                    }
                    else
                    {
                        dict[b.Key].Add(f.ToString());
                    }
                }
            }
            printMap();
        }

    }
    public interface MapInterface
    {
        void emitMap(String key, String value);
    }
    public interface ReduceInterface
    {
        void emitMapReduce();
    }
}
