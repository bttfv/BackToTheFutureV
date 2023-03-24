using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class ComponentsHandler : HandlerPrimitive
    {
        //Hoodbox
        private int _warmUp = 0;
        private int _coolDown = 0;
        private bool _wasOn;
        private bool _wasStruck;

        private float _fluxBandsCooldown = -1;

        public ComponentsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnReenterStarted += OnReenterStarted;
            Events.OnReenterEnded += OnReenterEnded;
            Events.OnVehicleSpawned += OnReenterEnded;
            Events.SetHoodboxWarmedUp += Instant;
            TimeHandler.OnDayNightChange += OnDayNightChange;

            OnDayNightChange();
        }

        public void OnReenterStarted()
        {
            if (Properties.HasBeenStruckByLightning)
            {
                _wasStruck = true;
            }
            else
            {
                _wasStruck = false;
            }
        }

        public void OnReenterEnded()
        {
            if (Mods.Plate == PlateType.Outatime && !Properties.IsFlying)
            {
                Mods.Plate = PlateType.Empty;
            }

            if (Mods.Hook == HookState.On && _wasStruck)
            {
                Mods.Hook = HookState.Removed;
                _wasStruck = false;
            }

            if (Mods.IsDMC12 && Properties.IsFlying && Mods.Reactor == ReactorType.MrFusion)
            {
                Properties.PhotoGlowingCoilsActive = true;
                _fluxBandsCooldown = 0;
            }

            if (Mods.Hoodbox == ModState.Off || Properties.AreHoodboxCircuitsReady)
            {
                return;
            }

            Instant();
        }

        private void Instant()
        {
            Props.HoodboxLights.SpawnProp();
            Properties.AreHoodboxCircuitsReady = true;
        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        private void OnDayNightChange()
        {
            Props.HoodboxLights?.Delete();

            if (TimeHandler.IsNight)
            {
                Props.HoodboxLights.SwapModel(ModelHandler.HoodboxLightsNight);
            }
            else
            {
                Props.HoodboxLights.SwapModel(ModelHandler.HoodboxLights);
            }
        }

        private void HookProcess()
        {
            if ((Mods.Hook == HookState.On || Mods.Hook == HookState.OnDoor) && !FusionUtils.PlayerPed.IsInVehicle())
            {
                if (FusionUtils.PlayerPed.DistanceToSquared2D(Vehicle, "window_rf", 1))
                {
                    if (Mods.Hook == HookState.OnDoor)
                    {
                        TextHandler.Me.ShowHelp("ApplyHook");
                    }
                    else
                    {
                        TextHandler.Me.ShowHelp("RemoveHook");
                    }

                    if (Game.IsControlJustPressed(GTA.Control.Context))
                    {
                        if (Mods.Hook == HookState.OnDoor)
                        {
                            Mods.Hook = HookState.On;
                        }
                        else
                        {
                            Mods.Hook = HookState.OnDoor;
                        }
                    }
                }
            }
        }

        private void CompassProcess()
        {
            Props.Compass?.MoveProp(Vector3.Zero, new Vector3(0, 0, Vehicle.Heading));

            if (Props.Compass.Visible != Vehicle.IsVisible)
            {
                Props.Compass.Visible = Vehicle.IsVisible;
            }
        }

        private void BulovaProcess()
        {
            if (Mods.Bulova == ModState.On && !Props.BulovaClockRing.IsSpawned)
            {
                Props.BulovaClockHour.SpawnProp();
                Props.BulovaClockMinute.SpawnProp();
                Props.BulovaClockRing.SpawnProp();
            }
            else if (Mods.Bulova == ModState.Off && Props.BulovaClockRing.IsSpawned)
            {
                Props.BulovaClockHour?.Delete();
                Props.BulovaClockMinute?.Delete();
                Props.BulovaClockRing?.Delete();
            }
        }

        private void HoodboxProcess()
        {
            if (Properties.AreHoodboxCircuitsReady)
            {
                if (Mods.Hoodbox == ModState.Off)
                {
                    Stop();
                    return;
                }

                if (!Props.HoodboxLights.IsSpawned)
                {
                    Props.HoodboxLights.SpawnProp();
                }

                if (Vehicle.IsVisible != Props.HoodboxLights?.Visible)
                {
                    Props.HoodboxLights.Visible = Vehicle.IsVisible;
                }

                if (Properties.AreTimeCircuitsOn)
                {
                    _coolDown = 0;
                    _wasOn = true;
                }

                if (!Properties.AreTimeCircuitsOn && !Vehicle.IsEngineRunning && _wasOn && _coolDown == 0)
                {
                    _coolDown = Game.GameTime + 16000;
                    _wasOn = false;
                }

                if (_coolDown > 0 && _coolDown < Game.GameTime)
                {
                    Stop();
                }

                return;
            }

            if (_warmUp > 0 && _warmUp < Game.GameTime)
            {
                TextHandler.Me.ShowHelp("WarmupComplete");
                Props.HoodboxLights.SpawnProp();

                _warmUp = 0;
                Properties.AreHoodboxCircuitsReady = true;

                return;
            }

            if (Mods.Hoodbox == ModState.Off || Properties.AreHoodboxCircuitsReady || _warmUp > 0 || FusionUtils.PlayerPed.IsInVehicle() || TcdEditer.IsEditing || RCGUIEditer.IsEditing)
            {
                return;
            }

            if (FusionUtils.PlayerPed.DistanceToSquared2D(Vehicle, "bonnet", 1.5f))
            {
                TextHandler.Me.ShowHelp("Warmup");

                if (Game.IsControlJustPressed(GTA.Control.Context))
                {
                    _warmUp = Game.GameTime + 8000;
                }
            }
        }

        public override void Tick()
        {
            HookProcess();

            CompassProcess();

            HoodboxProcess();

            BulovaProcess();

            if (_fluxBandsCooldown > -1)
            {
                _fluxBandsCooldown += Game.LastFrameTime;

                if (_fluxBandsCooldown > 2)
                {
                    Properties.PhotoGlowingCoilsActive = false;
                    _fluxBandsCooldown = -1;
                }
            }
        }

        public override void Stop()
        {
            Props.HoodboxLights?.Delete();
            Properties.AreHoodboxCircuitsReady = false;
            _warmUp = 0;
            _coolDown = 0;
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
