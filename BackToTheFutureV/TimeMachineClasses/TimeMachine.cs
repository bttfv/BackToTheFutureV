using BackToTheFutureV.TimeMachineClasses.Handlers;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses
{
    public class TimeMachine
    {
        public Vehicle Vehicle { get; }
        public DMC12 DMC12 { get; }

        public EventsHandler Events { get; private set; }
        public PropertiesHandler Properties { get; private set; }
        public TimeMachineMods Mods { get; private set; }
        public SoundsHandler Sounds { get; private set; }
        public PropsHandler Props { get; private set; }
        public PlayersHandler Players { get; private set; }
        public ScaleformsHandler Scaleforms { get; private set; }
        public ParticlesHandler Particles { get; private set; }

        public CustomCameraHandler CustomCameraManager { get; private set; }

        public TimeMachineCamera CustomCamera 
        { 
            get
            {
                return (TimeMachineCamera)CustomCameraManager.CurrentCameraIndex;
            } 
            set
            {
                CustomCameraManager.Show((int)value);
            }
        }

        public TimeMachineClone Clone => new TimeMachineClone(this);
        public TimeMachineClone LastDisplacementClone { get; set; }
        public Ped OriginalPed;

        private readonly Dictionary<string, Handler> registeredHandlers = new Dictionary<string, Handler>();

        private VehicleBone boneLf;
        private VehicleBone boneRf;

        private Vector3 leftSuspesionOffset;
        private Vector3 rightSuspesionOffset;

        private bool _firstRedSetup = true;

        private Blip Blip;

        public bool IsReady { get; } = false;

        public bool Disposed { get; private set; }

        public WaybackMachine WaybackMachine { get; set; }

        public bool IsWaybackPlaying => WaybackMachine != null && WaybackMachine.IsPlaying;

        public bool CreateCloneSpawn { get; set; } = false;

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

            Mods = new TimeMachineMods(this, wormholeType);

            Properties = new PropertiesHandler(this);

            registeredHandlers.Add("EventsHandler", Events = new EventsHandler(this));
            registeredHandlers.Add("SoundsHandler", Sounds = new SoundsHandler(this));
            registeredHandlers.Add("PropsHandler", Props = new PropsHandler(this));
            registeredHandlers.Add("PlayersHandler", Players = new PlayersHandler(this));
            registeredHandlers.Add("ScaleformsHandler", Scaleforms = new ScaleformsHandler(this));
            registeredHandlers.Add("ParticlesHandler", Particles = new ParticlesHandler(this));

            registeredHandlers.Add("SpeedoHandler", new SpeedoHandler(this));
            registeredHandlers.Add("TimeTravelHandler", new TimeTravelHandler(this));
            registeredHandlers.Add("TCDHandler", new TCDHandler(this));
            registeredHandlers.Add("InputHandler", new InputHandler(this));
            registeredHandlers.Add("RcHandler", new RcHandler(this));
            registeredHandlers.Add("FuelHandler", new FuelHandler(this));
            registeredHandlers.Add("ReentryHandler", new ReentryHandler(this));
            registeredHandlers.Add("SparksHandler", new SparksHandler(this));

            registeredHandlers.Add("FlyingHandler", new FlyingHandler(this));
            registeredHandlers.Add("RailroadHandler", new RailroadHandler(this));
            registeredHandlers.Add("LightningStrikeHandler", new LightningStrikeHandler(this));

            if (Mods.IsDMC12)
            {
                registeredHandlers.Add("SIDHandler", new SIDHandler(this));
                registeredHandlers.Add("FluxCapacitorHandler", new FluxCapacitorHandler(this));
                registeredHandlers.Add("FreezeHandler", new FreezeHandler(this));
                registeredHandlers.Add("TFCHandler", new TFCHandler(this));
                registeredHandlers.Add("ComponentsHandler", new ComponentsHandler(this));
                registeredHandlers.Add("EngineHandler", new EngineHandler(this));
                registeredHandlers.Add("StarterHandler", new StarterHandler(this));
                registeredHandlers.Add("DriverAIHandler", new DriverAIHandler(this));

                VehicleBone.TryGetForVehicle(Vehicle, "suspension_lf", out boneLf);
                VehicleBone.TryGetForVehicle(Vehicle, "suspension_rf", out boneRf);

                leftSuspesionOffset = Vehicle.Bones["suspension_lf"].GetRelativeOffsetPosition(new Vector3(0.025f, 0, 0.005f));
                rightSuspesionOffset = Vehicle.Bones["suspension_rf"].GetRelativeOffsetPosition(new Vector3(-0.03f, 0, 0.005f));
            }

            LastDisplacementClone = Clone;
            LastDisplacementClone.Properties.DestinationTime = Utils.CurrentTime;

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

            IsReady = true;
        }

        public T GetHandler<T>()
        {
            if (registeredHandlers.TryGetValue(typeof(T).Name, out Handler handler))
                return (T)(object)handler;

            return default;
        }

        private void DisposeAllHandlers()
        {
            foreach (var handler in registeredHandlers.Values)
                handler.Dispose();
        }

        public void Process()
        {
            if (!IsReady)
                return;

            if (!Vehicle.IsFunctioning())
            {
                TimeMachineHandler.RemoveTimeMachine(this, false);

                return;
            }

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
                    if (Utils.IsAllTiresBurst(Vehicle))
                        Utils.SetTiresBurst(Vehicle, false);
                }
                else
                {
                    if (!Utils.IsAllTiresBurst(Vehicle))
                        Utils.SetTiresBurst(Vehicle, true);
                }
            }

            if (Mods.IsDMC12)
            {
                //In certain situations car can't be entered after hover transformation, here is forced enter task.
                if (Utils.PlayerVehicle == null && Game.IsControlJustPressed(GTA.Control.Enter) && TimeMachineHandler.ClosestTimeMachine == this && TimeMachineHandler.SquareDistToClosestTimeMachine <= 15 && World.GetClosestVehicle(Utils.PlayerPed.Position, TimeMachineHandler.SquareDistToClosestTimeMachine) == this)
                {
                    if (Function.Call<Vehicle>(Hash.GET_VEHICLE_PED_IS_ENTERING, Utils.PlayerPed) != Vehicle)
                        Utils.PlayerPed.Task.EnterVehicle(Vehicle);
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
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0.75f);
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0.75f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearLeft, -0.25f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearRight, -0.25f);
                        break;
                    case SuspensionsType.LiftFront:
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0.75f);
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0.75f);
                        break;
                    case SuspensionsType.LiftRear:
                        Utils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0.75f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearRight, 0.75f);
                        break;
                    case SuspensionsType.LiftFrontAndRear:
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0.75f);
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0.75f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0.75f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearRight, 0.75f);
                        break;
                    case SuspensionsType.LowerFront:
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontLeft, -0.25f);
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontRight, -0.25f);
                        break;
                    case SuspensionsType.LowerRear:
                        Utils.LiftUpWheel(Vehicle, WheelId.RearLeft, -0.25f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearRight, -0.25f);
                        break;
                    case SuspensionsType.LowerFrontAndRear:
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontLeft, -0.25f);
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontRight, -0.25f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearLeft, -0.25f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearRight, -0.25f);
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
            }

            if (Utils.PlayerVehicle != Vehicle && Vehicle.IsVisible && !Properties.Story)
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
            
            foreach (var entry in registeredHandlers)
                entry.Value.Process();

            PhotoMode();

            CustomCameraManager.Process();

            if (Properties.Story || !WaybackMachineHandler.Enabled)
                return;

            if (WaybackMachine == null || !WaybackMachine.IsRecording && !WaybackMachine.IsPlaying)
                if (Properties.TimeTravelPhase < TimeTravelPhase.InTime)
                {
                    if (Utils.PlayerVehicle == Vehicle)
                        WaybackMachineHandler.Create(this);
                    else
                        WaybackMachineHandler.TryFind(this);

                    if (CreateCloneSpawn)
                    {
                        RemoteTimeMachineHandler.AddRemote(Clone, WaybackMachine);
                        CreateCloneSpawn = false;
                    }                        
                }                                       
        }

        private void UpdateBlip()
        {
            if (Blip != null && Blip.Exists())
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF1")}";
                        Blip.Color = BlipColor.NetPlayer22;
                        break;

                    case WormholeType.BTTF2:
                        Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF2")}";
                        Blip.Color = BlipColor.NetPlayer21;
                        break;

                    case WormholeType.BTTF3:
                        if (Mods.Wheel == WheelType.RailroadInvisible)
                        {
                            Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3RR")}";
                            Blip.Color = BlipColor.Orange;
                        }
                        else
                        {
                            Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3")}";
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

            Properties.IsFueled = false;
            Properties.AreTimeCircuitsBroken = true;
            Properties.AreFlyingCircuitsBroken = true;
            Utils.SetTiresBurst(Vehicle, true);

            Vehicle.FuelLevel = 0;            
        }

        public void Repair()
        {
            if (!Mods.IsDMC12)
                return;

            Mods.Wheel = WheelType.Red;
            Utils.SetTiresBurst(Vehicle, false);
            Mods.SuspensionsType = SuspensionsType.LiftFront;
            Mods.Hoodbox = ModState.On;

            Vehicle.FuelLevel = 60.0f;
        }

        private void PhotoMode() 
        {
            if (Properties.PhotoWormholeActive && Players.Wormhole != null && !Players.Wormhole.IsPlaying)
                Players.Wormhole.Play(true);

            if (!Properties.PhotoWormholeActive && Players.Wormhole != null && Players.Wormhole.IsPlaying && Properties.IsPhotoModeOn)
                Players.Wormhole.Stop();

            if (Properties.PhotoGlowingCoilsActive && Props.Coils != null && !Props.Coils.IsSpawned)
            {
                if (Utils.CurrentTime.Hour >= 20 || (Utils.CurrentTime.Hour >= 0 && Utils.CurrentTime.Hour <= 5))
                    Props.Coils.SwapModel(ModelHandler.CoilsGlowingNight);
                else
                    Props.Coils.SwapModel(ModelHandler.CoilsGlowing);

                Mods.OffCoils = ModState.Off;
                Props.Coils.SpawnProp();
            }

            if (!Properties.PhotoGlowingCoilsActive && Props.Coils != null && Props.Coils.IsSpawned)
            {
                Mods.OffCoils = ModState.On;
                Props.Coils.Delete();
            }

            if (Properties.PhotoFluxCapacitorActive && !Properties.IsFluxDoingBlueAnim)
                Events.OnWormholeStarted?.Invoke();

            if (!Properties.PhotoFluxCapacitorActive && Properties.IsFluxDoingBlueAnim && Properties.IsPhotoModeOn)
                Events.OnTimeTravelInterrupted?.Invoke();

            if (Properties.PhotoEngineStallActive && !Properties.IsEngineStalling)
                Events.SetEngineStall?.Invoke(true);

            if (!Properties.PhotoEngineStallActive && Properties.IsEngineStalling && Properties.IsPhotoModeOn)
                Events.SetEngineStall?.Invoke(false);

            Properties.IsPhotoModeOn = Properties.PhotoWormholeActive | Properties.PhotoGlowingCoilsActive | Properties.PhotoFluxCapacitorActive | Properties.IsEngineStalling;
        }

        public void KeyDown(Keys key)
        {
            foreach (var entry in registeredHandlers)
                entry.Value.KeyDown(key);
        }

        public void Dispose(bool deleteVeh = true)
        {
            DisposeAllHandlers();

            CustomCameraManager.Abort();

            Blip?.Delete();

            if (Mods.IsDMC12)
                DMC12Handler.RemoveInstantlyDelorean(DMC12, deleteVeh);
            else if (deleteVeh)
                Vehicle.DeleteCompletely();

            Disposed = true;
        }

        public override string ToString()
        {
            return TimeMachineHandler.TimeMachinesNoStory.IndexOf(this).ToString();
        }

        public static implicit operator Vehicle(TimeMachine timeMachine) => timeMachine.Vehicle;
        public static implicit operator Entity(TimeMachine timeMachine) => timeMachine.Vehicle;
    }
}
