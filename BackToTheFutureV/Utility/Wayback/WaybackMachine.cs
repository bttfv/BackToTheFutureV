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
        public List<WaybackPed> Replicas { get; } = new List<WaybackPed>();

        public Guid GUID { get; private set; } = Guid.Empty;

        public PedReplica PedReplica { get; private set; }

        private int Ped1Handle { get; set; }
        private int Ped2Handle { get; set; }
        private bool UsePed1 { get; set; } = true;

        public Ped Ped
        {
            get => (Ped)Entity.FromHandle(UsePed1 ? Ped1Handle : Ped2Handle);
            private set
            {
                if (UsePed1)
                {
                    if (value.NotNullAndExists())
                        Ped1Handle = value.Handle;
                    else
                        Ped1Handle = 0;
                }
                else
                {
                    if (value.NotNullAndExists())
                        Ped2Handle = value.Handle;
                    else
                        Ped2Handle = 0;
                }
            }
        }

        public int LastRecReplicaIndex { get; private set; } = 0;
        public WaybackPed LastRecReplica => Replicas[LastRecReplicaIndex];

        public int ReplicaIndex { get; private set; } = 0;
        public WaybackPed CurrentReplica => Replicas[ReplicaIndex];
        public WaybackPed PreviousReplica
        {
            get
            {
                if (ReplicaIndex == 0)
                    return CurrentReplica;

                return Replicas[ReplicaIndex - 1];
            }
        }
        public WaybackPed NextReplica
        {
            get
            {
                if (ReplicaIndex == Replicas.Count - 1)
                    return CurrentReplica;

                return Replicas[ReplicaIndex + 1];
            }
        }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public WaybackStatus Status { get; private set; } = WaybackStatus.Idle;

        public bool WaitForReentry { get; private set; }

        public WaybackMachine(Ped ped, Guid guid)
        {
            Ped = ped;
            GUID = guid;

            PedReplica = new PedReplica(Ped);

            Replicas.Add(new WaybackPed(GUID, Ped));

            StartTime = LastRecReplica.Time;

            Status = WaybackStatus.Recording;
        }

        public void StartOn(Ped ped, bool waitForReentry = false)
        {
            Ped = ped;
            WaitForReentry = waitForReentry;

            StartTime = Utils.CurrentTime;
        }

        public void Tick()
        {
            switch (Status)
            {
                case WaybackStatus.Idle:
                    if (Utils.CurrentTime.Between(StartTime, EndTime))
                    {
                        if (WaitForReentry)
                        {
                            if (TimeMachineHandler.GetTimeMachineFromReplicaGUID(GUID).Properties.TimeTravelPhase == TimeTravelPhase.Reentering)
                                return;

                            ReplicaIndex = 0;
                            WaitForReentry = false;
                        }
                        else
                            ReplicaIndex = Replicas.FindIndex(x => x.Time >= Utils.CurrentTime);

                        Status = WaybackStatus.Playing;

                        if (!Ped.NotNullAndExists())
                            Spawn();

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

        private void Spawn()
        {
            float adjustedRatio = ((CurrentReplica.FrameTime + NextReplica.FrameTime) / 2) / Game.LastFrameTime;

            Ped = World.GetClosestPed(Utils.Lerp(CurrentReplica.Position, NextReplica.Position, adjustedRatio), 1, PedReplica.Model);

            if (Ped.NotNullAndExists())
                return;

            Ped = PedReplica.Spawn(Utils.Lerp(CurrentReplica.Position, NextReplica.Position, adjustedRatio), Utils.Lerp(CurrentReplica.Heading, NextReplica.Heading, adjustedRatio));
        }

        public void Clone(Ped ped)
        {
            UsePed1 = !UsePed1;
            Ped = ped;

            Record().Event = WaybackPedEvent.Clone;
        }

        public WaybackPed Record(WaybackVehicle waybackVehicle = default)
        {
            if (Status != WaybackStatus.Recording)
                return null;

            if (!Ped.NotNullAndExists() || Utils.CurrentTime < StartTime)
            {
                Stop();
                return null;
            }

            WaybackPed recordedReplica = new WaybackPed(GUID, Ped);

            if (waybackVehicle != default)
                recordedReplica.WaybackVehicle = waybackVehicle;

            Replicas.Add(recordedReplica);

            LastRecReplicaIndex++;

            return recordedReplica;
        }

        private void Play()
        {
            if (!Ped.NotNullAndExists())
                return;

            if (CurrentReplica.Event == WaybackPedEvent.Clone)
            {
                Ped?.Task.ClearAllImmediately();

                UsePed1 = !UsePed1;
                Spawn();
            }

            CurrentReplica.Apply(Ped, NextReplica);

            if (ReplicaIndex == Replicas.Count - 1)
                Status = WaybackStatus.Idle;
            else
                ReplicaIndex++;
        }

        public void Stop()
        {
            if (Status == WaybackStatus.Recording)
                EndTime = LastRecReplica.Time.AddMinutes(-1);

            ReplicaIndex = 0;
            Ped1Handle = 0;
            Ped2Handle = 0;
            Status = WaybackStatus.Idle;
        }

        public static WaybackMachine FromData(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                try
                {
                    return (WaybackMachine)Utils.BinaryFormatter.Deserialize(stream);
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
                Utils.BinaryFormatter.Serialize(stream, command);
                return stream.ToArray();
            }
        }
    }
}
