using System.Windows.Forms;
using System.Drawing;
using BackToTheFutureV.GUI;
using GTA;
using GTA.Math;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using Screen = GTA.UI.Screen;
using BackToTheFutureV.Settings;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class SpeedoHandler : Handler
    {
        private readonly RenderTarget _speedoRt;
        private readonly ScaleformGui _speedoRtScaleform;
        private readonly AnimateProp speedoProp;
        private readonly AnimateProp _compass;

        private readonly AudioPlayer _speedBeep;

        private int nextCheck;
        private int currentSpeed;

        public SpeedoHandler(TimeCircuits circuits) : base(circuits)
        {
            //speedoProp = new AnimateProp(Vehicle, ModelHandler.BTTFSpeedo, new Vector3(-0.2543122f, 0.3725779f, 0.7174588f), Vector3.Zero);
            _compass = new AnimateProp(Vehicle, ModelHandler.Compass, "bttf_compass");
            speedoProp = new AnimateProp(Vehicle, ModelHandler.BTTFSpeedo, "bttf_speedo");
            _speedoRt = new RenderTarget(ModelHandler.BTTFSpeedo, "bttf_speedo");
            _speedoRtScaleform = new ScaleformGui("bttf_3d_speedo")
            {
                DrawInPauseMenu = true
            };

            _speedBeep = circuits.AudioEngine.Create("general/speedoTick.wav", Presets.Interior);
            _speedBeep.Volume = 1f;
            _speedBeep.MinimumDistance = 0.3f;
            _speedBeep.SourceBone = "bttf_speedo";

            _speedoRt.OnRenderTargetDraw += () =>
            {
                var aspectRatio = Screen.Resolution.Width / (float)Screen.Resolution.Height;

                _speedoRtScaleform.Render2D(new PointF(0.5f, 0.5f), new SizeF(0.9f, 0.9f));
            };

            speedoProp.SpawnProp();

            UpdateGUI(GUI, "8", "8");
        }

        public override void Dispose()
        {
            speedoProp?.Dispose();
            _compass?.Dispose();
        }

        public override void KeyPress(Keys key)
        {
        }

        public override void Process()
        {
            if (!TimeCircuits.Delorean.IsGivenScaleformPriority || Main.PlayerPed.Position.DistanceToSquared(Vehicle.Position) > 6f * 6f)
                return;

            _compass?.SpawnProp(Vector3.Zero, new Vector3(0, 0, Vehicle.Heading), false);

            if (_compass.Prop.IsVisible != Vehicle.IsVisible)
                _compass.Prop.IsVisible = Vehicle.IsVisible;

            _speedoRt.Draw();

            if (Game.GameTime < nextCheck) return;

            if (!(TimeCircuits.IsReentering || TimeCircuits.IsTimeTraveling))
            {
                int mphSpeed = ((int) MPHSpeed);

                if (mphSpeed > 88)
                    mphSpeed = 88;

                string mphSpeedStr = mphSpeed.ToString("00");

                string speedDigit1 = mphSpeedStr.Substring(0, 1);
                string speedDigit2 = mphSpeedStr.Substring(1, 1);

                if (mphSpeed > currentSpeed || mphSpeed < currentSpeed)
                {
                    currentSpeed = mphSpeed;

                    if (ModSettings.PlaySpeedoBeep && Vehicle.IsVisible)
                        _speedBeep.Play();

                    UpdateGUI(_speedoRtScaleform, speedDigit1, speedDigit2);

                    if (!TcdEditer.IsEditing)
                        UpdateGUI(GUI, speedDigit1, speedDigit2);
                }
            }

            nextCheck = Game.GameTime + 20;
        }

        private void UpdateGUI(ScaleformGui scaleform, string digit1, string digit2)
        {
            if (digit1 != "0")
            {
                scaleform.CallFunction("SET_DIGIT_1", int.Parse(digit1));
            }
            else
            {
                scaleform.CallFunction("SET_DIGIT_1", 15);
            }

            scaleform.CallFunction("SET_DIGIT_2", int.Parse(digit2));
        }

        public override void Stop()
        {
        }
    }
}