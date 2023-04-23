using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class FuelHandler : HandlerPrimitive
    {
        private int _nextBlink;
        private int _nextId;
        private int _blinks;

        private const int TotalBlinks = 16;

        private bool _isBlinking;

        private float _reactorGlowingTime = 0;
        private float _refuelTime = 0;

        private readonly NativeInput InteractPressed;

        private TaskSequence refuelSequence;

        public FuelHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            InteractPressed = new NativeInput(GTA.Control.Context);
            InteractPressed.OnControlJustReleased += OnControlJustReleased;
            InteractPressed.OnControlLongPressed += OnControlLongPressed;

            SetEmpty(false);

            Events.StartFuelBlink += StartFuelBlink;
            Events.SetReactorState += SetReactorState;

            Events.OnReactorTypeChanged += OnReactorTypeChanged;
            OnReactorTypeChanged();
        }

        private void OnReactorTypeChanged()
        {
            Players.Refuel?.Dispose();

            if (Mods.Reactor == ReactorType.Nuclear)
            {
                Players.Refuel = new PlutoniumRefillPlayer(TimeMachine);
            }
            else
            {
                Players.Refuel = new MrFusionRefillPlayer(TimeMachine);
            }

            _refuelTime = 0;

            if (Properties.ReactorState != ReactorState.Closed)
            {
                SetReactorState(ReactorState.Opened);
            }
        }

        private void OnControlJustReleased()
        {
            if (Properties.ReactorState != ReactorState.Opened || Players.Refuel.IsPlaying || !IsPlayerInPosition())
            {
                return;
            }

            if (HasFuel())
            {
                SetReactorState(ReactorState.Refueling);
            }
            else
            {
                if (Mods.Reactor == ReactorType.MrFusion)
                {
                    TextHandler.Me.ShowNotification("NotEnoughGarbage");
                }
                else
                {
                    TextHandler.Me.ShowNotification("NotEnoughPlutonium");
                }
            }
        }

        private void OnControlLongPressed()
        {
            if ((Properties.ReactorState != ReactorState.Opened && Properties.ReactorState != ReactorState.Closed) || Players.Refuel.IsPlaying || !IsPlayerInPosition())
            {
                return;
            }

            SetReactorState(Properties.ReactorState == ReactorState.Closed ? ReactorState.Opened : ReactorState.Closed);
        }

        private void SetReactorState(ReactorState reactorState)
        {
            if (Properties.ReactorState == reactorState)
            {
                return;
            }

            Properties.ReactorState = reactorState;

            if (WaybackSystem.CurrentPlayerRecording != default && !Properties.IsWayback)
                WaybackSystem.CurrentPlayerRecording.OverrideVehicle = Vehicle;

            switch (reactorState)
            {
                case ReactorState.Opened:
                case ReactorState.Closed:
                    Players.Refuel?.Play();
                    break;
                case ReactorState.Refueling:
                    Refuel();
                    break;
            }
        }

        private void Refuel()
        {
            Ped refuelPed = IsPedInPosition();

            if (Properties.ReactorCharge >= Constants.MaxReactorCharge && refuelPed == FusionUtils.PlayerPed)
            {
                return;
            }

            if (Mods.Reactor == ReactorType.Nuclear)
            {
                Sounds.PlutoniumRefuel?.Play();
            }

            if (refuelPed == FusionUtils.PlayerPed && !ModSettings.InfiniteFuel)
            {
                if (Mods.Reactor == ReactorType.MrFusion)
                {
                    InternalInventory.Current.Trash--;
                }
                else
                {
                    InternalInventory.Current.Plutonium--;
                }
            }

            if (refuelPed != null)
            {
                refuelSequence = new TaskSequence();

                refuelSequence.AddTask.ClearAllImmediately();
                refuelSequence.AddTask.TurnTo(Vehicle.Bones["mr_fusion_handle"].Position, 1000);
                refuelSequence.AddTask.PlayAnimation("anim@narcotics@trash", "drop_front");
                refuelSequence.AddTask.ClearAnimation("anim@narcotics@trash", "drop_front");

                refuelPed?.Task.PerformSequence(refuelSequence);
            }

            Properties.ReactorCharge++;

            SetEmpty(false);
        }

        private void StartFuelBlink()
        {
            if (Properties.IsFueled)
            {
                return;
            }

            _isBlinking = true;
        }

        public override void Tick()
        {
            Players.Refuel?.Tick();

            if (Properties.ReactorState == ReactorState.Refueling)
            {
                if (_refuelTime > 3)
                {
                    Properties.ReactorState = ReactorState.Opened;
                    _refuelTime = 0;
                }
                else
                {
                    _refuelTime += Game.LastFrameTime;
                }
            }

            if (Mods.Reactor == ReactorType.MrFusion && Properties.ReactorState == ReactorState.Closed && FusionUtils.IsBulletPresent(Vehicle.Bones["mr_fusion_handle"].Position, 0.085f) && FusionUtils.Random.NextDouble() < 0.2)
            {
                SetReactorState(ReactorState.Opened);
            }

            // Pulsing animation while refueling for plutonium (bttf1) delorean
            if (Mods.Reactor == ReactorType.Nuclear && ModSettings.GlowingPlutoniumReactor)
            {
                if (!Properties.IsFueled && Properties.ReactorState != ReactorState.Refueling)
                {
                    if (Mods.GlowingReactor == ModState.On)
                    {
                        Mods.GlowingReactor = ModState.Off;
                    }
                }
                else
                {
                    if (Properties.ReactorState == ReactorState.Refueling)
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
            {
                Mods.GlowingReactor = ModState.Off;
            }

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
                {
                    SetEmpty(true);
                }
            }

            if (Properties.ReactorState == ReactorState.Refueling && Mods.Reactor == ReactorType.MrFusion)
            {
                if (Vehicle.IsVisible && !Particles.MrFusionSmoke.IsPlaying)
                {
                    Particles.MrFusionSmoke.Play();
                }

                if (!Vehicle.IsVisible && Particles.MrFusionSmoke.IsPlaying)
                {
                    Particles.MrFusionSmoke.Stop();
                }
            }

            if (Players.Refuel.IsPlaying || !IsPlayerInPosition())
            {
                return;
            }

            switch (Properties.ReactorState)
            {
                case ReactorState.Opened:
                    if (HasFuel() && Properties.ReactorCharge < Constants.MaxReactorCharge)
                    {
                        TextHandler.Me.ShowHelp("RefuelReactor");
                    }
                    else
                    {
                        TextHandler.Me.ShowHelp("CloseReactor");
                    }

                    break;
                case ReactorState.Closed:
                    TextHandler.Me.ShowHelp("OpenReactor");
                    break;
            }
        }

        private bool HasFuel()
        {
            if (ModSettings.InfiniteFuel)
            {
                return true;
            }

            if (Mods.Reactor == ReactorType.Nuclear && InternalInventory.Current.Plutonium > 0)
            {
                return true;
            }

            if (Mods.Reactor == ReactorType.MrFusion && InternalInventory.Current.Trash > 0)
            {
                return true;
            }

            return false;
        }

        private bool IsPlayerInPosition()
        {
            return IsPedInPosition(Vehicle, FusionUtils.PlayerPed);
        }

        private Ped IsPedInPosition()
        {
            Ped ped = World.GetClosestPed(Vehicle.Bones["mr_fusion"].Position, 1.9f);

            if (!IsPedInPosition(Vehicle, ped))
            {
                ped = null;
            }

            return ped;
        }

        internal static bool IsPedInPosition(Vehicle vehicle, Ped ped)
        {
            if (!ped.NotNullAndExists() || ped.IsInVehicle())
            {
                return false;
            }

            Vector3 bootPos = vehicle.Bones["mr_fusion"].Position;

            Vector3 dir;
            float angle, dist;

            if (ped == FusionUtils.PlayerPed)
            {
                dir = bootPos - GameplayCamera.Position;
                angle = Vector3.Angle(dir, GameplayCamera.Direction);
                dist = FusionUtils.PlayerPed.DistanceToSquared2D(vehicle, "mr_fusion");
            }
            else
            {
                dir = bootPos - ped.Position;
                angle = Vector3.Angle(dir, ped.ForwardVector);
                dist = ped.DistanceToSquared2D(vehicle, "mr_fusion") - 0.1f;
            }

            return angle < 45 && dist < 1.8f;
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
            if (FusionUtils.PlayerVehicle == Vehicle)
            {
                Properties.HUDProperties.Empty = isOn ? EmptyType.On : EmptyType.Off;
            }

            if (Vehicle.IsVisible == false)
            {
                return;
            }

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
            if (FusionUtils.PlayerVehicle != Vehicle)
            {
                return;
            }

            Properties.HUDProperties.Empty = EmptyType.Hide;
        }
    }
}
