using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using KlangRageAudioLibrary;
using BackToTheFutureV.Entities;
using GTA.Native;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class LightningStrikeHandler : Handler
    {
        private int _nextCheck;
        private readonly AudioPlayer _lightningStrike;
        
        private int _flashes;
        private int _nextFlash;

        private int _nextForce;

        private bool _hasBeenStruckByLightning;

        public LightningStrikeHandler(TimeCircuits circuits) : base(circuits)
        {
            _lightningStrike = circuits.AudioEngine.Create("bttf2/timeTravel/lightingStrike.wav", Presets.Exterior);
        }

        public override void Process()
        {
            if (TimeCircuitsBroken && !Main.PlayerPed.IsInVehicle())
            {
                var dist = Main.PlayerPed.Position.DistanceToSquared(Vehicle.Bones["bonnet"].Position);

                if (!(dist <= 2f * 2f))
                    return;

                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Repair_TimeCircuits"));

                if (Game.IsControlJustPressed(GTA.Control.Context))
                    Mods.Hoodbox = ModState.On;
            }

            if (_hasBeenStruckByLightning && _flashes <= 3)
            {
                if(Game.GameTime > _nextFlash)
                {
                    // Flash the screen
                    ScreenFlash.FlashScreen(0.25f);

                    // Update _flashes count
                    _flashes++;

                    // Update next flash
                    _nextFlash = Game.GameTime + 650;

                    // Dont do it anymore if flashed enough times
                    if (_flashes > 3)
                    {                        
                        _flashes = 0;
                        _hasBeenStruckByLightning = false;
                    }
                }

                if(IsFlying && Game.GameTime > _nextForce)
                {
                    Vehicle.ApplyForce(Vector3.RandomXYZ() * 3f, Vector3.RandomXYZ());

                    _nextForce = Game.GameTime + 100;
                }

                return;
            }

            if (!ModSettings.LightningStrikeEvent || World.Weather != Weather.ThunderStorm || TimeCircuits.IsTimeTraveling || TimeCircuits.IsReentering || Game.GameTime < _nextCheck) return;
          
            if ((Mods.Hook == HookState.On && Vehicle.GetMPHSpeed() >= 88 && !IsFlying) | (Vehicle.HeightAboveGround >= 20 && IsFlying)) 
            {
                if (Utils.Random.NextDouble() < 0.2)
                    Strike();
                else
                    _nextCheck = Game.GameTime + 10000;
            }
        }

        private void Strike()
        {            
            if (IsOn)
            {
                // Time travel by lightning strike
                _lightningStrike.Play();

                if (Mods.Hook == HookState.On && !IsFlying)
                {
                    TimeCircuits.GetHandler<TimeTravelHandler>().StartTimeTravelling(false, 700);
                    _flashes = 2;
                }
                else
                {
                    TimeCircuits.GetHandler<TimeTravelHandler>().StartTimeTravelling(true, 2000);
                    _flashes = 0;

                    TimeCircuits.SetTimeCircuitsBroken(true);
                }
                
                DeloreanCopy deloreanCopy = TimeCircuits.Delorean.Copy;
                deloreanCopy.Circuits.DestinationTime = deloreanCopy.Circuits.DestinationTime.AddYears(70);
                RemoteDeloreansHandler.AddDelorean(deloreanCopy);
            }
            else
                Function.Call(Hash.FORCE_LIGHTNING_FLASH);

            if (!IsFlying && !IsOn)
                TimeCircuits.SetTimeCircuitsBroken(true);

            if (IsFlying)
                FlyingCircuitsBroken = true;

            Vehicle.EngineHealth -= 700;

            _hasBeenStruckByLightning = true;
            _nextCheck = Game.GameTime + 60000;
        }

        public override void KeyPress(Keys key)
        {

        }

        public override void Stop()
        {
            
        }

        public override void Dispose()
        {

        }
    }
}