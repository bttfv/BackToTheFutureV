using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Serialization;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BackToTheFutureV
{
    [Serializable]
    public class VehicleInfo
    {
        public int Model { get; set; }
        public Vector3 Velocity { get; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public float Heading { get; set; }
        public float Speed { get; }
        public float Health { get; }
        public float EngineHealth { get; }
        public bool EngineRunning { get; }
        public VehicleColor PrimaryColor { get; }
        public VehicleColor SecondaryColor { get; }
        public int Livery { get; }

        public VehicleInfo(Vehicle veh)
        {
            Model = veh.Model;
            Velocity = veh.Velocity;
            Position = veh.Position;
            Rotation = veh.Rotation;
            Heading = veh.Heading;
            Speed = veh.Speed;
            Health = veh.HealthFloat;
            EngineHealth = veh.EngineHealth;
            EngineRunning = veh.IsEngineRunning;
            PrimaryColor = veh.Mods.PrimaryColor;
            SecondaryColor = veh.Mods.SecondaryColor;
            Livery = veh.Mods.Livery;
        }

        public Vehicle Spawn()
        {
            Vehicle veh = World.CreateVehicle(Model, Position, Heading);

            ApplyTo(veh);

            return veh;
        }

        public Vehicle Spawn(Vector3 position, float heading)
        {
            Vehicle veh = World.CreateVehicle(Model, position, heading);

            ApplyTo(veh, true); ;

            return veh;
        }

        public void ApplyTo(Vehicle veh, bool noPos = false)
        {
            if (!noPos)
            {
                veh.Position = Position;
                veh.Rotation = Rotation;
                veh.Heading = Heading;
            }
            
            veh.Velocity = Velocity;            
            veh.Speed = Speed;
            veh.HealthFloat = Health;
            veh.EngineHealth = EngineHealth;
            veh.IsEngineRunning = EngineRunning;
            veh.Mods.PrimaryColor = PrimaryColor;
            veh.Mods.SecondaryColor = SecondaryColor;
            veh.Mods.Livery = Livery;
        }
    }

    public class PedInfo
    {
        public PedInfo(Ped ped)
        {
            Model = ped.Model;
            Type = Function.Call<int>(Hash.GET_PED_TYPE, ped);

            Position = ped.Position;
            Rotation = ped.Rotation;
            Heading = ped.Heading;

            SitInCar = ped.IsSittingInVehicle();

            if (SitInCar)
                VehicleSeat = ped.SeatIndex;
        }

        public int Model { get; set; }
        public int Type { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public float Heading { get; set; }
        public bool SitInCar { get; set; }
        public VehicleSeat VehicleSeat { get; set; }

        public Ped Spawn()
        {
            Ped ped = Function.Call<Ped>(Hash.CREATE_PED, Type, Model, Position.X, Position.Y, Position.Z, Heading, false, false);

            ped.Rotation = Rotation;

            return ped;
        }

        public Ped Spawn(Vector3 position, float heading)
        {
            return Function.Call<Ped>(Hash.CREATE_PED, Type, Model, position.X, position.Y, position.Z, heading, false, false);
        }

        public Ped Spawn(Vehicle vehicle)
        {
            return Function.Call<Ped>(Hash.CREATE_PED_INSIDE_VEHICLE, vehicle, Type, Model, SitInCar ? VehicleSeat : VehicleSeat.Driver, false, false);
        }

        public Ped Spawn(Vehicle vehicle, VehicleSeat vehicleSeat)
        {
            return Function.Call<Ped>(Hash.CREATE_PED_INSIDE_VEHICLE, vehicle, Type, Model, vehicleSeat, false, false);
        }
    }

    public class Moment
    {
        public Moment(DateTime currentDate, Weather weather, float puddleLevel, int wantedLevel, IEnumerable<VehicleInfo> infos)
        {
            CurrentDate = currentDate;
            Weather = weather;
            PuddleLevel = puddleLevel;
            WantedLevel = wantedLevel;
            Vehicles = infos.ToList();
        }

        public Weather Weather { get; set; }

        public int WantedLevel { get; set; }

        public float PuddleLevel { get; set; }

        public DateTime CurrentDate { get; set; }

        public List<VehicleInfo> Vehicles { get; set; }
    }

    public class Era
    {
        /// <summary>
        /// The start of this era in years, for e.g. 2018
        /// </summary>
        [XmlAttribute]
        public int EraStart { get; set; }

        /// <summary>
        /// The end of this era in years, for e.g. 3018
        /// </summary>
        [XmlAttribute]
        public int EraEnd { get; set; }

        /// <summary>
        /// List of the vehicles that can spawn in this Era
        /// </summary>
        public EraVehicleInfo[] SpawnableVehicles { get; set; }

        private int maxProbability = -1;

        public EraVehicleInfo GetRandomVehicle()
        {
            var maxProbability = Math.Max(GetMaxProbability(), 100);
            var randomNum = Utils.Random.NextDouble(0, maxProbability);

            var cumulative = 0;
            foreach (var vehicleInfo in SpawnableVehicles)
            {
                cumulative += vehicleInfo.Probability;

                if (randomNum < cumulative)
                    return vehicleInfo;
            }

            return null;
        }

        private int GetMaxProbability()
        {
            if (maxProbability != -1)
                return maxProbability;

            int prob = 0;

            foreach(var veh in SpawnableVehicles)
            {
                if (veh.Probability > prob)
                    prob = veh.Probability;
            }

            maxProbability = prob;
            return prob;
        }

        private static List<Era> loadedEras = new List<Era>();

        public static void LoadEraXmls(string path)
        {
            var files = Directory.GetFiles(path);

            foreach(var file in files)
            {
                var extension = Path.GetExtension(file);
                if (extension != ".xml") return; // TODO: Log to output file

                var era = Utils.DeserializeObject<Era>(file);

                if (era != null)
                    loadedEras.Add(era);
            }

            // TODO: Log in output file: xx amount of eras loaded
        }

        public static Era GetEraForCurrentTime()
        {
            var time = Utils.GetWorldTime();

            foreach(var era in loadedEras)
            {
                if(time.Year >= era.EraStart && time.Year <= era.EraEnd)
                {
                    return era;
                }
            }

            return null;
        }
    }

    public class EraVehicleInfo
    {
        /// <summary>
        /// The model of the vehicle
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Value between 0 - 100 that defines the probability of its spawn. 
        /// If 100, will spawn 100% of the time. If 0, will spawn 0% of the time
        /// </summary>
        public int Probability { get; set; }

        /// <summary>
        /// The zones in which the vehicle can spawn in
        /// Use "all" to spawn in all zones
        /// </summary>
        public string[] Zone { get; set; }
    }
}
