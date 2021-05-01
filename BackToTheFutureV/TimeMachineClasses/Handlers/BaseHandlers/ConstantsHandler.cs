using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class ConstantsHandler : HandlerPrimitive
    {
        public bool HasScaleformPriority => TimeMachineHandler.ClosestTimeMachine == TimeMachine;

        public bool Over88MphSpeed { get; private set; }

        public bool OverTimeTravelAtSpeed { get; private set; }
        public int TimeTravelAtSpeed
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return 88;
                    case WormholeType.BTTF2:
                        return 88;
                    case WormholeType.BTTF3:
                        return Properties.IsOnTracks ? 82 : 65;
                    default:
                        return 88;
                }
            }
        }

        public int TimeTravelAtTime { get; private set; }
        public int StabilizationSoundAtTime { get; private set; }
        public float TimeTravelCooldown { get; private set; } = -1;

        public int WormholeLengthTime
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return 6000;
                    case WormholeType.BTTF2:
                        return 3900;
                    case WormholeType.BTTF3:
                        return 5200;
                    default:
                        return 5200;
                }
            }
        }

        public bool OverSIDMaxAtSpeed { get; private set; }
        public int SIDMaxAtSpeed
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return 82;
                    case WormholeType.BTTF2:
                        return 82;
                    case WormholeType.BTTF3:
                        return Properties.IsOnTracks ? 82 : 59;
                    default:
                        return 82;
                }
            }
        }

        public string WormholeRenderTargetName
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return "bttf_wormhole";
                    case WormholeType.BTTF2:
                        return "bttf_wormhole_blue";
                    case WormholeType.BTTF3:
                        return "bttf_wormhole_red";
                    default:
                        return "bttf_wormhole";
                }
            }
        }

        public int WormholeScaleformIndex
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return 0;
                    case WormholeType.BTTF2:
                        return 1;
                    case WormholeType.BTTF3:
                        return 2;
                    default:
                        return 0;
                }
            }
        }

        public CustomModel WormholeModel
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return TimeHandler.IsNight ? ModelHandler.WormholeVioletNight : ModelHandler.WormholeViolet;
                    case WormholeType.BTTF2:
                        return TimeHandler.IsNight ? ModelHandler.WormholeBlueNight : ModelHandler.WormholeBlue;
                    case WormholeType.BTTF3:
                        return TimeHandler.IsNight ? ModelHandler.WormholeRedNight : ModelHandler.WormholeRed;
                    default:
                        return TimeHandler.IsNight ? ModelHandler.WormholeVioletNight : ModelHandler.WormholeViolet;
                }
            }
        }

        public CustomModel SparkModel
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return TimeHandler.IsNight ? ModelHandler.SparkNightModel : ModelHandler.SparkModel;
                    case WormholeType.BTTF2:
                        return TimeHandler.IsNight ? ModelHandler.SparkNightModel : ModelHandler.SparkModel;
                    case WormholeType.BTTF3:
                        return TimeHandler.IsNight ? ModelHandler.SparkRedNightModel : ModelHandler.SparkRedModel;
                    default:
                        return TimeHandler.IsNight ? ModelHandler.SparkNightModel : ModelHandler.SparkModel;
                }
            }
        }

        public CustomModel CoilsModel
        {
            get
            {
                if (TimeHandler.IsNight)
                    return ModelHandler.CoilsGlowingNight;

                return ModelHandler.CoilsGlowing;
            }
        }

        public int MaxReactorCharge
        {
            get
            {
                if (Mods.Reactor == ReactorType.Nuclear)
                    return 1;

                return 3;
            }
        }

        public int FireTrailsAppearTime => (FireTrailsIs99 || Properties.IsFlying) ? -1 : 0;

        public float FireTrailsDisappearTime => (FireTrailsIs99 || Properties.IsFlying) ? 1 : 15;

        public bool FireTrailsUseBlue => Mods.WormholeType == WormholeType.BTTF1;

        public bool FireTrailsIs99 => Properties.IsFlying && Properties.HasBeenStruckByLightning;

        public int FireTrailsLength => Properties.IsOnTracks ? 100 : 50;

        public string LowerWormholeType => Mods.WormholeType.ToString().ToLower();
        public bool IsStockWheel => Mods.Wheel == WheelType.Stock || Mods.Wheel == WheelType.StockInvisible || Mods.Wheel == WheelType.DMC || Mods.Wheel == WheelType.DMCInvisible;
        public bool FullDamaged => Mods.Wheel == WheelType.Stock && Mods.Wheels.Burst && Vehicle.EngineHealth <= 0 && Properties.AreFlyingCircuitsBroken && Properties.AreTimeCircuitsBroken;

        public WheelType RoadWheel
        {
            get
            {
                if (IsStockWheel)
                {
                    if (Mods.Wheel == WheelType.Stock || Mods.Wheel == WheelType.StockInvisible)
                        return WheelType.Stock;
                    else
                        return WheelType.DMC;
                }
                else
                    return WheelType.Red;
            }
        }

        public CustomModel WheelModel => IsStockWheel ? ModelHandler.WheelProp : ModelHandler.RedWheelProp;
        public CustomModel WheelRearModel => IsStockWheel ? ModelHandler.RearWheelProp : ModelHandler.RedWheelProp;

        public ConstantsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnLightningStrike += StartTimeTravelCooldown;
            Events.OnTimeTravelStarted += StartTimeTravelCooldown;
            Events.OnReenterEnded += StartTimeTravelCooldown;
            Events.OnTimeCircuitsToggle += () =>
            {
                ResetAll();
            };
        }

        public void StartTimeTravelCooldown()
        {
            TimeTravelCooldown = 0;
            ResetAll();
        }

        public void ResetAll()
        {
            TimeTravelAtTime = 0;
            StabilizationSoundAtTime = 0;

            Over88MphSpeed = false;
            OverSIDMaxAtSpeed = false;
            OverTimeTravelAtSpeed = false;
        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Tick()
        {
            if (TimeTravelCooldown > -1)
            {
                TimeTravelCooldown += Game.LastFrameTime;

                if (TimeTravelCooldown >= 30)
                    TimeTravelCooldown = -1;
                else
                    return;
            }

            if (Properties.BlockSparks || Properties.HasBeenStruckByLightning || Properties.IsPhotoModeOn || !Properties.AreTimeCircuitsOn)
                return;

            if (Vehicle.GetMPHSpeed() >= SIDMaxAtSpeed && !OverSIDMaxAtSpeed)
            {
                OverSIDMaxAtSpeed = true;
                Events.OnSIDMaxSpeedReached?.Invoke(true);
            }

            if (Vehicle.GetMPHSpeed() < SIDMaxAtSpeed && OverSIDMaxAtSpeed)
            {
                TimeTravelAtTime = 0;
                StabilizationSoundAtTime = 0;

                OverSIDMaxAtSpeed = false;
                Events.OnSIDMaxSpeedReached?.Invoke(false);
            }

            if (Vehicle.GetMPHSpeed() >= TimeTravelAtSpeed && !OverTimeTravelAtSpeed)
            {
                OverTimeTravelAtSpeed = true;
                Events.OnTimeTravelSpeedReached?.Invoke(true);
            }

            if (Vehicle.GetMPHSpeed() < TimeTravelAtSpeed && OverTimeTravelAtSpeed)
            {
                OverTimeTravelAtSpeed = false;
                Events.OnTimeTravelSpeedReached?.Invoke(false);
            }

            if (Vehicle.GetMPHSpeed() >= 88 && !Over88MphSpeed)
            {
                TimeTravelAtTime = Game.GameTime + WormholeLengthTime;
                StabilizationSoundAtTime = Game.GameTime + 1000;

                Over88MphSpeed = true;
            }

            if (Vehicle.GetMPHSpeed() < 88 && Over88MphSpeed)
                Over88MphSpeed = false;
        }

        public override void Stop()
        {

        }
    }
}
