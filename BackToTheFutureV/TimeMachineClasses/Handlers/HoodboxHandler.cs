using System.Windows.Forms;
using GTA;
using GTA.Math;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Settings;
using GTA.Native;
using BackToTheFutureV.Vehicles;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class HoodboxHandler : Handler
    {
        private bool _state = false;
        private bool _applyAlpha = false;
        private float _alphaLevel = 0;
        private bool _isNight;

        public HoodboxHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnReenterCompleted += OnReenterCompleted;
        }

        public void StartWarmup()
        {            
            _state = true;
            _applyAlpha = true;
        }

        public void OnReenterCompleted()
        {
            if (!Mods.IsDMC12 || Mods.Hoodbox == ModState.Off)
                return;

            _alphaLevel = 52f;
            _state = true;
            LoadLights(true);            
            Props.HoodboxLights.SpawnProp();
            Props.HoodboxLights.Prop.Opacity = (int)_alphaLevel;
            Properties.AreHoodboxCircuitsReady = true;
        }

        public override void KeyPress(Keys key)
        {

        }

        private void LoadLights(bool force = false)
        {
            if (!force && _isNight == Utils.IsNight())
                return;

            _isNight = Utils.IsNight();

            Props.HoodboxLights?.Dispose();

            if (_isNight)
                Props.HoodboxLights = new AnimateProp(Vehicle, ModelHandler.HoodboxLightsNight, "bonnet");
            else
                Props.HoodboxLights = new AnimateProp(Vehicle, ModelHandler.HoodboxLights, "bonnet");
        }

        public override void Process()
        {                                    
            if (_state)
            {
                if (Mods.Hoodbox == ModState.Off)
                {
                    Stop();
                    return;
                }

                LoadLights(Props.HoodboxLights == null);

                if (Props.HoodboxLights != null)
                {                    
                    if (!Props.HoodboxLights.IsSpawned)
                        Props.HoodboxLights.SpawnProp();

                    if (Vehicle.IsVisible != Props.HoodboxLights?.Prop.IsVisible)
                        Props.HoodboxLights.Prop.IsVisible = Vehicle.IsVisible;

                    if (_applyAlpha)
                    {
                        _alphaLevel += Game.LastFrameTime * 6f;

                        if (_alphaLevel > 52)
                        {
                            _applyAlpha = false;

                            if(!TcdEditer.IsEditing)
                                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hoodbox_Warmup_Error"));

                            Properties.AreHoodboxCircuitsReady = true;
                        }                            
                        else
                            Props.HoodboxLights.Prop.Opacity = (int)_alphaLevel;
                    }
                }

                return;
            }

            if (Mods.Hoodbox == ModState.Off | Main.PlayerPed.IsInVehicle()) return;

            var worldPos = Vehicle.Bones["bonnet"].Position;

            var dist = Main.PlayerPed.Position.DistanceToSquared(worldPos);

            if (!(dist <= 2f * 2f)) 
                return;

            if(!TcdEditer.IsEditing)
                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hoodbox_Warmup_Start"));

            if (Game.IsControlJustPressed(GTA.Control.Context))
                StartWarmup();
        }

        public override void Stop()
        {
            Props.HoodboxLights?.DeleteProp();
            Props.HoodboxLights = null;
            _state = false;
            _applyAlpha = false;
            _alphaLevel = 0;
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
