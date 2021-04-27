using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class TimeMachine
    {
        public Vehicle Vehicle { get; }
        public DMC12 DMC12 { get; }

        public EventsHandler Events { get; private set; }
        public PropertiesHandler Properties { get; private set; }
        public ModsHandler Mods { get; private set; }
        public SoundsHandler Sounds { get; private set; }
        public PropsHandler Props { get; private set; }
        public PlayersHandler Players { get; private set; }
        public ScaleformsHandler Scaleforms { get; private set; }
        public ParticlesHandler Particles { get; private set; }
        public ConstantsHandler Constants { get; private set; }

        public CustomCameraHandler CustomCameraManager { get; private set; }

        public TimeMachineCamera CustomCamera
        {
            get => (TimeMachineCamera)CustomCameraManager.CurrentCameraIndex;
            set => CustomCameraManager.Show((int)value);
        }

        public TimeMachineClone LastDisplacementClone { get; set; }
        public Ped OriginalPed;

        private readonly Dictionary<string, HandlerPrimitive> registeredHandlers = new Dictionary<string, HandlerPrimitive>();

        private VehicleBone boneLf;
        private VehicleBone boneRf;

        private Vector3 leftSuspesionOffset;
        private Vector3 rightSuspesionOffset;

        private bool _firstRedSetup = true;

        private Blip Blip;

        public bool IsReady { get; } = false;

        public bool Disposed { get; private set; }

        public TimeMachine(Vehicle vehicle, WormholeType wormholeType)
        {
            Vehicle = vehicle;

            if (vehicle.Model == ModelHandler.DMC12)
            {
                DMC12 = DMC12Handler.GetDeloreanFromVehicle(vehicle);

                if (DMC12 == null)
                    DMC12 = new DMC12(vehicle);
            }

            Vehicle.IsPersistent = true;

            TimeMachineHandler.AddTimeMachine(this);

            Events = new EventsHandler(this);
            Mods = new ModsHandler(this, wormholeType);
            Properties = new PropertiesHandler(Guid.NewGuid());

            registeredHandlers.Add("ConstantsHandler", Constants = new ConstantsHandler(this));
            registeredHandlers.Add("SoundsHandler", Sounds = new SoundsHandler(this));

            Props = new PropsHandler(this);
            Scaleforms = new ScaleformsHandler(this);
            Players = new PlayersHandler(this);
            Particles = new ParticlesHandler(this);

            registeredHandlers.Add("SpeedoHandler", new SpeedoHandler(this));
            registeredHandlers.Add("TimeTravelHandler", new TimeTravelHandler(this));
            registeredHandlers.Add("TCDHandler", new TCDHandler(this));
            registeredHandlers.Add("InputHandler", new InputHandler(this));
            registeredHandlers.Add("RcHandler", new RcHandler(this));
            registeredHandlers.Add("ReentryHandler", new ReentryHandler(this));
            registeredHandlers.Add("SparksHandler", new SparksHandler(this));

            registeredHandlers.Add("RailroadHandler", new RailroadHandler(this));

            if (Mods.IsDMC12 || Vehicle.CanHoverTransform())
            {
                registeredHandlers.Add("FlyingHandler", new FlyingHandler(this));
                registeredHandlers.Add("LightningStrikeHandler", new LightningStrikeHandler(this));
            }

            if (Mods.IsDMC12)
            {
                registeredHandlers.Add("FuelHandler", new FuelHandler(this));
                registeredHandlers.Add("SIDHandler", new SIDHandler(this));
                registeredHandlers.Add("FluxCapacitorHandler", new FluxCapacitorHandler(this));
                registeredHandlers.Add("FreezeHandler", new FreezeHandler(this));
                registeredHandlers.Add("TFCHandler", new TFCHandler(this));
                registeredHandlers.Add("ComponentsHandler", new ComponentsHandler(this));
                registeredHandlers.Add("EngineHandler", new EngineHandler(this));
                registeredHandlers.Add("StarterHandler", new StarterHandler(this));
                //registeredHandlers.Add("DriverAIHandler", new DriverAIHandler(this));
                registeredHandlers.Add("ClockHandler", new ClockHandler(this));

                VehicleBone.TryGetForVehicle(Vehicle, "suspension_lf", out boneLf);
                VehicleBone.TryGetForVehicle(Vehicle, "suspension_rf", out boneRf);

                leftSuspesionOffset = Vehicle.Bones["suspension_lf"].GetRelativeOffsetPosition(new Vector3(0.025f, 0, 0.005f));
                rightSuspesionOffset = Vehicle.Bones["suspension_rf"].GetRelativeOffsetPosition(new Vector3(-0.025f, 0, 0.005f));
            }

            LastDisplacementClone = this.Clone();
            LastDisplacementClone.Properties.DestinationTime = FusionUtils.CurrentTime.AddSeconds(-FusionUtils.CurrentTime.Second);

            Events.OnWormholeTypeChanged += UpdateBlip;

            if (Vehicle.Model == ModelHandler.DeluxoModel)
                Mods.HoverUnderbody = ModState.On;

            CustomCameraManager = new CustomCameraHandler();

            //DestinationDate
            CustomCameraManager.Add(Vehicle, new Vector3(-0.1f, 0.16f, 0.7f), new Vector3(0.12f, 1.1f, 0.45f), 28);

            //DriverSeat
            CustomCameraManager.Add(Vehicle, new Vector3(0.53f, 0.11f, 0.727f), new Vector3(-0.445f, -0.08f, 0.65f), 46);

            //DigitalSpeedo
            CustomCameraManager.Add(Vehicle, new Vector3(-0.12f, 0.06f, 0.8f), new Vector3(-0.48f, 0.98f, 0.656f), 50);

            //AnalogSpeedo
            CustomCameraManager.Add(Vehicle, new Vector3(-0.44f, 0.186f, 0.64f), new Vector3(-0.4f, 1.17f, 0.47f), 46);

            //FrontPassengerWheelLookAtRear
            CustomCameraManager.Add(Vehicle, new Vector3(1.64f, 2.34f, 0.27f), new Vector3(1.25f, 1.42f, 0.28f), 38);

            //TrainApproaching
            CustomCameraManager.Add(Vehicle, new Vector3(2.43f, -2.43f, 0.33f), new Vector3(1.44f, -2.465f, 0.466f), 50);

            //RightSide
            CustomCameraManager.Add(Vehicle, new Vector3(3.33f, -1.0f, 0.736f), new Vector3(2.35f, -0.77f, 0.78f), 50);

            //FrontToBack
            CustomCameraManager.Add(Vehicle, new Vector3(0, 5.43f, 1.2f), new Vector3(0, 4.43f, 1.2f), 50);

            //FrontOnRail
            CustomCameraManager.Add(Vehicle, new Vector3(0, 2.656f, 0.23f), new Vector3(0, 3.656f, 0.23f), 50);

            //FrontToBackRightSide
            CustomCameraManager.Add(Vehicle, new Vector3(1.15f, 4.64f, 0.42f), new Vector3(0.87f, 3.68f, 0.49f), 50);

            //LicensePlate
            CustomCameraManager.Add(Vehicle, new Vector3(0f, -3.97f, 0.41f), new Vector3(0f, -2.97f, 0.36f), 50);

            //TimeTravelOnTracks
            CustomCameraManager.Add(Vehicle, new Vector3(-9.71f, -3.84f, 8.42f), new Vector3(-8.91f, -3.54f, 7.90f), 50);

            //DigitalSpeedoTowardsFront
            CustomCameraManager.Add(Vehicle, new Vector3(-0.41f, 0f, 0.83f), new Vector3(-0.42f, 0.96f, 0.78f), 40);

            //RearCarTowardsFront
            CustomCameraManager.Add(Vehicle, new Vector3(0f, -7.81f, 2f), new Vector3(0f, -6.81f, 2f), 50);

            IsReady = true;
        }

        public T GetHandler<T>()
        {
            if (registeredHandlers.TryGetValue(typeof(T).Name, out HandlerPrimitive handler))
                return (T)(object)handler;

            return default;
        }

        private void DisposeAllHandlers()
        {
            foreach (HandlerPrimitive handler in registeredHandlers.Values)
                handler.Dispose();
        }

        public void Tick()
        {
            if (!IsReady)
                return;

            if (!Vehicle.IsFunctioning())
            {
                TimeMachineHandler.RemoveTimeMachine(this, false);

                return;
            }

            if (!Vehicle.IsVisible)
                Vehicle.IsEngineRunning = false;

            if (Properties.IsWayback && TimeMachineHandler.CurrentTimeMachine == this)
                Properties.IsWayback = false;

            Function.Call(Hash._SET_VEHICLE_ENGINE_TORQUE_MULTIPLIER, Vehicle, Properties.TorqueMultiplier);

            if (Mods.HoverUnderbody == ModState.Off && Mods.IsDMC12)
                VehicleControl.SetDeluxoTransformation(Vehicle, 0f);

            if (Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole | Properties.IsRemoteControlled)
                Vehicle.LockStatus = VehicleLockStatus.PlayerCannotLeaveCanBeBrokenIntoPersist;
            else
                Vehicle.LockStatus = VehicleLockStatus.None;

            Vehicle.IsRadioEnabled = false;

            if (Mods.Wheel == WheelType.RailroadInvisible)
            {
                if (Properties.IsOnTracks)
                {
                    if (Mods.Wheels.Burst)
                        Mods.Wheels.Burst = false;
                }
                else
                {
                    if (!Mods.Wheels.Burst)
                        Mods.Wheels.Burst = true;
                }
            }

            if (Mods.IsDMC12)
            {
                //In certain situations car can't be entered after hover transformation, here is forced enter task.
                if (FusionUtils.PlayerVehicle == null && Game.IsControlJustPressed(GTA.Control.Enter) && TimeMachineHandler.ClosestTimeMachine == this && TimeMachineHandler.SquareDistToClosestTimeMachine <= 15 && World.GetClosestVehicle(FusionUtils.PlayerPed.Position, TimeMachineHandler.SquareDistToClosestTimeMachine) == this)
                {
                    if (Function.Call<Vehicle>(Hash.GET_VEHICLE_PED_IS_ENTERING, FusionUtils.PlayerPed) != Vehicle || Vehicle.Driver != null)
                    {
                        if (Vehicle.Driver != null)
                        {
                            TaskSequence taskSequence = new TaskSequence();
                            taskSequence.AddTask.LeaveVehicle(LeaveVehicleFlags.WarpOut);
                            taskSequence.AddTask.WanderAround();

                            Vehicle.Driver.Task.PerformSequence(taskSequence);
                        }

                        FusionUtils.PlayerPed.Task.EnterVehicle(Vehicle, VehicleSeat.Driver);
                    }
                }

                VehicleWindowCollection windows = Vehicle.Windows;
                windows[VehicleWindowIndex.BackLeftWindow].Remove();
                windows[VehicleWindowIndex.BackRightWindow].Remove();
                windows[VehicleWindowIndex.ExtraWindow4].Remove();

                Vehicle.Doors[VehicleDoorIndex.Trunk].Break(false);
                Vehicle.Doors[VehicleDoorIndex.BackRightDoor].Break(false);

                if (Mods.Hoodbox == ModState.On)
                    Vehicle.Doors[VehicleDoorIndex.Hood].CanBeBroken = false;

                if (Mods.SuspensionsType != SuspensionsType.Stock && Properties.TorqueMultiplier != 2.4f)
                    Properties.TorqueMultiplier = 2.4f;

                switch (Mods.SuspensionsType)
                {
                    case SuspensionsType.LiftFrontLowerRear:
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0.75f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0.75f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearLeft, -0.25f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearRight, -0.25f);
                        break;
                    case SuspensionsType.LiftFront:
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0.75f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0.75f);
                        break;
                    case SuspensionsType.LiftRear:
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0.75f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearRight, 0.75f);
                        break;
                    case SuspensionsType.LiftFrontAndRear:
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0.75f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0.75f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0.75f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearRight, 0.75f);
                        break;
                    case SuspensionsType.LowerFront:
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontLeft, -0.25f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontRight, -0.25f);
                        break;
                    case SuspensionsType.LowerRear:
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearLeft, -0.25f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearRight, -0.25f);
                        break;
                    case SuspensionsType.LowerFrontAndRear:
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontLeft, -0.25f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontRight, -0.25f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearLeft, -0.25f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearRight, -0.25f);
                        break;
                }

                if (Mods.Wheel == WheelType.Red && Mods.HoverUnderbody == ModState.Off)
                {
                    if (_firstRedSetup)
                    {
                        boneLf?.SetTranslation(leftSuspesionOffset);
                        boneRf?.SetTranslation(rightSuspesionOffset);

                        _firstRedSetup = false;
                    }

                    if (VehicleControl.GetWheelSize(Vehicle) != 1.1f)
                        VehicleControl.SetWheelSize(Vehicle, 1.1f);
                }
                else if (!_firstRedSetup)
                {
                    boneLf?.ResetTranslation();
                    boneRf?.ResetTranslation();

                    _firstRedSetup = true;
                }

                Mods.SyncMods();

                if (Props.LicensePlate.IsPlaying)
                {
                    if (Props.LicensePlate[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].StepRatio > 0.1f)
                        Props.LicensePlate[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].StepRatio -= Game.LastFrameTime;
                }
            }

            if (FusionUtils.PlayerVehicle != Vehicle && Vehicle.IsVisible && !Properties.Story)
            {
                if (Blip == null || !Blip.Exists())
                {
                    Blip = Vehicle.AddBlip();
                    Blip.Sprite = Mods.IsDMC12 ? BlipSprite.Deluxo : BlipSprite.PersonalVehicleCar;

                    UpdateBlip();
                }
            }
            else if (Blip != null && Blip.Exists())
                Blip.Delete();

            //if (TimeMachineHandler.CurrentTimeMachine == this)
            //    Main.CustomStopwatch.StartNewRecord();

            foreach (KeyValuePair<string, HandlerPrimitive> entry in registeredHandlers)
            {
                entry.Value.Tick();

                //if (TimeMachineHandler.CurrentTimeMachine == this)
                //    Main.CustomStopwatch.WriteAndReset(entry.Key);
            }

            //if (TimeMachineHandler.CurrentTimeMachine == this)
            //    Main.CustomStopwatch.Stop();

            if (Properties.Boost != 0)
            {
                Vehicle.ApplyForce(Vehicle.ForwardVector * Properties.Boost, Vector3.Zero);
                Properties.Boost = 0;
            }

            PhotoMode();

            CustomCameraManager.Tick();
        }

        private void UpdateBlip()
        {
            if (Blip != null && Blip.Exists())
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        Blip.Name = TextHandler.GetLocalizedText("BTTF1");
                        Blip.Color = BlipColor.NetPlayer22;
                        break;

                    case WormholeType.BTTF2:
                        Blip.Name = TextHandler.GetLocalizedText("BTTF2");
                        Blip.Color = BlipColor.NetPlayer21;
                        break;

                    case WormholeType.BTTF3:
                        if (Mods.Wheel == WheelType.RailroadInvisible)
                        {
                            Blip.Name = TextHandler.GetLocalizedText("BTTF3RR");
                            Blip.Color = BlipColor.Orange;
                        }
                        else
                        {
                            Blip.Name = TextHandler.GetLocalizedText("BTTF3");
                            Blip.Color = BlipColor.Red;
                        }
                        break;
                }
            }
        }

        public void Break()
        {
            if (!Mods.IsDMC12)
                return;

            Mods.HoverUnderbody = ModState.Off;

            Properties.ReactorCharge = 0;
            Properties.AreTimeCircuitsBroken = true;
            Properties.AreFlyingCircuitsBroken = true;

            Mods.Wheels.Burst = true;
            Vehicle.EngineHealth = -4000;
        }

        public bool Repair(bool timeCircuits, bool flyingCircuits, bool engine)
        {
            if (timeCircuits)
            {
                if (FusionUtils.CurrentTime.Year >= 1985)
                    Properties.AreTimeCircuitsBroken = false;
                else if (FusionUtils.CurrentTime.Year >= 1947)
                    Mods.Hoodbox = ModState.On;
                else
                    TextHandler.ShowSubtitle("UnableRepairTC");

                return !Properties.AreTimeCircuitsBroken || Mods.Hoodbox == ModState.On;
            }

            if (flyingCircuits)
            {
                if (FusionUtils.CurrentTime.Year >= 2015)
                    Properties.AreFlyingCircuitsBroken = false;
                else
                    TextHandler.ShowSubtitle("UnableRepairFC");

                return !Properties.AreFlyingCircuitsBroken;
            }

            if (engine)
            {
                if (Mods.Wheels.Burst)
                {
                    if (FusionUtils.CurrentTime.Year < 1982)
                    {
                        Mods.Wheel = WheelType.Red;
                        Mods.SuspensionsType = SuspensionsType.LiftFront;
                    }
                    else
                        Mods.Wheels.Burst = false;
                }

                Vehicle.EngineHealth = 1000;
            }

            return true;
        }

        private void PhotoMode()
        {
            if (Properties.IsPhotoModeOn && !Vehicle.IsVisible)
            {
                Properties.PhotoWormholeActive = false;
                Properties.PhotoGlowingCoilsActive = false;
                Properties.PhotoFluxCapacitorActive = false;
                Properties.PhotoEngineStallActive = false;
                Properties.PhotoSIDMaxActive = false;
            }

            if (Properties.PhotoWormholeActive && Players.Wormhole != null && !Players.Wormhole.IsPlaying)
                Players.Wormhole.Play(true);

            if (!Properties.PhotoWormholeActive && Players.Wormhole != null && Players.Wormhole.IsPlaying && Properties.IsPhotoModeOn)
                Players.Wormhole.Stop();

            if (Properties.PhotoGlowingCoilsActive && Props.Coils != null && !Props.Coils.IsSpawned)
            {
                Mods.OffCoils = ModState.Off;
                Props.Coils.SpawnProp();
            }

            if (!Properties.PhotoGlowingCoilsActive && Props.Coils != null && Props.Coils.IsSpawned && Properties.TimeTravelPhase != TimeTravelPhase.OpeningWormhole)
            {
                Mods.OffCoils = ModState.On;
                Props.Coils.Delete();
            }

            if (Properties.PhotoFluxCapacitorActive && !Properties.IsFluxDoingBlueAnim)
                Events.OnWormholeStarted?.Invoke();

            if (!Properties.PhotoFluxCapacitorActive && Properties.IsFluxDoingBlueAnim && Properties.IsPhotoModeOn)
                Events.OnSparksInterrupted?.Invoke();

            if (Properties.PhotoEngineStallActive && !Properties.IsEngineStalling)
                Events.SetEngineStall?.Invoke(true);

            if (!Properties.PhotoEngineStallActive && Properties.IsEngineStalling && Properties.IsPhotoModeOn)
                Events.SetEngineStall?.Invoke(false);

            if (Properties.PhotoSIDMaxActive && !Properties.ForceSIDMax)
                Properties.ForceSIDMax = true;

            if (!Properties.PhotoSIDMaxActive && Properties.ForceSIDMax)
                Properties.ForceSIDMax = false;

            Properties.IsPhotoModeOn = Properties.PhotoWormholeActive | Properties.PhotoGlowingCoilsActive | Properties.PhotoFluxCapacitorActive | Properties.IsEngineStalling | Properties.PhotoSIDMaxActive;
        }

        public void KeyDown(KeyEventArgs e)
        {
            foreach (KeyValuePair<string, HandlerPrimitive> entry in registeredHandlers)
                entry.Value.KeyDown(e);
        }

        public void Dispose(bool deleteVeh = true)
        {
            DisposeAllHandlers();

            Props.Dispose();
            Scaleforms.Dispose();
            Players.Dispose();
            Particles.Dispose();

            CustomCameraManager.Abort();

            Blip?.Delete();

            if (Mods.IsDMC12)
                DMC12Handler.RemoveInstantlyDelorean(DMC12, deleteVeh);
            else if (deleteVeh)
                Vehicle.DeleteCompletely();

            Disposed = true;

            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return TimeMachineHandler.TimeMachines.IndexOf(this).ToString();
        }

        public static implicit operator Vehicle(TimeMachine timeMachine)
        {
            if (!timeMachine.NotNullAndExists())
                return null;

            return timeMachine.Vehicle;
        }

        public static implicit operator Entity(TimeMachine timeMachine)
        {
            if (!timeMachine.NotNullAndExists())
                return null;

            return timeMachine.Vehicle;
        }

        public static implicit operator InputArgument(TimeMachine timeMachine)
        {
            if (!timeMachine.NotNullAndExists())
                return null;

            return timeMachine.Vehicle;
        }
    }
}
