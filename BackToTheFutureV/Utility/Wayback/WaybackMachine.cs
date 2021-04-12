using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Collections.Generic;
using System.IO;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class WaybackMachine
    {
        public List<WaybackRecord> Records { get; } = new List<WaybackRecord>();

        public Guid GUID { get; private set; } = Guid.Empty;

        private int PedHandle { get; set; }
        public Ped Ped
        {
            get => (Ped)Entity.FromHandle(PedHandle);
            private set
            {
                if (value.NotNullAndExists())
                    PedHandle = value.Handle;
                else
                    PedHandle = 0;
            }
        }

        public int LastRecordedIndex { get; private set; } = -1;
        public WaybackRecord LastRecord => Records[LastRecordedIndex];

        public int CurrentIndex { get; private set; } = 0;
        public WaybackRecord CurrentRecord => Records[CurrentIndex];
        public WaybackRecord NextRecord
        {
            get
            {
                if (CurrentIndex >= LastRecordedIndex)
                    return CurrentRecord;

                return Records[CurrentIndex + 1];
            }
        }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public WaybackStatus Status { get; private set; } = WaybackStatus.Idle;

        public bool IsPlayer { get; private set; }

        public bool WaitForReentry { get; private set; }

        private bool SkipNextRecord = false;

        public WaybackMachine(Ped ped, Guid guid)
        {
            Ped = ped;
            GUID = guid;

            IsPlayer = Ped == FusionUtils.PlayerPed;
            Status = WaybackStatus.Recording;

            Record();
            StartTime = LastRecord.Time;

            SkipNextRecord = true;
        }

        public void StartOn(Ped ped, bool waitForReentry = false)
        {
            Ped = ped;
            WaitForReentry = waitForReentry;

            StartTime = FusionUtils.CurrentTime;
        }

        public void Tick()
        {
            switch (Status)
            {
                case WaybackStatus.Idle:
                    if (FusionUtils.CurrentTime.Between(StartTime, EndTime))
                    {
                        if (WaitForReentry)
                        {
                            if (TimeMachineHandler.GetTimeMachineFromReplicaGUID(GUID).Properties.TimeTravelPhase == TimeTravelPhase.Reentering)
                                return;

                            CurrentIndex = 0;
                            WaitForReentry = false;
                        }
                        else
                            CurrentIndex = Records.FindIndex(x => x.Time >= FusionUtils.CurrentTime);

                        Status = WaybackStatus.Playing;

                        if (!Ped.NotNullAndExists())
                            Ped = CurrentRecord.Spawn(NextRecord);

                        Play();
                    }
                    break;
                case WaybackStatus.Recording:
                    if (IsPlayer && Ped != FusionUtils.PlayerPed)
                        SwitchPed(FusionUtils.PlayerPed);

                    Record();
                    break;
                case WaybackStatus.Playing:
                    Play();
                    break;
            }
        }

        private void SwitchPed(Ped ped)
        {
            Ped = ped;

            SkipNextRecord = false;

            Record().Ped.SwitchPed = true;

            SkipNextRecord = true;
        }

        public WaybackRecord Record(TimeMachine timeMachine, WaybackVehicleEvent wvEvent, int timeTravelDelay = 0)
        {
            if (Status != WaybackStatus.Recording)
                return null;

            if (!Ped.NotNullAndExists() || FusionUtils.CurrentTime < StartTime)
            {
                Stop();
                return null;
            }

            WaybackRecord waybackRecord = new WaybackRecord(Ped, timeMachine, wvEvent, timeTravelDelay);

            Records.Add(waybackRecord);

            LastRecordedIndex++;

            SkipNextRecord = true;

            return waybackRecord;
        }

        private WaybackRecord Record()
        {
            if (SkipNextRecord)
            {
                SkipNextRecord = false;
                return null;
            }

            if (Status != WaybackStatus.Recording)
                return null;

            if (!Ped.NotNullAndExists() || FusionUtils.CurrentTime < StartTime)
            {
                Stop();
                return null;
            }

            WaybackRecord waybackRecord = new WaybackRecord(Ped);

            Records.Add(waybackRecord);

            LastRecordedIndex++;

            return waybackRecord;
        }

        private void Play()
        {
            if (!Ped.NotNullAndExists())
                return;

            if (CurrentRecord.Ped.SwitchPed)
            {
                Ped?.Task.ClearAllImmediately();
                Ped = CurrentRecord.Spawn(NextRecord);
            }

            CurrentRecord.Apply(Ped, NextRecord);

            if (CurrentIndex >= LastRecordedIndex)
                Status = WaybackStatus.Idle;
            else
                CurrentIndex++;
        }

        public void Stop()
        {
            if (Status == WaybackStatus.Recording)
                EndTime = LastRecord.Time.AddMinutes(-1);

            CurrentIndex = 0;
            PedHandle = 0;
            IsPlayer = false;
            Status = WaybackStatus.Idle;
        }

        public static WaybackMachine FromData(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                try
                {
                    return (WaybackMachine)FusionUtils.BinaryFormatter.Deserialize(stream);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static implicit operator byte[](WaybackMachine command)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                FusionUtils.BinaryFormatter.Serialize(stream, command);
                return stream.ToArray();
            }
        }
    }
}
