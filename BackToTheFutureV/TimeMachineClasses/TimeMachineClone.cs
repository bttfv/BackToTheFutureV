using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.TimeMachineClasses
{
    [Serializable]
    public class TimeMachineCloneManager
    {
        public List<TimeMachineClone> timeMachineClones = new List<TimeMachineClone>();

        public TimeMachineCloneManager(List<TimeMachine> timeMachines)
        {
            foreach (var x in timeMachines)
                timeMachineClones.Add(x.Clone);
        }

        public void SpawnAll()
        {
            foreach (var x in timeMachineClones)
                x.Spawn();
        }

        private static string _saveFile = "./scripts/BackToTheFutureV/TimeMachines.dmc12";

        public static void Delete()
        {
            if (File.Exists(_saveFile))
                File.Delete(_saveFile);
        }

        public static void Save(List<TimeMachine> timeMachines)
        {
            IFormatter formatter = new BinaryFormatter();

            Stream stream = new FileStream(_saveFile, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, new TimeMachineCloneManager(timeMachines));
            stream.Close();
        }

        public static TimeMachineCloneManager Load()
        {
            if (!File.Exists(_saveFile))
                return null;

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(_saveFile, FileMode.Open, FileAccess.Read);

            TimeMachineCloneManager timeMachineCloneManager  = (TimeMachineCloneManager)formatter.Deserialize(stream);

            stream.Close();

            return timeMachineCloneManager;
        }
    }

    [Serializable]
    public class TimeMachineClone
    {
        public BaseMods Mods { get; }
        public BaseProperties Properties { get; }
        public VehicleInfo Vehicle { get; }

        public TimeMachineClone(TimeMachine timeMachine)
        {
            Mods = timeMachine.Mods.Clone();
            Properties = timeMachine.Properties.Clone();
            Vehicle = new VehicleInfo(timeMachine.Vehicle);
        }

        public TimeMachine Spawn()
        {
            Model model = new Model(Vehicle.Model);

            if (model == null)
                return null;

            Vehicle veh = World.GetClosestVehicle(Vehicle.Position, 1.0f, model);

            if (veh == null)
            {
                ModelHandler.RequestModel(model);
                veh = World.CreateVehicle(model, Vehicle.Position, Vehicle.Heading);
            }

            TimeMachine timeMachine = new TimeMachine(veh, Mods.WormholeType);
            
            ApplyTo(timeMachine);

            return timeMachine;
        }

        public void ApplyTo(TimeMachine timeMachine)
        {
            Vehicle.ApplyTo(timeMachine.Vehicle, false);
            Mods.ApplyTo(timeMachine);
            Properties.ApplyTo(timeMachine);
        }
    }
}
