using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using GTA.Math;
using System;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class Gauge
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

            Prop = new AnimateProp(Vehicle, modelName, Offset, Vector3.Zero);
            Prop.SpawnProp();
        }

        public void Process()
        {
            if (!rotate) return;

            if (On && Rotation < MaxRot)
            {
                Rotation = Utils.Lerp(Rotation, MaxRot, Game.LastFrameTime * 2f);

                var diff = Math.Abs(Rotation - MaxRot);
                if (diff <= 0.01)
                    rotate = false;
            }
            else if (!On && Rotation > 0)
            {
                Rotation = Utils.Lerp(Rotation, 0, Game.LastFrameTime * 4f);

                var diff = Math.Abs(Rotation - 0);
                if (diff <= 0.01)
                    rotate = false;
            }

            Prop.MoveProp(Vector3.Zero, new Vector3(0, Rotation, 0), false);
        }

        public void Dispose()
        {
            Prop?.Dispose();
        }
    }

    public class TFCHandler : Handler
    {
        private readonly Gauge _gaugeNeedle1;
        private readonly Gauge _gaugeNeedle2;
        private readonly Gauge _gaugeNeedle3;

        private int playAt;
        private bool hasPlayed;

        public TFCHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            _gaugeNeedle1 = new Gauge(ModelHandler.GaugeModels[1], new Vector3(0.2549649f, 0.4890909f, 0.6371477f), 25f, Vehicle);
            _gaugeNeedle2 = new Gauge(ModelHandler.GaugeModels[2], new Vector3(0.3632151f, 0.4841858f, 0.6369596f), 25f, Vehicle);
            _gaugeNeedle3 = new Gauge(ModelHandler.GaugeModels[3], new Vector3(0.509564f, 0.4745394f, 0.6380013f), 50f, Vehicle);

            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
        }   

        private void OnTimeCircuitsToggle()
        {
            if(Properties.AreTimeCircuitsOn)
            {
                Props.TFCOn?.SpawnProp();
                Props.TFCOff?.Delete();

                Props.TFCHandle?.Play();

                playAt = Game.GameTime + 2000;
            }
            else
            {
                Props.TFCOff?.SpawnProp();
                Props.TFCOn?.Delete();

                Props.TFCHandle?.Play();

                _gaugeNeedle1.On = false;
                _gaugeNeedle2.On = false;
                _gaugeNeedle3.On = false;

                Props.GaugeGlow?.Delete();
            }

            hasPlayed = false;
        }

        public override void KeyDown(Keys key)
        {
        }

        public override void Process()
        {
            if (!hasPlayed && Properties.AreTimeCircuitsOn && Game.GameTime > playAt)
            {
                if (Mods.Reactor == ReactorType.Nuclear)
                    Sounds.PlutoniumGauge?.Play();

                _gaugeNeedle1.On = true;
                _gaugeNeedle2.On = true;
                _gaugeNeedle3.On = true;

                Props.GaugeGlow?.SpawnProp();

                hasPlayed = true;
            }

            _gaugeNeedle1.Process();
            _gaugeNeedle2.Process();
            _gaugeNeedle3.Process();
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
