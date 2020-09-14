using BackToTheFutureV.Memory;
using BackToTheFutureV.TimeMachineClasses.Handlers;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses
{
    public class TimeMachine
    {
        public Vehicle Vehicle;
        public DMC12 DMC12 { get; }

        public EventsHandler Events { get; private set; }
        public PropertiesHandler Properties { get; }
        public TimeMachineMods Mods { get; }
        public SoundsHandler Sounds { get; private set; }
        public PropsHandler Props { get; private set; }
        public PlayersHandler Players { get; private set; }
        public ScaleformsHandler Scaleforms { get; private set; }
        public PtfxHandler SFX { get; private set; }

        public TimeMachineClone Clone => new TimeMachineClone(this);
        public TimeMachineClone LastDisplacementClone { get; set; }
        public Ped OriginalPed;

        private readonly Dictionary<string, Handler> registeredHandlers = new Dictionary<string, Handler>();

        private VehicleBone boneLf;
        private VehicleBone boneRf;

        private Vector3 leftSuspesionOffset;
        private Vector3 rightSuspesionOffset;

        private bool _firstRedSetup = true;

        public bool Disposed { get; private set; }

        public TimeMachine(DMC12 dmc12, WormholeType wormholeType)
        {
            DMC12 = dmc12;
            Vehicle = DMC12.Vehicle;

            Mods = new TimeMachineMods(this, wormholeType);

            Properties = new PropertiesHandler(this);

            BuildTimeMachine();
        }

        public TimeMachine(Vehicle vehicle, WormholeType wormholeType)
        {
            Vehicle = vehicle;

            if (vehicle.Model == ModelHandler.DMC12)
            {
                DMC12 = DMC12Handler.GetDeloreanFromVehicle(vehicle);

                if (DMC12 == null)
                    DMC12 = new DMC12(vehicle);
            }

            Mods = new TimeMachineMods(this, wormholeType);

            Properties = new PropertiesHandler(this);

            BuildTimeMachine();
        }

        private void BuildTimeMachine()
        {
            registeredHandlers.Add("EventsHandler", Events = new EventsHandler(this));
            registeredHandlers.Add("SoundsHandler", Sounds = new SoundsHandler(this));
            registeredHandlers.Add("PropsHandler", Props = new PropsHandler(this));
            registeredHandlers.Add("PlayersHandler", Players = new PlayersHandler(this));
            registeredHandlers.Add("ScaleformsHandler", Scaleforms = new ScaleformsHandler(this));
            registeredHandlers.Add("PtfxHandler", SFX = new PtfxHandler(this));

            registeredHandlers.Add("SpeedoHandler", new SpeedoHandler(this));
            registeredHandlers.Add("TimeTravelHandler", new TimeTravelHandler(this));                        
            registeredHandlers.Add("TCDHandler", new TCDHandler(this));            
            registeredHandlers.Add("InputHandler", new InputHandler(this));
            registeredHandlers.Add("RcHandler", new RcHandler(this));
            registeredHandlers.Add("FuelHandler", new FuelHandler(this));
            registeredHandlers.Add("ReentryHandler", new ReentryHandler(this));                        
            registeredHandlers.Add("TimeCircuitsErrorHandler", new TimeCircuitsErrorHandler(this));            
            registeredHandlers.Add("SparksHandler", new SparksHandler(this));
            registeredHandlers.Add("FlyingHandler", new FlyingHandler(this));
            registeredHandlers.Add("RailroadHandler", new RailroadHandler(this));

            if (Mods.IsDMC12)
            {                
                registeredHandlers.Add("FluxCapacitorHandler", new FluxCapacitorHandler(this));
                registeredHandlers.Add("FreezeHandler", new FreezeHandler(this));
                registeredHandlers.Add("PlutoniumGaugeHandler", new PlutoniumGaugeHandler(this));
                registeredHandlers.Add("TFCHandler", new TFCHandler(this));                
                registeredHandlers.Add("HookHandler", new HookHandler(this));
                registeredHandlers.Add("HoodboxHandler", new HoodboxHandler(this));
                registeredHandlers.Add("EngineHandler", new EngineHandler(this));
                registeredHandlers.Add("PhotoHandler", new PhotoHandler(this));                
                registeredHandlers.Add("LightningStrikeHandler", new LightningStrikeHandler(this));
                registeredHandlers.Add("StarterHandler", new StarterHandler(this));

                VehicleBone.TryGetForVehicle(Vehicle, "suspension_lf", out boneLf);
                VehicleBone.TryGetForVehicle(Vehicle, "suspension_rf", out boneRf);

                leftSuspesionOffset = Vehicle.Bones["suspension_lf"].GetRelativeOffsetPosition(new Vector3(0.025f, 0, 0.005f));
                rightSuspesionOffset = Vehicle.Bones["suspension_rf"].GetRelativeOffsetPosition(new Vector3(-0.03f, 0, 0.005f));
            }

            LastDisplacementClone = Clone;
            LastDisplacementClone.Properties.DestinationTime = Main.CurrentTime;

            TimeMachineHandler.AddTimeMachine(this);
        }

        public T GetHandler<T>()
        {
            if (registeredHandlers.TryGetValue(typeof(T).Name, out Handler handler))
                return (T)(object)handler;

            return default;
        }

        public void DisposeAllHandlers()
        {
            foreach (var handler in registeredHandlers.Values)
                handler.Dispose();
        }

        public void Process()
        {
            if (Mods.HoverUnderbody == ModState.Off && Mods.IsDMC12)
                VehicleControl.SetDeluxoTransformation(Vehicle, 0f);

            if (!Properties.IsRemoteControlled)
                if (Properties.IsTimeTravelling || Properties.IsReentering)
                    Vehicle.LockStatus = VehicleLockStatus.StickPlayerInside;
                else
                    Vehicle.LockStatus = VehicleLockStatus.None;

            Vehicle.IsRadioEnabled = false;

            if (Mods.IsDMC12)
            {
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

                VehicleWindowCollection windows = Vehicle.Windows;
                windows[VehicleWindowIndex.BackLeftWindow].Remove();
                windows[VehicleWindowIndex.BackRightWindow].Remove();
                windows[VehicleWindowIndex.ExtraWindow4].Remove();

                Vehicle.Doors[VehicleDoorIndex.Trunk].Break(false);
                Vehicle.Doors[VehicleDoorIndex.BackRightDoor].Break(false);

                if (Mods.Hoodbox == ModState.On)
                    Vehicle.Doors[VehicleDoorIndex.Hood].CanBeBroken = false;

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
            }

            if (Main.PlayerVehicle != Vehicle)
            {
                if (Vehicle.AttachedBlips.Count() > 0)
                {
                    switch (Mods.WormholeType)
                    {
                        case WormholeType.BTTF1:
                            Vehicle.AttachedBlip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF1")}";
                            Vehicle.AttachedBlip.Color = BlipColor.NetPlayer22;
                            break;

                        case WormholeType.BTTF2:
                            Vehicle.AttachedBlip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF2")}";
                            Vehicle.AttachedBlip.Color = BlipColor.NetPlayer21;
                            break;

                        case WormholeType.BTTF3:
                            if (Mods.Wheel == WheelType.RailroadInvisible)
                            {
                                Vehicle.AttachedBlip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3RR")}";
                                Vehicle.AttachedBlip.Color = BlipColor.Orange;
                            }
                            else
                            {
                                Vehicle.AttachedBlip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3")}";
                                Vehicle.AttachedBlip.Color = BlipColor.Red;
                            }
                            break;
                    }
                }
                else
                    CreateBlip();
            }
            else if (Vehicle.AttachedBlips.Count() > 0)
                Vehicle.AttachedBlip.Delete();

            foreach (var entry in registeredHandlers)
                entry.Value.Process();
        }

        public void CreateBlip()
        {
            Vehicle.AddBlip();

            Vehicle.AttachedBlip.Sprite = BlipSprite.Deluxo;
        }

        public void KeyDown(Keys key)
        {
            foreach (var entry in registeredHandlers)
                entry.Value.KeyPress(key);
        }

        public void Dispose(bool deleteVeh = true)
        {
            DisposeAllHandlers();

            if (Mods.IsDMC12)
                DMC12.Dispose(deleteVeh);
            else if (deleteVeh)            
                Vehicle.Delete();
            
            Disposed = true;
        }
    }
}
