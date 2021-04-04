using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class Wayback
    {
        public List<WaybackPedReplica> WaybackPedReplicas { get; } = new List<WaybackPedReplica>();

        public Guid GUID { get; private set; } = Guid.Empty;

        public PedReplica PedReplica { get; private set; }
        public Ped Ped { get; set; }

        public WaybackPedReplica StartReplica => WaybackPedReplicas.FirstOrDefault(x => x.Time >= Utils.CurrentTime);

        public int ReplicaIndex { get; private set; } = -1;
        public WaybackPedReplica CurrentReplica => WaybackPedReplicas[ReplicaIndex];

        public int StartGameTime { get; private set; }
        public int StartPlayGameTime { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public WaybackStatus Status { get; private set; } = WaybackStatus.Idle;

        public Wayback(Ped ped)
        {
            TimeHandler.OnTimeChanged += OnTimeChanged;

            GUID = Guid.NewGuid();

            PedReplica = new PedReplica(ped);

            Ped = ped;
            StartGameTime = Game.GameTime;

            WaybackPedReplicas.Add(new WaybackPedReplica(Ped, StartGameTime));

            StartTime = WaybackPedReplicas.First().Time;

            Status = WaybackStatus.Recording;
        }

        private void OnTimeChanged(DateTime dateTime)
        {
            if (Status == WaybackStatus.Playing && dateTime.Between(StartTime, EndTime))
                return;

            Stop();
        }

        public void Tick()
        {
            switch (Status)
            {
                case WaybackStatus.Idle:
                    if (Utils.CurrentTime.Between(StartTime, EndTime))
                    {
                        ReplicaIndex = WaybackPedReplicas.IndexOf(StartReplica);

                        if (!Ped.NotNullAndExists())
                            Spawn();

                        StartPlayGameTime = Game.GameTime + CurrentReplica.Timestamp;
                        Status = WaybackStatus.Playing;

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
            Ped = World.GetClosestPed(CurrentReplica.Position, 1, PedReplica.Model);

            if (Ped.NotNullAndExists())
                return;

            Ped = PedReplica.Spawn(CurrentReplica.Position, CurrentReplica.Heading);
        }

        private WaybackPedReplica Record()
        {
            if (!Ped.NotNullAndExists() || Utils.CurrentTime < StartTime)
            {
                Stop();
                return null;
            }

            WaybackPedReplica recordedReplica = new WaybackPedReplica(Ped, StartGameTime);

            if (recordedReplica.Event == WaybackPedReplicas.Last().Event && (recordedReplica.Event == WaybackPedEvent.EnteringVehicle || recordedReplica.Event == WaybackPedEvent.LeavingVehicle))
                return null;

            WaybackPedReplicas.Add(recordedReplica);

            return recordedReplica;
        }

        private void Play()
        {
            if (!Ped.NotNullAndExists())
                return;

            if (Game.GameTime <= (CurrentReplica.Timestamp + StartPlayGameTime))
                return;

            if (ReplicaIndex >= WaybackPedReplicas.Count - 1)
            {
                CurrentReplica.Apply(Ped, CurrentReplica, StartPlayGameTime);
                Status = WaybackStatus.Idle;
            }
            else
            {
                CurrentReplica.Apply(Ped, WaybackPedReplicas[ReplicaIndex + 1], StartPlayGameTime);
                ReplicaIndex++;
            }
        }

        public void Stop()
        {
            if (Status == WaybackStatus.Recording)
                EndTime = WaybackPedReplicas.Last().Time;

            Ped = null;
            Status = WaybackStatus.Idle;
        }
    }
}
