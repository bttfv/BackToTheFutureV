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
        private List<WaybackRecord> Records { get; } = new List<WaybackRecord>();

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
        public WaybackRecord LastRecord
        {
            get
            {
                if (LastRecordedIndex < 0)
                    return Records[0];

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
                    return CurrentRecord;

                return Records[CurrentIndex - 1];
            }
        }
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

        public WaybackMachine(Ped ped, Guid guid)
        {
            Ped = ped;
            GUID = guid;

            IsPlayer = Ped == FusionUtils.PlayerPed;
            Status = WaybackStatus.Recording;

            if (IsPlayer)
                WaybackSystem.CurrentPlayerRecording?.Stop();
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
                            Ped = CurrentRecord.Spawn(NextRecord);

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
                            return;

                        WaitForReentry = false;
                    }

                    Play();
                    break;
            }
        }

        private WaybackRecord Record()
        {
            if ((IsPlayer && !FusionUtils.PlayerPed.IsAlive) || (!IsPlayer && !Ped.ExistsAndAlive()) || FusionUtils.CurrentTime < StartTime)
            {
                Stop();
                return null;
            }

            WaybackRecord waybackRecord;

            if (IsPlayer)
                waybackRecord = new WaybackRecord(FusionUtils.PlayerPed);
            else
                waybackRecord = new WaybackRecord(Ped);

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
            if (!Ped.ExistsAndAlive())
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
