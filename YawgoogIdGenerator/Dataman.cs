using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls.Ribbon;

namespace YawgoogIdGenerator
{
    public class PersonInfo
    {
    }
    public class ParseInfo
    {
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Preferred_Name { get; set; }
        public string Position { get; set; }
        public string Pronouns { get; set; }
        public string Camp {  get; set; }
        public string Photo { get; set; }
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


            return $"{First_Name}\t{Last_Name}\t{Preferred_Name}\t{Position}\t{Pronouns}\t{Camp}\t{Photo}\t{debool}";
        }
        public ParseInfo()
        {
            First_Name = "";
            Last_Name = "";
            Preferred_Name = "";
            Position = "";
            Pronouns = "";
            Camp = "";
            Photo = "";
            Done = false;
        }
        public ParseInfo(ParseInfo other)
        {
            First_Name = other.First_Name;
            Last_Name = other.Last_Name;
            Preferred_Name = other.Preferred_Name;
            Position = other.Position;
            Pronouns = other.Pronouns;
            Camp = other.Camp;
            Photo= other.Photo;
            Done = other.Done;
        }
        public ParseInfo(string line)
        {
            var split = line.Split('\t');
            First_Name = split[0];
            Last_Name = split[1];
            Preferred_Name = split[2];
            Position = split[3];
            split[4] = split[4].Trim('\"');
            Pronouns = split[4];
            Camp = split[5];
            Photo = split[6];
            if (split[7] == "x")
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
