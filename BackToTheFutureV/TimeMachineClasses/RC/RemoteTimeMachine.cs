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

        public bool Spawned => TimeMachine != null && TimeMachine.Vehicle != null && TimeMachine.Vehicle.Exists();

        private int _timer;
        private bool _hasPlayedWarningSound;

        private readonly AudioPlayer _warningSound;

        public RemoteTimeMachine(TimeMachineClone timeMachineClone)
        {
            TimeMachineClone = timeMachineClone;

            _timer = Game.GameTime + 3000;

            _warningSound = Main.CommonAudioEngine.Create("general/rc/warning.wav", Presets.Exterior);            
            _warningSound.MinimumDistance = 0.5f;
            _warningSound.Volume = 0.5f;
        }

        public void Process()
        {
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

            if (Utils.GetWorldTime() > TimeMachineClone.Properties.DestinationTime && Utils.GetWorldTime() < (TimeMachineClone.Properties.DestinationTime + new TimeSpan(0, 0, 10)))
            {
                Reenter();

                _hasPlayedWarningSound = false;
                _timer = Game.GameTime + 3000;
            }
        }

        public bool ForceReenter()
        {
            if (Spawned)
                return false;

            TimeMachineClone.Properties.DestinationTime = Main.CurrentTime;

            return true;
        }

        public void Reenter()
        {
            Spawn();

            Utils.HideVehicle(TimeMachine.Vehicle, true);
            
            TimeMachine.Events.OnReenter?.Invoke();
        }

        private void Spawn()
        {
            if (Blip != null && Blip.Exists())
                Blip.Delete();

            TimeMachine = TimeMachineClone.Spawn();

            TimeMachine.LastDisplacementClone = TimeMachineClone;
        }

        public void ExistenceCheck(DateTime time)
        {
            if (TimeMachineClone.Properties.DestinationTime > time)
            {
                if (TimeMachine != null && TimeMachine.Vehicle.Exists() && Main.PlayerVehicle != TimeMachine.Vehicle)
                {
                    TimeMachineHandler.RemoveTimeMachine(TimeMachine);
                    TimeMachine = null;
                }
            }
            else if (TimeMachineClone.Properties.DestinationTime < time)
            {
                if (TimeMachine == null || !TimeMachine.Vehicle.Exists())
                    Spawn();
            }
        }

        public void Dispose()
        {
            _warningSound?.Dispose();         
        }
    }
}