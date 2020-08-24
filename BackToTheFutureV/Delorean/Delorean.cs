using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Memory;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Delorean.Handlers;

namespace BackToTheFutureV.Delorean
{
    public class DeloreanTimeMachine : DMC12
    {
        /// <summary>
        /// The <seealso cref="TimeCircuits"/> of this Delorean.
        /// </summary>
        public TimeCircuits Circuits { get; private set; }

        /// <summary>
        /// Whether the Delorean is in time or not.
        /// </summary>
        public bool IsInTime { get; set; }

        /// <summary>
        /// The remote ped AI;
        /// </summary>
        public RemotePedAI RemotePedAI { get; set; }

        /// <summary>
        /// Whether this delorean is given scaleform priority or not.
        /// </summary>
        public bool IsGivenScaleformPriority { get; set; }

        public DeloreanCopy Copy { get { return new DeloreanCopy(this); } }

        private DeloreanCopy _lastDisplacementCopy = null;

        public DeloreanCopy LastDisplacementCopy { get
            {
                if (_lastDisplacementCopy == null)
                {
                    _lastDisplacementCopy = new DeloreanCopy(this, true);
                    _lastDisplacementCopy.Circuits.DestinationTime = Main.CurrentTime;
                }
                    
                return _lastDisplacementCopy;
            } set => _lastDisplacementCopy = value; }
        
        public DeloreanTimeMachine(Vehicle vehicle, bool isNew, DeloreanType deloreanType = DeloreanType.Unknown) : base(vehicle, isNew)
        {
            if (isNew && Mods.DeloreanType == DeloreanType.Unknown)
            {
                if (deloreanType != DeloreanType.Unknown)
                    Mods.DeloreanType = deloreanType;
                else
                    Mods.DeloreanType = DeloreanType.BTTF1;
            }
                
            Circuits = new TimeCircuits(this);

            _BTTFDecals = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.BTTFDecals), Vector3.Zero, Vector3.Zero);
            _BTTFDecals.SpawnProp();

            if (isNew)
            {
                VehicleBone.TryGetForVehicle(vehicle, "suspension_lf", out boneLf);
                VehicleBone.TryGetForVehicle(vehicle, "suspension_rf", out boneRf);

                leftSuspesionOffset = vehicle.Bones["suspension_lf"].GetRelativeOffsetPosition(new Vector3(0.025f, 0, 0.005f));
                rightSuspesionOffset = vehicle.Bones["suspension_rf"].GetRelativeOffsetPosition(new Vector3(-0.03f, 0, 0.005f));

                LastDisplacementCopy = Copy;
                LastDisplacementCopy.Circuits.DestinationTime = Main.CurrentTime;
            }

            DeloreanHandler.AddTimeMachine(this);
        }

        private AnimateProp _BTTFDecals;

        private VehicleBone boneLf;
        private VehicleBone boneRf;

        private Vector3 leftSuspesionOffset;
        private Vector3 rightSuspesionOffset;

        private bool _firstRedSetup = true;

        public override void Tick()
        {
            base.Tick();

            if (Mods.HoverUnderbody == ModState.Off)
                VehicleControl.SetDeluxoTransformation(Vehicle, 0f); // Force Delorean to not transform, is a crude fix.

            if (!Circuits.IsRemoteControlled)
                if (Circuits.IsTimeTraveling || Circuits.IsReentering)
                    Vehicle.LockStatus = VehicleLockStatus.StickPlayerInside;
                else
                    Vehicle.LockStatus = VehicleLockStatus.None;

            Vehicle.IsRadioEnabled = false;

            if (Mods.Wheel == WheelType.RailroadInvisible)
            {
                if (Circuits.IsOnTracks)
                {
                    if (Utils.IsAllTiresBurst(Vehicle))
                        Utils.SetTiresBurst(Vehicle, false);
                } else
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
                
            Circuits?.Tick();
        }

        public override void KeyDown(Keys key)
        {
            base.KeyDown(key);

            Circuits?.KeyDown(key);
        }

        public override void Dispose(bool deleteVehicle = true)
        {
            base.Dispose(deleteVehicle);

            _BTTFDecals?.Dispose();

            Circuits?.DisposeAllHandlers();
        }

        public void Convert(DeloreanType To)
        {
            if (DeloreanType == To | DeloreanType == DeloreanType.DMC12 | To == DeloreanType.DMC12)
                return;
            
            if (Circuits.IsFlying)
                Circuits.FlyingHandler.SetFlyMode(false, true);

            Mods.Convert(To);
        }

        public static implicit operator Vehicle(DeloreanTimeMachine dv) => dv.Vehicle;
        public static explicit operator DeloreanTimeMachine(Vehicle b) => DeloreanHandler.GetTimeMachineFromVehicle(b);
        public static implicit operator Entity(DeloreanTimeMachine d) => Entity.FromHandle(d.Vehicle.Handle);
        public static explicit operator DeloreanTimeMachine(Entity b) => DeloreanHandler.GetTimeMachineFromVehicle((Vehicle)b);

    }

    public class DMC12 
    {
        /// <summary>
        /// A <seealso cref="GTA.Vehicle"/> object representing this Delorean.
        /// </summary>
        public Vehicle Vehicle { get; }

        /// <summary>
        /// <seealso cref="BackToTheFutureV.Delorean.DeloreanType"/> of this Delorean.
        /// </summary>
        public DeloreanType DeloreanType { get => (DeloreanType)Vehicle.Mods[VehicleModType.TrimDesign].Index;  }

        /// <summary>
        /// String representing the lower case <seealso cref="DeloreanType"/> of this Delorean.
        /// </summary>
        public string LowerCaseDeloreanType => DeloreanType.ToString().ToLower().Replace("rr","");

        public Vector3 LastVelocity;

        public DeloreanMods Mods;

        /// <summary>
        /// Allows to get or set the miles-per-hour speed of the Delorean directly.
        /// </summary>
        public float MPHSpeed
        {
            get
            {
                return Vehicle.GetMPHSpeed();
            }
            set
            {
                Vehicle.SetMPHSpeed(value);
            }
        }

        /// <summary>
        /// Allows you to know if this Delorean is a TimeMachine or a normal DMC12.
        /// </summary>
        public bool IsTimeMachine { get => DeloreanType > 0; }

        public bool IsStockWheel => Mods.Wheel == WheelType.Stock || Mods.Wheel == WheelType.StockInvisible;

        /// <summary>
        /// Manager for the delorean. Controls needles, suspension, radiator fans.
        /// </summary>
        public DeloreanManager NeedleManager { get; }
        
        public Blip DeloreanBlip { get; set; }

        public DMC12(Vehicle vehicle, bool isNew)
        {
            Vehicle = vehicle;

            // Probably solution for LOD1 bug
            Function.Call(Hash.REQUEST_VEHICLE_HIGH_DETAIL_MODEL, Vehicle);

            Utils.HideVehicle(Vehicle, false);

            Function.Call((Hash)0x1201E8A3290A3B98, Vehicle, false);
            Function.Call((Hash)0x28B18377EB6E25F6, Vehicle, false);

            Mods = new DeloreanMods(vehicle, isNew);

            NeedleManager = new DeloreanManager(this);

            CreateBlip();

            DeloreanHandler.AddDelorean(this);
        }

        private void CreateBlip()
        {
            DeloreanBlip = Vehicle.AddBlip();
            DeloreanBlip.Sprite = BlipSprite.Deluxo;
            DeloreanBlip.Name = "DeLorean";
        }

        public virtual void Tick()
        {
            if (DeloreanType == DeloreanType.DMC12)
                VehicleControl.SetDeluxoTransformation(Vehicle, 0f); // Force Delorean to not transform, is a crude fix.

            if (Main.PlayerVehicle == Vehicle)
                DeloreanBlip.Delete();
            else if (!DeloreanBlip.Exists())
                CreateBlip();

            if (DeloreanBlip.Exists())
            {
                switch (DeloreanType)
                {
                    case DeloreanType.BTTF1:
                        DeloreanBlip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF1")}";
                        DeloreanBlip.Color = BlipColor.NetPlayer22;
                        break;

                    case DeloreanType.BTTF2:
                        DeloreanBlip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF2")}";
                        DeloreanBlip.Color = BlipColor.NetPlayer21;
                        break;

                    case DeloreanType.BTTF3:
                        if (Mods.Wheel == WheelType.RailroadInvisible)
                        {
                            DeloreanBlip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3RR")}";
                            DeloreanBlip.Color = BlipColor.Orange;
                        }
                        else
                        {
                            DeloreanBlip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3")}";
                            DeloreanBlip.Color = BlipColor.Red;
                        }
                        break;
                }
            }

            Function.Call(Hash.DISABLE_CONTROL_ACTION, 31, 337);

            NeedleManager?.Process();
        }

        public virtual void KeyDown(Keys key)
        {

        }

        public virtual void Dispose(bool deleteVehicle = true)
        {
            if (Vehicle.GetPedOnSeat(VehicleSeat.Driver) != Main.PlayerPed)
                Vehicle?.GetPedOnSeat(VehicleSeat.Driver)?.Delete();

            if (deleteVehicle)
            {
                int handle = Vehicle.Handle;
                unsafe
                {
                    Function.Call(Hash.SET_ENTITY_AS_NO_LONGER_NEEDED, &handle);
                }
                Vehicle?.Delete();
            }
            DeloreanBlip?.Delete();

            NeedleManager?.Dispose();

            Mods.RemoveRrWheels();
        }

        public static DMC12 CreateDelorean(Vehicle vehicle)
        {            
            switch (vehicle.Mods[VehicleModType.TrimDesign].Index)
            {
                case -1:
                    vehicle.Mods[VehicleModType.TrimDesign].Index = 0;
                    return new DMC12(vehicle, false);
                case 0:
                    return new DMC12(vehicle, false);
                default:
                    return new DeloreanTimeMachine(vehicle, false);
            }
        }

        public static DMC12 CreateDelorean(Vector3 position, float heading = 0, DeloreanType type = DeloreanType.BTTF1)
        {
            Vehicle Vehicle = World.CreateVehicle(ModelHandler.RequestModel(ModelHandler.DMC12), position, heading);

            if (ModSettings.CinematicSpawn)
                Vehicle.Position = Utils.GetPositionOnGround(Vehicle.Position, 0);

            ModelHandler.DMC12.MarkAsNoLongerNeeded();

            Vehicle.Mods.InstallModKit();

            Vehicle.Mods[VehicleModType.TrimDesign].Index = (int)type;
            Vehicle.Mods[VehicleModType.TrimDesign].Variation = false;

            if (type == DeloreanType.DMC12)
                return new DMC12(Vehicle, true);

            return new DeloreanTimeMachine(Vehicle, true);
        }

        public static implicit operator Vehicle(DMC12 dv) => dv.Vehicle;
        public static explicit operator DMC12(Vehicle b) => DeloreanHandler.GetDeloreanFromVehicle(b);
        public static implicit operator Entity(DMC12 d) => Entity.FromHandle(d.Vehicle.Handle);
        public static explicit operator DMC12(Entity b) => DeloreanHandler.GetDeloreanFromVehicle((Vehicle)b);

    }
}
