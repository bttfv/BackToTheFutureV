using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class LightningStrikeHandler : Handler
    {
        public bool HasBeenStruckByLightning { get; set; }

        private int _nextCheck;
        private readonly AudioPlayer _lightningStrike;
        private FlyingHandler _flyingHandler => TimeCircuits?.GetHandler<FlyingHandler>();

        private int _flashes;
        private int _nextFlash;
        private bool _isFlashing;

        private int _nextForce;

        public LightningStrikeHandler(TimeCircuits circuits) : base(circuits)
        {
            _lightningStrike = circuits.AudioEngine.Create("bttf2/timeTravel/lightingStrike.wav", Presets.Exterior);

            TimeCircuits.OnTimeTravelComplete += OnTimeTravelComplete;
        }

        private void OnTimeTravelComplete()
        {
            if(HasBeenStruckByLightning)
            {
                //_flyingHandler.SetFlyMode(false, true);
            }
        }

        public override void Process()
        {
            if (HasBeenStruckByLightning && !Main.PlayerPed.IsInVehicle())
            {
                var dist = Main.PlayerPed.Position.DistanceToSquared(Vehicle.Bones["bonnet"].Position);

                if (!(dist <= 2f * 2f))
                    return;

                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Repair_TimeCircuits"));

                if (Game.IsControlJustPressed(GTA.Control.Context))
                {
                    Mods.Hoodbox = ModState.On;

                    TimeCircuits.IsOn = false;
                    TimeCircuits.OnTimeCircuitsToggle?.Invoke();                    
                }                    
            }

            if (HasBeenStruckByLightning && _isFlashing && _flashes <= 3)
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
                        if (Mods.Hook == HookState.On && !_flyingHandler.IsFlying)
                            HasBeenStruckByLightning = false;

                        _isFlashing = false;
                        _flashes = 0;
                    }
                }

                if(_flyingHandler.IsFlying && Game.GameTime > _nextForce)
                {
                    Vehicle.ApplyForce(Vector3.RandomXYZ() * 3f, Vector3.RandomXYZ());

                    _nextForce = Game.GameTime + 100;
                }

                return;
            }

            if (World.Weather != Weather.ThunderStorm || TimeCircuits.IsTimeTraveling || TimeCircuits.IsReentering || Game.GameTime < _nextCheck) return;
          
            if ((Mods.Hook == HookState.On && Vehicle.GetMPHSpeed() >= 88) | (Vehicle.HeightAboveGround >= 20 && _flyingHandler.IsFlying)) 
            {
                if (Utils.Random.NextDouble() < 0.2)
                    Strike();
                else
                    _nextCheck = Game.GameTime + 10000;
            }
        }

        private void Strike()
        {
            // Time travel by lightning strike
            _lightningStrike.Play();

            if (IsOn)
            {
                if (Mods.Hook == HookState.On && !_flyingHandler.IsFlying)
                {
                    TimeCircuits.GetHandler<TimeTravelHandler>().StartTimeTravelling(false, 700);
                    _flashes = 2;
                }
                else
                {
                    TimeCircuits.GetHandler<TimeTravelHandler>().StartTimeTravelling(true, 2000);
                    _flashes = 0;

                    TimeCircuits.IsOn = false;
                    TimeCircuits.OnTimeCircuitsToggle?.Invoke();

                    _flyingHandler.FlyingCircuitsBroken = true;
                }
                
                DeloreanCopy deloreanCopy = TimeCircuits.Delorean.Copy;
                deloreanCopy.Circuits.DestinationTime = deloreanCopy.Circuits.DestinationTime.AddYears(70);
                RemoteDeloreansHandler.AddDelorean(deloreanCopy);
            }
            else
                _flyingHandler.SetFlyMode(false);

            Vehicle.EngineHealth -= 700;
            
            HasBeenStruckByLightning = true;
            _isFlashing = true;            
        }

        public override void KeyPress(Keys key)
        {

        }

        public override void Stop()
        {
            HasBeenStruckByLightning = false;
        }

        public override void Dispose()
        {

        }
    }
}