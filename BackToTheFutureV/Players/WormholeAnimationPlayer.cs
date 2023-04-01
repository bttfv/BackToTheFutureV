using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class WormholeAnimationPlayer : Players.Player
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

        public WormholeAnimationPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            Scaleforms.WormholeRT?.Dispose();

            if (Mods.IsDMC12)
            {
                Scaleforms.WormholeRT = new RenderTarget(Constants.WormholeModel, Constants.WormholeRenderTargetName, Vehicle, "bttf_wormhole");
            }
            else
            {
                Scaleforms.WormholeRT = new RenderTarget(Constants.WormholeModel, Constants.WormholeRenderTargetName, Vehicle, new Vector3(0, Vehicle.Model.Dimensions.frontTopRight.Y + 1, 0.4f));
            }

            Scaleforms.WormholeRT.OnRenderTargetDraw += OnRenderTargetDraw;

            _sparks = new List<SparkPlayer>();

            _wheSpark = new EmitterSparkPlayer(timeMachine, SparkType.WHE);
            _leftSpark = new EmitterSparkPlayer(timeMachine, SparkType.Left);
            _rightSpark = new EmitterSparkPlayer(timeMachine, SparkType.Right);

            foreach (List<Vector3> sparks in SparkOffsets)
            {
                _sparks.Add(new SparkPlayer(TimeMachine, sparks));
            }

            if (Mods.IsDMC12)
            {
                Props.Coils?.Dispose();
                Props.Coils = new AnimateProp(Constants.CoilsModel, Vehicle, Vector3.Zero, Vector3.Zero);
            }
        }

        private bool _hasStartedWormhole;

        private int _startWormholeAt;
        private int _endAt;
        private int numOfProps;

        // Coil flickering (for BTTF3)
        private int _nextFlicker;

        private int _nextSpark;
        private readonly List<SparkPlayer> _sparks;

        private readonly EmitterSparkPlayer _wheSpark;
        private readonly EmitterSparkPlayer _leftSpark;
        private readonly EmitterSparkPlayer _rightSpark;

        private bool _playWormhole;

        private void OnRenderTargetDraw()
        {
            ScaleformsHandler.WormholeScaleforms[Constants.WormholeScaleformIndex].Render2D(new PointF(0.5f, 0.5f), 0.9f);
        }

        private void HandleSparks()
        {
            foreach (SparkPlayer spark in _sparks)
            {
                spark.Tick();
            }

            _wheSpark.Tick();
            _leftSpark.Tick();
            _rightSpark.Tick();

            if (Game.GameTime < _nextSpark || Game.GameTime < _startWormholeAt || !_hasStartedWormhole)
            {
                return;
            }

            List<SparkPlayer> validSparks = _sparks.Where(x => !x.IsPlaying).ToList();

            if (validSparks.Count > 0)
            {
                int randomSpark = FusionUtils.Random.Next(validSparks.Count);

                if (!validSparks[randomSpark].IsPlaying)
                {
                    validSparks[randomSpark].Speed = (float)FusionUtils.Random.NextDouble(15f, 21f);
                    validSparks[randomSpark].Play();
                }
            }

            _nextSpark = Game.GameTime + 130;
        }

        private void HandleCoilFlicker()
        {
            if (numOfProps == 11 || Game.GameTime < _nextFlicker)
            {
                return;
            }

            //// Choose how many coil props can spawn at one time
            float by = (Vehicle.GetMPHSpeed() - 65f) / (88f - 65f);

            if (Properties.PhotoWormholeActive)
            {
                by = (70 - 65f) / (88f - 65f);
            }

            numOfProps = FusionUtils.Lerp(1, 11, by);

            // Delete all other props
            Props.SeparatedCoils?.Delete();

            if (Properties.ReactorState != ReactorState.Closed)
            {
                numOfProps = 6;
            }

            if (numOfProps >= 11)
            {
                numOfProps = 11;

                Mods.OffCoils = ModState.Off;

                Props.Coils?.SpawnProp();
            }
            else
            {
                List<int> propsToBeSpawned = Enumerable.Range(0, 11).OrderBy(x => FusionUtils.Random.Next()).Take(numOfProps).ToList();

                foreach (int propindex in propsToBeSpawned)
                {
                    Props.SeparatedCoils[propindex].SpawnProp();
                }

                // Set next flicker 
                _nextFlicker = Game.GameTime + FusionUtils.Random.Next(30, 60);
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

            IsPlaying = true;
            _hasStartedWormhole = false;

            // Spawn the coil model
            if (Mods.WormholeType != WormholeType.BTTF3 && Properties.ReactorState == ReactorState.Closed)
            {
                Mods.OffCoils = ModState.Off;

                Props.Coils?.SpawnProp();
            }

            /*if (Constants.DeluxoProto)
            {
                Vehicle.Mods.DashboardColor = (VehicleColor)70;
            }*/

            Events.OnWormholeStarted?.Invoke();
        }

        public override void Stop()
        {
            IsPlaying = false;
            _hasStartedWormhole = false;
            numOfProps = 0;

            Props.Coils?.Delete();

            Props.SeparatedCoils?.Delete();

            Mods.GlowingEmitter = ModState.Off;

            Mods.OffCoils = ModState.On;

            Particles?.Sparks?.Stop(true);

            _sparks?.ForEach(x => x?.Stop());

            _wheSpark?.Stop();

            _leftSpark?.Stop();

            _rightSpark?.Stop();

            Particles?.WheelsFire?.Stop();

            Particles?.WheelsSparks?.Stop();

            Scaleforms.WormholeRT?.DeleteProp();

            /*if (Constants.DeluxoProto)
            {
                Vehicle.Mods.DashboardColor = (VehicleColor)12;
            }*/
        }

        public override void Tick()
        {
            if (!IsPlaying)
            {
                return;
            }

            // Handle coil flickering for BTTF3
            if (Mods.IsDMC12 && (Mods.WormholeType == WormholeType.BTTF3 || Properties.ReactorState != ReactorState.Closed))
            {
                HandleCoilFlicker();
            }

            if (Properties.IsFueled || Properties.PhotoWormholeActive)
            {
                if (ModSettings.GlowingWormholeEmitter)
                {
                    Mods.GlowingEmitter = ModState.On;
                }
                HandleSparks();
            }

            if (Mods.WormholeType != WormholeType.BTTF2 && !Properties.IsFlying && !Particles.WheelsFire.IsPlaying)
            {
                Particles.WheelsFire.Play();
            }

            if (Mods.WormholeType == WormholeType.BTTF3)
            {
                if (!Properties.IsFlying && !Particles.WheelsSparks.IsPlaying)
                {
                    Particles.WheelsSparks.Play();
                }

                if (!Particles.Sparks.IsPlaying)
                {
                    Particles.Sparks?.Play();
                }
            }

            // Some wormhole sparks logic.
            if (Game.GameTime >= _startWormholeAt && _hasStartedWormhole)
            {
                // Draw the wormhole RenderTarget, so that the animation appears on the prop
                Scaleforms.WormholeRT.Draw();

                if (!Scaleforms.WormholeRT.Prop.IsSpawned && _playWormhole && (Vehicle.GetMPHSpeed() >= Constants.TimeTravelAtSpeed || Properties.PhotoWormholeActive))
                {
                    _wheSpark.Play();
                    _leftSpark.Play();
                    _rightSpark.Play();
                    Scaleforms.WormholeRT.CreateProp();
                    ScaleformsHandler.WormholeScaleforms[Constants.WormholeScaleformIndex].CallFunction("START_ANIMATION");
                    _hasStartedWormhole = true;
                }

                if (Mods.WormholeType != WormholeType.BTTF3 && Game.GameTime >= _endAt && _playWormhole && !Properties.PhotoWormholeActive)
                {
                    OnPlayerCompleted?.Invoke();
                    Stop();

                    return;
                }
            }

            if (!_hasStartedWormhole && _playWormhole && (Vehicle.GetMPHSpeed() >= Constants.TimeTravelAtSpeed || Properties.PhotoWormholeActive))
            {
                _startWormholeAt = Game.GameTime + 1000;
                _endAt = _startWormholeAt + Constants.WormholeLengthTime;
                _hasStartedWormhole = true;
            }
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
