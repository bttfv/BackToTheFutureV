using BackToTheFutureV.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BackToTheFutureV.TimeMachineClasses.RC
{
    public class RemoteTimeMachineHandler
    {
        public static List<RemoteTimeMachine> RemoteTimeMachines { get; private set; } = new List<RemoteTimeMachine>();
        public static List<RemoteTimeMachine> RemoteTimeMachinesOnlyReentry => RemoteTimeMachines.Where(x => x.Reentry).ToList();
        public static int TimeMachineCount => RemoteTimeMachines.Count;

        private static IFormatter formatter = new BinaryFormatter();
        private const int MAX_REMOTE_TIMEMACHINES = 10;

        public static RemoteTimeMachine GetTimeMachineFromIndex(int index) 
        { 
            try
            {
                return RemoteTimeMachines[index];
            } catch
            {
                return null;
            }
        } 
        
        public static RemoteTimeMachine AddRemote(TimeMachineClone timeMachineClone)
        {
            if (RemoteTimeMachines.Count > MAX_REMOTE_TIMEMACHINES)
            {
                RemoteTimeMachines[0].Dispose();
                RemoteTimeMachines.RemoveAt(0);
            }

            RemoteTimeMachine timeMachine;

            RemoteTimeMachines.Add(timeMachine = new RemoteTimeMachine(timeMachineClone));

            if (ModSettings.PersistenceSystem)
                Save();

            return timeMachine;
        }

        public static RemoteTimeMachine AddRemote(TimeMachineClone timeMachineClone, WaybackMachine waybackMachine)
        {
            if (RemoteTimeMachines.Count > MAX_REMOTE_TIMEMACHINES)
            {
                RemoteTimeMachines[0].Dispose();
                RemoteTimeMachines.RemoveAt(0);
            }

            RemoteTimeMachine timeMachine;

            RemoteTimeMachines.Add(timeMachine = new RemoteTimeMachine(timeMachineClone, waybackMachine));

            if (ModSettings.PersistenceSystem)
                Save();

            return timeMachine;
        }

        public static void ExistenceCheck(DateTime time)
        {
            RemoteTimeMachines.ForEach(x => x.ExistenceCheck(time));
        }

        public static void Process()
        {
            RemoteTimeMachines.ForEach(x => x.Process());
        }

        public static void DeleteAll()
        {
            RemoteTimeMachines.ForEach(x => x.Dispose());
            RemoteTimeMachines.Clear();

            if (File.Exists(_saveFile))
                File.Delete(_saveFile);
        }

        private static string _saveFile = "./scripts/BackToTheFutureV/RemoteTimeMachines.dmc12";

        public static void Save()
        {
            Stream stream = new FileStream(_saveFile, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, RemoteTimeMachines.Select(x => x.TimeMachineClone).ToList());
            stream.Close();
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(_saveFile))
                    return;

                Stream stream = new FileStream(_saveFile, FileMode.Open, FileAccess.Read);

                List<TimeMachineClone> timeMachineClones = (List<TimeMachineClone>)formatter.Deserialize(stream);

                stream.Close();

                foreach (var x in timeMachineClones)
                    RemoteTimeMachines.Add(new RemoteTimeMachine(x));
            } catch
            {
                if (File.Exists(_saveFile))
                    File.Delete(_saveFile);
            }          
        }

        public static void Dispose()
        {
            foreach (var x in RemoteTimeMachines)
                x?.Dispose();
        }
    }
}
