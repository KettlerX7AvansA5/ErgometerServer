using ErgometerLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ErgometerServer
{
    public class FileHandler
    {
        public static string DataFolder { get; } = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Ergometer");
        public static string UsersFile { get; } = Path.Combine(DataFolder, "users.ergo");

        public static void CheckStorage()
        {
            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }

            if (!File.Exists(UsersFile))
            {
                using (Stream stream = File.Open(UsersFile, FileMode.Create))
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(1);
                    writer.Write("Doctor0tVfW");
                    writer.Write("password");
                }
            }
        }

        //SESSION 
        public static int GenerateSession()
        {
            string[] existingSessions = Directory.GetDirectories(DataFolder);

            Random rand = new Random();
            int sessionID = rand.Next(0, int.MaxValue);


            while (existingSessions.Contains(sessionID.ToString()))
            {
                sessionID = rand.Next(int.MinValue, int.MaxValue);
            }

            return sessionID;
        }

        public static void CreateSession(int session, string naam)
        {
            Directory.CreateDirectory(GetSessionFolder(session));

            using (File.Create(Path.Combine(GetSessionFile(session)))) ;
            using (File.Create(Path.Combine(GetSessionMetingen(session)))) ;
            using (File.Create(Path.Combine(GetSessionChat(session)))) ;

            File.WriteAllText(GetSessionFile(session), naam + Environment.NewLine + Helper.Now);
            Console.WriteLine("Created session at " + Helper.MillisecondsToTime(Helper.Now));
        }

        public static void WriteMetingen(int session, List<Meting> metingen)
        {
            if (metingen.Count <= 5 && Directory.Exists(GetSessionFolder(session)))
            {
                Directory.Delete(GetSessionFolder(session), true);
            }
            else
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(metingen.ToArray());
                File.WriteAllText(GetSessionMetingen(session), json);
                Console.WriteLine("Writing metingen: " + GetSessionMetingen(session));
            }
        }

        public static List<Meting> ReadMetingen(int session)
        {
            string json = File.ReadAllText(GetSessionMetingen(session));

            List<Meting> metingen = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Meting>>(json);
            Console.WriteLine("Reading metingen: " + GetSessionMetingen(session));
            return metingen;
        }

        public static List<Tuple<int, string, double>> GetAllSessions()
        {
            string[] directories = Directory.GetDirectories(DataFolder);
            List<Tuple<int,string,double>> sessiondata = new List<Tuple<int, string, double>>();
            for (int i = 0; i < directories.Length; i++)
            {
                string props = File.ReadAllText(GetSessionFile(int.Parse(directories[i])));
                string[] properties = props.Split('\n');
                string name = properties[0];
                double date = double.Parse(properties[1]);
                sessiondata.Add(new Tuple<int, string, double>(int.Parse(directories[i]), name, date));
            }
            return sessiondata;
        }

        public static void WriteChat(int session, List<ChatMessage> chat)
        {
            string write = "";
            foreach (ChatMessage c in chat)
            {
                write += c.ToString() + "\n";
            }

            File.WriteAllText(GetSessionChat(session), write);
            Console.WriteLine("Writing chat: " + GetSessionChat(session));
        }

        private static string GetSessionFolder(int session)
        {
            return Path.Combine(DataFolder, session.ToString());
        }
        private static string GetSessionFile(int session)
        {
            return Path.Combine(DataFolder, session.ToString(), "session.prop");
        }
        private static string GetSessionMetingen(int session)
        {
            return Path.Combine(DataFolder, session.ToString(), "metingen.ergo");
        }
        private static string GetSessionChat(int session)
        {
            return Path.Combine(DataFolder, session.ToString(), "chat.log");
        }

        //USER MANAGEMENT
        public static Dictionary<string, string> LoadUsers()
        {
            Dictionary<string, string> users = new Dictionary<string, string>();

            using (Stream stream = File.Open(UsersFile, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(stream);
                int count = reader.ReadInt32();
                for (int n = 0; n < count; n++)
                {
                    var key = reader.ReadString();
                    var value = reader.ReadString();
                    users.Add(key, value);
                }
            }

            return users;
        }

        public static void SaveUsers(Dictionary<string, string> users)
        {
            using (Stream stream = File.Open(UsersFile, FileMode.Open))
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(users.Count);
                foreach (var kvp in users)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
                writer.Flush();
            }
        }
    }
}
