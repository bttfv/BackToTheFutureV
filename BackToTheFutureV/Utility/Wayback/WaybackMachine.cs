using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Collections.Generic;
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
                    Play();
                    break;
            }
        }

        private void Record()
        {
            if ((IsPlayer && !FusionUtils.PlayerPed.IsAlive) || (!IsPlayer && !Ped.ExistsAndAlive()) || FusionUtils.CurrentTime < StartTime)
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
                waybackRecord.Ped.SwitchPed = true;
                PedHandle = FusionUtils.PlayerPed.Handle;
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
                EndTime = LastRecord.Time;
            }

            CurrentIndex = 0;
            PedHandle = 0;
            IsPlayer = false;
            Status = WaybackStatus.Idle;
        }
    }
}
