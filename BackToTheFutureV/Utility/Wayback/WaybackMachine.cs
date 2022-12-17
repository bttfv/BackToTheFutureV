using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
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

        public Guid GUID { get; private set; } = Guid.Empty;

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

        public WaybackStatus Status { get; private set; } = WaybackStatus.Idle;

        public bool IsPlayer { get; private set; }

        public bool WaitForReentry { get; private set; }

        public WaybackMachine(Ped ped, Guid guid)
        {
            Ped = ped;
            GUID = guid;

            IsPlayer = Ped == FusionUtils.PlayerPed;
            Status = WaybackStatus.Recording;

            if (IsPlayer)
            {
                WaybackSystem.CurrentPlayerRecording?.Stop();
            }
        }

        public void StartOn(Ped ped, bool waitForReentry = false)
        {
            Ped = ped;
            WaitForReentry = waitForReentry;

            CurrentIndex = 0;
            Status = WaybackStatus.Playing;
        }

        public void Tick()
        {
            switch (Status)
            {
                case WaybackStatus.Idle:
                    if (FusionUtils.CurrentTime.Between(StartTime, EndTime))
                    {
                        CurrentIndex = Records.FindIndex(x => x.Time >= FusionUtils.CurrentTime);

                        Status = WaybackStatus.Playing;

                        if (!Ped.NotNullAndExists())
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
                    if (WaitForReentry)
                    {
                        if (TimeMachineHandler.GetTimeMachineFromReplicaGUID(GUID).Properties.TimeTravelPhase == TimeTravelPhase.Reentering)
                        {
                            return;
                        }
                        WaitForReentry = false;
                    }

                    Play();
                    break;
            }
        }

        private WaybackRecord Record()
        {
            if ((IsPlayer && !FusionUtils.PlayerPed.IsAlive) || (!IsPlayer && !Ped.ExistsAndAlive()) || FusionUtils.CurrentTime < StartTime || Game.IsMissionActive)
            {
                Stop();
                return null;
            }

            WaybackRecord waybackRecord;

            if (IsPlayer)
            {
                waybackRecord = new WaybackRecord(FusionUtils.PlayerPed);
            }
            else
            {
                waybackRecord = new WaybackRecord(Ped);
            }

            if (IsPlayer && PedHandle != FusionUtils.PlayerPed.Handle)
            {
                waybackRecord.Ped.SwitchPed = true;
                PedHandle = FusionUtils.PlayerPed.Handle;
            }

            Records.Add(waybackRecord);

            LastRecordedIndex++;

            return waybackRecord;
        }

        private void Play()
        {
            if ((Ped.NotNullAndExists() && Ped.IsDead && ModSettings.TimeParadox) || (Ped.NotNullAndExists() && Ped.LastVehicle.NotNullAndExists() && Ped.LastVehicle.IsConsideredDestroyed && ModSettings.TimeParadox))
            {
                if (FusionUtils.PlayerVehicle.NotNullAndExists() && FusionUtils.PlayerVehicle.IsTimeMachine() && TimeMachineHandler.CurrentTimeMachine.Properties.IsRemoteControlled)
                {
                    RemoteTimeMachineHandler.StopRemoteControl();
                }
                if (FusionUtils.PlayerPed.Model != Main.ResetPed.Model)
                {
                    Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, Main.SwitchedPed, true).LoadScene();
                    Function.Call(Hash.CHANGE_PLAYER_PED, Game.Player, Main.SwitchedPed, false, false);
                }
                if (FusionUtils.PlayerPed.IsInVehicle() && !(FusionUtils.PlayerVehicle.IsInAir || FusionUtils.PlayerVehicle.IsInWater) && FusionUtils.PlayerVehicle.Speed > 5 && Game.Player.CanControlCharacter)
                {
                    Game.Player.CanControlCharacter = false;
                    Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, FusionUtils.PlayerPed, FusionUtils.PlayerVehicle, FusionEnums.DriveAction.BrakeStrong, 5000);
                }
                if (FusionUtils.PlayerPed.IsInVehicle() && !(FusionUtils.PlayerVehicle.IsInAir || FusionUtils.PlayerVehicle.IsInWater) && !FusionUtils.PlayerPed.IsLeavingVehicle() && FusionUtils.PlayerVehicle.Speed < 5)
                {
                    Game.Player.CanControlCharacter = false;
                    FusionUtils.PlayerVehicle.IsPersistent = false;
                    FusionUtils.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                    Main.SwitchedVehicle = null;
                }
                if (FusionUtils.PlayerPed.IsFullyOutVehicle() || (FusionUtils.PlayerPed.IsInVehicle() && (FusionUtils.PlayerVehicle.IsInAir || FusionUtils.PlayerVehicle.IsInWater)))
                {
                    if (FusionUtils.PlayerPed.IsInVehicle() && (FusionUtils.PlayerVehicle.IsInAir || FusionUtils.PlayerVehicle.IsInWater))
                    {
                        Game.Player.CanControlCharacter = false;
                        FusionUtils.PlayerVehicle.IsPersistent = false;
                        Main.SwitchedVehicle = null;
                    }
                    Function.Call(Hash.SET_PED_DROPS_WEAPON, FusionUtils.PlayerPed);
                    FusionUtils.PlayerPed.Weapons.RemoveAll();
                    foreach (WeaponReplica x in Main.ResetPed.Weapons)
                    {
                        x.Give(FusionUtils.PlayerPed);
                    }
                    Script.Yield();
                    FusionUtils.PlayerPed.Kill();
                    Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "respawn_controller");
                    Function.Call(Hash.IGNORE_NEXT_RESTART, true);
                    Function.Call(Hash.PAUSE_DEATH_ARREST_RESTART, true);
                    WaybackSystem.Paradox = true;
                    WaybackSystem.paradoxDelay = Game.GameTime + 3600;
                    GTA.UI.Screen.FadeOut(8000);
                    WaybackSystem.Abort();
                }
            }

            if (!Ped.ExistsAndAlive())
            {
                return;
            }

            if (CurrentRecord.Ped.SwitchPed)
            {
                Ped?.Task.ClearAllImmediately();
                Ped = CurrentRecord.Spawn(NextRecord);
            }

            if ((!CurrentRecord.Ped.Replica.Components.OfType<int>().SequenceEqual(PreviousRecord.Ped.Replica.Components.OfType<int>()) && !CurrentRecord.Ped.SwitchPed && !PreviousRecord.Ped.SwitchPed) || (!CurrentRecord.Ped.Replica.Props.OfType<int>().SequenceEqual(PreviousRecord.Ped.Replica.Props.OfType<int>()) && !CurrentRecord.Ped.SwitchPed && !PreviousRecord.Ped.SwitchPed))
            {
                for (int x = 0; x <= 11; x++)
                {
                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, Ped, x, CurrentRecord.Ped.Replica.Components[x, 0], CurrentRecord.Ped.Replica.Components[x, 1], CurrentRecord.Ped.Replica.Components[x, 2]);
                }
                for (int x = 0; x <= 12; x++)
                {
                    Function.Call(Hash.SET_PED_PROP_INDEX, Ped, x, CurrentRecord.Ped.Replica.Props[x, 0], CurrentRecord.Ped.Replica.Props[x, 1], true);
                }
            }

            CurrentRecord.Apply(Ped, NextRecord);

            if (CurrentIndex >= LastRecordedIndex)
            {
                Status = WaybackStatus.Idle;
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
                EndTime = LastRecord.Time.AddMinutes(-1);
            }

            CurrentIndex = 0;
            PedHandle = 0;
            IsPlayer = false;
            Status = WaybackStatus.Idle;
        }
    }
}
