using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BackToTheFutureV.Players
{
    public class WormholeAnimationPlayer : Player
    {
        // Sparks that fly around of car, have blue color for bttf1/2 and red for bttf3
        public static readonly List<List<Vector3>> SparkOffsets = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
                new Vector3(0, 2.512924f, 0.719179f),
                new Vector3(0, 1.741328f, 1.112428f),
                new Vector3(0, 0.948842f, 1.461248f),
                new Vector3(0, 0.128709f, 1.738517f),
                new Vector3(0, -0.714542f, 1.934559f),
                new Vector3(0, -1.572115f, 2.05374f),
                new Vector3(0, -2.436388f, 2.106712f)
            },
            new List<Vector3>()
            {
                new Vector3(-0.115846f, 2.439398f, 0.885905f),
                new Vector3(-0.431891f, 1.817174f, 1.39876f),
                new Vector3(-0.761264f, 1.209961f, 1.921057f),
                new Vector3(-1.120302f, 0.638691f, 2.463616f),
                new Vector3(-1.547203f, 0.166296f, 3.047611f)
            },
            new List<Vector3>()
            {
                new Vector3(0.115846f, 2.439398f, 0.885905f),
                new Vector3(0.43189f, 1.817174f, 1.39876f),
                new Vector3(0.761264f, 1.209961f, 1.921058f),
                new Vector3(1.120301f, 0.638691f, 2.463616f),
                new Vector3(1.547203f, 0.166296f, 3.047612f)
            },
            new List<Vector3>()
            {
                new Vector3(-0.39143f, 2.475288f, 0.742706f),
                new Vector3(-0.750809f, 1.783014f, 1.096595f),
                new Vector3(-0.991762f, 0.966849f, 1.25254f),
                new Vector3(-1.185049f, 0.126814f, 1.33482f),
                new Vector3(-1.354231f, -0.721275f, 1.380775f),
                new Vector3(-1.50879f, -1.57308f, 1.404962f),
                new Vector3(-1.65337f, -2.426935f, 1.41441f)
            },
            new List<Vector3>()
            {
                new Vector3(0.510651f, 2.502009f, 0.77206f),
                new Vector3(0.876344f, 1.814953f, 1.120965f),
                new Vector3(1.110193f, 0.994474f, 1.265065f),
                new Vector3(1.29644f, 0.152204f, 1.340689f),
                new Vector3(1.459076f, -0.697358f, 1.383078f),
                new Vector3(1.607347f, -1.550331f, 1.405459f),
                new Vector3(1.74589f, -2.405195f, 1.414376f)
            },
            new List<Vector3>()
            {
                new Vector3(-0.537056f, 2.617612f, 0.651508f),
                new Vector3(-1.126138f, 2.075544f, 0.981871f),
                new Vector3(-1.673436f, 1.495115f, 1.318119f),
                new Vector3(-2.134565f, 0.84859f, 1.661537f),
                new Vector3(-2.461998f, 0.126195f, 2.005894f),
                new Vector3(-2.626427f, -0.654779f, 2.33845f),
                new Vector3(-2.633279f, -1.462411f, 2.648154f),
                new Vector3(-2.511802f, -2.271113f, 2.931375f),
                new Vector3(-2.293917f, -3.068072f, 3.189906f),
                new Vector3(-2.00494f, -3.848973f, 3.42741f)
            },
            new List<Vector3>()
            {
                new Vector3(0.701011f, 2.617612f, 0.651508f),
                new Vector3(1.290093f, 2.075544f, 0.981871f),
                new Vector3(1.837391f, 1.495115f, 1.318119f),
                new Vector3(2.29852f, 0.84859f, 1.661537f),
                new Vector3(2.625953f, 0.126195f, 2.005894f),
                new Vector3(2.790382f, -0.654779f, 2.338449f),
                new Vector3(2.797234f, -1.46241f, 2.648154f),
                new Vector3(2.675757f, -2.271113f, 2.931374f),
                new Vector3(2.457872f, -3.068072f, 3.189907f),
                new Vector3(2.168895f, -3.848972f, 3.42741f)
            },
            new List<Vector3>()
            {
                new Vector3(-0.847276f, 2.517416f, 0.506803f),
                new Vector3(-1.38483f, 1.846296f, 0.599559f),
                new Vector3(-1.769817f, 1.07715f, 0.686075f),
                new Vector3(-2.012557f, 0.250274f, 0.764431f),
                new Vector3(-2.158454f, -0.600096f, 0.836129f),
                new Vector3(-2.240482f, -1.459543f, 0.902991f)
            },
            new List<Vector3>()
            {
                new Vector3(0.482773f, 2.517416f, 0.506803f),
                new Vector3(1.020328f, 1.846296f, 0.599559f),
                new Vector3(1.405314f, 1.07715f, 0.686075f),
                new Vector3(1.648054f, 0.250275f, 0.764431f),
                new Vector3(1.793951f, -0.600096f, 0.836129f),
                new Vector3(1.875979f, -1.459543f, 0.902991f)
            }
        };

        public static List<ScaleformGui> WormholeScaleforms = new List<ScaleformGui>();

        static WormholeAnimationPlayer()
        {
            WormholeScaleforms.Add(new ScaleformGui("bttf_wormhole_scaleform") { DrawInPauseMenu = true });
            WormholeScaleforms.Add(new ScaleformGui("bttf_wormhole_scaleform_blue") { DrawInPauseMenu = true });
            WormholeScaleforms.Add(new ScaleformGui("bttf_wormhole_scaleform_red") { DrawInPauseMenu = true });
        }

        public int MaxTime { get; set; }
        
        public static readonly Vector3 wormholeOffset = new Vector3(0.02835939f, 2.822448f, 0.8090208f);

        public WormholeAnimationPlayer(TimeMachine timeMachine, int maxTime = 4350) : base(timeMachine)
        {
            _wheelPtfxes = new List<PtfxEntityPlayer>();

            MaxTime = maxTime;

            string wormholeRenderTargetName = "bttf_wormhole"; // default

            switch(TimeMachine.Mods.WormholeType)
            {
                case WormholeType.BTTF1:

                    SetupWheelPTFXs("veh_xs_vehicle_mods", "veh_nitrous", new Vector3(0, -0.25f, -0.15f), new Vector3(0, 0, 0), 1.3f);

                    _wormholeModel = ModelHandler.WormholeViolet;
                    _wormholeNightModel = ModelHandler.WormholeVioletNight;
                    _sparkModel = ModelHandler.SparkModel;
                    _sparkNightModel = ModelHandler.SparkNightModel;
                    wormholeScaleformIndex = 0;
                    wormholeRenderTargetName = "bttf_wormhole";
                    break;

                case WormholeType.BTTF2:
                
                    _wormholeModel = ModelHandler.WormholeBlue;
                    _wormholeNightModel = ModelHandler.WormholeBlueNight;
                    _sparkModel = ModelHandler.SparkModel;
                    _sparkNightModel = ModelHandler.SparkNightModel;
                    wormholeScaleformIndex = 1;
                    wormholeRenderTargetName = "bttf_wormhole_blue";
                    break;

                case WormholeType.BTTF3:

                    SetupWheelPTFXs("veh_xs_vehicle_mods", "veh_nitrous", new Vector3(0, -0.25f, -0.15f), new Vector3(0, 0, 0), 1.3f);
                    SetupWheelPTFXs("des_bigjobdrill", "ent_ray_big_drill_start_sparks", new Vector3(0, 0, 0.18f), new Vector3(90f, 0, 0), 1f, true);

                    _sparkPTFX = new PtfxEntityPlayer("scr_paletoscore", "scr_paleto_box_sparks", Props.InvisibleProp, Vector3.Zero, Vector3.Zero, 1.5f, true, true, 300);

                    _wormholeModel = ModelHandler.WormholeRed;
                    _wormholeNightModel = ModelHandler.WormholeRedNight;
                    _sparkModel = ModelHandler.SparkRedModel;
                    _sparkNightModel = ModelHandler.SparkRedNightModel;
                    wormholeScaleformIndex = 2;
                    wormholeRenderTargetName = "bttf_wormhole_red";
                    break;
            }

            if (Mods.IsDMC12)
                _wormholeRT = new RenderTarget(TimeHandler.IsNight ? _wormholeNightModel : _wormholeModel, wormholeRenderTargetName, TimeMachine.Vehicle, "bttf_wormhole");
            else
                _wormholeRT = new RenderTarget(TimeHandler.IsNight ? _wormholeNightModel : _wormholeModel, wormholeRenderTargetName, TimeMachine.Vehicle, new Vector3(0, Vehicle.Model.Dimensions.frontTopRight.Y + 1, 0.4f));

            _wormholeRT.OnRenderTargetDraw += OnRenderTargetDraw;
                       
            _sparks = new List<SparkPlayer>();

            foreach(List<Vector3> sparks in SparkOffsets)
                _sparks.Add(new SparkPlayer(TimeMachine, sparks, TimeHandler.IsNight ? _sparkNightModel : _sparkModel));

            if (Mods.IsDMC12)
                _coilsProp = new AnimateProp(TimeMachine.Vehicle, TimeHandler.IsNight ? ModelHandler.CoilsGlowingNight : ModelHandler.CoilsGlowing, Vector3.Zero, Vector3.Zero);
        }

        private RenderTarget _wormholeRT;
        int wormholeScaleformIndex;
        private bool _hasStartedWormhole;

        private int _startSparksAt;
        private int _startWormholeAt;
        private int _endAt;
        private int numOfProps;

        // Coil flickering (for BTTF3)
        private int _nextFlicker;
        private List<AnimateProp> _separatedCoils;

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
            WormholeScaleforms[wormholeScaleformIndex].Render2D(new PointF(0.5f, 0.5f), 0.9f);
        }

        private void SetupSeparatedCoils()
        {
            _separatedCoils = new List<AnimateProp>();

            foreach(var coilModel in ModelHandler.CoilSeparated.Values)
            {
                _separatedCoils.Add(new AnimateProp(TimeMachine.Vehicle, coilModel, Vector3.Zero, Vector3.Zero));
                _separatedCoils.Last().SpawnProp();
                _separatedCoils.Last().Visible = false;
            }
                
        }

        private void HandleSparks()
        {
            foreach(var spark in _sparks)
                spark.Process();

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
            float by = (Vehicle.GetMPHSpeed() - 65f) / (88f - 65f);

            if (TimeMachine.Properties.PhotoWormholeActive)
                by = (70 - 65f) / (88f - 65f);

            numOfProps = Utils.Lerp(1, 11, by);

            // Delete all other props
            _separatedCoils?.ForEach(x => { x.Visible = false; });

            if (numOfProps >= 11)
            {
                numOfProps = 11;

                TimeMachine.Mods.OffCoils = ModState.Off;

                _coilsProp?.SpawnProp();
            } else
            {               
                List<int> propsToBeSpawned = Enumerable.Range(0, 11).OrderBy(x => Utils.Random.Next()).Take(numOfProps).ToList();

                foreach (var propindex in propsToBeSpawned)
                    _separatedCoils[propindex].Visible = true;

                // Set next flicker 
                _nextFlicker = Game.GameTime + Utils.Random.Next(30, 60);
            }
        }

        private void SetupWheelPTFXs(string particleAssetName, string particleName, Vector3 wheelOffset, Vector3 wheelRot, float size = 3f, bool doLoopHandling = false)
        {
            foreach (var wheelName in Utils.WheelNames)
            {
                var worldPos = TimeMachine.Vehicle.Bones[wheelName].Position;
                var offset = TimeMachine.Vehicle.GetPositionOffset(worldPos);

                offset += wheelOffset;

                var ptfx = new PtfxEntityPlayer(particleAssetName, particleName, TimeMachine.Vehicle, offset, wheelRot, size, true, doLoopHandling);

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

            if (TimeMachine.Mods.WormholeType == WormholeType.BTTF3)
                SetupSeparatedCoils();            

            IsPlaying = true;
            _hasStartedWormhole = false;            
            _startSparksAt = Game.GameTime + 1000;

            // Spawn the coil model
            if (TimeMachine.Mods.WormholeType != WormholeType.BTTF3)
            {
                TimeMachine.Mods.OffCoils = ModState.Off;

                _coilsProp?.SpawnProp();
            }

            TimeMachine.Events.OnWormholeStarted?.Invoke();
        }

        public override void Stop()
        {
            IsPlaying = false;
            _hasStartedWormhole = false;            
            numOfProps = 0;

            _coilsProp?.Delete();

            _separatedCoils?.ForEach(x => x?.Delete());

            _separatedCoils?.Clear();

            TimeMachine.Mods.OffCoils = ModState.On;

            _sparkPTFX?.StopNonLooped();
            
            _sparks?.ForEach(x => x?.Stop());

            _wheelPtfxes?.ForEach(x => x?.Stop());

            _wormholeRT?.DeleteProp();
        }

        public override void Process()
        {
            if (!IsPlaying)
                return;

            // Handle coil flickering for BTTF3
            if (Mods.IsDMC12 && TimeMachine.Mods.WormholeType == WormholeType.BTTF3)
                HandleCoilFlicker();

            if (_wheelPtfxes != null && !Properties.IsFlying)
            {
                foreach (var wheelPTFX in _wheelPtfxes)
                {
                    if (!wheelPTFX.IsPlaying)
                        wheelPTFX.Play();

                    wheelPTFX.Process();
                }
            }

            if (TimeMachine.Properties.IsFueled || TimeMachine.Properties.PhotoWormholeActive)
                HandleSparks();

            if (_sparkPTFX != null && !_sparkPTFX.IsPlaying)
                _sparkPTFX.Play();

            _sparkPTFX?.Process();

            // Some wormhole sparks logic.
            if (Game.GameTime >= _startWormholeAt && _hasStartedWormhole)
            {
                // Draw the wormhole RenderTarget, so that the animation appears on the prop
                _wormholeRT.Draw();

                if(!_wormholeRT.Prop.IsSpawned && _playWormhole && (Vehicle.GetMPHSpeed() >= 88 || TimeMachine.Properties.PhotoWormholeActive))
                {
                    _wormholeRT.CreateProp();
                    WormholeScaleforms[wormholeScaleformIndex].CallFunction("START_ANIMATION");
                    _hasStartedWormhole = true;
                }

                if (TimeMachine.Mods.WormholeType != WormholeType.BTTF3 && Game.GameTime >= _endAt && _playWormhole && !TimeMachine.Properties.PhotoWormholeActive)
                {
                    OnCompleted?.Invoke();
                    Stop();

                    return;
                }
            }

            if (!_hasStartedWormhole && _playWormhole && (Vehicle.GetMPHSpeed() >= 88 || TimeMachine.Properties.PhotoWormholeActive))
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
            _sparkPTFX?.StopNonLooped();
        }
    }
}
