﻿using BackToTheFutureV.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses
{
    [Serializable]
    internal class TimeMachineCloneHandler
    {
        public List<TimeMachineClone> timeMachineClones = new List<TimeMachineClone>();

        public TimeMachineCloneHandler(List<TimeMachine> timeMachines)
        {
            foreach (TimeMachine x in timeMachines)
                timeMachineClones.Add(x.Clone());
        }

        public void SpawnAll()
        {
            foreach (TimeMachineClone x in timeMachineClones)
                x.Spawn(SpawnFlags.CheckExists);
        }

        private static string _saveFile = "./scripts/BackToTheFutureV/TimeMachines.dmc12";
        private static IFormatter formatter = new BinaryFormatter();

        public static void Delete()
        {
            if (File.Exists(_saveFile))
                File.Delete(_saveFile);
        }

        public static void Save(List<TimeMachine> timeMachines)
        {
            Stream stream = new FileStream(_saveFile, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, new TimeMachineCloneHandler(timeMachines));
            stream.Close();
        }

        public static TimeMachineCloneHandler Load()
        {
            if (!File.Exists(_saveFile))
                return null;

            Stream stream = new FileStream(_saveFile, FileMode.Open, FileAccess.Read);

            TimeMachineCloneHandler timeMachineCloneManager = (TimeMachineCloneHandler)formatter.Deserialize(stream);

            stream.Close();

            return timeMachineCloneManager;
        }
    }
}