using FusionLibrary;
using GTA;
using GTA.Math;
using System;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class Gauge
    {
        public AnimateProp Prop { get; }
        public float MaxRot { get; }
        public Vehicle Vehicle { get; }
        public Vector3 Offset { get; }
        public float Rotation { get; private set; }

        public bool On
        {
            get => on;

            set
            {
                on = value;
                rotate = true;
            }
        }

        private bool on;
        private bool rotate;

        public Gauge(Model modelName, Vector3 offset, float maxRot, Vehicle vehicle)
        {
            MaxRot = maxRot;
            Vehicle = vehicle;
            Offset = offset;

            Prop = new AnimateProp(modelName, Vehicle, Offset, Vector3.Zero);
            Prop.SpawnProp();
        }

        public void Tick()
        {
            if (!rotate)
            {
                return;
            }

            if (On && Rotation < MaxRot)
            {
                Rotation = FusionUtils.Lerp(Rotation, MaxRot, Game.LastFrameTime * 2f);

                float diff = Math.Abs(Rotation - MaxRot);
                if (diff <= 0.01)
                {
                    rotate = false;
                }
            }
            else if (!On && Rotation > 0)
            {
                Rotation = FusionUtils.Lerp(Rotation, 0, Game.LastFrameTime * 4f);

                float diff = Math.Abs(Rotation - 0);
                if (diff <= 0.01)
                {
                    rotate = false;
                }
            }

            Prop.MoveProp(Vector3.Zero, new Vector3(0, Rotation, 0), false);
        }

        public void Dispose()
        {
            Prop?.Dispose();
        }
    }

    internal class TFCHandler : HandlerPrimitive
    {
        private readonly Gauge _gaugeNeedle1;
        private readonly Gauge _gaugeNeedle2;
        private readonly Gauge _gaugeNeedle3;

        private int playAt;
        private bool hasPlayed;

        public TFCHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            _gaugeNeedle1 = new Gauge(ModelHandler.GaugeModels[0], new Vector3(0.2549649f, 0.4890909f, 0.6371477f), 25f, Vehicle);
            _gaugeNeedle2 = new Gauge(ModelHandler.GaugeModels[1], new Vector3(0.3632151f, 0.4841858f, 0.6369596f), 25f, Vehicle);
            _gaugeNeedle3 = new Gauge(ModelHandler.GaugeModels[2], new Vector3(0.509564f, 0.4745394f, 0.6380013f), 50f, Vehicle);

            Props.TFCOff.SpawnProp();
            Props.TFCOn.SpawnProp();
            Props.TFCOn.Visible = false;
            Props.GaugeGlow.SpawnProp();
            Props.GaugeGlow.Visible = false;

            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
        }

        private void OnTimeCircuitsToggle()
        {
            if (Properties.AreTimeCircuitsOn)
            {
                Props.TFCOn.Visible = true;
                Props.TFCOff.Visible = false;

                Props.TFCHandle?.Play();

                playAt = Game.GameTime + 2000;
            }
            else
            {
                Props.TFCOff.Visible = true;
                Props.TFCOn.Visible = false;

                Props.TFCHandle?.Play();

                _gaugeNeedle1.On = false;
                _gaugeNeedle2.On = false;
                _gaugeNeedle3.On = false;

                Props.GaugeGlow.Visible = false;
            }

            hasPlayed = false;
        }

        public override void KeyDown(KeyEventArgs e)
        {
        }

        public override void Tick()
        {
            if (!hasPlayed && Properties.AreTimeCircuitsOn && Game.GameTime > playAt)
            {
                if (Mods.Reactor == ReactorType.Nuclear)
                {
                    Sounds.PlutoniumGauge?.Play();
                }

                _gaugeNeedle1.On = true;
                _gaugeNeedle2.On = true;
                if (Properties.IsFueled)
                {
                    _gaugeNeedle3.On = true;
                }
                Props.TFCOff.Visible = false;
                Props.TFCOn.Visible = true;
                Props.GaugeGlow.Visible = true;

                hasPlayed = true;
            }
            if (hasPlayed && Properties.AreTimeCircuitsOn && !Properties.IsFueled && _gaugeNeedle3.On)
            {
                _gaugeNeedle3.On = false;
            }
            if (hasPlayed && Properties.AreTimeCircuitsOn && Properties.IsFueled && !_gaugeNeedle3.On)
            {
                _gaugeNeedle3.On = true;
            }

            _gaugeNeedle1.Tick();
            _gaugeNeedle2.Tick();
            _gaugeNeedle3.Tick();
        }

        public override void Stop()
        {
        }

        public override void Dispose()
        {
            _gaugeNeedle1.Dispose();
            _gaugeNeedle2.Dispose();
            _gaugeNeedle3.Dispose();
        }
    }
}
