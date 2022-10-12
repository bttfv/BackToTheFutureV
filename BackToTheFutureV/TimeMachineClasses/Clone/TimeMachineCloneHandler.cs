using FusionLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class TimeMachineCloneHandler
    {
        public List<TimeMachineClone> timeMachineClones = new List<TimeMachineClone>();

        public TimeMachineCloneHandler(List<TimeMachine> timeMachines)
        {
            foreach (TimeMachine x in timeMachines)
            {
                timeMachineClones.Add(x.Clone());
            }
        }

        public void SpawnAll()
        {
            foreach (TimeMachineClone x in timeMachineClones)
            {
                x.Spawn(SpawnFlags.CheckExists);
            }
        }

        private static readonly string _saveFile = "./scripts/BackToTheFutureV/TimeMachines.dmc12";

        public static void Delete()
        {
            if (File.Exists(_saveFile))
            {
                File.Delete(_saveFile);
            }
        }

        public static void Save(List<TimeMachine> timeMachines)
        {
            using (Stream stream = new FileStream(_saveFile, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                FusionUtils.BinaryFormatter.Serialize(stream, new TimeMachineCloneHandler(timeMachines));
            }
        }

        public static TimeMachineCloneHandler Load()
        {
            if (!File.Exists(_saveFile))
            {
                return null;
            }

            using (Stream stream = new FileStream(_saveFile, FileMode.Open, FileAccess.Read, FileShare.Write))
            {
                TimeMachineCloneHandler timeMachineCloneManager = (TimeMachineCloneHandler)FusionUtils.BinaryFormatter.Deserialize(stream);

                return timeMachineCloneManager;
            }
        }
    }
}
