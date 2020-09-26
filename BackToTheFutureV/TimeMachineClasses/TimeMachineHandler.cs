using BackToTheFutureV.TimeMachineClasses.Handlers;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses
{
    public class TimeMachineHandler
    {
        public static TimeMachine LastTimeMachine { get; private set; }
        public static TimeMachine ClosestTimeMachine { get; private set; }
        public static TimeMachine CurrentTimeMachine { get; private set; }
        public static float SquareDistToClosestTimeMachine { get; private set; } = -1;
        
        public static List<TimeMachine> TimeMachines = new List<TimeMachine>();
        private static List<TimeMachine> _timeMachinesToAdd = new List<TimeMachine>();
        private static Dictionary<TimeMachine, bool> _timeMachinesToRemove = new Dictionary<TimeMachine, bool>();
        private static Dictionary<TimeMachine, bool> _timeMachinesToRemoveWaitSounds = new Dictionary<TimeMachine, bool>();

        public static int TimeMachineCount => TimeMachines.Count;
        private static bool _savedEmpty;

        public static void SaveAllTimeMachines()
        {
            if (TimeMachineCount == 0 && _savedEmpty)
                return;

            TimeMachineCloneManager.Save(TimeMachines);

            _savedEmpty = TimeMachineCount == 0;
        }

        public static void LoadAllTimeMachines()
        {
            try
            {
                TimeMachineCloneManager.Load()?.SpawnAll();
            }
            catch
            {
                TimeMachineCloneManager.Delete();
            }
        }

        public static TimeMachine CreateTimeMachine(DMC12 dmc12, WormholeType wormholeType)
        {
            if (dmc12 == null)
                return null;

            if (dmc12.Vehicle == null)
                return null;

            TimeMachine timeMachine = GetTimeMachineFromVehicle(dmc12.Vehicle);

            if (timeMachine != null)
                return timeMachine;

            return new TimeMachine(dmc12, wormholeType);
        }

        public static TimeMachine CreateTimeMachine(Vehicle vehicle, WormholeType wormholeType)
        {
            if (vehicle == null)
                return null;

            if (vehicle.Model.IsTrain)
                return null;

            TimeMachine timeMachine = GetTimeMachineFromVehicle(vehicle);

            if (timeMachine != null)
                return timeMachine;

            return new TimeMachine(vehicle, wormholeType);
        }

        public static TimeMachine CreateTimeMachine(Vector3 position, float heading = 0, WormholeType wormholeType = WormholeType.BTTF1)
        {
            return new TimeMachine(DMC12Handler.CreateDMC12(position, heading), wormholeType);
        }

        public static void AddTimeMachine(TimeMachine vehicle)
        {
            if (_timeMachinesToAdd.Contains(vehicle) || TimeMachines.Contains(vehicle))
                return;

            _timeMachinesToAdd.Add(vehicle);
        }

        public static void RemoveTimeMachine(TimeMachine vehicle, bool deleteVeh = true, bool waitSoundsComplete = false)
        {
            if (_timeMachinesToRemove.ContainsKey(vehicle))
                return;

            if (waitSoundsComplete)
            {
                if (!_timeMachinesToRemoveWaitSounds.ContainsKey(vehicle))
                    _timeMachinesToRemoveWaitSounds.Add(vehicle, deleteVeh);
            }
            else
                _timeMachinesToRemove.Add(vehicle, deleteVeh);
        }

        public static void RemoveInstantlyTimeMachine(TimeMachine vehicle, bool deleteVeh = true)
        {
            if (_timeMachinesToRemoveWaitSounds.ContainsKey(vehicle))
                _timeMachinesToRemoveWaitSounds.Remove(vehicle);
            
            vehicle?.Dispose(deleteVeh);

            TimeMachines.Remove(vehicle);
        }

        public static void RemoveAllTimeMachines(bool noCurrent = false)
        {
            foreach (var veh in TimeMachines.ToList())
            {
                if (noCurrent && veh.Vehicle == Main.PlayerVehicle)
                    continue;

                RemoveTimeMachine(veh);
            }
        }

        public static TimeMachine SpawnWithReentry(WormholeType wormholeType = WormholeType.BTTF1, string presetName = default)
        {
            Ped ped = Main.PlayerPed;

            if (RCManager.IsRemoteOn)
            {
                ped = RCManager.RemoteControlling.OriginalPed;
                RCManager.StopRemoteControl(true);
            }

            TimeMachineClone timeMachineClone = null;

            if (presetName != default)
                timeMachineClone = TimeMachineClone.Load(presetName);

            TimeMachine timeMachine;

            if (timeMachineClone != null)
            {
                timeMachineClone.Vehicle.Position = ped.GetOffsetPosition(new Vector3(0, 25, 0));
                timeMachineClone.Vehicle.Heading = ped.Heading + 180;

                timeMachine = timeMachineClone.Spawn(true, true);

                timeMachine.Properties.TorqueMultiplier = 1;
            }                
            else
            {
                timeMachine = CreateTimeMachine(Main.PlayerPed.GetOffsetPosition(new Vector3(0, 25, 0)), Main.PlayerPed.Heading + 180, wormholeType);

                Utils.HideVehicle(timeMachine.Vehicle, true);

                timeMachine.Properties.DestinationTime = Main.CurrentTime;

                timeMachine.Properties.AreTimeCircuitsOn = true;
                timeMachine.Events.SetTimeCircuits?.Invoke(true);

                timeMachine.Events.OnReenter?.Invoke();
            }

            return timeMachine;
        }

        public static TimeMachine Spawn(WormholeType wormholeType, bool warpInPlayer = false, string presetName = default)
        {
            Ped ped = Main.PlayerPed;

            if (RCManager.IsRemoteOn)
            {
                ped = RCManager.RemoteControlling.OriginalPed;
                RCManager.StopRemoteControl(true);
            }

            Vector3 spawnPos;

            if (Main.PlayerVehicle != null)
                spawnPos = Main.PlayerVehicle.Position.Around(5f);
            else
                spawnPos = ped.Position;

            TimeMachineClone timeMachineClone = null;

            if (presetName != default)
                timeMachineClone = TimeMachineClone.Load(presetName);

            TimeMachine timeMachine;

            if (timeMachineClone != null)
            {
                timeMachineClone.Vehicle.Position = spawnPos;
                timeMachineClone.Vehicle.Heading = ped.Heading;

                timeMachine = timeMachineClone.Spawn(true, false);

                timeMachine.Properties.TorqueMultiplier = 1;
            }
            else
            {
                timeMachine = CreateTimeMachine(spawnPos, ped.Heading, wormholeType);
            }

            if (warpInPlayer)
            {
                if (RCManager.IsRemoteOn)
                    RCManager.StopRemoteControl(true);

                Main.PlayerPed.Task.WarpIntoVehicle(timeMachine, VehicleSeat.Driver);
            }
                
            return timeMachine;
        }

        public static void KeyDown(Keys e)
        {
            TimeMachines.ForEach(x => x.KeyDown(e));
        }

        public static void Process()
        {
            if (_timeMachinesToRemoveWaitSounds.Count > 0)
            {
                foreach (var timeMachine in _timeMachinesToRemoveWaitSounds)
                    if (!timeMachine.Key.Sounds.AudioEngine.IsAnyInstancePlaying)
                        RemoveTimeMachine(timeMachine.Key, timeMachine.Value);
            }

            if (_timeMachinesToRemove.Count > 0)
            {
                foreach (var timeMachine in _timeMachinesToRemove)
                    RemoveInstantlyTimeMachine(timeMachine.Key, timeMachine.Value);

                _timeMachinesToRemove.Clear();
            }

            if (_timeMachinesToAdd.Count > 0)
            {
                TimeMachines.AddRange(_timeMachinesToAdd);
                _timeMachinesToAdd.Clear();
            }

            UpdateClosestTimeMachine();

            foreach (var timeMachine in TimeMachines)
                timeMachine.Process();
        }

        public static void Abort()
        {
            TimeMachines.ForEach(x => x.Dispose(false));
        }

        public static TimeMachine GetTimeMachineFromIndex(int index)
        {
            if (index > TimeMachineCount - 1)
                return default;

            return TimeMachines[index];
        }

        public static TimeMachine GetTimeMachineFromVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
                return null;

            foreach (var timeMachine in TimeMachines)
            {
                if (timeMachine.Vehicle == vehicle)
                    return timeMachine;
            }

            foreach (var timeMachine in _timeMachinesToAdd)
            {
                if (timeMachine.Vehicle == vehicle)
                    return timeMachine;
            }

            return null;
        }

        public static bool IsVehicleATimeMachine(Vehicle vehicle)
        {
            foreach (var timeMachine in TimeMachines)
                if (timeMachine.Vehicle == vehicle)
                    return true;

            foreach (var timeMachine in _timeMachinesToAdd)
                if (timeMachine.Vehicle == vehicle)
                    return true;

            return false;
        }

        public static void ExistenceCheck(DateTime time)
        {
            TimeMachines.ForEach(x =>
            {
                if (x.LastDisplacementClone.Properties.DestinationTime > time && Main.PlayerVehicle != x.Vehicle)
                    RemoveTimeMachine(x);
            });
        }

        public static void UpdateClosestTimeMachine()
        {
            if (Main.PlayerVehicle != null && CurrentTimeMachine != null && CurrentTimeMachine.Vehicle == Main.PlayerVehicle)
            {
                if (ClosestTimeMachine != CurrentTimeMachine)
                {
                    ClosestTimeMachine = CurrentTimeMachine;
                    SquareDistToClosestTimeMachine = 0;
                }

                if (!CurrentTimeMachine.Properties.IsGivenScaleformPriority)
                {
                    CurrentTimeMachine.Properties.IsGivenScaleformPriority = true;
                    CurrentTimeMachine.Events.OnScaleformPriority?.Invoke();
                }

                return;
            }
                
            CurrentTimeMachine = null;

            if (TimeMachines.Count == 0 && SquareDistToClosestTimeMachine != -1)
            {
                ClosestTimeMachine = null;
                SquareDistToClosestTimeMachine = -1;
            }
                            
            foreach (var timeMachine in TimeMachines)
            {                
                float dist = timeMachine.Vehicle.Position.DistanceToSquared(Main.PlayerPed.Position);

                if (ClosestTimeMachine == timeMachine)
                    SquareDistToClosestTimeMachine = dist;

                if (ClosestTimeMachine != timeMachine && (SquareDistToClosestTimeMachine == -1 || dist < SquareDistToClosestTimeMachine))
                {
                    if (ClosestTimeMachine != null)
                        ClosestTimeMachine.Properties.IsGivenScaleformPriority = false;

                    ClosestTimeMachine = timeMachine;

                    ClosestTimeMachine.Properties.IsGivenScaleformPriority = true;
                    ClosestTimeMachine.Events.OnScaleformPriority?.Invoke();

                    SquareDistToClosestTimeMachine = dist;               
                }
            }

            if (RCManager.IsRemoteOn)
            {
                CurrentTimeMachine = RCManager.RemoteControlling;

                return;
            }
                
            if (ClosestTimeMachine != null && Main.PlayerVehicle == ClosestTimeMachine.Vehicle)
                CurrentTimeMachine = ClosestTimeMachine;
        }
    }
}
