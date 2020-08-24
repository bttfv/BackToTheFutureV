using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackToTheFutureV.Delorean
{
    [Serializable]
    public class DeloreanCopyManager
    {
        public List<DeloreanCopy> deloreanCopies = new List<DeloreanCopy>();

        public DeloreanCopyManager(List<DeloreanTimeMachine> deloreanTimeMachines)
        {
            foreach (var x in deloreanTimeMachines)
                deloreanCopies.Add(x.Copy);
        }

        public void SpawnAll()
        {
            foreach (var x in deloreanCopies)
                x.Spawn();
        }

        private static string _saveFile = "./scripts/BackToTheFutureV/TimeMachines.dmc12";

        public static void Delete()
        {
            if (File.Exists(_saveFile))
                File.Delete(_saveFile);
        }

        public static void Save(List<DeloreanTimeMachine> deloreanTimeMachines)
        {
            IFormatter formatter = new BinaryFormatter();

            Stream stream = new FileStream(_saveFile, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, new DeloreanCopyManager(deloreanTimeMachines));
            stream.Close();
        }

        public static DeloreanCopyManager Load()
        {
            if (!File.Exists(_saveFile))
                return null;

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(_saveFile, FileMode.Open, FileAccess.Read);

            DeloreanCopyManager deLoreanCopyManager = (DeloreanCopyManager)formatter.Deserialize(stream);

            stream.Close();

            return deLoreanCopyManager;
        }
    }

    [Serializable]
    public class DeloreanCopy
    {
        public DeloreanModsCopy Mods;
        public TimeCircuitsCopy Circuits;
        public VehicleInfo VehicleInfo;

        public DeloreanCopy(DeloreanTimeMachine sourceCar, bool noLastDisplacementCopy = false)
        {
            Mods = new DeloreanModsCopy(sourceCar.Mods);
            Circuits = new TimeCircuitsCopy(sourceCar.Circuits, noLastDisplacementCopy);
            VehicleInfo = new VehicleInfo(sourceCar.Vehicle);
        }

        public void ApplyTo(DeloreanTimeMachine destinationCar)
        {
            VehicleInfo.ApplyTo(destinationCar.Vehicle, false);
            Mods.ApplyTo(destinationCar);
            Circuits.ApplyTo(destinationCar.Circuits);
        }

        public void SetupTimeTravel(bool forReentry)
        {
            Circuits.IsFueled = forReentry;

            if (forReentry)
            {
                if (Mods.Hook == HookState.Removed)
                    Mods.Hook = HookState.On;

                if (Mods.Plate == PlateType.Empty)
                    Mods.Plate = PlateType.Outatime;
            } else
            {
                if (Mods.Hook == HookState.On)
                    Mods.Hook = HookState.Removed;

                if (Mods.Plate == PlateType.Outatime)
                    Mods.Plate = PlateType.Empty;
            }
        }

        public DeloreanTimeMachine Spawn()
        {
            Model model = new Model(VehicleInfo.Model);

            if (model == null)
                return null;

            Vehicle veh = World.GetClosestVehicle(VehicleInfo.Position, 1.0f, model);

            if (veh == null)
            {
                ModelHandler.RequestModel(model);
                veh = World.CreateVehicle(model, VehicleInfo.Position, VehicleInfo.Heading);
            }
                
            DeloreanTimeMachine _destinationCar = new DeloreanTimeMachine(veh, true, Mods.DeloreanType);
            ApplyTo(_destinationCar);

            return _destinationCar;
        }
    }

    [Serializable]
    public class TimeCircuitsCopy
    {
        public DeloreanCopy LastDisplacementCopy { get; }
        public bool IsOn { get; }
        public DateTime DestinationTime { get; set; }
        public DateTime PreviousTime { get; }
        public bool IsFueled { get; set; }
        public bool WasOnTracks { get; }
        public bool IsWarmedUp { get; }
        public bool IsFlying { get; }        
        public bool CutsceneMode { get; }
        public bool IsRemoteControlled { get; }
        public bool IsFreezing { get; }
        public float IceValue { get; }
        public Vector3 LastVelocity { get;  }
        
        public TimeCircuitsCopy(TimeCircuits circuits, bool noLastDisplacementCopy = false)
        {
            if (!noLastDisplacementCopy)
                LastDisplacementCopy = circuits.Delorean.LastDisplacementCopy;

            IsOn = circuits.IsOn;
            DestinationTime = circuits.DestinationTime;
            PreviousTime = circuits.PreviousTime;
            IsFueled = circuits.IsFueled;
            WasOnTracks = circuits.WasOnTracks;
            IsWarmedUp = circuits.IsWarmedUp;
            IsFlying = circuits.IsFlying;
            CutsceneMode = circuits.GetHandler<TimeTravelHandler>().CutsceneMode;
            IsRemoteControlled = circuits.IsRemoteControlled;            
            LastVelocity = circuits.Delorean.LastVelocity;
            IsFreezing = circuits.IsFreezing;

            if (IsFreezing)
                IceValue = Function.Call<float>(Hash.GET_VEHICLE_ENVEFF_SCALE, circuits.Vehicle);
        }

        public void ApplyTo(TimeCircuits circuits)
        {
            circuits.Delorean.LastDisplacementCopy = LastDisplacementCopy;
            circuits.IsOn = IsOn;
            circuits.DestinationTime = DestinationTime;
            circuits.PreviousTime = PreviousTime;
            circuits.IsFueled = IsFueled;
            circuits.WasOnTracks = WasOnTracks;
            circuits.GetHandler<TimeTravelHandler>().CutsceneMode = CutsceneMode;
            circuits.IsRemoteControlled = IsRemoteControlled;
            circuits.Delorean.LastVelocity = LastVelocity;

            if (IsWarmedUp)
                circuits.GetHandler<HoodboxHandler>().SetInstant();

            if (IsFlying)
                circuits.GetHandler<FlyingHandler>().SetFlyMode(true, true);

            if (IsFreezing)
            {
                Function.Call(Hash.SET_VEHICLE_ENVEFF_SCALE, circuits.Vehicle, IceValue);
                circuits.IsFreezing = true;
            }
        }
    }

    [Serializable]
    public class DeloreanModsCopy
    {
        public DeloreanType DeloreanType;

        public WheelType Wheel;

        public ModState Exterior;

        public ModState Interior;

        public ModState OffCoils;

        public ModState GlowingEmitter;

        public ModState GlowingReactor;

        public ModState DamagedBumper;

        public ModState HoverUnderbody;

        public ModState SteeringWheelsButtons;

        public ModState Vents;

        public ModState Seats;

        public ReactorType Reactor;

        public PlateType Plate;

        public ExhaustType Exhaust;

        public ModState Hoodbox;

        public HookState Hook;

        public SuspensionsType SuspensionsType;

        public DeloreanModsCopy(DeloreanMods deloreanMods)
        {
            DeloreanType = deloreanMods.DeloreanType;
            SuspensionsType = deloreanMods.SuspensionsType;
            Wheel = deloreanMods.Wheel;
            Exterior = deloreanMods.Exterior;
            Interior = deloreanMods.Interior;
            OffCoils = deloreanMods.OffCoils;
            GlowingEmitter = deloreanMods.GlowingEmitter;
            GlowingReactor = deloreanMods.GlowingReactor;
            DamagedBumper = deloreanMods.DamagedBumper;
            SteeringWheelsButtons = deloreanMods.SteeringWheelsButtons;
            HoverUnderbody = deloreanMods.HoverUnderbody;
            Vents = deloreanMods.Vents;
            Seats = deloreanMods.Seats;
            Reactor = deloreanMods.Reactor;
            Exhaust = deloreanMods.Exhaust;
            Hoodbox = deloreanMods.Hoodbox;
            Hook = deloreanMods.Hook;
            Plate = deloreanMods.Plate;            
        }

        public void ApplyTo(DeloreanTimeMachine timeMachine)
        {
            DeloreanMods deloreanMods = timeMachine.Mods;

            deloreanMods.DeloreanType = DeloreanType;
            deloreanMods.SuspensionsType = SuspensionsType;
            deloreanMods.Wheel = Wheel;
            deloreanMods.Exterior = Exterior;
            deloreanMods.Interior = Interior;
            deloreanMods.OffCoils = OffCoils;
            deloreanMods.GlowingEmitter = GlowingEmitter;
            deloreanMods.GlowingReactor = GlowingReactor;
            deloreanMods.DamagedBumper = DamagedBumper;
            deloreanMods.SteeringWheelsButtons = SteeringWheelsButtons;
            deloreanMods.HoverUnderbody = HoverUnderbody;
            deloreanMods.Vents = Vents;
            deloreanMods.Seats = Seats;
            deloreanMods.Reactor = Reactor;
            deloreanMods.Exhaust = Exhaust;
            deloreanMods.Hoodbox = Hoodbox;
            deloreanMods.Hook = Hook;
            deloreanMods.Plate = Plate;            
        }

        public static string PresetsPath = "./scripts/BackToTheFutureV/presets";

        public static bool PresetExists(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            return File.Exists($"{PresetsPath}/{name}");
        }

        public static List<string> ListPresets()
        {
            if (!Directory.Exists(PresetsPath))
                Directory.CreateDirectory(PresetsPath);

            return new DirectoryInfo(PresetsPath).GetFiles("*.dmc12").Select(x => x.Name.Replace(".dmc12", "")).ToList();
        }

        public static void DeleteSave(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            File.Delete($"{PresetsPath}/{name}");
        }

        public static void RenameSave(string name, string newName)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            if (!newName.ToLower().EndsWith(".dmc12"))
                newName = newName + ".dmc12";

            File.Move($"{PresetsPath}/{name}", $"{PresetsPath}/{newName}");
        }

        private static string RemoveIllegalFileNameChars(string input, string replacement = "")
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(input, replacement);
        }

        public void Save(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            name = RemoveIllegalFileNameChars(name);

            IFormatter formatter = new BinaryFormatter();

            if (!Directory.Exists(PresetsPath))
                Directory.CreateDirectory(PresetsPath);

            Stream stream = new FileStream($"{PresetsPath}/{name}", FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, this);
            stream.Close();
        }

        public static DeloreanModsCopy Load(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream($"{PresetsPath}/{name}", FileMode.Open, FileAccess.Read);

            DeloreanModsCopy deloreanModsCopy = (DeloreanModsCopy)formatter.Deserialize(stream);

            stream.Close();

            return deloreanModsCopy;
        }
    }
}
