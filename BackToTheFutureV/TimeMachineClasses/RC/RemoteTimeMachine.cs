using System;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using GTA;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.TimeMachineClasses.RC
{    
    public class RemoteTimeMachine
    {
        public TimeMachineClone TimeMachineClone { get; }

        public TimeMachine TimeMachine { get; private set; }

        public Blip Blip;

        public bool Spawned => TimeMachineHandler.Exists(TimeMachine);

        private int _timer;
        private bool _hasPlayedWarningSound;

        private readonly AudioPlayer _warningSound;

        public RemoteTimeMachine(TimeMachineClone timeMachineClone)
        {
            TimeMachineClone = timeMachineClone;

            TimeMachineClone.Properties.TimeTravelType = TimeTravelType.RC;

            TimeMachineClone.Properties.TimeTravelPhase = TimeTravelPhase.Completed;

            _timer = Game.GameTime + 3000;

            _warningSound = Main.CommonAudioEngine.Create("general/rc/warning.wav", Presets.Exterior);            
            _warningSound.MinimumDistance = 0.5f;
            _warningSound.Volume = 0.5f;
        }

        public void Process()
        {            
            if (!Spawned && TimeMachine != null)
                TimeMachine = null;

            if (Game.GameTime < _timer) 
                return;

            if(Utils.GetWorldTime() > (TimeMachineClone.Properties.DestinationTime - new TimeSpan(0, 0, 10)) && Utils.GetWorldTime() < TimeMachineClone.Properties.DestinationTime)
            {
                if(TimeMachineClone.Properties.IsRemoteControlled && !_hasPlayedWarningSound)
                {
                    _warningSound.SourceEntity = Main.PlayerPed;
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

        public void Spawn(ReenterType reenterType)
        {
            if (Blip != null && Blip.Exists())
                Blip.Delete();

            switch (reenterType)
            {
                case ReenterType.Normal:
                    TimeMachine = TimeMachineClone.Spawn(SpawnFlags.ForceReentry);
                    TimeMachine.LastDisplacementClone = TimeMachineClone;                    
                    break;
                case ReenterType.Spawn:
                    TimeMachine = TimeMachineClone.Spawn(SpawnFlags.NoVelocity);
                    TimeMachine.LastDisplacementClone = TimeMachineClone;

                    if (!TimeMachine.Properties.HasBeenStruckByLightning && TimeMachine.Mods.IsDMC12)
                        TimeMachine.Properties.IsFueled = false;

                    TimeMachine.Events.OnVehicleSpawned?.Invoke();
                    break;
                case ReenterType.Forced:
                    TimeMachineClone.Properties.DestinationTime = Main.CurrentTime;
                    break;
            }
        }

        public void ExistenceCheck(DateTime time)
        {
            if (TimeMachineClone.Properties.DestinationTime < time && !Spawned)
                Spawn(ReenterType.Spawn);
        }

        public void Dispose()
        {
            if (Blip != null && Blip.Exists())
                Blip.Delete();

            _warningSound?.Dispose();         
        }

        public override string ToString()
        {
            return RemoteTimeMachineHandler.RemoteTimeMachines.IndexOf(this).ToString();
        }
    }
}