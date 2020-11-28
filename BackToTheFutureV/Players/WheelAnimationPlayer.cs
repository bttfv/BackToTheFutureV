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

namespace BackToTheFutureV.Players
{
    public delegate void OnAnimCompleted();

    public class WheelAnimationPlayer : Player
    {
        public const float MAX_OFFSET = -0.17f;
        public const float MAX_STRUT_OFFSET = -0.18f;
        public const float MAX_ROTATION = -90f;
        public const float MAX_PISTON_ROTATION = -90f;

        public bool IsWheelsOpen { get; private set; }

        public bool IsVisible => leftStruts[0].IsSpawned && leftStruts[0].Prop.IsVisible;

        public OnAnimCompleted OnAnimCompleted { get; set; }

        private WheelType _roadWheel;

        private List<AnimateProp> leftWheelProps = new List<AnimateProp>();
        private List<AnimateProp> rightWheelProps = new List<AnimateProp>();

        private List<AnimateProp> leftStruts = new List<AnimateProp>();
        private List<AnimateProp> leftPistons = new List<AnimateProp>();
        private List<AnimateProp> leftDisks = new List<AnimateProp>();

        private List<AnimateProp> rightStruts = new List<AnimateProp>();
        private List<AnimateProp> rightPistons = new List<AnimateProp>();
        private List<AnimateProp> rightDisks = new List<AnimateProp>();

        private List<AnimateProp> leftWheelGlowProps = new List<AnimateProp>();
        private List<AnimateProp> rightWheelGlowProps = new List<AnimateProp>();

        private bool _firstAnimation = true;

        private int currentStep;
        private float currentRotation;
        private float currentOffset;

        private float currentStrutOffset;
        private float currentPistonRotation;

        //0.4681351f
        public Vector3 strutFrontOffset = new Vector3(-0.5655205f, 1.267366f, 0.3080211f);
        public Vector3 diskOffsetFromStrut = new Vector3(-0.23691f, 0.002096051f, -0.1387549f);
        public Vector3 pistonOffsetFromDisk = new Vector3(-0.05293572f, -0.002848367f, 0.0005371129f);

        public Vector3 strutRearOffset = new Vector3(-0.5655205f, -1.200989f, 0.327969f);
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
                Vector3 offset = wheel.Value;
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

                AnimateProp wheelAnimateProp = new AnimateProp(Vehicle, wheelModel, offset, Vector3.Zero);

                AnimateProp wheelGlowAnimateProp = new AnimateProp(null, wheelGlowModel, Vector3.Zero, Vector3.Zero);

                AnimateProp strut = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.Strut), strutOffset, leftWheel ? Vector3.Zero : new Vector3(0, 0, 180));
                strut.SpawnProp();

                AnimateProp disk = new AnimateProp(strut.Prop, ModelHandler.RequestModel(ModelHandler.Disk), frontWheel ? diskOffsetFromStrut : diskOffsetFromRearStrut, new Vector3(0, leftWheel ? 90 : -90, 0));
                disk.SpawnProp();

                AnimateProp piston = new AnimateProp(disk.Prop, ModelHandler.RequestModel(ModelHandler.Piston), frontWheel ? pistonOffsetFromDisk : pistonOffsetFromRearDisk, Vector3.Zero);
                piston.SpawnProp();

                if (leftWheel)
                {
                    leftStruts.Add(strut);
                    leftPistons.Add(piston);
                    leftDisks.Add(disk);

                    leftWheelProps.Add(wheelAnimateProp);
                    leftWheelGlowProps.Add(wheelGlowAnimateProp);
                }
                else
                {
                    rightStruts.Add(strut);
                    rightPistons.Add(piston);
                    rightDisks.Add(disk);

                    rightWheelProps.Add(wheelAnimateProp);
                    rightWheelGlowProps.Add(wheelGlowAnimateProp);
                }
            }

            currentPistonRotation = MAX_PISTON_ROTATION;

            ApplyAnimation(currentOffset, currentStrutOffset, currentRotation, currentPistonRotation);
        }

        private void ReloadWheelModels()
        {
            if (_roadWheel == Mods.Wheel)
                return;

            leftWheelProps.ForEach(x => x?.Delete());
            leftWheelProps.Clear();

            rightWheelProps.ForEach(x => x?.Delete());
            rightWheelProps.Clear();

            _roadWheel = Properties.IsStockWheel ? WheelType.Stock : WheelType.Red;

            Model modelWheel = _roadWheel == WheelType.Stock ? ModelHandler.WheelProp : ModelHandler.RedWheelProp;
            Model modelWheelRear = _roadWheel == WheelType.Stock ? ModelHandler.RearWheelProp : ModelHandler.RedWheelProp;

            foreach (var wheel in Vehicle.GetWheelPositions())
            {
                string wheelName = wheel.Key;
                Vector3 offset = wheel.Value;
                bool leftWheel = (wheelName.Contains("lf") || wheelName.Contains("lr"));
                bool frontWheel = (wheelName.Contains("lf") || wheelName.Contains("rf"));

                Model wheelModel = !frontWheel ? modelWheelRear : modelWheel;

                ModelHandler.RequestModel(wheelModel);

                AnimateProp wheelAnimateProp = new AnimateProp(Vehicle, wheelModel, offset, Vector3.Zero);

                if (leftWheel)
                    leftWheelProps.Add(wheelAnimateProp);
                else
                    rightWheelProps.Add(wheelAnimateProp);
            }
        }

        public override void Play()
        {
            Play(true);
        }

        public override void Stop()
        {
            leftWheelGlowProps.ForEach(x => x?.Delete());
            rightWheelGlowProps.ForEach(x => x?.Delete());

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
                    leftWheelProps.ForEach(x => x.Delete());
                    rightWheelProps.ForEach(x => x.Delete());

                    Mods.Wheel = _roadWheel;
                }                
            }
        }

        private void SetAnimationPosition()
        {
            if (IsWheelsOpen)
            {
                currentOffset = MAX_OFFSET;
                currentStrutOffset = MAX_STRUT_OFFSET;
                currentRotation = MAX_ROTATION;
                currentPistonRotation = 0;
            }
            else
            {
                currentOffset = 0;
                currentStrutOffset = 0;
                currentRotation = 0;
                currentPistonRotation = MAX_PISTON_ROTATION;                
            }

            ApplyAnimation(currentOffset, currentStrutOffset, currentRotation, currentPistonRotation);

            if (IsWheelsOpen && _roadWheel == WheelType.Stock)
            {
                for (int i = 0; i < leftWheelProps.Count; i++)
                {
                    leftWheelGlowProps[i].Entity = leftWheelProps[i].Prop;
                    leftWheelGlowProps[i].SpawnProp(false);
                }

                for (int i = 0; i < rightWheelProps.Count; i++)
                {
                    rightWheelGlowProps[i].Entity = rightWheelProps[i].Prop;
                    rightWheelGlowProps[i].SpawnProp(false);
                }
            }
        }

        public override void Process()
        {
            if (!IsPlaying)
                return;

            ApplyAnimation(currentOffset, currentStrutOffset, currentRotation, currentPistonRotation);

            switch (currentStep)
            {
                case 0:
                    if (IsWheelsOpen)
                    {
                        // Wheel stuff
                        float offsetToAdd = (0.17f * Game.LastFrameTime) / 0.75f;
                        currentOffset -= offsetToAdd;

                        // Strut stuff
                        offsetToAdd = (0.18f * Game.LastFrameTime) / 0.75f;
                        currentStrutOffset -= offsetToAdd;
                       
                        if (currentOffset < -0.17f || currentStrutOffset < -0.18f)
                            currentStep++;
                    }
                    else
                    {
                        // Wheel stuff
                        float numToAdd = (90f * Game.LastFrameTime) / 0.75f;
                        currentRotation += numToAdd;

                        // Piston stuff
                        numToAdd = (90f * Game.LastFrameTime) / 0.75f;
                        currentPistonRotation -= numToAdd;

                        if (currentRotation >= 0)
                            currentStep++;
                    }

                    break;

                case 1:
                    if (IsWheelsOpen)
                    {
                        // Wheel stuff
                        float numToAdd = (90f * Game.LastFrameTime) / 0.75f;
                        currentRotation -= numToAdd;

                        // Piston stuff
                        numToAdd = (90f * Game.LastFrameTime) / 0.75f;
                        currentPistonRotation += numToAdd;

                        if (currentRotation < -90)
                        {
                            SetInstant(true, false);
                            OnAnimCompleted.Invoke();
                        }
                    }
                    else
                    {
                        // Wheel stuff
                        float offsetToAdd = (0.17f * Game.LastFrameTime) / 0.75f;
                        currentOffset += offsetToAdd;

                        // Strut stuff
                        offsetToAdd = (0.18f * Game.LastFrameTime) / 0.75f;
                        currentStrutOffset += offsetToAdd;

                        if (currentOffset >= 0 || currentStrutOffset >= 0)
                        {
                            SetInstant(false, false);
                            OnAnimCompleted.Invoke();
                        }
                    }
                    break;
            }
        }

        public void ApplyAnimation(float offset, float strutOffset, float rotation, float pistonRotation)
        {
            if (!_firstAnimation)
            {
                foreach (var wheelProp in leftWheelProps)
                {
                    wheelProp.SpawnProp(new Vector3(offset, 0, 0), new Vector3(0, rotation, 0), false);
                }

                foreach (var wheelProp in rightWheelProps)
                {
                    wheelProp.SpawnProp(new Vector3(-offset, 0, 0), new Vector3(0, -rotation - 180f, 0), false);
                }
            }

            foreach (var piston in leftPistons)
            {
                piston.SpawnProp(Vector3.Zero, new Vector3(0, pistonRotation, 0), false);
            }

            foreach (var piston in rightPistons)
            {
                piston.SpawnProp(Vector3.Zero, new Vector3(0, pistonRotation, 0), false);
            }

            foreach (var strut in leftStruts)
            {
                strut.SpawnProp(new Vector3(strutOffset, 0, 0), Vector3.Zero, false);
            }

            foreach (var strut in rightStruts)
            {
                strut.SpawnProp(new Vector3(-strutOffset, 0, 0), Vector3.Zero, false);
            }

            foreach (var disk in leftDisks)
            {
                disk.SpawnProp(Vector3.Zero, new Vector3(0, rotation, 0), false);
            }

            foreach (var disk in rightDisks)
            {
                disk.SpawnProp(Vector3.Zero, new Vector3(0, rotation - 180f, 0), false);
            }

            if (_firstAnimation)
                _firstAnimation = false;
        }

        public override void Dispose()
        {
            foreach (var wheelProp in leftWheelProps)
            {
                wheelProp?.Dispose();
            }

            foreach (var wheelProp in rightWheelProps)
            {
                wheelProp?.Dispose();
            }

            foreach (var glowProp in leftWheelGlowProps)
            {
                glowProp?.Dispose();
            }

            foreach (var glowProp in rightWheelGlowProps)
            {
                glowProp?.Dispose();
            }

            foreach (var strut in leftStruts)
            {
                strut?.Dispose();
            }

            foreach (var piston in leftPistons)
            {
                piston?.Dispose();
            }

            foreach (var disk in leftDisks)
            {
                disk?.Dispose();
            }

            foreach (var strut in rightStruts)
            {
                strut?.Dispose();
            }

            foreach (var piston in rightPistons)
            {
                piston?.Dispose();
            }

            foreach (var disk in rightDisks)
            {
                disk?.Dispose();
            }
        }
    }
}