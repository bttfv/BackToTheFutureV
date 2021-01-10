using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using GTA.Math;
using System;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Players
{
    public delegate void OnAnimCompleted();

    public class WheelAnimationPlayer : Player
    {
        public const float MAX_POSITION_OFFSET = 0.20f;
        public const float MAX_ROTATION_OFFSET = -90f;

        public bool IsWheelsOpen { get; private set; }

        public OnAnimCompleted OnAnimCompleted { get; set; }

        private WheelType _roadWheel;

        private AnimatePropsHandler AllProps = new AnimatePropsHandler();

        private AnimatePropsHandler GlowWheels = new AnimatePropsHandler();
        private AnimatePropsHandler Wheels = new AnimatePropsHandler();

        public Vector3 strutFrontOffset = new Vector3(-0.5455205f, 1.267366f, 0.2580211f);
        public Vector3 diskOffsetFromStrut = new Vector3(-0.23691f, 0.002096051f, -0.1387549f);
        public Vector3 pistonOffsetFromDisk = new Vector3(-0.05293572f, -0.002848367f, 0.0005371129f);

        public Vector3 strutRearOffset = new Vector3(-0.5455205f, -1.200989f, 0.2580211f);
        public Vector3 diskOffsetFromRearStrut = new Vector3(-0.2380092f, 0.002005455f, -0.1414804f);
        public Vector3 pistonOffsetFromRearDisk = new Vector3(-0.05293572f, -0.002848367f, 0.0005371129f);

        public WheelAnimationPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            AllProps.OnAnimCompleted += OnAnimationCompleted;

            _roadWheel = Properties.IsStockWheel ? WheelType.Stock : WheelType.Red;

            Model modelWheel = _roadWheel == WheelType.Stock ? ModelHandler.WheelProp : ModelHandler.RedWheelProp;
            Model modelWheelRear = _roadWheel == WheelType.Stock ? ModelHandler.RearWheelProp : ModelHandler.RedWheelProp;
            
            foreach (WheelId wheel in Enum.GetValues(typeof(WheelId)))
            {
                bool leftWheel = wheel == WheelId.FrontLeft | wheel == WheelId.RearLeft;
                bool frontWheel = wheel == WheelId.FrontLeft | wheel == WheelId.FrontRight;

                Model wheelModel = frontWheel ? modelWheel : modelWheelRear;
                Model wheelGlowModel = frontWheel ? ModelHandler.WheelGlowing : ModelHandler.RearWheelGlowing;

                Vector3 strutOffset = Vector3.Zero;

                switch (wheel)
                {
                    case WheelId.FrontLeft:
                        strutOffset = strutFrontOffset;
                        break;
                    case WheelId.FrontRight:
                        strutOffset = new Vector3(-strutFrontOffset.X, strutFrontOffset.Y, strutFrontOffset.Z);
                        break;
                    case WheelId.RearLeft:
                        strutOffset = strutRearOffset;
                        break;
                    case WheelId.RearRight:
                        strutOffset = new Vector3(-strutRearOffset.X, strutRearOffset.Y, strutRearOffset.Z);
                        break;
                }

                AnimateProp strut = new AnimateProp(Vehicle, ModelHandler.Strut, strutOffset, leftWheel ? Vector3.Zero : new Vector3(0, 0, 180));
                if (leftWheel)
                    strut[AnimationType.Offset][AnimationStep.First][Coordinate.X].Setup(true, true, false, strutOffset.X - MAX_POSITION_OFFSET, strutOffset.X, 1, 0.24f, 1);
                else
                    strut[AnimationType.Offset][AnimationStep.First][Coordinate.X].Setup(true, true, true, strutOffset.X, strutOffset.X + MAX_POSITION_OFFSET, 1, 0.24f, 1);                
                strut.SpawnProp();

                AnimateProp disk = new AnimateProp(strut, ModelHandler.Disk, frontWheel ? diskOffsetFromStrut : diskOffsetFromRearStrut, new Vector3(0, 90, 0));
                disk[AnimationType.Rotation][AnimationStep.Second][Coordinate.Y].Setup(true, true, false, 0, 90, 1, 120, 1);                
                disk.SpawnProp();

                AnimateProp piston = new AnimateProp(disk, ModelHandler.Piston, frontWheel ? pistonOffsetFromDisk : pistonOffsetFromRearDisk, new Vector3(0, -90, 0));
                piston[AnimationType.Rotation][AnimationStep.Second][Coordinate.Y].Setup(true, true, true, -90, 0, 1, 120, 1);
                piston.SpawnProp();

                AnimateProp wheelAnimateProp = new AnimateProp(disk, wheelModel, Vector3.Zero, new Vector3(0, -90, 0));

                AnimateProp wheelGlowAnimateProp = new AnimateProp(null, wheelGlowModel, Vector3.Zero, Vector3.Zero);

                GlowWheels.Add(wheelGlowAnimateProp);
                Wheels.Add(wheelAnimateProp);
                                                             
                AllProps.Add(strut);
                AllProps.Add(piston);
                AllProps.Add(disk);
                AllProps.Add(wheelGlowAnimateProp);                
            }
        }

        public void OnAnimationCompleted(AnimationStep animationStep)
        {
            if (!IsPlaying)
                return;

            if (IsWheelsOpen)
            {
                switch (animationStep)
                {
                    case AnimationStep.First:
                        AllProps.Play(AnimationStep.Second);
                        break;
                    case AnimationStep.Second:
                        Stop();
                        OnAnimCompleted.Invoke();
                        break;
                }
            }
            else
            {
                switch (animationStep)
                {
                    case AnimationStep.Second:
                        AllProps.Play();
                        break;
                    case AnimationStep.First:
                        Stop();
                        OnAnimCompleted.Invoke();
                        break;
                }
            }
        }

        private void ReloadWheelModels()
        {
            if (_roadWheel == Mods.Wheel)
                return;

            _roadWheel = Properties.IsStockWheel ? WheelType.Stock : WheelType.Red;

            Model modelWheel = _roadWheel == WheelType.Stock ? ModelHandler.WheelProp : ModelHandler.RedWheelProp;
            Model modelWheelRear = _roadWheel == WheelType.Stock ? ModelHandler.RearWheelProp : ModelHandler.RedWheelProp;

            Wheels[0].SwapModel(modelWheel);
            Wheels[1].SwapModel(modelWheel);
            Wheels[2].SwapModel(modelWheelRear);
            Wheels[3].SwapModel(modelWheelRear);
        }

        public override void Play()
        {
            Play(true);
        }

        public override void Stop()
        {
            if (IsWheelsOpen)
            {
                if ( _roadWheel == WheelType.Stock)
                {
                    for (int i = 0; i < 4; i++)
                        GlowWheels[i].TransferTo(Wheels[i]);

                    GlowWheels.SpawnProp();
                }
            }
            else
            {
                if (!Properties.IsLanding)
                {
                    Wheels.Delete();

                    ReloadWheelModels();

                    Mods.Wheel = _roadWheel;
                }
            }                

            IsPlaying = false;
            PlayerSwitch.Disable = false;
        }

        public void SetInstant(bool open)
        {
            IsWheelsOpen = open;

            if (IsWheelsOpen)
            {
                ReloadWheelModels();

                Mods.Wheel = _roadWheel.GetVariantWheelType();

                if (!Wheels.IsSpawned)
                    Wheels.SpawnProp();

                AllProps.Play(AnimationStep.Second, true, true);
            }
            else
            {
                AllProps.Play(AnimationStep.Second, true, true);

                Wheels.Delete();

                Mods.Wheel = _roadWheel;
            }
        }

        public void Play(bool open)
        {
            Stop();

            if (IsWheelsOpen == open)
                return;

            IsWheelsOpen = open;

            if (IsWheelsOpen)
                ReloadWheelModels();
            else
                GlowWheels.Delete();

            Mods.Wheel = _roadWheel.GetVariantWheelType();

            IsPlaying = true;

            if (IsWheelsOpen && !Wheels.IsSpawned)
                Wheels.SpawnProp();

            if (open)
                AllProps.Play();
            else
                AllProps.Play(AnimationStep.Second);

            PlayerSwitch.Disable = true;
        }

        public override void Process()
        {
            
        }

        public override void Dispose()
        {
            AllProps.Dispose();
            Wheels.Dispose();
            GlowWheels.Dispose();
        }
    }
}