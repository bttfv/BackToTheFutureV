using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.TimeMachineClasses;
using GTA;

namespace BackToTheFutureV.TimeMachineClasses.RC
{
    public class RemoteTimeMachineHandler
    {
        private static List<RemoteTimeMachine> remoteTimeMachines = new List<RemoteTimeMachine>();

        private const int MAX_REMOTE_DELOREANS = 10;
        public static int TimeMachineCount => remoteTimeMachines.Count;
        public static RemoteTimeMachine GetTimeMachineFromIndex(int index) 
        { 
            try
            {
                return remoteTimeMachines[index];
            } catch
            {
                return null;
            }
        } 
        
        public static void AddRemote(TimeMachineClone timeMachineClone)
        {
            if (remoteTimeMachines.Count > MAX_REMOTE_DELOREANS)
            {
                remoteTimeMachines[0].Dispose();
                remoteTimeMachines.RemoveAt(0);
            }

            //deloreanCopy.SetupTimeTravel(true);

            remoteTimeMachines.Add(new RemoteTimeMachine(timeMachineClone));
            Save();
        }

        public static void ExistenceCheck(DateTime time)
        {
            remoteTimeMachines.ForEach(x => x.ExistenceCheck(time));
        }

        public static void Tick()
        {
            remoteTimeMachines.ForEach(x => x.Process());
        }

        public static void DeleteAll()
        {
            remoteTimeMachines.Clear();

            if (File.Exists(_saveFile))
                File.Delete(_saveFile);
        }

        private static string _saveFile = "./scripts/BackToTheFutureV/RemoteTimeMachines.dmc12";

        public static void Save()
        {
            IFormatter formatter = new BinaryFormatter();

            Stream stream = new FileStream(_saveFile, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, remoteTimeMachines.Select(x => x.TimeMachineClone).ToList());
            stream.Close();
        }

        public static void Load()
        {
            if (!File.Exists(_saveFile))
                return;

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(_saveFile, FileMode.Open, FileAccess.Read);

            List<TimeMachineClone> timeMachineClones = (List<TimeMachineClone>)formatter.Deserialize(stream);

            stream.Close();

            foreach (var x in timeMachineClones)
                remoteTimeMachines.Add(new RemoteTimeMachine(x));            
        }
    }
}
