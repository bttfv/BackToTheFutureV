using GTA;
using GTA.Math;
using BackToTheFutureV.Players;
using System.Windows.Forms;
using BackToTheFutureV.Utility;
using KlangRageAudioLibrary;
using GTA.Native;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class ReentryHandler : Handler
    {
        private AudioPlayer _reentryAudio;

        private readonly PtfxEntityPlayer _flash;

        private int _currentStep;
        private int _gameTimer;

        public bool IsReentering { get; private set; }

        public ReentryHandler(TimeCircuits circuits) : base(circuits)
        {
            LoadRes();

            _flash = new PtfxEntityPlayer("core", "ent_anim_paparazzi_flash", Vehicle, Vector3.Zero, Vector3.Zero, 50f);
        }

        public void LoadRes()
        {
            _reentryAudio =
                TimeCircuits.AudioEngine.Create($"{LowerCaseDeloreanType}/timeTravel/reentry.wav", Presets.ExteriorLoud);
        }

        public void StartReentering(bool noTimeSet = false)
        {
            IsReentering = true;
        }

        public override void Process()
        {
            if (!IsReentering) return;
            if (Game.GameTime < _gameTimer) return;

            // Time will be fixed to your destination time until reentry is completed.
            Utils.SetWorldTime(DestinationTime);

            switch (_currentStep)
            {
                case 0:
                    _reentryAudio.Play();

                    _flash.Play();

                    Function.Call(Hash.ADD_SHOCKING_EVENT_AT_POSITION, 88, Vehicle.Position.X, Vehicle.Position.Y, Vehicle.Position.Z, 1f);

                    var timeToAdd = 500;

                    switch (DeloreanType)
                    {
                        case DeloreanType.BTTF1:
                            timeToAdd = 100;
                            break;
                        case DeloreanType.BTTF2:
                        case DeloreanType.BTTF3:
                            timeToAdd = 600;
                            break;
                    }

                    _gameTimer = Game.GameTime + timeToAdd;
                    _currentStep++;
                    break;

                case 1:

                    _flash.Play();

                    timeToAdd = 500;

                    switch (DeloreanType)
                    {
                        case DeloreanType.BTTF1:
                            timeToAdd = 300;
                            break;
                        case DeloreanType.BTTF2:
                        case DeloreanType.BTTF3:
                            timeToAdd = 600;
                            break;
                    }

                    _gameTimer = Game.GameTime + timeToAdd;
                    _currentStep++;
                    break;

                case 2:

                    _flash.Play();

                    _currentStep++;
                    break;

                case 3:
                    Stop();

                    TimeCircuits?.OnReentryComplete?.Invoke();

                    break;
            }
        }

        public override void Stop()
        {
            _currentStep = 0;
            _gameTimer = 0;
            IsReentering = false;

            if (Mods.HoverUnderbody == ModState.On)
                TimeCircuits.GetHandler<FlyingHandler>().CanConvert = true;

            if (Mods.Hoodbox == ModState.On && !TimeCircuits.IsWarmedUp)
                TimeCircuits.GetHandler<HoodboxHandler>().SetInstant();

            if (Mods.Hook == HookState.On)
                Mods.Hook = HookState.Removed;

            if (Mods.Plate == PlateType.Outatime)
                Mods.Plate = PlateType.Empty;
        }

        public override void Dispose()
        {
            _reentryAudio?.Dispose();
        }

        public override void KeyPress(Keys key)
        {

        }
    }
}