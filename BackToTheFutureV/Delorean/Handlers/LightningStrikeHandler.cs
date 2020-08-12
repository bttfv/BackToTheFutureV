using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class LightningStrikeHandler : Handler
    {
        public bool HasBeenStruckByLightning { get; private set; }

        private int _nextCheck;
        private readonly AudioPlayer _lightningStrike;
        private readonly FlyingHandler _flyingHandler;

        private int _flashes;
        private int _nextFlash;
        private bool _isFlashing;

        private int _nextForce;

        public LightningStrikeHandler(TimeCircuits circuits) : base(circuits)
        {
            _lightningStrike = circuits.AudioEngine.Create("bttf2/timeTravel/lightingStrike.wav", Presets.Exterior);

            _flyingHandler = TimeCircuits.GetHandler<FlyingHandler>();

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
                        _isFlashing = false;
                        _flashes = 0;
                    }
                }

                if(Game.GameTime > _nextForce)
                {
                    Vehicle.ApplyForce(Vector3.RandomXYZ() * 3f, Vector3.RandomXYZ());

                    _nextForce = Game.GameTime + 100;
                }

                return;
            }

            if (Mods.HoverUnderbody == ModState.Off || !_flyingHandler.IsFlying || World.Weather != Weather.ThunderStorm || TimeCircuits.IsTimeTraveling || TimeCircuits.IsReentering || Game.GameTime < _nextCheck || Vehicle.HeightAboveGround < 20) return;

            if (Utils.Random.NextDouble() < 0.2)
                Strike();
            else
                _nextCheck = Game.GameTime + 10000;
        }

        private void Strike()
        {
            // Time travel by lightning strike
            _lightningStrike.Play();

            if (IsOn)
            {
                TimeCircuits.GetHandler<TimeTravelHandler>().StartTimeTravelling(true, 2000);

                DeloreanCopy deloreanCopy = TimeCircuits.Delorean.Copy;
                deloreanCopy.Circuits.DestinationTime.AddYears(70);
                RemoteDeloreansHandler.AddDelorean(deloreanCopy);
            }
            //else
            //    _flyingHandler.SetFlyMode(false);

            TimeCircuits.IsOn = false;
            TimeCircuits.OnTimeCircuitsToggle?.Invoke();

            Vehicle.EngineHealth -= 700;

            _flyingHandler.CanConvert = false;
            HasBeenStruckByLightning = true;
            _isFlashing = true;
            _flashes = 0;
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