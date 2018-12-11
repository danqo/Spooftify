using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDeer1
{
    public static class DFS
    {
        const char START_LETTER = 'A';
        const char END_LETTER = 'F';
        public static void Clear(Mapper mapper)
        {
            mapper.getMap().Clear();
        }

        public static void Print(Mapper mapper)
        {
            foreach (string key in mapper.getMap().Keys)
            {
                Console.WriteLine(key);
                foreach (string value in mapper.getMap()[key])
                {
                    Console.WriteLine("\t\t" + value);
                }
            }
        }

        public static void Reduce(Mapper mapper)
        {
            mapper.reduce();
        }

        public static List<string> Map(Mapper mapper, string fn)
        {
            List<string> invalidEntries = new List<string>();
            System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, ("UserJson\\" + fn)));
            string line;
            while ((line = file.ReadLine()) != null)
            {
                Console.WriteLine(line);
                if (lineBelongs(line))
                {
                    AddToMap(mapper, line);
                }
                else
                {
                    invalidEntries.Add(line);
                }
            }
            file.Close();
            return invalidEntries;
        }

        public static bool lineBelongs(string line)
        {
            char firstChar = line.ToUpper()[0];
            return firstChar.CompareTo(START_LETTER) >= 0 && firstChar.CompareTo(END_LETTER) <= 0;
        }

        public static void AddToMap(Mapper mapper, string line)
        {
            char[] delimiterChars = { ':', ';' };
            string[] entries = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < entries.Length; i++)
            {
                if (i != 0)
                {
                    mapper.map(entries[0], entries[i]);
                }
            }
        }

        public static int Count(Mapper mapper)
        {
            int counter = 0;
            foreach (string key in mapper.getMap().Keys)
            {
                counter += mapper.getMap()[key].Count;
            }
            return counter;
        }

        public static void Write(Mapper mapper, string fn)
        {
            List<string> lines = new List<string>();
            string path = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, ("UserJson\\New" + fn));
            foreach(var dictKey in mapper.getMap().Keys)
            {
                string lineValue = "";
                foreach(var dictValue in mapper.getMap()[dictKey])
                {
                    lineValue += dictValue + ";";
                }
                lines.Add(dictKey + ":" + lineValue);
            }
            File.WriteAllLines(path, lines);
        }
    }
}
