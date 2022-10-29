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
            DriversDoor.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle(5f)?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsConsideredDestroyed;
            PassengerDoor.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle(5f)?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsConsideredDestroyed;
            Hood.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle(5f)?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsConsideredDestroyed;
            Trunk.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle(5f)?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsConsideredDestroyed && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsTimeMachine();
            Engine.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle(5f)?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsConsideredDestroyed && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsTimeMachine();
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            Vehicle vehicle = FusionUtils.PlayerPed.GetClosestVehicle(5f);

            switch (sender)
            {
                case NativeItem item when item == DriversDoor:
                    if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 0) > 0f)
                    {
                        Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 0, false);
                    }
                    else
                    {
                        Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 0, false, false);
                    }
                    break;
                case NativeItem item when item == PassengerDoor:
                    if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 1) > 0f)
                    {
                        Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 1, false);
                    }
                    else
                    {
                        Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 1, false, false);
                    }
                    break;
                case NativeItem item when item == Hood:
                    if (Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, vehicle, 4) > 0f)
                    {
                        Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, vehicle, 4, false);
                    }
                    else
                    {
                        Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, vehicle, 4, false, false);
                    }
                    break;
                case NativeItem item when item == Trunk:
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
                    break;
                case NativeItem item when item == Engine:
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
