using BackToTheFutureV.Settings;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using GTA.Math;
using System.Windows.Forms;

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
            Props.HoodboxLights?.Delete();

            if (TimeHandler.IsNight)
                Props.HoodboxLights.SwapModel(ModelHandler.HoodboxLightsNight);
            else
                Props.HoodboxLights.SwapModel(ModelHandler.HoodboxLights);
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
            Props.HoodboxLights?.Delete();
            _warmUp = 0;
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
