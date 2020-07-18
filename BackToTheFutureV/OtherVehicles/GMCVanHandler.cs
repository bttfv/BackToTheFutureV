using System;
using System.Collections.Generic;
using System.Linq;
using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BackToTheFutureV.OtherVehicles
{
    public class GMCVanHandler
    {
        public Vehicle Vehicle;
        
        public bool IsCargoVehicleInside => (cargoVehicle?.IsAttachedTo(Vehicle)).HasValue ? cargoVehicle.IsAttachedTo(Vehicle) : false;
        public bool IsCargoAttached => IsCargoVehicleInside && cargoVehicle.IsAttachedTo(Vehicle);
        public bool Open { get; private set; }

        private bool IsPlayerInsideVan => Main.PlayerVehicle != null && Main.PlayerVehicle == Vehicle;
        private bool IsPlayerInsideCargo => cargoVehicle != null && Main.PlayerVehicle == cargoVehicle;
        private bool IsPlayerInAnyVehicle => Main.PlayerVehicle != null && Main.PlayerVehicle != Vehicle;

        private Vehicle cargoVehicle;
        private AnimateProp finalRamp;
        private Vector3 finalRampRot = new Vector3(-180f, 0, 0);
        private AnimateProp rampSupports;
        private Vector3 rampSupportsRot = new Vector3(-90f, 0, 0);

        //private AudioPlayer rampOpeningSound;
        private bool doAnimation = false;

        private float rampAngle = 0f;
        private const float maxRampAngle =  0.71f;
        private const int soundTot = 25375;

        public GMCVanHandler(Vehicle vehicle)
        {
            Vehicle = vehicle;

            Open = false;

            finalRamp = new AnimateProp(vehicle, ModelHandler.GMCVanRamps, "ramps_bone");
            rampSupports = new AnimateProp(vehicle, ModelHandler.GMCVanSupports, "supports_bone");            

            finalRamp.SpawnProp(Vector3.Zero,finalRampRot);
            rampSupports.SpawnProp(Vector3.Zero,rampSupportsRot);

            //rampOpeningSound = new AudioPlayer($"VanOpening.wav", false);
        }

        public void KeyDown(KeyEventArgs key)
        {
            if (key.KeyCode == Keys.Add)
                ToggleRampAnimation();
        }

        public void Process()
        {
            //if (IsPlayerInsideVan && Game.IsControlJustPressed(GTA.Control.VehicleHandbrake))
            //    ToggleRampAnimation();

            if (doAnimation)
                ProcessAnimation();

            //Vehicle.Doors[VehicleDoorIndex.Trunk].AngleRatio = 0.71f;
            Vehicle.Doors[VehicleDoorIndex.Trunk].CanBeBroken = false;

            finalRamp.ProcessPropExistance();
            rampSupports.ProcessPropExistance();

            finalRamp.Prop.IsCollisionEnabled = true;
            rampSupports.Prop.IsCollisionEnabled = true;

            if (cargoVehicle == null)
                CheckForCargoVehicle();
            else if (!IsCargoVehicleInside && Vehicle.Position.DistanceTo(cargoVehicle.Position) > 2.6f)
            {
                if (DeloreanHandler.IsVehicleATimeMachine(cargoVehicle))
                {
                    DeloreanHandler.GetTimeMachineFromVehicle(cargoVehicle).Circuits.GetHandler<FlyingHandler>().CanConvert = true;
                }

                cargoVehicle = null;
            }
                

            if (Game.IsControlJustPressed(GTA.Control.VehicleDuck))
            {
                if (IsCargoVehicleInside)
                {
                    if (IsPlayerInsideVan)
                        ToggleCargoVehicle();
                }
                else
                {
                    if (IsPlayerInsideCargo)
                        ToggleCargoVehicle();
                }               
            }
        }

        private void ProcessAnimation()
        {
            //switch (isWheelsOpen)
            //{
            //    case true:
            //        float tmp = (maxRampAngle / soundTot) * rampOpeningSound.PlayPosition;
            //        GTA.UI.Screen.ShowSubtitle(tmp.ToString());
            //        Vehicle.Doors[VehicleDoorIndex.Trunk].AngleRatio = tmp;
            //        break;
            //    case false:                    
            //        Vehicle.Doors[VehicleDoorIndex.Trunk].AngleRatio = maxRampAngle - (maxRampAngle / soundTot) * rampOpeningSound.PlayPosition;
            //        break;
            //}

            //if (rampOpeningSound.Finished)
            //    doAnimation = false;
        }

        public void ToggleRampAnimation()
        {
            //doAnimation = true;
            //isWheelsOpen = !isWheelsOpen;

            //rampOpeningSound.Play(Vehicle);

            //if (isWheelsOpen)
            //    rampOpeningSound.Volume = 1f;
            //else
            //    rampOpeningSound.Volume = 0f;
        }

        public void ToggleCargoVehicle()
        {           
            if (IsCargoAttached)
            {
                cargoVehicle.Detach();
                Main.PlayerPed.Task.WarpIntoVehicle(cargoVehicle, VehicleSeat.Driver);
            }
            else
            {
                Vector3 rot = cargoVehicle.Rotation;

                rot.Z = 0;
                rot.X -= Vehicle.Rotation.X;

                cargoVehicle.AttachTo(Vehicle, Vehicle.GetPositionOffset(cargoVehicle.Position), rot);
                Main.PlayerPed.Task.WarpIntoVehicle(Vehicle, VehicleSeat.Driver);
            }
            cargoVehicle.IsCollisionEnabled = !IsCargoAttached;
        }

        private void CheckForCargoVehicle()
        {
            if (IsPlayerInAnyVehicle && Vehicle.Position.DistanceTo(Main.PlayerVehicle.Position) <= 2.4f)
            {
                cargoVehicle = Main.PlayerVehicle;

                if (DeloreanHandler.IsVehicleATimeMachine(cargoVehicle))
                {
                    DeloreanHandler.GetTimeMachineFromVehicle(cargoVehicle).Circuits.GetHandler<FlyingHandler>().CanConvert = false;
                }
            }                            
        }

        public void Delete()
        {
            if (IsCargoAttached)
                ToggleCargoVehicle();

            finalRamp?.DeleteProp();
            rampSupports?.DeleteProp();

            Vehicle?.Delete();
        }
    }
}
