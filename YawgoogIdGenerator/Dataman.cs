using System.Data;
using System.IO;
using System.Windows;

namespace YawgoogIdGenerator
{
    public class PersonInfo
    {
    }
    public class ParseInfo
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Pronouns { get; set; }
        public string Photo { get; set; }
        public string Camp {  get; set; }
        public bool Done { get; set; }
        public string Warnings { get; set; }

        public override string ToString()
        {
            string debool;
            if (Done == true)
            {
                debool = "x";
            }
            else
            {
                debool = "";
            }


            return $"{Name},{Position},{Pronouns},{Photo},{Camp},{debool}";
        }
        public ParseInfo()
        {
            Name = "";
            Position = "";
            Pronouns = "";
            Photo = "";
            Camp = "";
            Done = false;
        }
        public ParseInfo(ParseInfo other)
        {
            Name = other.Name;
            Position = other.Position;
            Pronouns = other.Pronouns;
            Photo= other.Photo;
            Camp = other.Camp;
            Done = other.Done;
        }
        public ParseInfo(string line)
        {
            var split = line.Split(',');
            Name = split[0];
            Position = split[1];
            Pronouns = split[2];
            Photo = split[3];
            Camp = split[4];
            if (split[5] == "x")
            {
                Done = true;
            }
            else
            {
                Done = false;
            }

        }
    }
    class Dataman
    {
        public List<ParseInfo> data;
        //TODO
        public void saveasData()
        {

        }
        public void saveData(string file)
        {
            try
            {
                string csv = String.Join("\n", data);

                File.WriteAllText($"{file}", csv);
            }
            catch (Exception)
            {
                MessageBox.Show("Another Program is using this file close out the app using it");
            }
        }

        public bool populateData(string file)
        {
            try
            {
                data = File.ReadLines(file).Select(line => new ParseInfo(line)).ToList();
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Couldn't Generate CSV File Did you hit cancel instead of save?");
                return false;
            }

        }
    }
}
