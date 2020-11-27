using System.Windows.Forms;
using GTA;
using GTA.Math;
using BackToTheFutureV.Utility;

using BackToTheFutureV.Settings;
using GTA.Native;
using BackToTheFutureV.Vehicles;
using BTTFVLibrary;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class ComponentsHandler : Handler
    {
        //Hook
        private Vector3 hookPosition = new Vector3(0.75f, 0f, 0f);

        //Hoodbox
        private int _warmUp = 0;
        
        public ComponentsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnReenterCompleted += OnReenterCompleted;
            Events.OnVehicleSpawned += OnReenterCompleted;
            Events.OnHoodboxReady += Instant;
            TimeHandler.OnDayNightChange += OnDayNightChange;

            OnDayNightChange();
        }

        public void OnReenterCompleted()
        {
            if (Mods.HoverUnderbody == ModState.On)
                Properties.CanConvert = true;

            if (Mods.Plate == PlateType.Outatime && Mods.Reactor == ReactorType.Nuclear && Mods.WormholeType == WormholeType.BTTF1)
                Mods.Plate = PlateType.Empty;

            if (Mods.Hook == HookState.On)
                Mods.Hook = HookState.Removed;

            if (Mods.Hoodbox == ModState.Off || Properties.AreHoodboxCircuitsReady)
                return;

            Instant();
        }

        private void Instant()
        {
            Props.HoodboxLights.SpawnProp();
            Properties.AreHoodboxCircuitsReady = true;
        }

        public override void KeyDown(Keys key)
        {

        }

        private void OnDayNightChange()
        {
            Props.HoodboxLights?.DeleteProp();

            if (TimeHandler.IsNight)
                Props.HoodboxLights.Model = ModelHandler.HoodboxLightsNight;
            else
                Props.HoodboxLights.Model = ModelHandler.HoodboxLights;
        }

        private void HookProcess()
        {
            if (Utils.PlayerPed.IsInVehicle())
                return;

            if (Mods.Hook != HookState.OnDoor)
                return;

            Vector3 worldPos = Vehicle.GetOffsetPosition(hookPosition);

            float dist = Utils.PlayerPed.Position.DistanceToSquared(worldPos);

            if (dist <= 2f * 2f)
            {
                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Apply_Hook"));

                if (Game.IsControlJustPressed(GTA.Control.Context))
                    Mods.Hook = HookState.On;
            }
        }

        private void CompassProcess()
        {
            Props.Compass?.SpawnProp(Vector3.Zero, new Vector3(0, 0, Vehicle.Heading), false);

            if (Props.Compass.Prop.IsVisible != Vehicle.IsVisible)
                Props.Compass.Prop.IsVisible = Vehicle.IsVisible;
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

                if (Vehicle.IsVisible != Props.HoodboxLights?.Prop.IsVisible)
                    Props.HoodboxLights.Prop.IsVisible = Vehicle.IsVisible;

                return;
            }

            if (_warmUp > 0 && _warmUp < Game.GameTime)
            {
                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hoodbox_Warmup_Error"));
                Props.HoodboxLights.SpawnProp();

                _warmUp = 0;
                Properties.AreHoodboxCircuitsReady = true;

                return;
            }

            if (Mods.Hoodbox == ModState.Off | Utils.PlayerPed.IsInVehicle() | TcdEditer.IsEditing | _warmUp > 0)
                return;

            if (!(Utils.PlayerPed.Position.DistanceToSquared(Vehicle.Bones["bonnet"].Position) <= 2f * 2f))
                return;

            Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hoodbox_Warmup_Start"));

            if (Game.IsControlJustPressed(GTA.Control.Context))
                _warmUp = Game.GameTime + 8000;
        }

        public override void Process()
        {
            HookProcess();

            CompassProcess();

            HoodboxProcess();
        }

        public override void Stop()
        {
            Props.HoodboxLights?.DeleteProp();
            _warmUp = 0;
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
