using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal static class TimeMachineHandler
    {
        public static TimeMachine ClosestTimeMachine { get; private set; }
        public static TimeMachine CurrentTimeMachine { get; private set; }
        public static float SquareDistToClosestTimeMachine { get; private set; } = -1;
        public static List<TimeMachine> StoryTimeMachines { get; } = new List<TimeMachine>();
        private static List<TimeMachine> AllTimeMachines { get; } = new List<TimeMachine>();
        public static List<TimeMachine> TimeMachines => AllTimeMachines.Except(StoryTimeMachines).ToList();

        private static readonly List<TimeMachine> _timeMachinesToAdd = new List<TimeMachine>();
        private static readonly Dictionary<TimeMachine, bool> _timeMachinesToRemove = new Dictionary<TimeMachine, bool>();
        private static readonly Dictionary<TimeMachine, bool> _timeMachinesToRemoveWaitSounds = new Dictionary<TimeMachine, bool>();

        public static int TimeMachineCount => TimeMachines.Count();

        private static Vehicle _checkVehicle;

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
            if (!vehicle.IsFunctioning())
            {
                return null;
            }

            if (vehicle.Model.IsTrain)
            {
                return null;
            }

            /*if (Main.DeluxoProtoSupport && vehicle.Model == ModelHandler.DeluxoModel)
            {
                Vector3 spawnPos = vehicle.Position;
                float spawnHeading = vehicle.Heading;
                FusionUtils.PlayerPed.IsVisible = false;
                vehicle.DeleteCompletely();
                vehicle = World.CreateVehicle("dproto", spawnPos, spawnHeading);
                FusionUtils.PlayerPed.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                vehicle.Mods.PrimaryColor = (VehicleColor)117;
                vehicle.Mods.SecondaryColor = (VehicleColor)13;
                vehicle.Mods.TrimColor = (VehicleColor)22;
                vehicle.Mods.DashboardColor = (VehicleColor)12;
                vehicle.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;
                vehicle.Mods.LicensePlate = " NOTIME ";
                vehicle.ToggleExtra(1, false);
                vehicle.ToggleExtra(2, false);
                vehicle.ToggleExtra(3, false);
                FusionUtils.PlayerPed.IsVisible = true;
            }
            else if (Main.DeluxoProtoSupport && vehicle.Model == "dproto")
            {
                vehicle.Mods.PrimaryColor = (VehicleColor)117;
                vehicle.Mods.SecondaryColor = (VehicleColor)13;
                vehicle.Mods.TrimColor = (VehicleColor)22;
                vehicle.Mods.DashboardColor = (VehicleColor)12;
                vehicle.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;
                vehicle.Mods.LicensePlate = " NOTIME ";
                vehicle.ToggleExtra(1, false);
                vehicle.ToggleExtra(2, false);
                vehicle.ToggleExtra(3, false);
            }*/

            TimeMachine timeMachine = GetTimeMachineFromVehicle(vehicle);

            if (timeMachine.NotNullAndExists())
            {
                return timeMachine;
            }

            return new TimeMachine(vehicle, wormholeType);
        }

        public static void AddTimeMachine(TimeMachine vehicle)
        {
            if (_timeMachinesToAdd.Contains(vehicle) || AllTimeMachines.Contains(vehicle))
            {
                return;
            }

            _timeMachinesToAdd.Add(vehicle);
        }

        public static void RemoveTimeMachine(TimeMachine vehicle, bool deleteVeh = true, bool waitSoundsComplete = false)
        {
            if (_timeMachinesToRemove.ContainsKey(vehicle))
            {
                return;
            }

            if (waitSoundsComplete)
            {
                if (!_timeMachinesToRemoveWaitSounds.ContainsKey(vehicle))
                {
                    _timeMachinesToRemoveWaitSounds.Add(vehicle, deleteVeh);
                }
            }
            else
            {
                _timeMachinesToRemove.Add(vehicle, deleteVeh);
                Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, true);
                PlayerSwitch.Disable = false;
            }
        }

        public static void RemoveInstantlyTimeMachine(TimeMachine vehicle, bool deleteVeh = true)
        {
            if (_timeMachinesToRemoveWaitSounds.ContainsKey(vehicle))
            {
                _timeMachinesToRemoveWaitSounds.Remove(vehicle);
            }

            vehicle?.Dispose(deleteVeh);

            AllTimeMachines.Remove(vehicle);
        }

        public static void RemoveAllTimeMachines(bool noCurrent = false)
        {
            foreach (TimeMachine veh in AllTimeMachines.ToList())
            {
                if (noCurrent && veh.Vehicle == FusionUtils.PlayerVehicle)
                {
                    continue;
                }

                RemoveTimeMachine(veh);
                Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, true);
                PlayerSwitch.Disable = false;
            }
        }

        public static TimeMachine Create(Vehicle vehicle, SpawnFlags spawnFlags = SpawnFlags.Default, WormholeType wormholeType = WormholeType.BTTF1)
        {
            return Create(spawnFlags, wormholeType, default, default, default, vehicle);
        }

        public static TimeMachine Create(TimeMachineClone timeMachineClone, SpawnFlags spawnFlags = SpawnFlags.Default)
        {
            return Create(spawnFlags, WormholeType.BTTF1, default, default, timeMachineClone);
        }

        public static TimeMachine Create(SpawnFlags spawnFlags = SpawnFlags.Default, WormholeType wormholeType = WormholeType.BTTF1, Vector3 position = default, float heading = default, TimeMachineClone timeMachineClone = default, Vehicle vehicle = default)
        {
            TimeMachine timeMachine = null;

            if (vehicle != default)
            {
                timeMachine = GetTimeMachineFromVehicle(vehicle);

                if (timeMachine.NotNullAndExists())
                {
                    return timeMachine;
                }
            }

            Ped ped = FusionUtils.PlayerPed;

            if (RemoteTimeMachineHandler.IsRemoteOn)
            {
                ped = RemoteTimeMachineHandler.RemoteControlling.OriginalPed;
            }

            Vehicle veh = null;
            Vector3 spawnPos;

            if (FusionUtils.PlayerVehicle != null && !RemoteTimeMachineHandler.IsRemoteOn)
            {
                spawnPos = ped.Position.Around(5f);
            }
            else
            {
                spawnPos = ped.Position;
            }

            if (spawnFlags.HasFlag(SpawnFlags.NoPosition))
            {
                spawnPos = position;
            }
            else
            {
                heading = ped.Heading;
            }

            if (spawnFlags.HasFlag(SpawnFlags.ForceReentry))
            {
                // First check to see if player is underground and spawn at same height if so
                // If not we check an offset in the sky then set to ground height to get proper ground to spawn on
                if (ped.Position.Z < ped.GetOffsetPosition(new Vector3(0, 0, 1000)).SetToGroundHeight().Z + 1)
                    spawnPos = ped.GetOffsetPosition(new Vector3(0, 25, 0));
                else
                    spawnPos = ped.GetOffsetPosition(new Vector3(0, 25, 1000)).SetToGroundHeight();

                heading = ped.Heading + 180;
            }

            if (spawnFlags.HasFlag(SpawnFlags.CheckExists) && timeMachineClone != default)
            {
                veh = World.GetClosestVehicle(timeMachineClone.Vehicle.Position, 5f, timeMachineClone.Vehicle.Model);
            }

            if (vehicle != default)
            {
                veh = vehicle;
            }

            if (timeMachineClone != default && timeMachineClone.Properties.TimeTravelType == TimeTravelType.RC)
            {
                spawnFlags |= SpawnFlags.NoDriver;
            }

            if (timeMachineClone != default && timeMachineClone.Properties.HasBeenStruckByLightning)
            {
                spawnFlags |= SpawnFlags.NoOccupants;
            }

            if (timeMachineClone != default && timeMachineClone.Vehicle.Model == ModelHandler.DMC12)
            {
                spawnFlags |= SpawnFlags.NoMods;
            }

            if (veh == null)
            {
                if (timeMachineClone != default)
                {
                    veh = timeMachineClone.Vehicle.Spawn(spawnFlags, spawnPos, heading);
                }
                else
                {
                    timeMachine = new TimeMachine(DMC12Handler.CreateDMC12(spawnPos, heading), wormholeType);
                }
            }
            else if (veh.IsTimeMachine())
            {
                timeMachine = GetTimeMachineFromVehicle(veh);
            }

            if (timeMachine == null)
            {
                if (timeMachineClone != default)
                {
                    timeMachine = TransformIntoTimeMachine(veh, timeMachineClone.Mods.WormholeType);
                    timeMachineClone.ApplyTo(timeMachine, spawnFlags);
                }
                else
                {
                    timeMachine = TransformIntoTimeMachine(veh, wormholeType);
                }
            }

            if (spawnFlags.HasFlag(SpawnFlags.WarpPlayer) && !spawnFlags.HasFlag(SpawnFlags.ForceReentry))
            {
                if (RemoteTimeMachineHandler.IsRemoteOn)
                {
                    RemoteTimeMachineHandler.StopRemoteControl(true);
                }

                FusionUtils.PlayerPed.SetIntoVehicle(timeMachine, VehicleSeat.Driver);
            }

            if (spawnFlags.HasFlag(SpawnFlags.Broken))
            {
                timeMachine.Break();
            }

            if (spawnFlags.HasFlag(SpawnFlags.ForceReentry))
            {
                timeMachine.Vehicle.SetVisible(false);

                if (spawnFlags.HasFlag(SpawnFlags.New))
                {
                    if (!ModSettings.WaybackSystem)
                    {
                        timeMachine.Properties.DestinationTime = FusionUtils.CurrentTime.AddSeconds(-FusionUtils.CurrentTime.Second);
                    }
                    else
                    {
                        timeMachine.Properties.DestinationTime = FusionUtils.CurrentTime;
                    }

                    if (timeMachine.Mods.WormholeType == WormholeType.BTTF2)
                    {
                        timeMachine.Properties.PreviousTime = new DateTime(2015, 10, 22, 19, 45, 0);
                    }

                    if (timeMachine.Mods.WormholeType == WormholeType.BTTF3)
                    {
                        timeMachine.Properties.AreHoodboxCircuitsReady = true;
                        timeMachine.Properties.PreviousTime = new DateTime(1955, 11, 16, 10, 20, 0);
                    }
                }

                timeMachine.Events.SetTimeCircuits?.Invoke(true);

                timeMachine.Events.OnReenterStarted?.Invoke();
            }

            if (timeMachine.Vehicle.IsSeatFree(VehicleSeat.Driver) && timeMachine.Properties.IsFlying)
            {
                timeMachine.Events.SetFlyMode.Invoke(false);
            }

            return timeMachine;
        }

        public static void KeyDown(KeyEventArgs e)
        {
            if (CurrentTimeMachine.IsFunctioning() && CurrentTimeMachine == FusionUtils.PlayerVehicle)
            {
                CurrentTimeMachine.KeyDown(e);
            }
        }

        public static void Tick()
        {
            if (_timeMachinesToRemoveWaitSounds.Count > 0)
            {
                foreach (KeyValuePair<TimeMachine, bool> timeMachine in _timeMachinesToRemoveWaitSounds)
                {
                    if (!timeMachine.Key.Sounds.TimeTravelCutscene.IsAnyInstancePlaying)
                    {
                        RemoveTimeMachine(timeMachine.Key, timeMachine.Value);
                    }
                }
            }

            if (_timeMachinesToRemove.Count > 0)
            {
                foreach (KeyValuePair<TimeMachine, bool> timeMachine in _timeMachinesToRemove)
                {
                    RemoveInstantlyTimeMachine(timeMachine.Key, timeMachine.Value);
                }

                _timeMachinesToRemove.Clear();
            }

            if (_timeMachinesToAdd.Count > 0)
            {
                AllTimeMachines.AddRange(_timeMachinesToAdd);
                _timeMachinesToAdd.Clear();
            }

            if (_checkVehicle != FusionUtils.PlayerVehicle)
            {
                _checkVehicle = FusionUtils.PlayerVehicle;

                if (_checkVehicle.IsFunctioning() && !_checkVehicle.IsTimeMachine() && DecoratorsHandler.CheckVehicle(_checkVehicle))
                {
                    Create(_checkVehicle);
                }
            }

            UpdateClosestTimeMachine();

            foreach (TimeMachine timeMachine in TimeMachines)
            {
                timeMachine.Tick();
            }
        }

        public static void Abort()
        {
            AllTimeMachines.ForEach(x => x.Dispose(false));
        }

        public static TimeMachine GetTimeMachineFromVehicle(Vehicle vehicle)
        {
            if (!vehicle.IsFunctioning())
            {
                return null;
            }

            TimeMachine timeMachine = AllTimeMachines.SingleOrDefault(x => x.Vehicle == vehicle);

            if (timeMachine == default)
            {
                timeMachine = _timeMachinesToAdd.SingleOrDefault(x => x.Vehicle == vehicle);

                if (timeMachine == default)
                {
                    return null;
                }
            }

            return timeMachine;
        }

        public static TimeMachine GetTimeMachineFromGUID(Guid guid)
        {
            TimeMachine timeMachine = AllTimeMachines.SingleOrDefault(x => x.Properties.GUID == guid);

            if (timeMachine == default)
            {
                timeMachine = _timeMachinesToAdd.SingleOrDefault(x => x.Properties.GUID == guid);

                if (timeMachine == default)
                {
                    return null;
                }
            }

            return timeMachine;
        }

        public static bool IsVehicleATimeMachine(Vehicle vehicle)
        {
            foreach (TimeMachine timeMachine in AllTimeMachines)
            {
                if (timeMachine.Vehicle == vehicle)
                {
                    return true;
                }
            }

            foreach (TimeMachine timeMachine in _timeMachinesToAdd)
            {
                if (timeMachine.Vehicle == vehicle)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ExistenceCheck(DateTime time)
        {
            TimeMachines.ForEach(x =>
            {
                if (x.LastDisplacementClone.Properties.DestinationTime > time && FusionUtils.PlayerVehicle != x.Vehicle)
                {
                    RemoveTimeMachine(x);
                }
            });
        }

        public static void UpdateClosestTimeMachine()
        {
            if (AllTimeMachines.Count == 0 && SquareDistToClosestTimeMachine != -1)
            {
                CurrentTimeMachine = null;
                ClosestTimeMachine = null;
                SquareDistToClosestTimeMachine = -1;
            }

            if (AllTimeMachines.Count == 0)
            {
                return;
            }

            if (CurrentTimeMachine != null && !FusionUtils.PlayerVehicle.IsFunctioning())
            {
                Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, true);
                PlayerSwitch.Disable = false;
                CurrentTimeMachine = null;
            }

            foreach (TimeMachine timeMachine in TimeMachines)
            {
                if (!timeMachine.IsFunctioning())
                {
                    continue;
                }

                if (timeMachine == FusionUtils.PlayerVehicle)
                {
                    CurrentTimeMachine = timeMachine;

                    if (ClosestTimeMachine != CurrentTimeMachine)
                    {
                        ClosestTimeMachine = CurrentTimeMachine;
                        SquareDistToClosestTimeMachine = 0;
                    }

                    if (CurrentTimeMachine.Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                    {
                        return;
                    }

                    if (!CurrentTimeMachine.Properties.HUDProperties.IsHUDVisible)
                    {
                        CurrentTimeMachine.Properties.HUDProperties.IsHUDVisible = true;
                    }

                    return;
                }

                float dist = FusionUtils.PlayerPed.DistanceToSquared2D(timeMachine);

                if (ClosestTimeMachine == timeMachine)
                {
                    SquareDistToClosestTimeMachine = dist;
                }

                if (ClosestTimeMachine != timeMachine && (SquareDistToClosestTimeMachine == -1 || dist < SquareDistToClosestTimeMachine))
                {
                    ClosestTimeMachine = timeMachine;
                    SquareDistToClosestTimeMachine = dist;
                }
            }
        }
    }
}
