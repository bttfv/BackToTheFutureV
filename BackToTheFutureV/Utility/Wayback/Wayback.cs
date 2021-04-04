using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class Wayback : Script
    {
        private static List<Wayback> Waybacks = new List<Wayback>();

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

        public Wayback()
        {
            Tick += WaybackMachine_Tick;
            TimeHandler.OnTimeChanged += OnTimeChanged;
        }

        public void Create(Ped ped)
        {
            GUID = Guid.NewGuid();

            PedReplica = new PedReplica(ped);

            Ped = ped;
            StartGameTime = Game.GameTime;

            WaybackPedReplicas.Add(new WaybackPedReplica(Ped, StartGameTime));

            StartTime = WaybackPedReplicas.First().Time;

            Status = WaybackStatus.Recording;

            Waybacks.Add(this);
        }

        private void OnTimeChanged(DateTime dateTime)
        {
            if (GUID == Guid.Empty)
            {
                Wayback wayback = InstantiateScript<Wayback>();

                wayback.Create(Utils.PlayerPed);

                return;
            }

            if (Status == WaybackStatus.Playing && dateTime.Between(StartTime, EndTime))
                return;

            Stop();
        }

        private void WaybackMachine_Tick(object sender, EventArgs e)
        {
            if (GUID == Guid.Empty)
            {
                Wayback wayback = InstantiateScript<Wayback>();

                wayback.Create(Utils.PlayerPed);

                Abort();

                return;
            }

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
                CurrentReplica.Apply(Ped, StartPlayGameTime, CurrentReplica);
                Status = WaybackStatus.Idle;
            }
            else
            {
                CurrentReplica.Apply(Ped, StartPlayGameTime, WaybackPedReplicas[ReplicaIndex]);

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
