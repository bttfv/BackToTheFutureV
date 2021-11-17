using FusionLibrary;
using FusionLibrary.Extensions;
using LemonUI.TimerBars;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BackToTheFutureV
{
    internal class RemoteTimeMachineHandler
    {
        public static readonly float MAX_DIST = 650f;

        public static TimeMachine RemoteControlling { get; private set; }
        public static bool IsRemoteOn => RemoteControlling != null;

        private static TimerBarCollection TimerBarCollection { get; }
        private static readonly TimerBarProgress SignalBar;

        public static List<RemoteTimeMachine> RemoteTimeMachines { get; private set; } = new List<RemoteTimeMachine>();
        public static int RemoteTimeMachineCount => RemoteTimeMachines.Count;

        private const int MAX_REMOTE_TIMEMACHINES = 10;

        static RemoteTimeMachineHandler()
        {
            TimerBarCollection = new TimerBarCollection(SignalBar = new TimerBarProgress(TextHandler.Me.GetLocalizedText("SignalStrength")))
            {
                Visible = false
            };

            CustomNativeMenu.ObjectPool.Add(TimerBarCollection);
        }

        public static void StartRemoteControl(TimeMachine timeMachine)
        {
            if (timeMachine == null)
            {
                return;
            }

            if (IsRemoteOn)
            {
                RemoteControlling.Events.SetRCMode?.Invoke(false, true);
            }

            timeMachine.Events.SetRCMode?.Invoke(true);
            RemoteControlling = timeMachine;

            PlayerSwitch.Disable = true;
        }

        public static void StopRemoteControl(bool instant = false)
        {
            RemoteControlling.Events.SetRCMode?.Invoke(false, instant);
            RemoteControlling = null;

            PlayerSwitch.Disable = false;
            TimerBarCollection.Visible = false;
        }

        public static RemoteTimeMachine GetTimeMachineFromIndex(int index)
        {
            try
            {
                return RemoteTimeMachines[index];
            }
            catch
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
            {
                Save();
            }

            return timeMachine;
        }

        public static void ExistenceCheck(DateTime time)
        {
            RemoteTimeMachines.ForEach(x => x.ExistenceCheck(time));
        }

        public static void Tick()
        {
            RemoteTimeMachines.ForEach(x => x.Tick());

            if (!IsRemoteOn)
            {
                return;
            }

            float squareDist = RemoteControlling.OriginalPed.DistanceToSquared2D(RemoteControlling.Vehicle);

            if (squareDist > MAX_DIST * MAX_DIST)
            {
                StopRemoteControl();
                return;
            }

            float percentage = ((MAX_DIST * MAX_DIST - squareDist) / (MAX_DIST * MAX_DIST)) * 100;

            if (!TimerBarCollection.Visible)
            {
                TimerBarCollection.Visible = true;
            }

            SignalBar.Progress = percentage;
        }

        public static void DeleteAll()
        {
            RemoteTimeMachines.ForEach(x => x.Dispose());
            RemoteTimeMachines.Clear();

            if (File.Exists(_saveFile))
            {
                File.Delete(_saveFile);
            }
        }

        private static readonly string _saveFile = "./scripts/BackToTheFutureV/RemoteTimeMachines.dmc12";

        public static void Save()
        {
            Stream stream = new FileStream(_saveFile, FileMode.Create, FileAccess.Write);

            FusionUtils.BinaryFormatter.Serialize(stream, RemoteTimeMachines.Select(x => x.TimeMachineClone).ToList());

            stream.Close();
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(_saveFile))
                {
                    return;
                }

                Stream stream = new FileStream(_saveFile, FileMode.Open, FileAccess.Read);

                List<TimeMachineClone> timeMachineClones = (List<TimeMachineClone>)FusionUtils.BinaryFormatter.Deserialize(stream);

                stream.Close();

                foreach (TimeMachineClone x in timeMachineClones)
                {
                    RemoteTimeMachines.Add(new RemoteTimeMachine(x));
                }
            }
            catch
            {
                if (File.Exists(_saveFile))
                {
                    File.Delete(_saveFile);
                }
            }
        }

        public static void Abort()
        {
            foreach (RemoteTimeMachine x in RemoteTimeMachines)
            {
                x?.Dispose();
            }
        }
    }
}
