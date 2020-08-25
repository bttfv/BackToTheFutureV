using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BackToTheFutureV.Players
{
    public class WormholeAnimationPlayer : Player
    {
        public int MaxTime { get; set; }
        public TimeCircuits TimeCircuits { get; private set; }

        public static readonly Vector3 wormholeOffset = new Vector3(0.02835939f, 2.822448f, 0.8090208f);

        public WormholeAnimationPlayer(TimeCircuits circuits, int maxTime = 4350)
        {
            _wheelPtfxes = new List<PtfxEntityPlayer>();

            TimeCircuits = circuits;
            MaxTime = maxTime;

            string wormholeScaleformName = "bttf_wormhole_scaleform"; // default
            string wormholeRenderTargetName = "bttf_wormhole"; // default

            switch(circuits.DeloreanType)
            {
                case DeloreanType.BTTF1:

                    SetupWheelPTFXs("veh_xs_vehicle_mods", "veh_nitrous", new Vector3(0, -0.25f, -0.15f), new Vector3(0, 0, 0), 1.3f);

                    _wormholeModel = ModelHandler.WormholeViolet;
                    _wormholeNightModel = ModelHandler.WormholeVioletNight;
                    _sparkModel = ModelHandler.SparkModel;
                    _sparkNightModel = ModelHandler.SparkNightModel;
                    wormholeScaleformName = "bttf_wormhole_scaleform";
                    wormholeRenderTargetName = "bttf_wormhole";
                    break;

                case DeloreanType.BTTF2:

                    _wormholeModel = ModelHandler.WormholeBlue;
                    _wormholeNightModel = ModelHandler.WormholeBlueNight;
                    _sparkModel = ModelHandler.SparkModel;
                    _sparkNightModel = ModelHandler.SparkNightModel;
                    wormholeScaleformName = "bttf_wormhole_scaleform_blue";
                    wormholeRenderTargetName = "bttf_wormhole_blue";
                    break;

                case DeloreanType.BTTF3:

                    SetupWheelPTFXs("veh_xs_vehicle_mods", "veh_nitrous", new Vector3(0, -0.25f, -0.15f), new Vector3(0, 0, 0), 1.3f);
                    SetupWheelPTFXs("des_bigjobdrill", "ent_ray_big_drill_start_sparks", new Vector3(0, 0, 0.18f), new Vector3(90f, 0, 0), 1f, true);

                    _sparkProp = new AnimateProp(TimeCircuits.Vehicle, ModelHandler.InvisibleProp, new Vector3(0, 3.4f, -0.6f), new Vector3(0, 0, 180));
                    _sparkProp.SpawnProp();
                    
                    _sparkPTFX = new PtfxEntityPlayer("scr_paletoscore", "scr_paleto_box_sparks", _sparkProp.Prop, Vector3.Zero, Vector3.Zero, 1.5f, true, true, 300);

                    //_sparkPTFX = new PtfxEntityPlayer("scr_paletoscore", "scr_paleto_box_sparks", TimeCircuits.Vehicle, new Vector3(0, 3.4f, -0.6f), new Vector3(0, 0, 180), 1.5f, true, true, 300);
                    //_sparkPTFX = new PtfxEntityBonePlayer("scr_reconstructionaccident", "scr_sparking_generator", TimeCircuits.Vehicle, "bonnet", new Vector3(0,-0.2f,0.2f), Vector3.Zero, 4f, true);

                    //_sparkPTFX = new List<PtfxEntityBonePlayer>();

                    //_sparkPTFX.Add(new PtfxEntityBonePlayer("core", "ent_amb_sparking_wires", TimeCircuits.Vehicle, "bonnet", new Vector3(-0.2f, -0.2f, 0.2f), new Vector3(0, -90, 0), 2f, true));
                    //_sparkPTFX.Add(new PtfxEntityBonePlayer("core", "ent_amb_sparking_wires", TimeCircuits.Vehicle, "bonnet", new Vector3(0, -0.2f, 0.2f), new Vector3(0, 0, 0), 2f, true));
                    //_sparkPTFX.Add(new PtfxEntityBonePlayer("core", "ent_amb_sparking_wires", TimeCircuits.Vehicle, "bonnet", new Vector3(0.2f, -0.2f, 0.2f), new Vector3(0, 90, 0), 2f, true));

                    _wormholeModel = ModelHandler.WormholeRed;
                    _wormholeNightModel = ModelHandler.WormholeRedNight;
                    _sparkModel = ModelHandler.SparkRedModel;
                    _sparkNightModel = ModelHandler.SparkRedNightModel;
                    wormholeScaleformName = "bttf_wormhole_scaleform_red";
                    wormholeRenderTargetName = "bttf_wormhole_red";
                    break;
            }
            
            _wormholeRT = new RenderTarget(_wormholeModel, wormholeRenderTargetName, TimeCircuits.Vehicle, "bttf_wormhole");
            _wormholeScaleform = new ScaleformGui(wormholeScaleformName);
            _wormholeRT.OnRenderTargetDraw += OnRenderTargetDraw;
            _wormholeScaleform.DrawInPauseMenu = true;

            _coilsProp = new AnimateProp(TimeCircuits.Vehicle, ModelHandler.CoilsGlowing, Vector3.Zero, Vector3.Zero);
           
            _sparks = new List<SparkPlayer>();
            foreach(List<Vector3> sparks in Constants.SparkOffsets)
            {
                _sparks.Add(new SparkPlayer(TimeCircuits.Vehicle, sparks, _sparkModel));
            }
        }

        private RenderTarget _wormholeRT;
        private ScaleformGui _wormholeScaleform;
        private bool _hasStartedWormhole;
        private bool _hasStartedFluxEffect;

        private int _startSparksAt;
        private int _startWormholeAt;
        private int _endAt;
        private int numOfProps;

        // Coil flickering (for BTTF3)
        private int _nextFlicker;
        private List<AnimateProp> _separatedCoils;

        private AnimateProp _sparkProp;
        private PtfxEntityPlayer _sparkPTFX;

        private int _nextSpark;
        private List<SparkPlayer> _sparks;

        private bool _playWormhole;

        private List<PtfxEntityPlayer> _wheelPtfxes = null;
        private AnimateProp _coilsProp;

        private Model _wormholeModel;
        private Model _wormholeNightModel;
        private Model _sparkModel;
        private Model _sparkNightModel;

        private void OnRenderTargetDraw()
        {
            _wormholeScaleform.Render2D(new PointF(0.5f, 0.5f), 0.9f);
        }

        private void UpdateCoilModel()
        {
            if (Main.CurrentTime.Hour >= 20 || (Main.CurrentTime.Hour >= 0 && Main.CurrentTime.Hour <= 5))
            {
                _coilsProp.Model = ModelHandler.CoilsGlowingNight;
                _wormholeRT.Prop.Model = _wormholeNightModel;

                _sparks.ForEach(x => x.UpdateSparkModel(_sparkNightModel));
            }
            else
            {
                _coilsProp.Model = ModelHandler.CoilsGlowing;
                _wormholeRT.Prop.Model = _wormholeModel;

                _sparks.ForEach(x => x.UpdateSparkModel(_sparkModel));
            }
        }

        private void SetupSeparatedCoils()
        {
            _separatedCoils = new List<AnimateProp>();

            foreach(var coilModel in ModelHandler.CoilSeparated.Values)
            {
                _separatedCoils.Add(new AnimateProp(TimeCircuits.Vehicle, coilModel, Vector3.Zero, Vector3.Zero));
                _separatedCoils.Last().SpawnProp(false);
                _separatedCoils.Last().Prop.IsVisible = false;
            }
                
        }

        private void HandleSparks()
        {
            foreach(var spark in _sparks)
            {
                spark.Process();
            }

            if (Game.GameTime < _nextSpark || Game.GameTime < _startSparksAt) return;

            List<SparkPlayer> validSparks = _sparks.Where(x => !x.IsPlaying).ToList();

            if(validSparks.Count > 0)
            {
                int randomSpark = Utils.Random.Next(validSparks.Count);

                if (!validSparks[randomSpark].IsPlaying)
                {
                    validSparks[randomSpark].Speed = (float)Utils.Random.NextDouble(15f, 21f);
                    validSparks[randomSpark].Play();
                }
            }

            _nextSpark = Game.GameTime + 130;
        }

        private void HandleCoilFlicker()
        {            
            if (numOfProps == 11 | Game.GameTime < _nextFlicker) return;

            //// Choose how many coil props can spawn at one time
            float by = (TimeCircuits.Delorean.MPHSpeed - 65f) / (88f - 65f);

            if (TimeCircuits.IsPhotoModeOn)
                by = (70 - 65f) / (88f - 65f);

            numOfProps = Utils.Lerp(1, 11, by);

            // Delete all other props
            _separatedCoils?.ForEach(x => { x.Prop.IsVisible = false; });

            if (numOfProps >= 11)
            {
                numOfProps = 11;

                TimeCircuits.Delorean.Mods.OffCoils = ModState.Off;

                _coilsProp.SpawnProp(false);
            } else
            {               
                List<int> propsToBeSpawned = Enumerable.Range(0, 11).OrderBy(x => Utils.Random.Next()).Take(numOfProps).ToList();

                foreach (var propindex in propsToBeSpawned)
                    _separatedCoils[propindex].Prop.IsVisible = true;

                // Set next flicker 
                _nextFlicker = Game.GameTime + Utils.Random.Next(30, 60);
            }
        }

        private void SetupWheelPTFXs(string particleAssetName, string particleName, Vector3 wheelOffset, Vector3 wheelRot, float size = 3f, bool doLoopHandling = false)
        {
            foreach (var wheelName in Utils.WheelNames)
            {
                var worldPos = TimeCircuits.Vehicle.Bones[wheelName].Position;
                var offset = TimeCircuits.Vehicle.GetPositionOffset(worldPos);

                offset += wheelOffset;

                var ptfx = new PtfxEntityPlayer(particleAssetName, particleName, TimeCircuits.Vehicle, offset, wheelRot, size, true, doLoopHandling);

                _wheelPtfxes.Add(ptfx);
            }
        }

        public void Play(bool playWormhole)
        {
            Play();

            _playWormhole = playWormhole;
        }

        public override void Play()
        {
            Stop();

            if (TimeCircuits.DeloreanType == DeloreanType.BTTF3)
                SetupSeparatedCoils();

            IsPlaying = true;
            _hasStartedWormhole = false;
            _hasStartedFluxEffect = false;
            _startSparksAt = Game.GameTime + 1000;

            // Update coil model, based on the time of day
            UpdateCoilModel();

            // Spawn the coil model
            if (TimeCircuits.DeloreanType != DeloreanType.BTTF3)
            {
                TimeCircuits.Delorean.Mods.OffCoils = ModState.Off;

                _coilsProp.SpawnProp(false);
            }
        }

        public override void Stop()
        {
            IsPlaying = false;
            _hasStartedWormhole = false;
            _hasStartedFluxEffect = false;
            numOfProps = 0;

            TimeCircuits.GetHandler<FluxCapacitorHandler>().StartNormalFluxing();

            _coilsProp?.DeleteProp();

            _separatedCoils?.ForEach(x => x?.DeleteProp());

            _separatedCoils?.Clear();

            TimeCircuits.Delorean.Mods.OffCoils = ModState.On;

            _sparkPTFX?.StopNonLooped();
            
            _sparks?.ForEach(x => x?.Stop());

            _wheelPtfxes?.ForEach(x => x?.Stop());

            _wormholeRT.DeleteProp();
        }

        public override void Process()
        {
            if (!IsPlaying) return;

            // Handle coil flickering for BTTF3
            if (TimeCircuits.DeloreanType == DeloreanType.BTTF3)
            {
                HandleCoilFlicker();
            }

            if (!_hasStartedFluxEffect)
            {
                TimeCircuits.GetHandler<FluxCapacitorHandler>().StartTimeTravelEffect();
                _hasStartedFluxEffect = true;
            }

            if (_wheelPtfxes != null)
            {
                foreach (var wheelPTFX in _wheelPtfxes)
                {
                    if (!wheelPTFX.IsPlaying)
                        wheelPTFX.Play();

                    wheelPTFX.Process();
                }
            }

            if (TimeCircuits.IsFueled)
                HandleSparks();

            if (_sparkPTFX != null && !_sparkPTFX.IsPlaying)
                _sparkPTFX.Play();

            _sparkPTFX?.Process();

            // Some wormhole sparks logic.
            if (Game.GameTime >= _startWormholeAt && _hasStartedWormhole)
            {
                // Draw the wormhole RenderTarget, so that the animation appears on the prop
                _wormholeRT.Draw();

                if(!_wormholeRT.Prop.IsSpawned && _playWormhole && (TimeCircuits.Delorean.MPHSpeed >= 88 || TimeCircuits.IsPhotoModeOn))
                {
                    _wormholeRT.CreateProp();
                    _wormholeScaleform.CallFunction("START_ANIMATION");
                    _hasStartedWormhole = true;
                }

                if (TimeCircuits.DeloreanType != DeloreanType.BTTF3 && Game.GameTime >= _endAt && _playWormhole)
                {
                    OnCompleted?.Invoke();
                    Stop();

                    return;
                }
            }

            if (!_hasStartedWormhole && _playWormhole && (TimeCircuits.Delorean.MPHSpeed >= 88 || TimeCircuits.IsPhotoModeOn))
            {
                _startWormholeAt = Game.GameTime + 1000;
                _endAt = _startWormholeAt + MaxTime;
                _hasStartedWormhole = true;
            }

        }

        public override void Dispose()
        {
            _wormholeRT?.Dispose();
            _coilsProp?.Dispose();
            _wheelPtfxes?.ForEach(x => x?.Dispose());
            _separatedCoils?.ForEach(x => x?.Dispose());
            _sparks?.ForEach(x => x?.Dispose());
            _wormholeScaleform?.Unload();
            _sparkPTFX?.StopNonLooped();
            _sparkProp?.Dispose();
        }
    }
}
