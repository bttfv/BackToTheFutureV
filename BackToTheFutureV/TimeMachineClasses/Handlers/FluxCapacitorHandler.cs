using FusionLibrary;
using GTA;
using System.Drawing;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class FluxCapacitorHandler : HandlerPrimitive
    {
        private readonly LightHandler FluxLights;

        public FluxCapacitorHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            FluxLights = new LightHandler(TimeMachine, TimeMachineHandler.TimeMachineCount + 1);
            FluxLights.Add("flux_capacitor", "windscreen", Color.FromArgb(118, 147, 230), 10, 5, 0, 45, 100);
            FluxLights.Add("windscreen", "flux_capacitor", Color.FromArgb(118, 147, 230), 10, 10, 0, 6, 0);

            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            Events.OnScaleformPriority += OnScaleformPriority;
            Events.OnWormholeTypeChanged += OnWormholeTypeChanged;
            Events.OnWormholeStarted += StartTimeTravelEffect;

            Events.OnTimeTravelStarted += StartNormalFluxing;
            Events.OnSparksInterrupted += StartNormalFluxing;

            OnWormholeTypeChanged();
        }

        public void StartTimeTravelEffect()
        {
            if (!Properties.IsFueled && !Properties.PhotoFluxCapacitorActive)
                return;

            if (Mods.WormholeType == WormholeType.BTTF3)
            {
                ScaleformsHandler.FluxCapacitor.CallFunction("START_ORANGE_ANIMATION");
            }
            else
            {
                ScaleformsHandler.FluxCapacitor.CallFunction("START_BLUE_ANIMATION");
            }

            Properties.IsFluxDoingAnim = true;
        }

        public void StartNormalFluxing()
        {
            ScaleformsHandler.FluxCapacitor.CallFunction("START_ANIMATION");
            Props.FluxGlow?.Delete();
            Properties.IsFluxDoingAnim = false;
        }

        private void OnScaleformPriority()
        {
            if (!Constants.HasScaleformPriority)
            {
                return;
            }

            Update();
        }

        private void OnTimeCircuitsToggle(bool instant = false)
        {
            if (Constants.HasScaleformPriority)
            {
                Update();
            }
        }

        private void OnWormholeTypeChanged()
        {
            if (Mods.WormholeType != WormholeType.BTTF3)
            {
                Props.FluxGlow.SwapModel(ModelHandler.FluxBlueModel);

                for (int i = 0; i < FluxLights.Lights.Count; i++)
                    FluxLights.Lights[i].Color = Color.FromArgb(118, 147, 230);
            }
            else
            {
                Props.FluxGlow.SwapModel(ModelHandler.FluxOrangeModel);

                for (int i = 0; i < FluxLights.Lights.Count; i++)
                    FluxLights.Lights[i].Color = Color.FromArgb(232, 196, 190);
            }
        }

        public override void Tick()
        {
            if (!Properties.AreTimeCircuitsOn)
            {
                return;
            }

            if (ModSettings.PlayFluxCapacitorSound)
            {
                if (!Vehicle.IsVisible && Sounds.FluxCapacitor.IsAnyInstancePlaying)
                {
                    Sounds.FluxCapacitor?.Stop();
                }

                if (!Sounds.FluxCapacitor.IsAnyInstancePlaying && Vehicle.IsVisible)
                {
                    Sounds.FluxCapacitor?.Play();
                }
            }
            else if (Sounds.FluxCapacitor.IsAnyInstancePlaying)
            {
                Sounds.FluxCapacitor?.Stop();
            }

            if (!Vehicle.IsVisible)
            {
                return;
            }

            if (Constants.HasScaleformPriority)
            {
                Scaleforms.FluxCapacitorRT?.Draw();
            }

            if (Properties.IsFluxDoingAnim)
            {
                FluxLights.Draw();

                if (!Props.FluxGlow.IsSpawned)
                    Props.FluxGlow.SpawnProp();
            }
            else if (Props.FluxGlow.IsSpawned)
            {
                Props.FluxGlow?.Delete();
            }
        }

        public void Update()
        {
            if (!Properties.AreTimeCircuitsOn)
            {
                ScaleformsHandler.FluxCapacitor.CallFunction("STOP_ANIMATION");
                Props.FluxGlow?.Delete();

                if (Sounds.FluxCapacitor.IsAnyInstancePlaying)
                {
                    Sounds.FluxCapacitor?.Stop();
                }
            }
            else
            {
                if (ModSettings.PlayFluxCapacitorSound && !Sounds.FluxCapacitor.IsAnyInstancePlaying)
                {
                    Sounds.FluxCapacitor?.Play();
                }

                StartNormalFluxing();
            }

            Properties.IsFluxDoingAnim = false;
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
