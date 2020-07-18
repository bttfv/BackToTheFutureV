using System.Windows.Forms;
using GTA;
using GTA.Math;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Settings;
using GTA.Native;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class HoodboxHandler : Handler
    {
        private bool _state = false;
        private bool _applyAlpha = false;
        private float _alphaLevel = 0;
        private bool _isNight;

        public bool IsWarmedUp => _state && !_applyAlpha;

        private AnimateProp _hoodboxLights;

        public HoodboxHandler(TimeCircuits circuits) : base(circuits)
        {
            
        }

        public void StartWarmup()
        {            
            _state = true;
            _applyAlpha = true;
        }

        public void SetInstant()
        {
            _alphaLevel = 52f;
            _state = true;
            LoadLights(true);            
            _hoodboxLights.SpawnProp();
            _hoodboxLights.Prop.Opacity = (int)_alphaLevel;
        }

        public override void KeyPress(Keys key)
        {
        }

        private void LoadLights(bool force = false)
        {
            if (!force && _isNight == Utils.IsNight())
                return;

            _isNight = Utils.IsNight();

            _hoodboxLights?.Dispose();

            if (_isNight)
                _hoodboxLights = new AnimateProp(Vehicle, ModelHandler.HoodboxLightsNight, "bonnet");
            else
                _hoodboxLights = new AnimateProp(Vehicle, ModelHandler.HoodboxLights, "bonnet");
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

                LoadLights(_hoodboxLights == null);

                if (_hoodboxLights != null)
                {                    
                    if (!_hoodboxLights.IsSpawned)
                        _hoodboxLights.SpawnProp();

                    if (Vehicle.IsVisible != _hoodboxLights?.Prop.IsVisible)
                        _hoodboxLights.Prop.IsVisible = Vehicle.IsVisible;

                    if (_applyAlpha)
                    {
                        _alphaLevel += Game.LastFrameTime * 6f;

                        if (_alphaLevel > 52)
                        {
                            _applyAlpha = false;

                            if(!TcdEditer.IsEditing)
                                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hoodbox_Warmup_Error"));
                        }                            
                        else
                            _hoodboxLights.Prop.Opacity = (int)_alphaLevel;
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
            _hoodboxLights?.DeleteProp();
            _hoodboxLights = null;
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
