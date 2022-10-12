using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
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

        public DoorsMenu() : base("Doors")
        {
            DriversDoor = NewItem("DriversDoor");
            PassengerDoor = NewItem("PassengerDoor");
            Hood = NewItem("Hood");
            Trunk = NewItem("Trunk");
            Engine = NewItem("Engine");
        }

        public override void Tick()
        {
            DriversDoor.Enabled = FusionUtils.PlayerPed.GetClosestVehicle(5f).NotNullAndExists() && FusionUtils.PlayerPed.GetClosestVehicle(5f).Model == ModelHandler.DMC12;
            PassengerDoor.Enabled = FusionUtils.PlayerPed.GetClosestVehicle(5f).NotNullAndExists() && FusionUtils.PlayerPed.GetClosestVehicle(5f).Model == ModelHandler.DMC12;
            Hood.Enabled = FusionUtils.PlayerPed.GetClosestVehicle(5f).NotNullAndExists() && FusionUtils.PlayerPed.GetClosestVehicle(5f).Model == ModelHandler.DMC12;
            Trunk.Enabled = FusionUtils.PlayerPed.GetClosestVehicle(5f).NotNullAndExists() && FusionUtils.PlayerPed.GetClosestVehicle(5f).Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsTimeMachine();
            Engine.Enabled = FusionUtils.PlayerPed.GetClosestVehicle(5f).NotNullAndExists() && FusionUtils.PlayerPed.GetClosestVehicle(5f).Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsTimeMachine();
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            Vehicle vehicle = FusionUtils.PlayerPed.GetClosestVehicle(5f);

            if (sender == DriversDoor)
            {
                if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 0) > 0f)
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 0, false);
                }
                else
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 0, false, false);
                }
            }
            if (sender == PassengerDoor)
            {
                if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 1) > 0f)
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 1, false);
                }
                else
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 1, false, false);
                }
            }
            if (sender == Hood)
            {
                if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 4) > 0f)
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 4, false);
                }
                else
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 4, false, false);
                }
            }
            if (sender == Trunk)
            {
                if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 5) > 0f && Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 3) == 0f)
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 5, false);
                }
                else if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 3) > 0f)
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 3, false);
                    Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 5, false);
                }
                else
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 5, false, false);
                }
            }
            if (sender == Engine)
            {
                if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 3) > 0f)
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 3, false);
                }
                else if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 5) > 0f && Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 3) == 0f)
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 3, false, false);
                }
                else if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 5) == 0f && Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 3) == 0f)
                {
                    Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 5, false, false);
                    Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 3, false, false);
                }
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
