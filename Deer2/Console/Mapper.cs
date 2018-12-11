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
        public SortedDictionary<string, List<string>> dict = new SortedDictionary<string, List<string>>();
        public void map(string key, string value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, new List<string>());
            }
            dict[key].Add(value);
            dict[key].Sort();
        }

        public void reduce()
        {
            SortedDictionary<string, List<string>> reducedDict = new SortedDictionary<string, List<string>>();
            foreach (string key in dict.Keys)
            {
                reducedDict.Add(key, new List<string>());
                reducedDict[key] = dict[key].Distinct().ToList();
            }
            dict = reducedDict;
        }

        public SortedDictionary<string, List<string>> getMap()
        {
            return dict;
        }
    }

    public interface MapInterface
    {
        void map(String key, String value);
    }

    public interface ReduceInterface
    {
        void reduce();
    }
}
