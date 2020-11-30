using System;
using System.Collections.Generic;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Players
{
    public delegate void OnAnimCompleted();

    public class WheelAnimationPlayer : Player
    {
        public const float MAX_POSITION_OFFSET = 0.20f;
        public const float MAX_ROTATION_OFFSET = -90f;

        public bool IsWheelsOpen { get; private set; }

        private AnimationStep WheelAnimation = AnimationStep.Off;

        public OnAnimCompleted OnAnimCompleted { get; set; }

        private WheelType _roadWheel;

        private AnimatePropsHandler AllProps = new AnimatePropsHandler();

        private AnimatePropsHandler GlowWheels = new AnimatePropsHandler();
        private AnimatePropsHandler Pistons = new AnimatePropsHandler();
        private AnimatePropsHandler Wheels = new AnimatePropsHandler();
        private AnimatePropsHandler Disks = new AnimatePropsHandler();
        private AnimatePropsHandler Struts = new AnimatePropsHandler();

        public Vector3 strutFrontOffset = new Vector3(-0.5455205f, 1.267366f, 0.2580211f);
        public Vector3 diskOffsetFromStrut = new Vector3(-0.23691f, 0.002096051f, -0.1387549f);
        public Vector3 pistonOffsetFromDisk = new Vector3(-0.05293572f, -0.002848367f, 0.0005371129f);

        public Vector3 strutRearOffset = new Vector3(-0.5455205f, -1.200989f, 0.2580211f);
        public Vector3 diskOffsetFromRearStrut = new Vector3(-0.2380092f, 0.002005455f, -0.1414804f);
        public Vector3 pistonOffsetFromRearDisk = new Vector3(-0.05293572f, -0.002848367f, 0.0005371129f);

        public WheelAnimationPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
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

                ModelHandler.RequestModel(wheelModel);
                ModelHandler.RequestModel(wheelGlowModel);

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

                AnimateProp strut = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.Strut), strutOffset, leftWheel ? Vector3.Zero : new Vector3(0, 0, 180));

                if (leftWheel)
                    strut.setOffsetSettings(Coordinate.X, false, false, strutOffset.X - MAX_POSITION_OFFSET, strutOffset.X, 1, 0.24f, 1, true);
                else
                    strut.setOffsetSettings(Coordinate.X, false, true, strutOffset.X, strutOffset.X + MAX_POSITION_OFFSET, 1, 0.24f, 1, true);

                strut.AnimationStopped += AnimationStopped;
                strut.SpawnProp();

                AnimateProp disk = new AnimateProp(strut.Prop, ModelHandler.RequestModel(ModelHandler.Disk), frontWheel ? diskOffsetFromStrut : diskOffsetFromRearStrut, new Vector3(0, 90, 0));
                disk.setRotationSettings(Coordinate.Y, false, false, 0, 90, 1, 120, 1, true);
                disk.AnimationStopped += AnimationStopped;
                disk.SpawnProp();

                AnimateProp piston = new AnimateProp(disk.Prop, ModelHandler.RequestModel(ModelHandler.Piston), frontWheel ? pistonOffsetFromDisk : pistonOffsetFromRearDisk, new Vector3(0, -90, 0));
                piston.setRotationSettings(Coordinate.Y, false, true, -90, 0, 1, 120, 1, true);
                piston.SpawnProp();

                AnimateProp wheelAnimateProp = new AnimateProp(disk.Prop, wheelModel, Vector3.Zero, new Vector3(0, -90, 0));

                AnimateProp wheelGlowAnimateProp = new AnimateProp(null, wheelGlowModel, Vector3.Zero, Vector3.Zero);

                Struts.Props.Add(strut);
                GlowWheels.Props.Add(wheelGlowAnimateProp);
                Pistons.Props.Add(piston);
                Wheels.Props.Add(wheelAnimateProp);
                Disks.Props.Add(disk);

                AllProps.Props.Add(strut);
                AllProps.Props.Add(piston);
                AllProps.Props.Add(disk);
                AllProps.Props.Add(wheelGlowAnimateProp);
            }
        }

        public void AnimationStopped(AnimateProp animateProp, Coordinate coordinate, CoordinateSetting coordinateSetting, bool IsRotation)
        {
            if (!IsPlaying)
                return;

            if (IsWheelsOpen)
            {
                switch (WheelAnimation)
                {
                    case AnimationStep.First:
                        if (Struts.getOffsetUpdate(Coordinate.X))
                            return;

                        Disks.setRotationIncreasing(Coordinate.Y, false);
                        Pistons.setRotationIncreasing(Coordinate.Y, true);

                        Disks.setRotationUpdate(Coordinate.Y, true);
                        Pistons.setRotationUpdate(Coordinate.Y, true);

                        WheelAnimation = AnimationStep.Second;
                        break;
                    case AnimationStep.Second:
                        if (Disks.getRotationUpdate(Coordinate.Y) | Pistons.getRotationUpdate(Coordinate.Y))
                            return;

                        Stop();
                        OnAnimCompleted.Invoke();
                        break;
                }
            }
            else
            {
                switch (WheelAnimation)
                {
                    case AnimationStep.First:
                        if (Disks.getRotationUpdate(Coordinate.Y) | Pistons.getRotationUpdate(Coordinate.Y))
                            return;

                        Struts[0].setOffsetIncreasing(Coordinate.X, true);
                        Struts[1].setOffsetIncreasing(Coordinate.X, false);
                        Struts[2].setOffsetIncreasing(Coordinate.X, true);
                        Struts[3].setOffsetIncreasing(Coordinate.X, false);

                        Struts.setOffsetUpdate(Coordinate.X, true);

                        WheelAnimation = AnimationStep.Second;
                        break;
                    case AnimationStep.Second:
                        if (Struts.getOffsetUpdate(Coordinate.X))
                            return;

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
            WheelAnimation = AnimationStep.Off;
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

                Struts[0].setOffsetAtMinimum(Coordinate.X);
                Struts[1].setOffsetAtMaximum(Coordinate.X);
                Struts[2].setOffsetAtMinimum(Coordinate.X);
                Struts[3].setOffsetAtMaximum(Coordinate.X);

                Disks.setRotationAtMinimum(Coordinate.Y);
                Pistons.setRotationAtMaximum(Coordinate.Y);
            }
            else
            {
                Struts[0].setOffsetAtMaximum(Coordinate.X);
                Struts[1].setOffsetAtMinimum(Coordinate.X);
                Struts[2].setOffsetAtMaximum(Coordinate.X);
                Struts[3].setOffsetAtMinimum(Coordinate.X);

                Disks.setRotationAtMaximum(Coordinate.Y);
                Pistons.setRotationAtMinimum(Coordinate.Y);

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

            WheelAnimation = AnimationStep.First;

            if (IsWheelsOpen)
                ReloadWheelModels();
            else
                GlowWheels.Delete();

            Mods.Wheel = _roadWheel.GetVariantWheelType();

            IsPlaying = true;

            if (IsWheelsOpen && !Wheels.IsSpawned)
                Wheels.SpawnProp();

            if (open)
            {
                Struts[0].setOffsetIncreasing(Coordinate.X, false);
                Struts[1].setOffsetIncreasing(Coordinate.X, true);
                Struts[2].setOffsetIncreasing(Coordinate.X, false);
                Struts[3].setOffsetIncreasing(Coordinate.X, true);
               
                Struts.setOffsetUpdate(Coordinate.X, true);
            } 
            else
            {
                Disks.setRotationIncreasing(Coordinate.Y, true);
                Pistons.setRotationIncreasing(Coordinate.Y, false);

                Disks.setRotationUpdate(Coordinate.Y, true);
                Pistons.setRotationUpdate(Coordinate.Y, true);
            }

            PlayerSwitch.Disable = true;                    
        }

        public override void Process()
        {

        }

        public override void Dispose()
        {
            AllProps.Dispose();
            Wheels.Dispose();
        }
    }
}