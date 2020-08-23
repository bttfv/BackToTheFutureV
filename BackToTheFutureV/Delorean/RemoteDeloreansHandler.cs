using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.Delorean;
using GTA;

namespace BackToTheFutureV
{
    public class RemoteDeloreansHandler
    {
        private static List<RemoteDelorean> remoteDeloreans = new List<RemoteDelorean>();

        private const int MAX_REMOTE_DELOREANS = 10;
        public static int TimeMachineCount => remoteDeloreans.Count;
        public static RemoteDelorean GetTimeMachineFromIndex(int index) 
        { 
            try
            {
                return remoteDeloreans[index];
            } catch
            {
                return null;
            }
        } 
        
        public static void AddDelorean(DeloreanCopy deloreanCopy)
        {
            if (remoteDeloreans.Count > MAX_REMOTE_DELOREANS)
            {
                remoteDeloreans[0].Dispose();
                remoteDeloreans.RemoveAt(0);
            }

            deloreanCopy.SetupTimeTravel(true);

            remoteDeloreans.Add(new RemoteDelorean(deloreanCopy));
            Save();
        }

        public static void ExistenceCheck(DateTime time)
        {
            remoteDeloreans.ForEach(x => x.ExistenceCheck(time));
        }

        public static void Tick()
        {
            remoteDeloreans.ForEach(x => x.Process());
        }

        public static void DeleteAll()
        {
            remoteDeloreans.Clear();

            if (File.Exists(_saveFile))
                File.Delete(_saveFile);
        }

        private static string _saveFile = "./scripts/BackToTheFutureV/RemoteTimeMachines.dmc12";

        public static void Save()
        {
            IFormatter formatter = new BinaryFormatter();

            Stream stream = new FileStream(_saveFile, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, remoteDeloreans.Select(x => x.DeloreanCopy).ToList());
            stream.Close();
        }

        public static void Load()
        {
            if (!File.Exists(_saveFile))
                return;

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(_saveFile, FileMode.Open, FileAccess.Read);

            List<DeloreanCopy> deloreanCopies = (List<DeloreanCopy>)formatter.Deserialize(stream);

            stream.Close();

            foreach (var x in deloreanCopies)
                remoteDeloreans.Add(new RemoteDelorean(x));            
        }
    }
}
