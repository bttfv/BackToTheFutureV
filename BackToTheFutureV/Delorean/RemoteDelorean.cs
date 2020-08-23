using System;
using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Utility;
using GTA;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean
{    
    public class RemoteDelorean
    {
        public DeloreanCopy DeloreanCopy { get; }

        public Blip Blip;

        private DeloreanTimeMachine DeloreanSpawned;

        private int _timer;
        private bool _hasPlayedWarningSound;

        private readonly AudioPlayer _warningSound;

        public RemoteDelorean(DeloreanCopy deloreanCopy)
        {
            DeloreanCopy = deloreanCopy;

            _timer = Game.GameTime + 3000;

            _warningSound = Main.CommonAudioEngine.Create("general/rc/warning.wav", Presets.Exterior);            
            _warningSound.MinimumDistance = 0.5f;
            _warningSound.Volume = 0.5f;
        }

        public void Process()
        {
            if (Game.GameTime < _timer) 
                return;

            if(Utils.GetWorldTime() > (DeloreanCopy.Circuits.DestinationTime - new TimeSpan(0, 0, 10)) && Utils.GetWorldTime() < DeloreanCopy.Circuits.DestinationTime)
            {
                if(DeloreanCopy.Circuits.IsRemoteControlled && !_hasPlayedWarningSound)
                {
                    _warningSound.SourceEntity = Main.PlayerPed;
                    _warningSound.Play();
                    _hasPlayedWarningSound = true;
                }

                ModelHandler.DMC12.Request();
            }

            if (Utils.GetWorldTime() > DeloreanCopy.Circuits.DestinationTime && Utils.GetWorldTime() < (DeloreanCopy.Circuits.DestinationTime + new TimeSpan(0, 0, 10)))
            {
                Reenter();

                _hasPlayedWarningSound = false;
                _timer = Game.GameTime + 3000;
            }
        }

        public void Reenter()
        {
            Spawn();

            Utils.HideVehicle(DeloreanSpawned.Vehicle, true);

            DeloreanSpawned.Circuits.GetHandler<TimeTravelHandler>().Reenter();
        }

        private void Spawn()
        {
            if (Blip != null && Blip.Exists())
                Blip.Delete();

            DeloreanSpawned = DeloreanCopy.Spawn();

            DeloreanSpawned.LastDisplacementCopy = DeloreanCopy;
        }

        public void ExistenceCheck(DateTime time)
        {
            if (DeloreanCopy.Circuits.DestinationTime > time)
            {
                if (DeloreanSpawned != null && DeloreanSpawned.Vehicle.Exists() && Main.PlayerVehicle != DeloreanSpawned.Vehicle)
                {
                    DeloreanHandler.RemoveDelorean(DeloreanSpawned);
                    DeloreanSpawned = null;
                }
            }
            else if (DeloreanCopy.Circuits.DestinationTime < time)
            {
                if (DeloreanSpawned == null || !DeloreanSpawned.Vehicle.Exists())
                    Spawn();
            }
        }

        public void Dispose()
        {
            _warningSound?.Dispose();         
        }
    }
}