using BackToTheFutureV.Menu;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.UI;
using System.Windows.Forms;
using static BackToTheFutureV.Utility.InternalEnums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    internal class FuelHandler : HandlerPrimitive
    {
        private int _nextBlink;
        private int _nextId;
        private int _blinks;

        private bool longPressed;

        private const int TotalBlinks = 16;

        private bool _isBlinking;

        private float _reactorGlowingTime = 0;

        private NativeInput InteractPressed;

        public FuelHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            InteractPressed = new NativeInput(GTA.Control.Context);
            InteractPressed.OnControlJustReleased += OnControlJustReleased;
            InteractPressed.OnControlLongPressed += OnControlLongPressed;

            SetEmpty(false);

            Events.StartFuelBlink += StartFuelBlink;
            Events.SetRefuel += Refuel;

            Events.OnReactorTypeChanged += OnReactorTypeChanged;
            OnReactorTypeChanged();
        }

        private void OnReactorTypeChanged()
        {
            Players.Refuel?.Dispose();

            if (Mods.Reactor == ReactorType.Nuclear)
                Players.Refuel = new PlutoniumRefillPlayer(TimeMachine);
            else
                Players.Refuel = new MrFusionRefillPlayer(TimeMachine);

            Players.Refuel.OnPlayerCompleted += OnCompleted;
            Properties.IsRefueling = false;
        }

        private void OnCompleted()
        {
            Properties.IsRefueling = !Properties.IsRefueling;
        }

        private void OnControlJustReleased()
        {
            if (!IsPedInPosition() || !Properties.IsRefueling || Players.Refuel.IsPlaying)
                return;

            if (HasFuel())
                Refuel(Utils.PlayerPed);
            else
            {
                if (Mods.Reactor == ReactorType.MrFusion)
                    Notification.Show(BTTFVMenu.GetLocalizedText("NotEnoughGarbage"));
                else
                    Notification.Show(BTTFVMenu.GetLocalizedText("NotEnoughPlutonium"));
            }
        }

        private void OnControlLongPressed()
        {
            if (longPressed || !IsPedInPosition() || Players.Refuel.IsPlaying)
                return;

            Players.Refuel?.Play();
            longPressed = true;
        }

        private void StartFuelBlink()
        {
            if (Properties.IsFueled)
                return;

            _isBlinking = true;
        }

        private void Refuel(Ped refuelPed)
        {
            if (Properties.ReactorCharge >= Constants.MaxReactorCharge && refuelPed == Utils.PlayerPed)
                return;

            if (Mods.Reactor == ReactorType.Nuclear)
                Sounds.PlutoniumRefuel?.Play();

            if (refuelPed == Utils.PlayerPed && !ModSettings.InfiniteFuel)
            {
                if (Mods.Reactor == ReactorType.MrFusion)
                    InternalInventory.Current.Trash--;
                else
                    InternalInventory.Current.Plutonium--;
            }

            TaskSequence taskSequence = new TaskSequence();

            taskSequence.AddTask.TurnTo(Vehicle.Bones["mr_fusion_handle"].Position, 1000);
            taskSequence.AddTask.PlayAnimation("anim@narcotics@trash", "drop_front");
            taskSequence.AddTask.ClearAnimation("anim@narcotics@trash", "drop_front");

            refuelPed?.Task.PerformSequence(taskSequence);

            Properties.ReactorCharge++;

            SetEmpty(false);
        }

        public override void Tick()
        {
            Players.Refuel?.Tick();

            if (longPressed && !Game.IsControlPressed(GTA.Control.Context))
                longPressed = false;

            // Pulsing animation while refueling for plutonium (bttf1) delorean
            if (Mods.Reactor == ReactorType.Nuclear && ModSettings.GlowingPlutoniumReactor)
            {
                if (!Properties.IsFueled && !Properties.IsRefueling)
                {
                    if (Mods.GlowingReactor == ModState.On)
                        Mods.GlowingReactor = ModState.Off;
                }
                else
                {
                    if (Properties.IsRefueling)
                    {
                        _reactorGlowingTime += Game.LastFrameTime;

                        if (_reactorGlowingTime > 0.25f)
                        {
                            Mods.GlowingReactor = Mods.GlowingReactor == ModState.On ? ModState.Off : ModState.On;

                            _reactorGlowingTime = 0;
                        }
                    }
                    else if (Mods.GlowingReactor != ModState.On)
                    {
                        Mods.GlowingReactor = ModState.On;

                        _reactorGlowingTime = 0;
                    }
                }
            }
            else if (Mods.GlowingReactor != ModState.Off)
                Mods.GlowingReactor = ModState.Off;

            // Empty animation

            // For BTTF1
            if (_isBlinking && !Properties.IsFueled && Mods.Reactor == ReactorType.Nuclear)
            {
                // Blink anim end
                if (_blinks > TotalBlinks && Game.GameTime > _nextBlink)
                {
                    _isBlinking = false;
                    _blinks = 0;

                    SetEmpty(true);
                }
                // Keep beeping
                else if (_blinks <= TotalBlinks && Game.GameTime > _nextBlink)
                {
                    // ID is kinda off/on
                    if (_nextId == 0)
                    {
                        SetEmpty(true);

                        _nextId = 1;
                        _nextBlink = Game.GameTime + 640;

                        Sounds.FuelEmpty?.Play();
                    }
                    else
                    {
                        SetEmpty(false);

                        _nextId = 0;
                        _nextBlink = Game.GameTime + 200;
                    }
                    _blinks++;
                }
            }
            // For BTTF2/3
            // Fueled -> EMPTY indicator isn't glowing, EMPTY in GUI is hidden
            // Not Fueled -> Glowing EMPTY indicator inside car and in GUI
            else
            {
                if (Properties.IsFueled)
                {
                    SetEmpty(false);
                    HideEmpty();
                }
                else
                    SetEmpty(true);
            }

            if (Properties.IsRefueling)
            {
                if (Mods.Reactor == ReactorType.MrFusion)
                {
                    if (Vehicle.IsVisible && !Particles.MrFusionSmoke.IsPlaying)
                        Particles.MrFusionSmoke.Play();

                    if (!Vehicle.IsVisible && Particles.MrFusionSmoke.IsPlaying)
                        Particles.MrFusionSmoke.Stop();
                }
            }

            if (!IsPedInPosition())
                return;

            if (Properties.IsRefueling)
            {
                if (HasFuel() && Properties.ReactorCharge < Constants.MaxReactorCharge)
                    Utils.DisplayHelpText(BTTFVMenu.GetLocalizedText("RefuelReactor"));
                else
                    Utils.DisplayHelpText(BTTFVMenu.GetLocalizedText("CloseReactor"));
            }
            else
                Utils.DisplayHelpText(BTTFVMenu.GetLocalizedText("OpenReactor"));
        }

        private bool HasFuel()
        {
            if (ModSettings.InfiniteFuel)
                return true;

            if (Mods.Reactor == ReactorType.Nuclear && InternalInventory.Current.Plutonium > 0)
                return true;

            if (Mods.Reactor == ReactorType.MrFusion && InternalInventory.Current.Trash > 0)
                return true;

            return false;
        }

        private bool IsPedInPosition()
        {
            return IsPedInPosition(Vehicle, Utils.PlayerPed);
        }

        internal static bool IsPedInPosition(Vehicle vehicle, Ped ped)
        {
            if (ped.IsInVehicle())
                return false;

            Vector3 bootPos = vehicle.Bones["mr_fusion"].Position;

            Vector3 dir;
            float angle, dist;

            if (ped == Utils.PlayerPed)
            {
                dir = bootPos - GameplayCamera.Position;
                angle = Vector3.Angle(dir, GameplayCamera.Direction);
                dist = Utils.PlayerPed.DistanceToSquared2D(vehicle, "mr_fusion");
            }
            else
            {
                dir = bootPos - ped.Position;
                angle = Vector3.Angle(dir, ped.ForwardVector);
                dist = ped.DistanceToSquared2D(vehicle, "mr_fusion") - 0.1f;
            }

            return angle < 45 && dist < 1.6f;
        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Stop()
        {

        }

        public override void Dispose()
        {

        }

        private void SetEmpty(bool isOn)
        {
            if (Utils.PlayerVehicle == Vehicle)
                Properties.HUDProperties.Empty = isOn ? HUD.Core.EmptyType.On : HUD.Core.EmptyType.Off;

            if (Vehicle.IsVisible == false)
                return;

            if (isOn)
            {
                Props.EmptyOff?.Delete();
                Props.EmptyGlowing?.SpawnProp();
            }
            else
            {
                Props.EmptyOff?.SpawnProp();
                Props.EmptyGlowing?.Delete();
            }
        }

        private void HideEmpty()
        {
            if (Utils.PlayerVehicle != Vehicle)
                return;

            Properties.HUDProperties.Empty = HUD.Core.EmptyType.Hide;
        }
    }
}