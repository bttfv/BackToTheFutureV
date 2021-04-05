using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class WaybackMachine
    {
        public List<WaybackPed> Replicas { get; } = new List<WaybackPed>();

        public Guid GUID { get; private set; } = Guid.Empty;

        public PedReplica PedReplica { get; private set; }
        public Ped Ped { get; set; }

        public int ReplicaIndex { get; private set; } = 0;
        public WaybackPed CurrentReplica => Replicas[ReplicaIndex];
        public WaybackPed NextReplica
        {
            get
            {
                if (ReplicaIndex == Replicas.Count - 1)
                    return CurrentReplica;

                return Replicas[ReplicaIndex + 1];
            }
        }

        public float AdjustedRatio
        {
            get
            {
                if (CurrentReplica.Timestamp == NextReplica.Timestamp)
                    return 0;

                return (Game.GameTime - StartPlayGameTime - CurrentReplica.Timestamp) / (NextReplica.Timestamp - CurrentReplica.Timestamp);
            }
        }

        public int StartRecGameTime { get; private set; }
        public int StartPlayGameTime { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public WaybackStatus Status { get; private set; } = WaybackStatus.Idle;

        public bool WaitForReentry { get; set; } = false;

        public WaybackMachine(Ped ped, Guid guid) : this(ped)
        {
            GUID = guid;
        }

        public WaybackMachine(Ped ped)
        {
            TimeHandler.OnTimeChanged += OnTimeChanged;

            GUID = Guid.NewGuid();

            PedReplica = new PedReplica(ped);

            Ped = ped;
            StartRecGameTime = Game.GameTime;

            Replicas.Add(new WaybackPed(Ped, StartRecGameTime));

            StartTime = Replicas.First().Time;

            Status = WaybackStatus.Recording;
        }

        private void OnTimeChanged(DateTime dateTime)
        {
            Stop();
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

                        StartPlayGameTime = Game.GameTime + CurrentReplica.Timestamp;
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
            Ped = World.GetClosestPed(Utils.Lerp(CurrentReplica.Position, NextReplica.Position, AdjustedRatio), 1, PedReplica.Model);

            if (Ped.NotNullAndExists())
                return;

            Ped = PedReplica.Spawn(Utils.Lerp(CurrentReplica.Position, NextReplica.Position, AdjustedRatio), Utils.Lerp(CurrentReplica.Heading, NextReplica.Heading, AdjustedRatio));
        }

        public WaybackPed Record(WaybackVehicle waybackVehicle)
        {
            if (!Ped.NotNullAndExists() || Utils.CurrentTime < StartTime)
            {
                Stop();
                return null;
            }

            WaybackPed recordedReplica = new WaybackPed(Ped, StartRecGameTime);

            if (recordedReplica.Event == Replicas.Last().Event && (recordedReplica.Event == WaybackPedEvent.EnteringVehicle || recordedReplica.Event == WaybackPedEvent.LeavingVehicle))
                return null;

            recordedReplica.WaybackVehicle = waybackVehicle;

            Replicas.Add(recordedReplica);

            return recordedReplica;
        }

        private WaybackPed Record()
        {
            if (!Ped.NotNullAndExists() || Utils.CurrentTime < StartTime)
            {
                Stop();
                return null;
            }

            WaybackPed recordedReplica = new WaybackPed(Ped, StartRecGameTime);

            if (recordedReplica.Event == Replicas.Last().Event && (recordedReplica.Event == WaybackPedEvent.EnteringVehicle || recordedReplica.Event == WaybackPedEvent.LeavingVehicle))
                return null;

            Replicas.Add(recordedReplica);

            return recordedReplica;
        }

        private void Play()
        {
            if (!Ped.NotNullAndExists())
                return;

            if (Game.GameTime <= (CurrentReplica.Timestamp + StartPlayGameTime))
                return;

            CurrentReplica.Apply(Ped, NextReplica, AdjustedRatio);

            if (ReplicaIndex == Replicas.Count - 1)
                Status = WaybackStatus.Idle;
            else
                ReplicaIndex++;
        }

        public void Stop()
        {
            if (Status == WaybackStatus.Recording)
                EndTime = Replicas.Last().Time;

            Ped = null;
            Status = WaybackStatus.Idle;
        }
    }
}
