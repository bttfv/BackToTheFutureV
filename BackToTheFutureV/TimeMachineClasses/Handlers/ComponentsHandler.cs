using BackToTheFutureV.Menu;
using BackToTheFutureV.Settings;
using BackToTheFutureV.Utility;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using static BackToTheFutureV.Utility.InternalEnums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    internal class ComponentsHandler : HandlerPrimitive
    {
        //Hook
        private Vector3 hookPosition = new Vector3(0.75f, 0f, 0f);

        //Hoodbox
        private int _warmUp = 0;

        private float _fluxBandsCooldown = -1;

        public ComponentsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnReenterEnded += OnReenterEnded;
            Events.OnVehicleSpawned += OnReenterEnded;
            Events.SetHoodboxWarmedUp += Instant;
            TimeHandler.OnDayNightChange += OnDayNightChange;

            OnDayNightChange();
        }

        public void OnReenterEnded()
        {
            if (Mods.HoverUnderbody == ModState.On)
                Properties.CanConvert = true;

            if (Mods.Plate == PlateType.Outatime && Mods.Reactor == ReactorType.Nuclear && Mods.WormholeType == WormholeType.BTTF1)
                Mods.Plate = PlateType.Empty;

            if (Mods.Hook == HookState.On)
                Mods.Hook = HookState.Removed;

            if (Mods.IsDMC12 && Properties.IsFlying && Mods.Reactor == ReactorType.MrFusion)
            {
                Properties.PhotoGlowingCoilsActive = true;
                _fluxBandsCooldown = 0;
            }

            if (Mods.Hoodbox == ModState.Off || Properties.AreHoodboxCircuitsReady)
                return;

            Instant();
        }

        private void Instant()
        {
            Props.HoodboxLights.SpawnProp();
            Properties.AreHoodboxCircuitsReady = true;
        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        private void OnDayNightChange()
        {
            Props.HoodboxLights?.Delete();

            if (TimeHandler.IsNight)
                Props.HoodboxLights.SwapModel(ModelHandler.HoodboxLightsNight);
            else
                Props.HoodboxLights.SwapModel(ModelHandler.HoodboxLights);
        }

        private void HookProcess()
        {
            if (Mods.Hook != HookState.OnDoor || Utils.PlayerPed.IsInVehicle())
                return;

            if (Utils.PlayerPed.DistanceToSquared2D(Vehicle, "window_rf", 1))
            {
                Utils.DisplayHelpText(BTTFVMenu.GetLocalizedText("ApplyHook"));

                if (Game.IsControlJustPressed(GTA.Control.Context))
                    Mods.Hook = HookState.On;
            }
        }

        private void CompassProcess()
        {
            Props.Compass?.MoveProp(Vector3.Zero, new Vector3(0, 0, Vehicle.Heading));

            if (Props.Compass.Visible != Vehicle.IsVisible)
                Props.Compass.Visible = Vehicle.IsVisible;
        }

        private void HoodboxProcess()
        {
            if (Properties.AreHoodboxCircuitsReady)
            {
                if (Mods.Hoodbox == ModState.Off)
                {
                    Stop();
                    return;
                }

                if (!Props.HoodboxLights.IsSpawned)
                    Props.HoodboxLights.SpawnProp();

                if (Vehicle.IsVisible != Props.HoodboxLights?.Visible)
                    Props.HoodboxLights.Visible = Vehicle.IsVisible;

                return;
            }

            if (_warmUp > 0 && _warmUp < Game.GameTime)
            {
                Utils.DisplayHelpText(BTTFVMenu.GetLocalizedText("WarmupComplete"));
                Props.HoodboxLights.SpawnProp();

                _warmUp = 0;
                Properties.AreHoodboxCircuitsReady = true;

                return;
            }

            if (Mods.Hoodbox == ModState.Off || Properties.AreHoodboxCircuitsReady || _warmUp > 0 || Utils.PlayerPed.IsInVehicle() || TcdEditer.IsEditing || RCGUIEditer.IsEditing)
                return;

            if (Utils.PlayerPed.DistanceToSquared2D(Vehicle, "bonnet", 1.5f))
            {
                Utils.DisplayHelpText(BTTFVMenu.GetLocalizedText("Warmup"));

                if (Game.IsControlJustPressed(GTA.Control.Context))
                    _warmUp = Game.GameTime + 8000;
            }
        }

        public override void Tick()
        {
            HookProcess();

            CompassProcess();

            HoodboxProcess();

            if (_fluxBandsCooldown > -1)
            {
                _fluxBandsCooldown += Game.LastFrameTime;

                if (_fluxBandsCooldown > 2)
                {
                    Properties.PhotoGlowingCoilsActive = false;
                    _fluxBandsCooldown = -1;
                }
            }
        }

        public override void Stop()
        {
            Props.HoodboxLights?.Delete();
            _warmUp = 0;
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
