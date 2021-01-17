using BackToTheFutureV.Utility;
using FusionLibrary;
using GTA;
using KlangRageAudioLibrary;
using System;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.RC
{
    public class RemoteTimeMachine
    {
        public TimeMachineClone TimeMachineClone { get; }
        public TimeMachine TimeMachine { get; private set; }
        public bool Reentry { get; } = true;
        public Blip Blip { get; set; }
        public bool Spawned => TimeMachineHandler.Exists(TimeMachine);

        private int _timer;
        private bool _hasPlayedWarningSound;

        private readonly AudioPlayer _warningSound;

        private bool blockSpawn = true;        
        public WaybackMachine WaybackMachine { get; }
        
        public RemoteTimeMachine(TimeMachineClone timeMachineClone)
        {
            TimeMachineClone = timeMachineClone;

            TimeMachineClone.Properties.TimeTravelType = TimeTravelType.RC;

            TimeMachineClone.Properties.TimeTravelPhase = TimeTravelPhase.Completed;

            _timer = Game.GameTime + 3000;

            _warningSound = Main.CommonAudioEngine.Create("general/rc/warning.wav", Presets.Exterior);            
            _warningSound.MinimumDistance = 0.5f;
            _warningSound.Volume = 0.5f;

            TimeHandler.OnTimeChanged += OnTimeChanged;
        }

        public RemoteTimeMachine(TimeMachineClone timeMachineClone, WaybackMachine waybackMachine) : this(timeMachineClone)
        {
            WaybackMachine = waybackMachine;
            Reentry = false;
        }

        public void OnTimeChanged(DateTime time)
        {
            if (!Reentry)
                blockSpawn = false;
        }

        public void Process()
        {            
            if (!Spawned && TimeMachine != null)
                TimeMachine = null;

            if (Game.GameTime < _timer) 
                return;

            if (!Reentry)
            {
                if (!blockSpawn && (Utils.CurrentTime - WaybackMachine.StartTime).Duration() < TimeSpan.FromMinutes(1))
                    WaybackMachine.TimeMachine = Spawn(ReenterType.Spawn);

                return;
            }

            if(Utils.GetWorldTime() > (TimeMachineClone.Properties.DestinationTime - new TimeSpan(0, 0, 10)) && Utils.GetWorldTime() < TimeMachineClone.Properties.DestinationTime)
            {
                if(TimeMachineClone.Properties.IsRemoteControlled && !_hasPlayedWarningSound)
                {
                    _warningSound.SourceEntity = Utils.PlayerPed;
                    _warningSound.Play();
                    _hasPlayedWarningSound = true;
                }

                ModelHandler.DMC12.Request();
            }

            if (!Spawned && Utils.GetWorldTime() > TimeMachineClone.Properties.DestinationTime && Utils.GetWorldTime() < (TimeMachineClone.Properties.DestinationTime + new TimeSpan(0, 0, 10)))
            {
                Spawn(ReenterType.Normal);

                _hasPlayedWarningSound = false;
                _timer = Game.GameTime + 3000;
            }
        }

        public TimeMachine Spawn(ReenterType reenterType)
        {
            if (Blip != null && Blip.Exists())
                Blip.Delete();

            if (!Reentry)
                blockSpawn = true;

            switch (reenterType)
            {
                case ReenterType.Normal:
                    TimeMachine = TimeMachineClone.Spawn(SpawnFlags.ForceReentry);
                    TimeMachine.LastDisplacementClone = TimeMachineClone;

                    return TimeMachine;
                case ReenterType.Spawn:
                    TimeMachine = TimeMachineClone.Spawn(SpawnFlags.NoVelocity);
                    TimeMachine.LastDisplacementClone = TimeMachineClone;

                    if (!TimeMachine.Properties.HasBeenStruckByLightning && TimeMachine.Mods.IsDMC12)
                        TimeMachine.Properties.IsFueled = false;

                    TimeMachine.Events.OnVehicleSpawned?.Invoke();
                    return TimeMachine;
                case ReenterType.Forced:
                    TimeMachineClone.Properties.DestinationTime = Utils.CurrentTime;
                    break;
            }

            return null;
        }

        public void ExistenceCheck(DateTime time)
        {
            if (TimeMachineClone.Properties.DestinationTime < time && !Spawned)
                Spawn(ReenterType.Spawn);
        }

        public void Dispose()
        {
            Blip?.Delete();

            _warningSound?.Dispose();         
        }

        public override string ToString()
        {
            return RemoteTimeMachineHandler.RemoteTimeMachinesOnlyReentry.IndexOf(this).ToString();
        }
    }
}