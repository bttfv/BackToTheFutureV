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
        public HoverVehicle HoverVehicle { get; }

        public EventsHandler Events { get; private set; }
        public PropertiesHandler Properties { get; private set; }
        public ModsHandler Mods { get; private set; }
        public SoundsHandler Sounds { get; private set; }
        public PropsHandler Props { get; private set; }
        public PlayersHandler Players { get; private set; }
        public ScaleformsHandler Scaleforms { get; private set; }
        public ParticlesHandler Particles { get; private set; }
        public ConstantsHandler Constants { get; private set; }
        public DecoratorsHandler Decorators { get; private set; }

        public CustomCameraHandler CustomCameraManager { get; private set; }

        public TimeMachineCamera CustomCamera
        {
            get => (TimeMachineCamera)CustomCameraManager.CurrentCameraIndex;

            set => CustomCameraManager.Show((int)value);
        }

        public TimeMachineClone LastDisplacementClone { get; set; }
        public Ped OriginalPed;

        private readonly Dictionary<string, HandlerPrimitive> registeredHandlers = new Dictionary<string, HandlerPrimitive>();

        private Blip Blip;

        public bool IsReady { get; } = false;

        public bool Disposed { get; private set; }

        public TimeMachine(Vehicle vehicle, WormholeType wormholeType)
        {
            Vehicle = vehicle;
            HoverVehicle = HoverVehicle.GetFromVehicle(vehicle);

            HoverVehicle.SoftLock = true;

            if (vehicle.Model == ModelHandler.DMC12)
            {
                DMC12 = DMC12Handler.GetDeloreanFromVehicle(vehicle);

                if (DMC12 == null)
                {
                    DMC12 = new DMC12(vehicle);
                }
            }

            Vehicle.IsPersistent = true;

            Vehicle.Decorator().DoNotDelete = true;
            Vehicle.Decorator().RemoveFromUsed = true;
            Vehicle.Decorator().IgnoreForSwap = true;

            TimeMachineHandler.AddTimeMachine(this);

            Events = new EventsHandler(this);
            Mods = new ModsHandler(this, wormholeType);
            Properties = new PropertiesHandler();

            registeredHandlers.Add("ConstantsHandler", Constants = new ConstantsHandler(this));
            registeredHandlers.Add("SoundsHandler", Sounds = new SoundsHandler(this));
            registeredHandlers.Add("PropsHandler", Props = new PropsHandler(this));

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

            if (Mods.IsDMC12)
            {
                registeredHandlers.Add("FlyingHandler", new FlyingHandler(this));
                registeredHandlers.Add("LightningStrikeHandler", new LightningStrikeHandler(this));
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

                //VehicleBone.TryGetForVehicle(Vehicle, "suspension_lf", out boneLf);
                //VehicleBone.TryGetForVehicle(Vehicle, "suspension_rf", out boneRf);

                //leftSuspesionOffset = Vehicle.Bones["suspension_lf"].GetRelativeOffsetPosition(new Vector3(0.025f, 0, 0.005f));
                //rightSuspesionOffset = Vehicle.Bones["suspension_rf"].GetRelativeOffsetPosition(new Vector3(-0.025f, 0, 0.005f));
            }

            Decorators = new DecoratorsHandler(this);

            LastDisplacementClone = this.Clone();
            LastDisplacementClone.Properties.DestinationTime = FusionUtils.CurrentTime.AddSeconds(-FusionUtils.CurrentTime.Second);

            Events.OnWormholeTypeChanged += UpdateBlip;

            if (Vehicle.Model == ModelHandler.Deluxo /*|| (Main.DeluxoProtoSupport && vehicle.Model == "dproto")*/)
            {
                Mods.HoverUnderbody = ModState.On;
            }

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

            //PlateCustom
            CustomCameraManager.Add(Vehicle, new Vector3(0.17f, -2.64f, 0.78f), new Vector3(-0.03f, -1.67f, 0.6f), 50);

            //ReactorCustom
            CustomCameraManager.Add(Vehicle, new Vector3(0.92f, -1.78f, 1.55f), new Vector3(0.02f, -1.60f, 1.17f), 50);

            //RimCustom
            CustomCameraManager.Add(Vehicle, new Vector3(1.74f, 1.79f, 0.48f), new Vector3(0.88f, 1.33f, 0.24f), 50);

            //HoodCustom
            CustomCameraManager.Add(Vehicle, new Vector3(-0.98f, 2.47f, 1.16f), new Vector3(-0.41f, 1.79f, 0.7f), 50);

            //ExhaustCustom
            CustomCameraManager.Add(Vehicle, new Vector3(-1.13f, -2.76f, 0.28f), new Vector3(-0.53f, -1.97f, 0.27f), 50);

            //HookCustom
            CustomCameraManager.Add(Vehicle, new Vector3(2.03f, -1.62f, 1.71f), new Vector3(1.22f, -1.16f, 1.36f), 50);

            //SuspensionsCustom
            CustomCameraManager.Add(Vehicle, new Vector3(-2.24f, -0.04f, 0.38f), new Vector3(-1.25f, -0.03f, 0.41f), 80);

            //HoverUnderbodyCustom
            CustomCameraManager.Add(Vehicle, new Vector3(-0.41f, 2.87f, 0.06f), new Vector3(-0.16f, 1.90f, 0.15f), 50);

            //SpeedoWithDestZoom
            CustomCameraManager.Add(Vehicle, new Vector3(-0.106f, -0.534f, 0.784f), new Vector3(-0.253f, 0.452f, 0.714f), 17, 2500).SetEnd(new Vector3(-0.027f, 0.014f, 0.687f), new Vector3(0.110f, 1.0f, 0.604f), 20, 500, 1500);

            //BulovaSetup
            CustomCameraManager.Add(Vehicle, Vehicle.Bones["Bulova_Clock"].RelativePosition + new Vector3(0f, -0.25f, 0f), Vehicle.Bones["Bulova_Clock"].RelativePosition, 50);

            IsReady = true;
        }

        public T GetHandler<T>()
        {
            if (registeredHandlers.TryGetValue(typeof(T).Name, out HandlerPrimitive handler))
            {
                return (T)(object)handler;
            }

            return default;
        }

        private void DisposeAllHandlers()
        {
            foreach (HandlerPrimitive handler in registeredHandlers.Values)
            {
                handler.Dispose();
            }
        }

        public void Tick()
        {
            if (!IsReady)
            {
                return;
            }

            if (!Vehicle.IsFunctioning())
            {
                Vehicle.Decorator().DoNotDelete = false;
                TimeMachineHandler.RemoveTimeMachine(this, false);

                return;
            }

            if (TimeMachineHandler.CurrentTimeMachine == this && Properties.IsWayback)
                Properties.IsWayback = false;

            //After reentry, story time machines spawn in an odd state. This code fixes the inability for player to enter the time machine from the mineshaft
            if (!TimeMachineHandler.ClosestTimeMachine.IsFunctioning() && Vehicle.IsFunctioning() && FusionUtils.PlayerPed.DistanceToSquared2D(Vehicle, 4.47f) && Constants.FullDamaged && Game.IsControlJustPressed(GTA.Control.Enter))
            {
                FusionUtils.PlayerPed.Task.EnterVehicle(Vehicle, VehicleSeat.Driver);
            }

            if (Constants.HasScaleformPriority != Properties.HasScaleformPriority)
            {
                Properties.HasScaleformPriority = Constants.HasScaleformPriority;
                Events.OnScaleformPriority?.Invoke();
                if (Mods.SuspensionsType != SuspensionsType.Stock || Mods.Wheel == WheelType.Red)
                {
                    Vehicle.Velocity += Vector3.UnitY * 0.3f;
                }
            }

            if (!Vehicle.IsVisible)
            {
                Vehicle.IsEngineRunning = false;
            }

            Function.Call(Hash.SET_VEHICLE_CHEAT_POWER_INCREASE, Vehicle, Decorators.TorqueMultiplier);

            if (Mods.HoverUnderbody == ModState.Off && Mods.IsDMC12)
            {
                VehicleControl.SetDeluxoTransformation(Vehicle, 0f);
            }

            if (Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole | Properties.IsRemoteControlled)
            {
                Vehicle.LockStatus = VehicleLockStatus.PlayerCannotLeaveCanBeBrokenIntoPersist;
            }
            else
            {
                Vehicle.LockStatus = VehicleLockStatus.None;
            }

            if (Mods.IsDMC12)
            {
                Vehicle.IsRadioEnabled = false;

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
                {
                    Vehicle.Doors[VehicleDoorIndex.Hood].CanBeBroken = false;
                }

                Mods.Tick();

                if (Props.LicensePlate.IsPlaying)
                {
                    if (Props.LicensePlate[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].StepRatio > 0.1f)
                    {
                        Props.LicensePlate[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].StepRatio -= Game.LastFrameTime;
                    }
                }
            }

            /*if (Constants.DeluxoProto)
            {
                if (Vehicle.IsExtraOn(1) && Properties.ReactorCharge != 0)
                {
                    Properties.ReactorCharge = 0;
                }
                else if (!Vehicle.IsExtraOn(1) && Properties.ReactorCharge != 1)
                {
                    Properties.ReactorCharge = 1;
                }

                if (Vehicle.IsExtraOn(2) && Mods.WormholeType != WormholeType.BTTF3)
                {
                    Mods.WormholeType = WormholeType.BTTF3;
                }
                else if (!Vehicle.IsExtraOn(2) && Vehicle.IsExtraOn(3) && Mods.WormholeType != WormholeType.BTTF2)
                {
                    Mods.WormholeType = WormholeType.BTTF2;
                }
                else if (!Vehicle.IsExtraOn(2) && !Vehicle.IsExtraOn(3) && Mods.WormholeType != WormholeType.BTTF1)
                {
                    Mods.WormholeType = WormholeType.BTTF1;
                }
            }*/

            if (FusionUtils.PlayerVehicle != Vehicle && Vehicle.Exists() && !Properties.Story && Vehicle.IsVisible)
            {
                if (Blip == null || !Blip.Exists())
                {
                    Blip = Vehicle.AddBlip();
                    Blip.Sprite = Mods.IsDMC12 ? BlipSprite.Deluxo : BlipSprite.Stopwatch;

                    UpdateBlip();
                }
            }
            else if (Blip != null && Blip.Exists())
            {
                Blip.Delete();
            }

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
        }

        private void UpdateBlip()
        {
            if (Blip != null && Blip.Exists())
            {
                if (Mods.IsDMC12)
                {
                    switch (Mods.WormholeType)
                    {
                        case WormholeType.BTTF1:
                            if (Mods.Hook == HookState.On || Mods.Hook == HookState.OnDoor)
                            {
                                Blip.Name = TextHandler.Me.GetLocalizedText("BTTF1H");
                                Blip.Color = BlipColor.NetPlayer20;
                            }
                            else
                            {
                                Blip.Name = TextHandler.Me.GetLocalizedText("BTTF1");
                                Blip.Color = BlipColor.NetPlayer22;
                            }
                            break;

                        case WormholeType.BTTF2:
                            Blip.Name = TextHandler.Me.GetLocalizedText("BTTF2");
                            Blip.Color = BlipColor.NetPlayer21;
                            break;

                        case WormholeType.BTTF3:
                            if (Mods.Wheel == WheelType.RailroadInvisible)
                            {
                                Blip.Name = TextHandler.Me.GetLocalizedText("BTTF3RR");
                                Blip.Color = BlipColor.Orange;
                            }
                            else
                            {
                                Blip.Name = TextHandler.Me.GetLocalizedText("BTTF3");
                                Blip.Color = BlipColor.Red;
                            }
                            break;
                    }
                }
                else
                {
                    Blip.Name = FusionUtils.AllVehiclesModels.Find(x => x.Hash == (Hash)Vehicle.Model.Hash).DisplayName;
                    Blip.Color = BlipColor.Yellow;
                }
            }
        }

        public void Break()
        {
            if (!Mods.IsDMC12)
            {
                return;
            }

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
                {
                    if (Mods.Hoodbox == ModState.On)
                    {
                        Mods.Hoodbox = ModState.Off;
                        Mods.WormholeType = WormholeType.BTTF2;
                    }
                    Properties.AreTimeCircuitsBroken = false;
                }
                else if (FusionUtils.CurrentTime.Year >= 1952)
                {
                    Mods.Hoodbox = ModState.On;
                }

                return !Properties.AreTimeCircuitsBroken || Mods.Hoodbox == ModState.On;
            }

            if (flyingCircuits)
            {
                if (FusionUtils.CurrentTime.Year >= 2015)
                {
                    Properties.AreFlyingCircuitsBroken = false;
                }

                return !Properties.AreFlyingCircuitsBroken;
            }

            if (engine)
            {
                if (Mods.Wheels.Burst)
                {
                    if (FusionUtils.CurrentTime.Year < 1981)
                    {
                        Mods.Wheel = WheelType.Red;
                        Mods.SuspensionsType = SuspensionsType.LiftFront;
                    }
                    else
                    {
                        Mods.Wheels.Burst = false;
                    }
                }

                Vehicle.Repair();
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
            {
                Players.Wormhole.Play(true);
            }

            if (!Properties.PhotoWormholeActive && Players.Wormhole != null && Players.Wormhole.IsPlaying && Properties.IsPhotoModeOn)
            {
                Players.Wormhole.Stop();
            }

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

            if (Properties.PhotoFluxCapacitorActive && !(Properties.IsFluxDoingBlueAnim || Properties.IsFluxDoingOrangeAnim))
            {
                Events.OnWormholeStarted?.Invoke();
            }

            if (!Properties.PhotoFluxCapacitorActive && (Properties.IsFluxDoingBlueAnim || Properties.IsFluxDoingOrangeAnim) && Properties.IsPhotoModeOn)
            {
                Events.OnSparksInterrupted?.Invoke();
            }

            if (Properties.PhotoEngineStallActive && !Properties.IsEngineStalling)
            {
                Events.SetEngineStall?.Invoke(true);
            }

            if (!Properties.PhotoEngineStallActive && Properties.IsEngineStalling && Properties.IsPhotoModeOn)
            {
                Events.SetEngineStall?.Invoke(false);
            }

            if (Properties.PhotoSIDMaxActive && !Properties.ForceSIDMax)
            {
                Properties.ForceSIDMax = true;
            }

            if (!Properties.PhotoSIDMaxActive && Properties.ForceSIDMax)
            {
                Properties.ForceSIDMax = false;
            }

            /*if (Constants.DeluxoProto)
            {
                if (Properties.PhotoGlowingCoilsActive && Vehicle.Mods.DashboardColor != (VehicleColor)70)
                {
                    Vehicle.Mods.DashboardColor = (VehicleColor)70;
                }

                if (!Properties.PhotoGlowingCoilsActive && Vehicle.Mods.DashboardColor != (VehicleColor)12 && Properties.TimeTravelPhase != TimeTravelPhase.OpeningWormhole)
                {
                    Vehicle.Mods.DashboardColor = (VehicleColor)12;
                }
            }*/

            Properties.IsPhotoModeOn = Properties.PhotoWormholeActive | Properties.PhotoGlowingCoilsActive | Properties.PhotoFluxCapacitorActive | Properties.IsEngineStalling | Properties.PhotoSIDMaxActive;
        }

        public void KeyDown(KeyEventArgs e)
        {
            foreach (KeyValuePair<string, HandlerPrimitive> entry in registeredHandlers)
            {
                entry.Value.KeyDown(e);
            }
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
            {
                DMC12Handler.RemoveInstantlyDelorean(DMC12, deleteVeh);
            }
            else if (deleteVeh)
            {
                Vehicle.DeleteCompletely();
            }

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
            {
                return null;
            }

            return timeMachine.Vehicle;
        }

        public static implicit operator Entity(TimeMachine timeMachine)
        {
            if (!timeMachine.NotNullAndExists())
            {
                return null;
            }

            return timeMachine.Vehicle;
        }

        public static implicit operator InputArgument(TimeMachine timeMachine)
        {
            if (!timeMachine.NotNullAndExists())
            {
                return null;
            }

            return timeMachine.Vehicle;
        }
    }
}
