using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses
{
    public class TimeMachineHandler
    {        
        public static TimeMachine ClosestTimeMachine { get; private set; }
        public static TimeMachine CurrentTimeMachine { get; private set; }
        public static float SquareDistToClosestTimeMachine { get; private set; } = -1;
        public static List<TimeMachine> StoryTimeMachines { get; private set; } = new List<TimeMachine>();
        public static List<TimeMachine> TimeMachines { get; private set; } = new List<TimeMachine>();
        public static List<TimeMachine> TimeMachinesNoStory => TimeMachines.Except(StoryTimeMachines).ToList();

        private static List<TimeMachine> _timeMachinesToAdd = new List<TimeMachine>();
        private static Dictionary<TimeMachine, bool> _timeMachinesToRemove = new Dictionary<TimeMachine, bool>();
        private static Dictionary<TimeMachine, bool> _timeMachinesToRemoveWaitSounds = new Dictionary<TimeMachine, bool>();

        public static int TimeMachineCount => TimeMachinesNoStory.Count();
        public static int StoryTimeMachineCount => StoryTimeMachines.Count;
        private static bool _savedEmpty;

        public static void SaveAllTimeMachines()
        {
            if (TimeMachineCount == 0 && _savedEmpty)
                return;

            TimeMachineCloneManager.Save(TimeMachinesNoStory);

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

        public static void AddStory(TimeMachine timeMachine)
        {
            if (!StoryTimeMachines.Contains(timeMachine))
            {
                timeMachine.Properties.Story = true;
                StoryTimeMachines.Add(timeMachine);
            }                
        }

        public static void RemoveStory(TimeMachine timeMachine)
        {
            if (StoryTimeMachines.Contains(timeMachine))
            {
                timeMachine.Properties.Story = false;
                StoryTimeMachines.Remove(timeMachine);
            }                
        }

        private static TimeMachine TransformIntoTimeMachine(Vehicle vehicle, WormholeType wormholeType)
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
                if (noCurrent && veh.Vehicle == Utils.PlayerVehicle)
                    continue;

                RemoveTimeMachine(veh);
            }
        }

        public static TimeMachine Create(Vehicle vehicle, SpawnFlags spawnFlags = SpawnFlags.Default, WormholeType wormholeType = WormholeType.BTTF1)
        {
            return Create(spawnFlags, wormholeType, default, default, default, default, vehicle);
        }

        public static TimeMachine Create(TimeMachineClone timeMachineClone, SpawnFlags spawnFlags = SpawnFlags.Default)
        {
            return Create(spawnFlags, WormholeType.BTTF1, default, default, timeMachineClone);
        }

        public static TimeMachine Create(string presetName, SpawnFlags spawnFlags = SpawnFlags.Default)
        {
            return Create(spawnFlags, WormholeType.BTTF1, default, default, default, presetName);
        }

        public static TimeMachine Create(SpawnFlags spawnFlags = SpawnFlags.Default, WormholeType wormholeType = WormholeType.BTTF1, Vector3 position = default, float heading = default, TimeMachineClone timeMachineClone = default, string presetName = default, Vehicle vehicle = default)
        {
            if (vehicle != default)
            {
                if (vehicle.IsTimeMachine())
                    return GetTimeMachineFromVehicle(vehicle);
            }

            Ped ped = Utils.PlayerPed;

            if (RCManager.IsRemoteOn)
                ped = RCManager.RemoteControlling.OriginalPed;

            Vehicle veh = null;
            Vector3 spawnPos;
            TimeMachine timeMachine = null;

            if (Utils.PlayerVehicle != null && !RCManager.IsRemoteOn)
                spawnPos = ped.Position.Around(5f);
            else
                spawnPos = ped.Position;

            if (spawnFlags.HasFlag(SpawnFlags.ForcePosition) && presetName == default)
                spawnPos = position;
            else
                heading = ped.Heading;
           
            if (spawnFlags.HasFlag(SpawnFlags.ForceReentry))
            {
                spawnPos = ped.GetOffsetPosition(new Vector3(0, 25, 0));
                heading = ped.Heading + 180;
            }

            if (presetName != default)
                timeMachineClone = TimeMachineClone.Load(presetName);

            if (spawnFlags.HasFlag(SpawnFlags.CheckExists) && timeMachineClone != default && vehicle != default)
                veh = World.GetClosestVehicle(timeMachineClone.Vehicle.Position, 1.0f, timeMachineClone.Vehicle.Model);

            if (vehicle != default)
                veh = vehicle;
            
            if (veh == null)            
            {
                if (timeMachineClone != default)
                    veh = timeMachineClone.Vehicle.Spawn(spawnFlags, spawnPos, heading);
                else
                    timeMachine = new TimeMachine(DMC12Handler.CreateDMC12(spawnPos, heading), wormholeType);
            } else if (veh.IsTimeMachine())
                timeMachine = GetTimeMachineFromVehicle(veh);

            if (timeMachine == null)
            {                
                if (timeMachineClone != default)
                {
                    timeMachine = TransformIntoTimeMachine(veh, timeMachineClone.Mods.WormholeType);
                    timeMachineClone.ApplyTo(timeMachine, spawnFlags);
                }                    
                else
                    timeMachine = TransformIntoTimeMachine(veh, wormholeType);
            }

            if (spawnFlags.HasFlag(SpawnFlags.WarpPlayer) && !spawnFlags.HasFlag(SpawnFlags.ForceReentry))
            {
                if (RCManager.IsRemoteOn)
                    RCManager.StopRemoteControl(true);

                Utils.PlayerPed.SetIntoVehicle(timeMachine, VehicleSeat.Driver);
            }

            if (spawnFlags.HasFlag(SpawnFlags.Broken))
                timeMachine.Break();

            if (spawnFlags.HasFlag(SpawnFlags.ForceReentry))
            {
                timeMachine.Vehicle.SetVisible(false);

                timeMachine.Properties.DestinationTime = Utils.CurrentTime;

                timeMachine.Properties.AreTimeCircuitsOn = true;
                timeMachine.Events.SetTimeCircuits?.Invoke(true);

                timeMachine.Events.OnReenter?.Invoke();
            }

            if (spawnFlags.HasFlag(SpawnFlags.New) && WaybackMachineHandler.Enabled)
                timeMachine.CreateCloneSpawn = true;

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
                    if (!timeMachine.Key.Sounds.TimeTravelCutscene.IsAnyInstancePlaying)
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

        public static bool Exists(TimeMachine timeMachine)
        {
            return TimeMachines.Contains(timeMachine) | _timeMachinesToAdd.Contains(timeMachine);
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
            TimeMachinesNoStory.ForEach(x =>
            {
                if (x.LastDisplacementClone.Properties.DestinationTime > time && Utils.PlayerVehicle != x.Vehicle)
                    RemoveTimeMachine(x);
            });
        }

        public static void UpdateClosestTimeMachine()
        {
            if (Utils.PlayerVehicle.IsFunctioning() && CurrentTimeMachine.IsFunctioning() && CurrentTimeMachine.Vehicle == Utils.PlayerVehicle)
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

                if (CurrentTimeMachine.Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                    return;

                if (!ExternalHUD.IsHUDVisible)
                    ExternalHUD.IsHUDVisible = true;

                if (!RemoteHUD.IsHUDVisible)
                    RemoteHUD.IsHUDVisible = true;

                if (CurrentTimeMachine.Mods.HoverUnderbody == ModState.On)
                    Function.Call(Hash.SET_PLAYER_CAN_DO_DRIVE_BY, Game.Player, false);

                return;
            }

            if (CurrentTimeMachine.IsFunctioning() && !Utils.PlayerVehicle.IsFunctioning())
            {
                if (ExternalHUD.IsHUDVisible)
                    ExternalHUD.SetOff();

                if (RemoteHUD.IsHUDVisible)
                    RemoteHUD.SetOff();

                Function.Call(Hash.SET_PLAYER_CAN_DO_DRIVE_BY, Game.Player, true);
            }

            CurrentTimeMachine = null;

            if (TimeMachines.Count == 0 && SquareDistToClosestTimeMachine != -1)
            {
                ClosestTimeMachine = null;
                SquareDistToClosestTimeMachine = -1;
            }
                            
            foreach (var timeMachine in TimeMachines)
            {                
                float dist = timeMachine.Vehicle.Position.DistanceToSquared(Utils.PlayerPed.Position);

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
                
            if (ClosestTimeMachine.IsFunctioning() && Utils.PlayerVehicle == ClosestTimeMachine.Vehicle)
            {
                CurrentTimeMachine = ClosestTimeMachine;

                if (CurrentTimeMachine.Properties.FullDamaged)
                    GTA.UI.Screen.ShowHelpTextThisFrame(string.Format(Game.GetLocalizedString("BTTFV_Restore_Damanged_Delorean"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_Restore"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu")));
            }                
        }
    }
}
