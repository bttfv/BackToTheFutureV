using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class TimeMachineClone
    {
        public ModsPrimitive Mods { get; }
        public PropertiesHandler Properties { get; }
        public VehicleReplica Vehicle { get; }

        public TimeMachineClone(TimeMachine timeMachine)
        {
            Mods = timeMachine.Mods.Clone();
            Properties = timeMachine.Properties.Clone();
            Vehicle = timeMachine.Vehicle.Clone();

            if (Properties.TimeTravelDestPos != Vector3.Zero)
            {
                Vehicle.Position = Vehicle.Position.TransferHeight(Properties.TimeTravelDestPos);
                Properties.TimeTravelDestPos = Vector3.Zero;
            }
        }

        public TimeMachine Spawn(SpawnFlags spawnFlags = SpawnFlags.Default)
        {
            return TimeMachineHandler.Create(this, spawnFlags);
        }

        public void ApplyTo(TimeMachine timeMachine, SpawnFlags spawnFlags)
        {
            Vehicle.ApplyTo(timeMachine.Vehicle, spawnFlags);
            Mods.ApplyTo(timeMachine);

            if (!spawnFlags.HasFlag(SpawnFlags.NoProperties))
            {
                Properties.ApplyTo(timeMachine);
            }
        }

        public void Save(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
            {
                name += ".dmc12";
            }

            name = FusionUtils.RemoveIllegalFileNameChars(name);

            if (!Directory.Exists(PresetsPath))
            {
                Directory.CreateDirectory(PresetsPath);
            }

            using (Stream stream = new FileStream($"{PresetsPath}/{name}", FileMode.Create, FileAccess.Write))
            {
                FusionUtils.BinaryFormatter.Serialize(stream, this);
            }
        }

        public static string PresetsPath = "./scripts/BackToTheFutureV/presets";

        public static bool PresetExists(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
            {
                name += ".dmc12";
            }

            return File.Exists($"{PresetsPath}/{name}");
        }

        public static List<string> ListPresets()
        {
            if (!Directory.Exists(PresetsPath))
            {
                Directory.CreateDirectory(PresetsPath);
            }

            return new DirectoryInfo(PresetsPath).GetFiles("*.dmc12").Select(x => x.Name.Replace(".dmc12", "")).ToList();
        }

        public static void DeleteSave(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
            {
                name += ".dmc12";
            }

            File.Delete($"{PresetsPath}/{name}");
        }

        public static void DeleteAll()
        {
            if (!Directory.Exists(PresetsPath))
            {
                return;
            }

            new DirectoryInfo(PresetsPath).GetFiles("*.dmc12").ToList().ForEach(x => x.Delete());
        }

        public static void RenameSave(string name, string newName)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
            {
                name += ".dmc12";
            }

            if (!newName.ToLower().EndsWith(".dmc12"))
            {
                newName += ".dmc12";
            }

            File.Move($"{PresetsPath}/{name}", $"{PresetsPath}/{newName}");
        }

        public static TimeMachineClone Load(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
            {
                name += ".dmc12";
            }

            using (Stream stream = new FileStream($"{PresetsPath}/{name}", FileMode.Open, FileAccess.Read))
            {
                TimeMachineClone baseMods = (TimeMachineClone)FusionUtils.BinaryFormatter.Deserialize(stream);

                return baseMods;
            }
        }
    }
}
