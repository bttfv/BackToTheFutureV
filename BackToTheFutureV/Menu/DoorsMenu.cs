using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace BackToTheFutureV
{
    internal class DoorsMenu : BTTFVMenu
    {
        private readonly NativeItem DriversDoor;
        private readonly NativeItem PassengerDoor;
        private readonly NativeItem Hood;
        private readonly NativeItem Trunk;
        private readonly NativeItem Engine;
        private readonly NativeItem All;

        public DoorsMenu() : base("Doors")
        {
            All = NewItem("All");
            DriversDoor = NewItem("DriversDoor");
            PassengerDoor = NewItem("PassengerDoor");
            Hood = NewItem("Hood");
            Trunk = NewItem("Trunk");
            Engine = NewItem("Engine");
        }

        public override void Tick()
        {
            DriversDoor.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle()?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle().IsConsideredDestroyed;
            PassengerDoor.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle()?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle().IsConsideredDestroyed;
            Hood.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle()?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle().IsConsideredDestroyed && !(FusionUtils.PlayerPed.GetClosestVehicle().IsTimeMachine() && TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerPed.GetClosestVehicle()).Mods.Hoodbox == InternalEnums.ModState.On);
            Trunk.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle()?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle().IsConsideredDestroyed && !FusionUtils.PlayerPed.GetClosestVehicle().IsTimeMachine();
            Engine.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle()?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle().IsConsideredDestroyed && !FusionUtils.PlayerPed.GetClosestVehicle().IsTimeMachine();
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            Vehicle vehicle = FusionUtils.PlayerPed.GetClosestVehicle();

            switch (sender)
            {
                case NativeItem item when item == DriversDoor:
                    if (vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].IsOpen)
                    {
                        vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].Close();
                    }
                    else
                    {
                        vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].Open();
                    }
                    break;
                case NativeItem item when item == PassengerDoor:
                    if (vehicle.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen)
                    {
                        vehicle.Doors[VehicleDoorIndex.FrontRightDoor].Close();
                    }
                    else
                    {
                        vehicle.Doors[VehicleDoorIndex.FrontRightDoor].Open();
                    }
                    break;
                case NativeItem item when item == Hood:
                    if (vehicle.Doors[VehicleDoorIndex.Hood].IsOpen)
                    {
                        vehicle.Doors[VehicleDoorIndex.Hood].Close();
                    }
                    else
                    {
                        vehicle.Doors[VehicleDoorIndex.Hood].Open();
                    }
                    break;
                case NativeItem item when item == Trunk:
                    if (vehicle.Doors[VehicleDoorIndex.Trunk].IsOpen && !vehicle.Doors[VehicleDoorIndex.BackRightDoor].IsOpen)
                    {
                        vehicle.Doors[VehicleDoorIndex.Trunk].Close();
                    }
                    else if (vehicle.Doors[VehicleDoorIndex.BackRightDoor].IsOpen)
                    {
                        vehicle.Doors[VehicleDoorIndex.BackRightDoor].Close();
                        vehicle.Doors[VehicleDoorIndex.Trunk].Close();
                    }
                    else
                    {
                        vehicle.Doors[VehicleDoorIndex.Trunk].Open();
                    }
                    break;
                case NativeItem item when item == Engine:
                    if (vehicle.Doors[VehicleDoorIndex.BackRightDoor].IsOpen)
                    {
                        vehicle.Doors[VehicleDoorIndex.BackRightDoor].Close();
                    }
                    else if (vehicle.Doors[VehicleDoorIndex.Trunk].IsOpen && !vehicle.Doors[VehicleDoorIndex.BackRightDoor].IsOpen)
                    {
                        vehicle.Doors[VehicleDoorIndex.BackRightDoor].Open();
                    }
                    else if (!vehicle.Doors[VehicleDoorIndex.Trunk].IsOpen && !vehicle.Doors[VehicleDoorIndex.BackRightDoor].IsOpen)
                    {
                        vehicle.Doors[VehicleDoorIndex.Trunk].Open();
                        vehicle.Doors[VehicleDoorIndex.BackRightDoor].Open();
                    }
                    break;
                case NativeItem item when item == All:
                    if (vehicle.IsAnyDoorOpen())
                    {
                        vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].Close();
                        vehicle.Doors[VehicleDoorIndex.FrontRightDoor].Close();
                        vehicle.Doors[VehicleDoorIndex.Hood].Close();
                        vehicle.Doors[VehicleDoorIndex.Trunk].Close();
                        vehicle.Doors[VehicleDoorIndex.BackRightDoor].Close();
                    }
                    else
                    {
                        vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].Open();
                        vehicle.Doors[VehicleDoorIndex.FrontRightDoor].Open();
                        vehicle.Doors[VehicleDoorIndex.Hood].Open();
                        vehicle.Doors[VehicleDoorIndex.Trunk].Open();
                        vehicle.Doors[VehicleDoorIndex.BackRightDoor].Open();
                    }

                    break;
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {

        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }

        public override void Menu_Shown(object sender, EventArgs e)
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }
    }
}
