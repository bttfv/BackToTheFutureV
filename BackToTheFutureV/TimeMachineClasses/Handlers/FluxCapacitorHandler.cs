using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Drawing;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal class FluxCapacitorHandler : HandlerPrimitive
    {
        private LightHandler FluxBlueLight;

        public FluxCapacitorHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            FluxBlueLight = new LightHandler(TimeMachine, TimeMachineHandler.TimeMachineCount + 1);
            FluxBlueLight.Add("flux_capacitor", "windscreen", Color.FromArgb(118, 147, 230), 10, 5, 0, 45, 100);
            FluxBlueLight.Add("windscreen", "flux_capacitor", Color.FromArgb(118, 147, 230), 10, 10, 0, 6, 0);

            Vector3 pos = Vehicle.Bones["flux_capacitor"].RelativePosition;
            Vector3 dir = pos.GetDirectionTo(new Vector3(-0.03805999f, -0.0819466f, 0.5508024f));

            FluxBlueLight.Add(pos, dir, Color.FromArgb(118, 147, 230), 10, 20, 0, 90, 100);

            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            Events.OnScaleformPriority += OnScaleformPriority;

            Events.OnWormholeStarted += StartTimeTravelEffect;

            Events.OnTimeTravelStarted += StartNormalFluxing;
            Events.OnSparksInterrupted += StartNormalFluxing;
        }

        public void StartTimeTravelEffect()
        {
            ScaleformsHandler.FluxCapacitor.CallFunction("START_BLUE_ANIMATION");
            Properties.IsFluxDoingBlueAnim = true;
        }

        public void StartNormalFluxing()
        {
            ScaleformsHandler.FluxCapacitor.CallFunction("START_ANIMATION");
            Props.FluxBlue?.Delete();
            Properties.IsFluxDoingBlueAnim = false;
        }

        private void OnScaleformPriority()
        {
            if (!Constants.HasScaleformPriority)
                return;

            Update();
        }

        private void OnTimeCircuitsToggle()
        {
            if (Constants.HasScaleformPriority)
                Update();
        }

        public override void Tick()
        {
            if (!Properties.AreTimeCircuitsOn)
                return;

            if (ModSettings.PlayFluxCapacitorSound)
            {
                if (!Vehicle.IsVisible && Sounds.FluxCapacitor.IsAnyInstancePlaying)
                    Sounds.FluxCapacitor?.Stop();

                if (!Sounds.FluxCapacitor.IsAnyInstancePlaying && Vehicle.IsVisible)
                    Sounds.FluxCapacitor?.Play();
            }
            else if (Sounds.FluxCapacitor.IsAnyInstancePlaying)
                Sounds.FluxCapacitor?.Stop();

            if (!Vehicle.IsVisible)
                return;

            if (Constants.HasScaleformPriority)
                Scaleforms.FluxCapacitorRT?.Draw();

            if (Properties.IsFluxDoingBlueAnim)
                FluxBlueLight.Draw();

            if (Properties.IsFluxDoingBlueAnim && !Props.FluxBlue.IsSpawned)
                Props.FluxBlue.SpawnProp();

            if (!Properties.IsFluxDoingBlueAnim && Props.FluxBlue.IsSpawned)
                Props.FluxBlue?.Delete();
        }

        public void Update()
        {
            if (!Properties.AreTimeCircuitsOn)
            {
                ScaleformsHandler.FluxCapacitor.CallFunction("STOP_ANIMATION");
                Props.FluxBlue?.Delete();

                if (Sounds.FluxCapacitor.IsAnyInstancePlaying)
                    Sounds.FluxCapacitor?.Stop();
            }
            else
            {
                if (ModSettings.PlayFluxCapacitorSound && !Sounds.FluxCapacitor.IsAnyInstancePlaying)
                    Sounds.FluxCapacitor?.Play();

                StartNormalFluxing();
            }

            Properties.IsFluxDoingBlueAnim = false;
            Properties.PhotoFluxCapacitorActive = false;
        }

        public override void Stop()
        {

        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {

        }
    }
}
