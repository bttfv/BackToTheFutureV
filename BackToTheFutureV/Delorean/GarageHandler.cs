using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using GTA;
using GTA.Math;
using NativeUI;
using BackToTheFutureV.Delorean;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA.UI;

namespace BackToTheFutureV
{
    public class GarageHandler
    {
        private static Vector3 garageEnter = new Vector3(487.6072f, -1314.429f, 28f);
        private static Vector3 garageExitRotation = new Vector3(0.6753192f, 0.1893532f, -64.23885f);
        private static Vector3 garageVehiclePosition = new Vector3(478.5183f, -1309.171f, 28.40384f);
        private static Vector3 garageVehicleRotation = new Vector3(0.1059294f, 0.1011007f, -153.394f);
        private static Vector3 garageCameraPosition = new Vector3(480.7745f, -1317.811f, 31.3951f);
        private static Vector3 garageCameraRotation = new Vector3(-15f, 6.403302E-07f, 7.73631f);

        private static bool isEnteringGarage;
        private static int enterGarageTimer;
        private static int currentEnterGarageStep;

        private static bool isExitingGarage;
        private static int exitGarageTimer;
        private static int currentExitGarageStep;

        private static bool canEnterGarage = true; // True by default
        public static bool IsInGarage { get; private set; }

        private static Vehicle currentVehicle;
        private static DMC12 currentDelorean;

        private static Camera garageCamera;

        private static DateTime? destinationTime;
        private static DateTime? previousTime;
        private static DeloreanType switchTo;
        private static bool hasHook;
        private static bool isSwitching;
        private static int currentSwitchStep;
        private static int switchTimer;
        private static bool carChanged;

        private static AudioPlayer garageCarChanged = new AudioPlayer("story/garage.wav", false);

        // Menu stuff
        private static UIMenu garageMenu;

        // Buttons
        private static UIMenuItem dmc12Button;
        private static UIMenuItem bttf1Button;
        private static UIMenuItem bttfHookButton;
        private static UIMenuItem bttf2Button;
        private static UIMenuItem exitButton;

        public static void Initialize()
        {
            garageMenu = new UIMenu("Garage", "SELECT AN OPTION:");
            garageMenu.ResetKey(UIMenu.MenuControls.Back);
            garageMenu.SetBannerType("./scripts/BackToTheFutureV/BTTFV.png");

            garageMenu.AddItem(dmc12Button = new UIMenuItem("DMC-12", "Converts your Delorean Time Machine into a regular DMC-12."));
            garageMenu.AddItem(bttf1Button = new UIMenuItem("BTTF1 Time Machine", "Converts your current DMC-12 into a BTTF1 Time Machine."));
            garageMenu.AddItem(bttfHookButton = new UIMenuItem("BTTF1 W/Hook Time Machine", "Converts your current DMC-12 into a BTTF1 Time Machine that has a hook."));
            garageMenu.AddItem(bttf2Button = new UIMenuItem("BTTF2 Time Machine", "Converts your current DMC-12 into a BTTF2 Time Machine."));
            garageMenu.AddItem(exitButton = new UIMenuItem("Exit", "Exit out of the garage."));

            garageMenu.OnItemSelect += GarageMenu_OnItemSelect;

            Main.MenuPool.Add(garageMenu);
        }

        private static void GarageMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if(selectedItem == exitButton)
            {
                ExitGarage();
            }
            else if(selectedItem == dmc12Button)
            {
                SwitchModel(DeloreanType.DMC12);
            }
            else if(selectedItem == bttf1Button)
            {
                SwitchModel(DeloreanType.BTTF1);
            }
            else if(selectedItem == bttf2Button)
            {
                SwitchModel(DeloreanType.BTTF2);
            }
            else if(selectedItem == bttfHookButton)
            {
                SwitchModel(DeloreanType.BTTF1, true);
            }
        }

        public static void Tick()
        {
            if(IsInGarage)
            {
                Game.DisableAllControlsThisFrame();
            }

            HandleSwitching();
            HandleGarargeEntry();
            HandleGarageEntering();
            HandleGarageExiting();
        }

        public static void EnterGarage(Vehicle vehicle)
        {
            var delorean = DeloreanHandler.GetDeloreanFromVehicle(vehicle);

            currentVehicle = vehicle;

            if (delorean != null)
                currentDelorean = delorean;

            IsInGarage = true;
            isEnteringGarage = true;
        }

        public static void ExitGarage()
        {
            isExitingGarage = true;
            IsInGarage = false;
        }

        private static void HandleGarargeEntry()
        {
            if (!IsInGarage && !isEnteringGarage && canEnterGarage)
            {
                Vehicle vehicle = Main.PlayerVehicle;

                if (vehicle != null && IsVehicleDMC12(vehicle))
                {
                    float distance = vehicle.Position.DistanceTo(garageEnter);

                    if (distance <= 25f)
                    {
                        // Draw the marker
                        World.DrawMarker(MarkerType.VerticalCylinder, garageEnter, Vector3.Zero, Vector3.Zero, new Vector3(4, 4, 4), Color.Red);
                    }

                    if (distance <= 5f && vehicle.Speed == 0)
                    {
                        EnterGarage(vehicle);
                    }
                }
            }
        }

        private static bool IsVehicleDMC12(Vehicle veh)
        {
            return veh.Model == ModelHandler.DMC12;
        }

        private static void SwitchModel(DeloreanType to, bool hook = false)
        {
            if (isSwitching) return;

            isSwitching = true;
            switchTo = to;
            hasHook = hook;
            carChanged = true;
        }

        private static void UpdateGarage()
        {
            var currentTime = Utils.GetWorldTime();

            if (currentTime < new DateTime(1985, 1, 1))
                bttf1Button.Enabled = false;
            else
                bttf1Button.Enabled = true;

            if (currentTime < new DateTime(2015, 1, 1))
                bttf2Button.Enabled = false;
            else
                bttf2Button.Enabled = true;

            if (currentDelorean != null && (currentDelorean.IsTimeMachine && ((DeloreanTimeMachine)currentDelorean).DeloreanType == DeloreanType.BTTF1))
                bttfHookButton.Enabled = true;
            else
                bttfHookButton.Enabled = false;
        }

        private static void HandleSwitching()
        {
            if (isSwitching)
            {
                if (Game.GameTime < switchTimer) return;

                switch(currentSwitchStep)
                {
                    case 0:
                        Screen.FadeOut(1000);
                        if (currentDelorean != null && currentDelorean.IsTimeMachine)
                        {
                            DeloreanTimeMachine timeMachine = currentDelorean as DeloreanTimeMachine;
                            destinationTime = timeMachine.Circuits.DestinationTime;
                            previousTime = timeMachine.Circuits.PreviousTime;
                        }
                        else
                        {
                            destinationTime = null;
                            previousTime = null;
                        }
                        switchTimer = Game.GameTime + 1200;
                        currentSwitchStep++;

                        break;

                    case 1:
                        // Do the switching here

                        // Delete old DMC12
                        if (currentDelorean != null)
                            DeloreanHandler.RemoveDelorean(currentDelorean);
                        else
                            currentVehicle?.Delete();

                        // Spawn new delorean
                        var newDelorean = DMC12.CreateDelorean(garageVehiclePosition, garageVehicleRotation.ToHeading(), switchTo, hasHook);
                        newDelorean.Vehicle.Rotation = garageVehicleRotation;

                        // Set the destination and previous time, so they're not lost
                        if(switchTo != DeloreanType.DMC12 && destinationTime != null && previousTime != null)
                        {
                            ((DeloreanTimeMachine)newDelorean).Circuits.DestinationTime = destinationTime.GetValueOrDefault();
                            ((DeloreanTimeMachine)newDelorean).Circuits.PreviousTime = previousTime.GetValueOrDefault();
                        }

                        // Warp player inside vehicle
                        Main.PlayerPed.Task.WarpIntoVehicle(newDelorean, VehicleSeat.Driver);

                        // Update variables
                        currentVehicle = newDelorean;
                        currentDelorean = newDelorean;

                        // Move forward
                        currentSwitchStep++;
                        switchTimer = Game.GameTime + 1000;

                        break;

                    case 2:
                        Screen.FadeIn(1000);
                        UpdateGarage();
                        Main.MenuPool.CloseAllMenus();
                        garageMenu.Visible = true;
                        switchTimer = 0;
                        currentSwitchStep = 0;
                        isSwitching = false;

                        break;
                }
            }
        }

        private static void HandleGarageExiting()
        {
            if(isExitingGarage)
            {
                if (Game.GameTime < exitGarageTimer) return;

                switch(currentExitGarageStep)
                {
                    case 0:
                        Main.MenuPool.CloseAllMenus();
                        Screen.FadeOut(1000);
                        exitGarageTimer = Game.GameTime + 1200;
                        currentExitGarageStep++;

                        break;

                    case 1:
                        canEnterGarage = false;
                        currentVehicle.Position = garageEnter;
                        currentVehicle.Rotation = garageExitRotation;
                        garageCamera = null;
                        World.RenderingCamera = null;
                        enterGarageTimer = Game.GameTime + 1000;
                        currentExitGarageStep++;

                        break;

                    case 2:
                        Screen.FadeIn(1000);

                        if(carChanged)
                        {
                            garageCarChanged.Play(currentVehicle);
                            carChanged = false;
                        }

                        currentExitGarageStep++;
                        break;

                    case 3:
                        float dist = currentVehicle.Position.DistanceTo(garageEnter);
                        if(dist >= 10)
                        {
                            canEnterGarage = true;
                            currentExitGarageStep = 0;
                            isExitingGarage = false;
                            exitGarageTimer = 0;

                            currentVehicle = null;
                            currentDelorean = null;
                        }

                        break;
                }
            }
        }

        private static void HandleGarageEntering()
        {
            if (isEnteringGarage)
            {
                if (Game.GameTime < enterGarageTimer) return;

                switch (currentEnterGarageStep)
                {
                    case 0:
                        Screen.FadeOut(1000);
                        enterGarageTimer = Game.GameTime + 1200;
                        currentEnterGarageStep++;

                        break;

                    case 1:
                        currentVehicle.Position = garageVehiclePosition;
                        currentVehicle.Rotation = garageVehicleRotation;
                        garageCamera = World.CreateCamera(garageCameraPosition, garageCameraRotation, GameplayCamera.FieldOfView);
                        World.RenderingCamera = garageCamera;
                        enterGarageTimer = Game.GameTime + 1000;
                        currentEnterGarageStep++;

                        break;

                    case 2:
                        Screen.FadeIn(1000);
                        UpdateGarage();
                        Main.MenuPool.CloseAllMenus();
                        garageMenu.Visible = true;
                        enterGarageTimer = 0;
                        currentEnterGarageStep = 0;
                        isEnteringGarage = false;

                        break;
                }
            }
        }
    }
}
