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
        public const float MAX_POSITION_OFFSET = 0.18f;
        public const float MAX_ROTATION_OFFSET = -90f;

        public bool IsWheelsOpen { get; private set; }

        public OnAnimCompleted OnAnimCompleted { get; set; }

        private WheelType _roadWheel;

        private AnimatePropsHandler AllProps = new AnimatePropsHandler();

        private AnimatePropsHandler GlowWheels = new AnimatePropsHandler();
        private AnimatePropsHandler Pistons = new AnimatePropsHandler();

        private AnimatePropsHandler LeftWheels = new AnimatePropsHandler();
        private AnimatePropsHandler LeftStruts = new AnimatePropsHandler();
        private AnimatePropsHandler LeftDisks = new AnimatePropsHandler();
        
        private AnimatePropsHandler RightWheels = new AnimatePropsHandler();
        private AnimatePropsHandler RightStruts = new AnimatePropsHandler();
        private AnimatePropsHandler RightDisks = new AnimatePropsHandler();
        
        private bool _firstAnimation = true;

        private int currentStep;
        private float currentRotation;
        private float currentPosition;

        //0.4681351f
        public Vector3 strutFrontOffset = new Vector3(-0.5655205f, 1.267366f, 0.2580211f);
        public Vector3 diskOffsetFromStrut = new Vector3(-0.23691f, 0.002096051f, -0.1387549f);
        public Vector3 pistonOffsetFromDisk = new Vector3(-0.05293572f, -0.002848367f, 0.0005371129f);

        public Vector3 strutRearOffset = new Vector3(-0.5655205f, -1.200989f, 0.2580211f);
        public Vector3 diskOffsetFromRearStrut = new Vector3(-0.2380092f, 0.002005455f, -0.1414804f);
        public Vector3 pistonOffsetFromRearDisk = new Vector3(-0.05293572f, -0.002848367f, 0.0005371129f);

        public WheelAnimationPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            _roadWheel = Properties.IsStockWheel ? WheelType.Stock : WheelType.Red;

            Model modelWheel = _roadWheel == WheelType.Stock ? ModelHandler.WheelProp : ModelHandler.RedWheelProp;
            Model modelWheelRear = _roadWheel == WheelType.Stock ? ModelHandler.RearWheelProp : ModelHandler.RedWheelProp;
            
            foreach (var wheel in Vehicle.GetWheelPositions())
            {
                string wheelName = wheel.Key;

                bool leftWheel = (wheelName.Contains("lf") || wheelName.Contains("lr"));
                bool frontWheel = (wheelName.Contains("lf") || wheelName.Contains("rf"));

                Model wheelModel = !frontWheel ? modelWheelRear : modelWheel;
                Model wheelGlowModel = !frontWheel ? ModelHandler.RearWheelGlowing : ModelHandler.WheelGlowing;
                Vector3 strutOffset = Vector3.Zero;

                ModelHandler.RequestModel(wheelModel);
                ModelHandler.RequestModel(wheelGlowModel);

                if (leftWheel && frontWheel)
                    strutOffset = strutFrontOffset;
                else if (leftWheel && !frontWheel)
                    strutOffset = strutRearOffset;
                else if (!leftWheel && frontWheel)
                    strutOffset = new Vector3(-strutFrontOffset.X, strutFrontOffset.Y, strutFrontOffset.Z);
                else if (!leftWheel && !frontWheel)
                    strutOffset = new Vector3(-strutRearOffset.X, strutRearOffset.Y, strutRearOffset.Z);
                
                AnimateProp wheelGlowAnimateProp = new AnimateProp(null, wheelGlowModel, Vector3.Zero, Vector3.Zero);

                AnimateProp strut = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.Strut), strutOffset, leftWheel ? Vector3.Zero : new Vector3(0, 0, 180));
                strut.SpawnProp();

                AnimateProp disk = new AnimateProp(strut.Prop, ModelHandler.RequestModel(ModelHandler.Disk), frontWheel ? diskOffsetFromStrut : diskOffsetFromRearStrut, new Vector3(0, leftWheel ? 90 : -90, 0));
                disk.SpawnProp();

                AnimateProp piston = new AnimateProp(disk.Prop, ModelHandler.RequestModel(ModelHandler.Piston), frontWheel ? pistonOffsetFromDisk : pistonOffsetFromRearDisk, Vector3.Zero);
                piston.SpawnProp();

                AnimateProp wheelAnimateProp = new AnimateProp(disk.Prop, wheelModel, Vector3.Zero, new Vector3(0, -90, 0));

                if (leftWheel)
                {
                    LeftStruts.Props.Add(strut);
                    LeftDisks.Props.Add(disk);
                    LeftWheels.Props.Add(wheelAnimateProp);
                }
                else
                {
                    RightStruts.Props.Add(strut);
                    RightDisks.Props.Add(disk);
                    RightWheels.Props.Add(wheelAnimateProp);
                }

                GlowWheels.Props.Add(wheelGlowAnimateProp);
                Pistons.Props.Add(piston);

                AllProps.Props.Add(strut);
                AllProps.Props.Add(piston);
                AllProps.Props.Add(disk);
                AllProps.Props.Add(wheelGlowAnimateProp);
            }

            ApplyAnimation();
        }

        private void ReloadWheelModels()
        {
            if (_roadWheel == Mods.Wheel)
                return;

            _roadWheel = Properties.IsStockWheel ? WheelType.Stock : WheelType.Red;

            Model modelWheel = _roadWheel == WheelType.Stock ? ModelHandler.WheelProp : ModelHandler.RedWheelProp;
            Model modelWheelRear = _roadWheel == WheelType.Stock ? ModelHandler.RearWheelProp : ModelHandler.RedWheelProp;

            foreach (var wheel in Vehicle.GetWheelPositions())
            {
                string wheelName = wheel.Key;
                
                bool leftWheel = (wheelName.Contains("lf") || wheelName.Contains("lr"));
                bool frontWheel = (wheelName.Contains("lf") || wheelName.Contains("rf"));

                Model wheelModel = !frontWheel ? modelWheelRear : modelWheel;

                ModelHandler.RequestModel(wheelModel);

                if (leftWheel)
                {
                    if (frontWheel)
                        LeftWheels.Props[0].SwapModel(wheelModel);
                    else
                        LeftWheels.Props[1].SwapModel(wheelModel);
                }
                else
                {
                    if (frontWheel)
                        RightWheels.Props[0].SwapModel(wheelModel);
                    else
                        RightWheels.Props[1].SwapModel(wheelModel);
                }
            }
        }

        public override void Play()
        {
            Play(true);
        }

        public override void Stop()
        {
            GlowWheels.Delete();

            IsPlaying = false;
            currentStep = 0;
            PlayerSwitch.Disable = false;
        }

        public void Play(bool open)
        {
            Stop();

            IsWheelsOpen = open;

            if (IsWheelsOpen)
                ReloadWheelModels();

            Mods.Wheel = _roadWheel.GetVariantWheelType();

            IsPlaying = true;

            PlayerSwitch.Disable = true;                    
        }

        public void SetInstant(bool open, bool trueInstant = true)
        {
            Stop();

            IsWheelsOpen = open;

            if (IsWheelsOpen)
            {
                if (trueInstant)
                {
                    ReloadWheelModels();
                    Mods.Wheel = _roadWheel.GetVariantWheelType();
                }
                                   
                SetAnimationPosition();
            }
            else
            {
                SetAnimationPosition();

                if (!Properties.IsLanding)
                {
                    LeftWheels.Delete();
                    RightWheels.Delete();

                    Mods.Wheel = _roadWheel;
                }                
            }
        }

        private void SetAnimationPosition()
        {
            if (IsWheelsOpen)
            {
                currentPosition = MAX_POSITION_OFFSET;
                currentRotation = MAX_ROTATION_OFFSET;
            }
            else
            {
                currentPosition = 0;
                currentRotation = 0;
            }

            ApplyAnimation();

            if (IsWheelsOpen && _roadWheel == WheelType.Stock)
            {
                for (int i = 0; i < 4; i++)
                    if (i < 2)
                        GlowWheels.Props[i].Entity = LeftWheels.Props[i].Prop;                
                    else
                        GlowWheels.Props[i].Entity = RightWheels.Props[i-2].Prop;

                GlowWheels.SpawnProp();
            }
        }

        public override void Process()
        {
            if (!IsPlaying)
                return;

            ApplyAnimation();

            switch (currentStep)
            {
                case 0:
                    if (IsWheelsOpen)
                    {
                        // Strut stuff
                        float offsetToAdd = (MAX_POSITION_OFFSET * Game.LastFrameTime) / 0.75f;
                        currentPosition += offsetToAdd;

                        if (currentPosition > MAX_POSITION_OFFSET)
                            currentStep++;
                    }
                    else
                    {
                        // Wheel stuff
                        float numToAdd = (-MAX_ROTATION_OFFSET * Game.LastFrameTime) / 0.75f;
                        currentRotation += numToAdd;

                        if (currentRotation >= 0)
                            currentStep++;
                    }
                    break;
                case 1:
                    if (IsWheelsOpen)
                    {
                        // Wheel stuff
                        float numToAdd = (-MAX_ROTATION_OFFSET * Game.LastFrameTime) / 0.75f;
                        currentRotation -= numToAdd;

                        if (currentRotation < MAX_ROTATION_OFFSET)
                        {
                            SetInstant(true, false);
                            OnAnimCompleted.Invoke();
                        }
                    }
                    else
                    {
                        // Strut stuff
                        float offsetToAdd = (MAX_POSITION_OFFSET * Game.LastFrameTime) / 0.75f;
                        currentPosition -= offsetToAdd;

                        if (currentPosition < 0)
                        {
                            SetInstant(false, false);
                            OnAnimCompleted.Invoke();
                        }
                    }
                    break;
            }
        }

        public void ApplyAnimation()
        {
            if (!_firstAnimation)
            {
                if (!LeftWheels.IsSpawned)
                    LeftWheels.SpawnProp();

                if (!RightWheels.IsSpawned)
                    RightWheels.SpawnProp();
            }

            LeftStruts.MoveProp(new Vector3(-currentPosition, 0, 0), Vector3.Zero, false);
            RightStruts.MoveProp(new Vector3(currentPosition, 0, 0), Vector3.Zero, false);

            LeftDisks.MoveProp(Vector3.Zero, new Vector3(0, currentRotation, 0), false);
            RightDisks.MoveProp(Vector3.Zero, new Vector3(0, currentRotation - 180f, 0), false);

            Pistons.MoveProp(Vector3.Zero, new Vector3(0, -90 - currentRotation, 0), false);

            if (_firstAnimation)
                _firstAnimation = false;
        }

        public override void Dispose()
        {
            AllProps.Dispose();
            LeftWheels.Dispose();
            RightWheels.Dispose();
        }
    }
}