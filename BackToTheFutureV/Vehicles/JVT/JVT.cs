using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class JVT
    {
        public TimeMachine TimeMachine { get; }

        public Vehicle FlyingTrain { get; }
        public Vehicle FlyingTender { get; }

        public Vehicle Train => WasFlying ? FlyingTrain : CustomTrain.Train;
        public Vehicle Tender => WasFlying ? FlyingTender : CustomTrain.Carriage(1);

        public CustomTrain CustomTrain { get; private set; }

        public bool IsFlying => VehicleControl.GetDeluxoTransformation(FlyingTrain) > 0;
        private bool WasFlying;

        private Vector3 tenderOffset;

        private NativeInput nativeInput = new NativeInput(GTA.Control.VehicleDuck);

        private float angleZ;
        private float angleX;

        public JVT(Vector3 position, bool direction)
        {
            CustomTrain = new CustomTrain(position, direction, 27, 1)
            {
                IsAutomaticBrakeOn = true,
                CanBeDriven = true,
                CheckDriver = true
            };

            CustomTrain.SetVisible(false);
            CustomTrain.SetCollision(false);

            FlyingTrain = World.CreateVehicle(ModelHandler.JVTFlyingModel, Train.Position);
            FlyingTrain.AttachTo(Train);
            FlyingTrain.IsCollisionEnabled = true;

            FlyingTender = World.CreateVehicle(ModelHandler.JVTTenderFlyingModel, Tender.Position);
            FlyingTender.AttachTo(Tender, new Vector3(0, 4.35f, 1.4f));
            FlyingTender.IsCollisionEnabled = true;

            nativeInput.OnControlLongPressed += OnControlLongPressed;

            TimeMachine = TimeMachineHandler.Create(Train, SpawnFlags.Default, WormholeType.BTTF3);
            TimeMachine.CustomTrain = CustomTrain;

            JVTHandler.Add(this);
        }

        private void OnControlLongPressed()
        {
            VehicleControl.SetDeluxoTransformation(FlyingTrain, IsFlying ? 0 : 1);
        }

        public void KeyDown(KeyEventArgs e)
        {

        }

        public void TryAttachRails()
        {
            CustomTrain = new CustomTrain(Train.Position, false, 27, 1);

            if (!CustomTrain.Train.SameDirection(Train))
            {
                CustomTrain.DeleteTrain();
                CustomTrain = new CustomTrain(Train.Position, true, 27, 1);

                if (!CustomTrain.Train.SameDirection(Train))
                {
                    CustomTrain?.DeleteTrain();
                    CustomTrain = null;
                    return;
                }
            }

            CustomTrain.SetVisible(false);
            CustomTrain.SetCollision(false);

            CustomTrain.SetToAttach(Train, Vector3.Zero, 0, 0);

            CustomTrain.SetPosition(Train.GetOffsetPosition(offset: Vector3.Zero.GetSingleOffset(Coordinate.Y, -1)));

            CustomTrain.OnVehicleAttached += CustomTrain_OnVehicleAttached;
        }

        private void CustomTrain_OnVehicleAttached(bool toRogersSierra = false)
        {
            WasFlying = false;

            TimeMachine.CustomTrain = CustomTrain;
            TimeMachine.SwitchVehicle(Train);
            FusionUtils.PlayerPed.SetIntoVehicle(Train, VehicleSeat.Driver);

            CustomTrain.DetachTargetVehicle();
            CustomTrain.IsReadyToAttach = false;

            FlyingTrain.AttachTo(Train);
            FlyingTrain.IsCollisionEnabled = true;

            FlyingTender.AttachTo(Tender, new Vector3(0, 4.35f, 1.4f));
            FlyingTender.IsCollisionEnabled = true;
        }

        public void Tick()
        {
            if (IsFlying && !WasFlying)
            {
                tenderOffset = FlyingTrain.GetPositionOffset(FlyingTender.Position);

                WasFlying = true;

                TimeMachine.SwitchVehicle(Train);
                FusionUtils.PlayerPed.SetIntoVehicle(Train, VehicleSeat.Driver);

                Train.Detach();
                Tender.Detach();

                //Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY, Tender, Train, 0, 0, tenderOffset.X, tenderOffset.Y, 0, 0, 0, 0, 0, 0, 0, 9999999f, true, true, true, true, true, 2);
                //Tender.AttachTo(Train, tenderOffset);
                //Tender.IsCollisionEnabled = true;
                //Tender.IsRecordingCollisions = true;

                CustomTrain.DeleteTrain();
                CustomTrain = null;
            }

            if (IsFlying)
            {
                if (TimeMachine.Mods.Type != TimeMachineType.FlyingJVT)
                    TimeMachine.Mods.Type = TimeMachineType.FlyingJVT;

                angleZ = FusionUtils.Lerp(angleZ, -Train.SteeringAngle, Game.LastFrameTime);
                angleX = FusionUtils.Lerp(angleX, Train.Rotation.X, Game.LastFrameTime);

                Tender.AttachTo(Train, tenderOffset, new Vector3(angleX, 0, angleZ));

                VehicleControl.SetDeluxoFlyMode(Train, 1f);
            }
            else if (TimeMachine.Mods.Type == TimeMachineType.FlyingJVT)
            {
                TimeMachine.Mods.Type = TimeMachineType.JVT;
                TryAttachRails();
            }

            if (CustomTrain == null)
                return;

            if (JVTHandler.CurrentJVT == this && !CustomTrain.IsAccelerationOn)
                CustomTrain.IsAccelerationOn = true;

            if (JVTHandler.CurrentJVT != this && CustomTrain.IsAccelerationOn)
                CustomTrain.IsAccelerationOn = false;

            CustomTrain?.Tick();
        }

        public void Dispose()
        {
            CustomTrain?.DeleteTrain();

            Train?.Delete();
            Tender?.Delete();
        }
    }
}
