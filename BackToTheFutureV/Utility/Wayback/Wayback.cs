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
        public List<WaybackMachineReplica> WaybackMachineReplicas { get; } = new List<WaybackMachineReplica>();
        public List<WaybackPedReplica> WaybackPedReplicas { get; } = new List<WaybackPedReplica>();

        public Guid GUID { get; private set; } = Guid.Empty;
        public Guid ReplicaGUID { get; private set; } = Guid.Empty;

        public Ped Ped { get; set; }

        private TimeMachine timeMachine;
        public TimeMachine TimeMachine
        {
            get => timeMachine;
            set
            {
                timeMachine = value;
                timeMachine.WaybackMachine = this;

                Ped = timeMachine.Vehicle.Driver;
            }
        }

        public WaybackMachineReplica StartMachineReplica => WaybackMachineReplicas.FirstOrDefault(x => x.Time >= Utils.CurrentTime);

        public int ReplicaIndex { get; private set; } = -1;

        public WaybackMachineReplica CurrentMachineReplica => WaybackMachineReplicas[ReplicaIndex];
        public WaybackPedReplica CurrentPedReplica => WaybackPedReplicas[ReplicaIndex];

        public int StartGameTime { get; private set; }
        public int StartPlayGameTime { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public bool IsRecording { get; private set; } = true;

        public bool IsPlaying { get; private set; } = false;

        public Wayback()
        {
            Tick += WaybackMachine_Tick;
        }

        public void Create(TimeMachine timeMachine)
        {
            ReplicaGUID = Guid.NewGuid();

            GUID = timeMachine.Properties.GUID;
            TimeMachine = timeMachine;
            Ped = timeMachine.Vehicle.Driver;
            StartGameTime = Game.GameTime;

            WaybackMachineReplicas.Add(new WaybackMachineReplica(TimeMachine, StartGameTime));

            StartTime = WaybackMachineReplicas.First().Time;

            TimeMachine.Events.OnSparksEnded += OnSparksEnded;
            TimeMachine.Events.SetOpenCloseReactor += SetOpenCloseReactor;
            TimeMachine.Events.SetRefuel += SetRefuel;

            WaybackHandler.WaybackMachines.Add(this);
        }

        private void OnSparksEnded(int delay = 0)
        {
            WaybackMachineReplica waybackMachineReplica = Record();

            waybackMachineReplica.Event = WaybackEvent.OnSparksEnded;
            waybackMachineReplica.TimeTravelDelay = delay;
        }

        private void SetOpenCloseReactor()
        {
            WaybackMachineReplica waybackMachineReplica = Record();

            waybackMachineReplica.Event = WaybackEvent.OpenCloseReactor;
        }

        private void SetRefuel(Ped ped)
        {
            WaybackMachineReplica waybackMachineReplica = Record();

            waybackMachineReplica.Event = WaybackEvent.RefuelReactor;
        }

        private void WaybackMachine_Tick(object sender, EventArgs e)
        {
            if (GUID == Guid.Empty)
                Abort();

            if (Utils.CurrentTime < StartTime)
            {
                if (IsRecording)
                    Stop();

                return;
            }

            if (IsRecording)
                Record();
            else
                Play();
        }

        private WaybackMachineReplica Record()
        {
            if (!TimeMachine.NotNullAndExists())
            {
                Stop();

                return null;
            }

            WaybackMachineReplica waybackMachineReplica;

            WaybackMachineReplicas.Add(waybackMachineReplica = new WaybackMachineReplica(TimeMachine, StartGameTime));

            if (Ped.NotNullAndExists())
                WaybackPedReplicas.Add(new WaybackPedReplica(Ped, StartGameTime));

            return waybackMachineReplica;
        }

        private void Play()
        {
            if (!TimeMachine.NotNullAndExists())
                return;

            if (!IsPlaying)
            {
                WaybackMachineReplica waybackReplica = StartMachineReplica;

                if (waybackReplica == default)
                    return;

                ReplicaIndex = WaybackMachineReplicas.IndexOf(waybackReplica);
                StartPlayGameTime = Game.GameTime;
                IsPlaying = true;
            }

            if (ReplicaIndex >= WaybackMachineReplicas.Count - 1)
            {
                CurrentMachineReplica.Apply(TimeMachine, StartPlayGameTime, CurrentMachineReplica);

                if (Ped.NotNullAndExists() && ReplicaIndex < WaybackPedReplicas.Count)
                    CurrentPedReplica.Apply(Ped, StartPlayGameTime, CurrentPedReplica);

                IsPlaying = false;
            }
            else
            {
                ReplicaIndex++;

                CurrentMachineReplica.Apply(TimeMachine, StartPlayGameTime, WaybackMachineReplicas[ReplicaIndex]);

                if (Ped.NotNullAndExists() && ReplicaIndex < WaybackPedReplicas.Count)
                    CurrentPedReplica.Apply(Ped, StartPlayGameTime, WaybackPedReplicas[ReplicaIndex]);
            }
        }

        public void Stop()
        {
            if (timeMachine != null)
                timeMachine.WaybackMachine = null;

            if (IsRecording)
                EndTime = WaybackMachineReplicas.Last().Time;

            timeMachine = null;
            IsRecording = false;
        }
    }
}
