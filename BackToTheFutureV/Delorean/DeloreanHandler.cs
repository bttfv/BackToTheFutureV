using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace BackToTheFutureV.Delorean
{
    public class DeloreanHandler
    {
        // DMC12s
        // Can be TimeMachines
        public static DMC12 LastDMC12Vehicle { get; private set; }
        public static DMC12 ClosestDMC12 { get; private set; }
        public static DMC12 CurrentDMC12 { get; private set; }
        public static float SquareDistToClosestDMC12 { get; private set; }

        // Time Machines
        // Cannot be DeloreanVehicles.
        public static DeloreanTimeMachine LastTimeMachine { get; private set; }
        public static DeloreanTimeMachine ClosestTimeMachine { get; private set; }
        public static DeloreanTimeMachine FurthestTimeMachine { get; private set; }
        public static DeloreanTimeMachine CurrentTimeMachine { get; private set; }
        public static float SquareDistToClosestTimeMachine { get; private set; }

        public static int TimeMachineCount { get => _timeMachines.Count; }

        public const int MAX_TIME_MACHINES = 50;

        private static int _getAllDelay;
        private static bool _savedEmpty;
        private static int _getClosestDelay;

        private static List<DMC12> _deloreans = new List<DMC12>();
        private static List<DeloreanTimeMachine> _timeMachines = new List<DeloreanTimeMachine>();
        private static Dictionary<DMC12, bool> _delosToRemove = new Dictionary<DMC12, bool>();
        private static Dictionary<DMC12, bool> _delosToRemoveSounds = new Dictionary<DMC12, bool>();
        private static List<DeloreanCopy> _delosToAdd = new List<DeloreanCopy>();

        public static void SaveAllDeLoreans()
        {
            if (TimeMachineCount == 0 && _savedEmpty)
                return;

            DeloreanCopyManager.Save(_timeMachines);

            _savedEmpty = TimeMachineCount == 0;
        }

        public static void LoadAllDeLoreans()
        {
            DeloreanCopyManager.Load()?.SpawnAll();
        }

        public static void SetDeloreansInTime(DateTime time)
        {
            foreach (var x in _timeMachines)
            {
                if (Main.PlayerVehicle != x.Vehicle && x.LastDisplacementCopy?.Circuits.DestinationTime > time)
                {
                    RemoteDeloreansHandler.AddDelorean(x.LastDisplacementCopy);
                    RemoveDelorean(x);
                }
            }
        }

        public static void AddDelorean(DMC12 vehicle)
        {
            if(!_deloreans.Contains(vehicle))
                _deloreans.Add(vehicle);
        }

        public static void AddTimeMachine(DeloreanTimeMachine timeMachine)
        {
            if (TimeMachineCount == MAX_TIME_MACHINES - 1)
                RemoveDelorean(FurthestTimeMachine);

            AddDelorean(timeMachine);

            if (!_timeMachines.Contains(timeMachine))
                _timeMachines.Add(timeMachine);
        }

        public static void AddInQuequeTimeMachine(DeloreanCopy deloreanCopy)
        {
            if (!_delosToAdd.Contains(deloreanCopy))
            {
                deloreanCopy.SetupTimeTravel(false);

                _delosToAdd.Add(deloreanCopy);
            }
                
        }

        public static void RemoveInstantlyDelorean(DMC12 vehicle, bool deleteVeh = true)
        {
            if (_delosToRemoveSounds.ContainsKey(vehicle))
                _delosToRemoveSounds.Remove(vehicle);

            if (vehicle.IsTimeMachine)
                _timeMachines.Remove((DeloreanTimeMachine)vehicle);

            vehicle.Dispose(deleteVeh);
            _deloreans.Remove(vehicle);
        }

        public static void RemoveDelorean(DMC12 vehicle, bool deleteVeh = true, bool checkPlayingSounds = false)
        {
            if (vehicle == null || _delosToRemove.ContainsKey(vehicle)) return;

            if (!checkPlayingSounds)
                _delosToRemove.Add(vehicle, deleteVeh);
            else if (!_delosToRemoveSounds.ContainsKey(vehicle))
                _delosToRemoveSounds.Add(vehicle, deleteVeh);
        }

        public static void RemoveAllDeloreans()
        {
            foreach(var delorean in _deloreans.ToList())
            {
                RemoveDelorean(delorean);
            }
        }

        public static DMC12 SpawnWithReentry(DeloreanType deloreanType = DeloreanType.BTTF1, string presetName = default)
        {
            if (deloreanType == DeloreanType.DMC12)
                return Spawn(deloreanType);

            DeloreanModsCopy deloreanModsCopy = null;

            if (presetName != default)
            {
                deloreanModsCopy = DeloreanModsCopy.Load(presetName);
                deloreanType = deloreanModsCopy.DeloreanType;
            }
                
            DeloreanTimeMachine deloreanTimeMachine = (DeloreanTimeMachine)DMC12.CreateDelorean(Main.PlayerPed.GetOffsetPosition(new Vector3(0, 25, 0)), Main.PlayerPed.Heading+180, deloreanType);
            
            if (deloreanModsCopy != null)
                deloreanModsCopy.ApplyTo(deloreanTimeMachine);

            Utils.HideVehicle(deloreanTimeMachine.Vehicle, true);

            deloreanTimeMachine.Circuits.DestinationTime = Main.CurrentTime;

            deloreanTimeMachine.Circuits.GetHandler<TimeTravelHandler>().Reenter(true);
            return deloreanTimeMachine;
        }

        public static DMC12 Spawn(DeloreanType deloreanType)
        {
            Ped ped = Main.PlayerPed;

            if (RCManager.RemoteControlling != null)
            {
                ped = RCManager.RemoteControlling.Circuits.GetHandler<RcHandler>().OriginalPed;
                RCManager.StopRemoteControl(true);
            }
            
            Vector3 spawnPos;

            if (Main.PlayerVehicle != null)
                spawnPos = Main.PlayerVehicle.Position.Around(5f);
            else
                spawnPos = ped.Position;

            DMC12 delorean = DMC12.CreateDelorean(spawnPos, ped.Heading, deloreanType);
            ped.SetIntoVehicle(delorean.Vehicle, VehicleSeat.Driver);

            delorean.Vehicle.PlaceOnGround();

            delorean.MPHSpeed = 1;

            return delorean;
        }

        public static bool IsVehicleADelorean(Vehicle vehicle)
        {
            foreach (var delorean in _deloreans)
            {
                if (delorean.Vehicle == vehicle)
                    return true;
            }

            return false;
        }
        
        public static bool IsVehicleATimeMachine(Vehicle vehicle)
        {
            foreach (var timeMachine in _timeMachines)
            {
                if (timeMachine.Vehicle == vehicle)
                    return true;
            }

            return false;
        }

        public static DMC12 GetDeloreanFromVehicle(Vehicle vehicle)
        {
            foreach(var delorean in _deloreans)
            {
                if (delorean.Vehicle == vehicle)
                    return delorean;
            }

            return null;
        }

        public static DeloreanTimeMachine GetTimeMachineFromVehicle(Vehicle vehicle)
        {
            foreach (var timeMachine in _timeMachines)
            {
                if (timeMachine.Vehicle == vehicle)
                    return timeMachine;
            }

            return null;
        }

        public static DeloreanTimeMachine GetTimeMachineFromIndex(int index)
        {
            if (index > _timeMachines.Count - 1)
                return null;

            return _timeMachines[index];
        }

        public static void Abort()
        {
            _deloreans.ForEach(x => x.Dispose(false));
        }

        public static void Tick()
        {
            if (Game.GameTime > _getClosestDelay)
            {
                UpdateClosestDeloreans();
                _getClosestDelay = Game.GameTime + 250;
            }
            
            if (Game.GameTime > _getAllDelay)
            {
                foreach (var veh in World.GetAllVehicles())
                {
                    if (veh.Model.Hash == ModelHandler.DMC12.Hash && GetDeloreanFromVehicle(veh) == null && veh.IsDead == false && veh.IsAlive)
                    {
                        DMC12 _dmc12 = DMC12.CreateDelorean(veh);

                        if (_dmc12.IsTimeMachine)
                        {
                            DeloreanTimeMachine _deloreanTimeMachine = (DeloreanTimeMachine)_dmc12;

                            if (_deloreanTimeMachine.LastDisplacementCopy == null)
                            {
                                _deloreanTimeMachine.LastDisplacementCopy = _deloreanTimeMachine.Copy;
                                _deloreanTimeMachine.LastDisplacementCopy.Circuits.DestinationTime = Main.CurrentTime;
                            }
                        }
                    }
                }

                _getAllDelay = Game.GameTime + 1000;
            }

            foreach (var delo in _delosToRemoveSounds)
            {
                DeloreanTimeMachine deloreanTimeMachine = (DeloreanTimeMachine)delo.Key;

                if (!deloreanTimeMachine.Circuits.AudioEngine.IsAnyInstancePlaying)
                    RemoveDelorean(delo.Key, delo.Value);
            }

            foreach (var delo in _delosToRemove)
                RemoveInstantlyDelorean(delo.Key, delo.Value);

            _delosToRemove.Clear();

            foreach (var delo in _delosToAdd)
                delo.Spawn();

            _delosToAdd.Clear();

            foreach (var delorean in _deloreans)
            {
                if (delorean.Vehicle.IsDead || !delorean.Vehicle.IsAlive)
                {
                    RemoveDelorean(delorean, false);
                    continue;
                } 

                delorean.Tick();

                if (Main.PlayerVehicle == delorean.Vehicle)
                    LastDMC12Vehicle = delorean;
            }

            foreach (var timeMachine in _timeMachines)
            {
                if (!timeMachine.Circuits.IsRemoteControlled && Main.PlayerVehicle == timeMachine.Vehicle)
                    LastTimeMachine = timeMachine;
            }
        }

        public static void UpdateClosestDeloreans()
        {
            GetClosestDeloreans(Main.PlayerPed,
                out float closestDeloreanDist, out DMC12 closestDelo, out DMC12 currentDelo,
                out float closestTimeMachineDist, out DeloreanTimeMachine closestTimeMachine, out DeloreanTimeMachine currentTimeMachine, out DeloreanTimeMachine furthestTimeMachine);

            CurrentDMC12 = currentDelo;

            if (ClosestDMC12 != closestDelo)
            {
                ClosestDMC12 = closestDelo;
            }

            if (ClosestTimeMachine != closestTimeMachine)
            {
                if (closestTimeMachine != null)
                {
                    closestTimeMachine.Circuits.OnScaleformPriority?.Invoke();
                    closestTimeMachine.IsGivenScaleformPriority = true;
                }

                if (ClosestTimeMachine != null)
                    ClosestTimeMachine.IsGivenScaleformPriority = false;

                ClosestTimeMachine = closestTimeMachine;
            }

            if (CurrentTimeMachine != currentTimeMachine)
            {
                CurrentTimeMachine = currentTimeMachine;
                CurrentTimeMachine?.Circuits?.OnEnteredDelorean?.Invoke();
            }

            if (FurthestTimeMachine != furthestTimeMachine)
                FurthestTimeMachine = furthestTimeMachine;

            SquareDistToClosestDMC12 = closestDeloreanDist;
            SquareDistToClosestTimeMachine = closestTimeMachineDist;
        }

        private static void GetClosestDeloreans(Entity entity, out float closestDeloreanDist, out DMC12 closestDelo, out DMC12 currentDelo, out float closestTimeMachineDist, out DeloreanTimeMachine closestTimeMachine, out DeloreanTimeMachine currentTimeMachine, out DeloreanTimeMachine furthestTimeMachine)
        {
            closestDeloreanDist = -1f;
            closestTimeMachineDist = -1f;
            closestDelo = null;
            currentDelo = null;
            closestTimeMachine = null;
            currentTimeMachine = null;

            float furthestTimeMachineDist = -1f;
            furthestTimeMachine = null;

            foreach(var delorean in _deloreans)
            {
                if(Main.PlayerVehicle == delorean.Vehicle)
                {
                    closestDelo = delorean;
                    currentDelo = delorean;
                    closestDeloreanDist = 0;

                    if(delorean.IsTimeMachine)
                    {
                        closestTimeMachine = (DeloreanTimeMachine)delorean;
                        currentTimeMachine = (DeloreanTimeMachine)delorean;
                        closestTimeMachineDist = 0;
                    }

                    break;
                }

                var sqrDist = delorean.Vehicle.Position.DistanceToSquared(entity.Position);

                if (sqrDist > furthestTimeMachineDist && furthestTimeMachine != delorean && delorean.IsTimeMachine)
                {
                    furthestTimeMachineDist = sqrDist;
                    furthestTimeMachine = (DeloreanTimeMachine)delorean;
                }

                if(closestDeloreanDist == -1f || sqrDist < closestDeloreanDist)
                {
                    closestDelo = delorean;
                    closestDeloreanDist = sqrDist;
                }

                if(delorean.IsTimeMachine && (closestTimeMachineDist == -1f || sqrDist < closestTimeMachineDist))
                {
                    closestTimeMachine = (DeloreanTimeMachine)delorean;
                    closestTimeMachineDist = sqrDist;
                }
            }

        }

        public static void KeyPressed(Keys key)
        {
            _deloreans.ForEach(x => x.KeyDown(key));
        }
    }
}
