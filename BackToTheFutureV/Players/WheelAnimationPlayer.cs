using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using System;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class WheelAnimationPlayer : Players.Player
    {
        private const float MAX_POSITION_OFFSET = 0.255f;
        private const float MAX_ROTATION_OFFSET = 90f;

        private bool AreWheelsOpen;

        private AnimatePropsHandler AllProps = new AnimatePropsHandler();

        private AnimatePropsHandler GlowWheels = new AnimatePropsHandler();
        private AnimatePropsHandler Wheels = new AnimatePropsHandler();

        private static readonly Vector3 strutOffsetFromWheel = new Vector3(0.2247245f, -0.004109263f, 0.09079965f);
        private static readonly Vector3 diskOffsetFromStrut = new Vector3(-0.23691f, 0.002096051f, -0.1387549f);
        private static readonly Vector3 pistonOffsetFromDisk = new Vector3(-0.05293572f, -0.002848367f, 0.0005371129f);

        public WheelAnimationPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            AllProps.OnAnimCompleted += OnAnimationCompleted;

            foreach (CVehicleWheel wheel in Mods.Wheels)
            {
                wheel.Reset();

                Vector3 strutOffset = Vector3.Zero;

                switch (wheel.WheelID)
                {
                    case WheelId.FrontLeft:
                        strutOffset = strutOffsetFromWheel.GetSingleOffset(Coordinate.Z, Mods.IsDMC12 ? 0 : 0.04f);
                        break;
                    case WheelId.FrontRight:
                        strutOffset = strutOffsetFromWheel.GetSingleOffset(Coordinate.Z, Mods.IsDMC12 ? 0 : 0.04f).InvertCoordinate(Coordinate.X);
                        break;
                    case WheelId.RearLeft:
                        strutOffset = strutOffsetFromWheel.GetSingleOffset(Coordinate.Z, 0.02f);
                        break;
                    case WheelId.RearRight:
                        strutOffset = strutOffsetFromWheel.GetSingleOffset(Coordinate.Z, 0.02f).InvertCoordinate(Coordinate.X);
                        break;
                }

                strutOffset = wheel.GetRelativeOffsetPosition(strutOffset);

                AnimateProp strut = new AnimateProp(ModelHandler.Strut, Vehicle, strutOffset, wheel.Left ? Vector3.Zero : new Vector3(0, 0, 180));
                if (wheel.Left)
                    strut[AnimationType.Offset][AnimationStep.First][Coordinate.X].Setup(true, false, strutOffset.X - MAX_POSITION_OFFSET, strutOffset.X, 1, 0.30f, 1);
                else
                    strut[AnimationType.Offset][AnimationStep.First][Coordinate.X].Setup(true, true, strutOffset.X, strutOffset.X + MAX_POSITION_OFFSET, 1, 0.30f, 1);
                strut.SpawnProp();

                AnimateProp disk = new AnimateProp(ModelHandler.Disk, strut, diskOffsetFromStrut, new Vector3(0, MAX_ROTATION_OFFSET, 0));
                disk[AnimationType.Rotation][AnimationStep.Second][Coordinate.Y].Setup(true, false, 0, MAX_ROTATION_OFFSET, 1, 120, 1);
                disk.SpawnProp();

                AnimateProp piston = new AnimateProp(ModelHandler.Piston, disk, pistonOffsetFromDisk, new Vector3(0, -MAX_ROTATION_OFFSET, 0));
                piston[AnimationType.Rotation][AnimationStep.Second][Coordinate.Y].Setup(true, true, -MAX_ROTATION_OFFSET, 0, 1, 120, 1);
                piston.SpawnProp();

                Model wheelModel = wheel.Front ? Constants.WheelModel : Constants.WheelRearModel;
                Model wheelGlowModel = wheel.Front ? ModelHandler.WheelGlowing : ModelHandler.RearWheelGlowing;

                AnimateProp wheelAnimateProp;

                if (wheel.Front)
                    wheelAnimateProp = new AnimateProp(wheelModel, disk, new Vector3(0, 0, -0.03f), new Vector3(0, -MAX_ROTATION_OFFSET, 0));
                else
                    wheelAnimateProp = new AnimateProp(wheelModel, disk, new Vector3(0, 0, -0.035f), new Vector3(0, -MAX_ROTATION_OFFSET, 0));

                AnimateProp wheelGlowAnimateProp = new AnimateProp(wheelGlowModel, null, Vector3.Zero, Vector3.Zero);

                GlowWheels.Add(wheelGlowAnimateProp);
                Wheels.Add(wheelAnimateProp);

                AllProps.Add(strut);
                AllProps.Add(piston);
                AllProps.Add(disk);
                AllProps.Add(wheelGlowAnimateProp);
            }

            AllProps.SaveAnimation();
        }

        public void OnAnimationCompleted(AnimationStep animationStep)
        {
            if (!IsPlaying)
                return;

            if (AreWheelsOpen)
            {
                switch (animationStep)
                {
                    case AnimationStep.First:
                        AllProps.Play(AnimationStep.Second);
                        break;
                    case AnimationStep.Second:
                        Stop();
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
                        break;
                }
            }
        }

        private void ReloadWheelModels()
        {
            foreach (CVehicleWheel wheel in Mods.Wheels)
                Wheels[Mods.Wheels.IndexOf(wheel)].SwapModel(wheel.Front ? Constants.WheelModel : Constants.WheelRearModel);
        }

        public override void Play()
        {
            Play(true);
        }

        public override void Stop()
        {
            if (TimeMachineHandler.CurrentTimeMachine == TimeMachine)
                FusionUtils.StopPadShake();

            IsPlaying = false;
            PlayerSwitch.Disable = false;

            AllProps.RestoreAnimation();

            if (AreWheelsOpen)
            {
                AllProps.Play(AnimationStep.Second, true, true);

                if (Constants.RoadWheel == WheelType.Stock || Constants.RoadWheel == WheelType.DMC)
                {
                    for (int i = 0; i < Mods.Wheels.Count; i++)
                    {
                        GlowWheels[i].TransferTo(Wheels[i]);
                        Props.HoverModeWheelsGlow[i].TransferTo(Wheels[i]);
                    }

                    GlowWheels.SpawnProp();
                }
            }
            else
            {
                if (!Properties.IsLanding)
                {
                    Wheels.Delete();

                    Mods.Wheel = Constants.RoadWheel;
                }

                Props.HoverModeWheelsGlow?.Delete();
            }

            OnPlayerCompleted?.Invoke();
        }

        public void SetInstant(bool open)
        {
            AreWheelsOpen = open;

            if (AreWheelsOpen)
            {
                ReloadWheelModels();

                Mods.Wheel = Constants.RoadWheel.GetVariantWheelType();

                if (!Wheels.IsSpawned)
                    Wheels.SpawnProp();

                UpdateWheelsRotations();
            }

            Stop();
        }

        public void Play(bool open)
        {
            if (AreWheelsOpen == open)
                return;

            AreWheelsOpen = open;

            if (AreWheelsOpen)
                ReloadWheelModels();
            else
                GlowWheels.Delete();

            Mods.Wheel = Constants.RoadWheel.GetVariantWheelType();

            IsPlaying = true;

            if (AreWheelsOpen && !Wheels.IsSpawned)
            {
                UpdateWheelsRotations();
                Wheels.SpawnProp();
            }

            if (open)
                AllProps.Play();
            else
                AllProps.Play(AnimationStep.Second);

            PlayerSwitch.Disable = true;
        }

        public override void Tick()
        {
            if (IsPlaying && !FusionUtils.IsPadShaking && TimeMachineHandler.CurrentTimeMachine == TimeMachine)
                FusionUtils.SetPadShake(100, 80);

            if (IsPlaying)
                UpdateWheelsRotations();
        }

        private void UpdateWheelsRotations()
        {
            float[] WheelsRotations = VehicleControl.GetWheelRotations(Vehicle);

            for (int i = 0; i < WheelsRotations.Length; i++)
            {
                WheelsRotations[i] = FusionUtils.Wrap(WheelsRotations[i], -(float)Math.PI, (float)Math.PI).ToDeg();

                if (i % 2 == 1)
                {
                    WheelsRotations[i] -= 180;
                    WheelsRotations[i] *= -1;
                }

                Wheels[i].setRotation(Coordinate.X, WheelsRotations[i], true);
            }
        }

        public override void Dispose()
        {
            AllProps.Dispose();
            Wheels.Dispose();
            GlowWheels.Dispose();
            Props.HoverModeWheelsGlow?.Delete();
        }
    }
}