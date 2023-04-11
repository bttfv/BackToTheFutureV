using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class WaybackMachine
    {
        private List<WaybackRecord> Records { get; } = new List<WaybackRecord>();

        private int PedHandle { get; set; }
        public Ped Ped
        {
            get => (Ped)Entity.FromHandle(PedHandle);

            private set
            {
                if (value.NotNullAndExists())
                {
                    PedHandle = value.Handle;
                }
                else
                {
                    PedHandle = 0;
                }
            }
        }

        public int LastRecordedIndex { get; private set; } = -1;
        public WaybackRecord LastRecord
        {
            get
            {
                if (LastRecordedIndex < 0)
                {
                    return Records[0];
                }

                return Records[LastRecordedIndex];
            }
        }

        public WaybackRecord PreviousRecording
        {
            get
            {
                if (LastRecordedIndex <= 0)
                {
                    return Records[0];
                }

                return Records[LastRecordedIndex - 1];
            }
        }

        public int CurrentIndex { get; private set; } = 0;
        public WaybackRecord CurrentRecord => Records[CurrentIndex];

        public WaybackRecord PreviousRecord
        {
            get
            {
                if (CurrentIndex <= 0)
                {
                    return CurrentRecord;
                }

                return Records[CurrentIndex - 1];
            }
        }
        public WaybackRecord NextRecord
        {
            get
            {
                if (CurrentIndex >= LastRecordedIndex)
                {
                    return CurrentRecord;
                }

                return Records[CurrentIndex + 1];
            }
        }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public Guid WaitForTimeMachineGUID { get; }

        public WaybackStatus Status { get; private set; } = WaybackStatus.Idle;

        public bool IsPlayer { get; private set; }

        public Vehicle OverrideVehicle { get; set; }

        public WaybackMachine(Ped ped)
        {
            Ped = ped;

            IsPlayer = Ped == FusionUtils.PlayerPed;
            Status = WaybackStatus.Recording;

            if (IsPlayer)
            {
                WaybackSystem.CurrentPlayerRecording?.Stop();
            }

            if (TimeMachineHandler.CurrentTimeMachine.NotNullAndExists() && TimeMachineHandler.CurrentTimeMachine == Ped.CurrentVehicle && TimeMachineHandler.CurrentTimeMachine.Constants.TimeTravelCooldown > -1)
            {
                WaitForTimeMachineGUID = TimeMachineHandler.CurrentTimeMachine.Properties.GUID;
            }
            else
            {
                WaitForTimeMachineGUID = Guid.Empty;
            }
        }

        private bool ArraysEqual(int[,] a1, int[,] a2, int size, bool comp)
        {
            for (int i = 0; i < size; i++)
            {
                if (a1[i, 0] != a2[i, 0])
                {
                    return false;
                }
                else if (a1[i, 1] != a2[i, 1])
                {
                    return false;
                }
                else if (comp && a1[i, 2] != a2[i, 2])
                {
                    return false;
                }
            }
            return true;
        }

        public void Tick()
        {
            switch (Status)
            {
                case WaybackStatus.Idle:
                    if (FusionUtils.CurrentTime.Between(StartTime, EndTime))
                    {
                        if (WaitForTimeMachineGUID != Guid.Empty)
                        {
                            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromGUID(WaitForTimeMachineGUID);

                            if (!timeMachine.NotNullAndExists() || timeMachine.Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                                return;

                            Ped = timeMachine.Vehicle.Driver;
                        }

                        CurrentIndex = Records.FindIndex(x => x.Time >= FusionUtils.CurrentTime);

                        Status = WaybackStatus.Playing;

                        if (!Ped.NotNullAndExists() && CurrentRecord?.Vehicle?.Event != WaybackVehicleEvent.TimeTravel)
                        {
                            Ped = CurrentRecord.Spawn(NextRecord);
                        }

                        Play();
                    }
                    break;
                case WaybackStatus.Recording:
                    Record();
                    break;
                case WaybackStatus.Playing:
                    Play();
                    break;
            }
        }

        private void Record()
        {
            if ((IsPlayer && !FusionUtils.PlayerPed.IsAlive) || (!IsPlayer && !Ped.ExistsAndAlive()) || FusionUtils.CurrentTime < StartTime || Game.IsMissionActive)
            {
                Stop();
                return;
            }

            WaybackRecord waybackRecord;

            if (IsPlayer)
            {
                waybackRecord = new WaybackRecord(FusionUtils.PlayerPed, OverrideVehicle);
            }
            else
            {
                waybackRecord = new WaybackRecord(Ped, OverrideVehicle);
            }

            OverrideVehicle = null;

            if (IsPlayer && PedHandle != FusionUtils.PlayerPed.Handle)
            {
                if (FusionUtils.PlayerVehicle.NotNullAndExists() && FusionUtils.PlayerVehicle.IsTimeMachine() &&
                    TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle).Properties.IsRemoteControlled)
                {
                    // Do nothing if RC is active since we want Wayback to keep controlling the original ped
                }
                else
                {
                    PedHandle = FusionUtils.PlayerPed.Handle;
                    waybackRecord.Ped.SwitchPed = true;
                }
            }

            // We need to make sure any WaybackPed is removed from the WaybackVehicle since Wayback should control it
            if (IsPlayer && FusionUtils.PlayerVehicle.NotNullAndExists())
            {
                foreach (PedReplica occupant in waybackRecord.Vehicle.Replica.Occupants.ToList())
                {
                    if (occupant.Model == waybackRecord.Ped.Replica.Model)
                    {
                        waybackRecord.Vehicle.Replica.Occupants.Remove(occupant);
                    }
                }
            }

            if (LastRecordedIndex > 0)
            {
                bool _sameComps = ArraysEqual(PreviousRecording.Ped.Replica.Components, waybackRecord.Ped.Replica.Components, 12, true);

                bool _sameProps = ArraysEqual(PreviousRecording.Ped.Replica.Props, waybackRecord.Ped.Replica.Props, 5, false);

                if (IsPlayer && (!_sameComps || !_sameProps))
                    waybackRecord.Ped.SwitchedClothes = true;

                if (IsPlayer && PreviousRecording.Ped.Replica.Weapons.Count != waybackRecord.Ped.Replica.Weapons.Count)
                    waybackRecord.Ped.SwitchedWeapons = true;
            }

            Records.Add(waybackRecord);

            LastRecordedIndex++;
        }

        private void Play()
        {
            if (!Ped.ExistsAndAlive())
            {
                return;
            }

            if (CurrentRecord.Vehicle != null && CurrentRecord.Vehicle.IsTimeMachine && Ped.CurrentVehicle.NotNullAndExists())
            {
                TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(Ped.CurrentVehicle);

                if (timeMachine.Properties.TimeTravelPhase >= TimeTravelPhase.InTime)
                {
                    Stop();

                    return;
                }
            }

            if (CurrentRecord.Ped.SwitchPed)
            {
                Ped?.Task.ClearAllImmediately();
                Ped = CurrentRecord.Spawn(NextRecord);
            }

            if (CurrentRecord.Ped.SwitchedWeapons)
            {
                foreach (WeaponReplica x in CurrentRecord.Ped.Replica.Weapons)
                {
                    x.Give(Ped);
                }
            }

            if (CurrentRecord.Ped.SwitchedClothes)
            {
                for (int x = 0; x < 12; x++)
                {
                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Ped, x, CurrentRecord.Ped.Replica.Components[x, 0], CurrentRecord.Ped.Replica.Components[x, 1], CurrentRecord.Ped.Replica.Components[x, 2]);
                }

                for (int x = 0; x < 5; x++)
                {
                    if (x <= 2)
                    {
                        Function.Call(Hash.SET_PED_PROP_INDEX, Ped, x, CurrentRecord.Ped.Replica.Props[x, 0], CurrentRecord.Ped.Replica.Props[x, 1], true);
                    }
                    else
                    {
                        Function.Call(Hash.SET_PED_PROP_INDEX, Ped, x + 3, CurrentRecord.Ped.Replica.Props[x, 0], CurrentRecord.Ped.Replica.Props[x, 1], true);
                    }
                }
            }

            CurrentRecord.Apply(Ped, NextRecord);

            if (CurrentIndex >= LastRecordedIndex)
            {
                Stop();
            }
            else
            {
                CurrentIndex++;
            }
        }

        public void Stop()
        {
            if (Status == WaybackStatus.Recording && Records.Count > 0)
            {
                StartTime = Records[0].Time;
                if (ModSettings.RealTime)
                    EndTime = LastRecord.Time.AddSeconds(-2);
                else
                    EndTime = LastRecord.Time.AddMinutes(-1);
            }

            CurrentIndex = 0;
            PedHandle = 0;
            IsPlayer = false;
            Status = WaybackStatus.Idle;
        }
    }
}
