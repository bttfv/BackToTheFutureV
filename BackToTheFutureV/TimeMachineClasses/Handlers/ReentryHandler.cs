using GTA;
using GTA.Math;
using BackToTheFutureV.Players;
using System.Windows.Forms;
using BackToTheFutureV.Utility;
using KlangRageAudioLibrary;
using GTA.Native;
using BackToTheFutureV.Memory;
using BackToTheFutureV.Vehicles;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class ReentryHandler : Handler
    {       
        private float _reentryTimer;
        private int _currentStep;
        private int _gameTimer;

        public ReentryHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            SFX.Flash = new PtfxEntityPlayer("core", "ent_anim_paparazzi_flash", Vehicle, Vector3.Zero, Vector3.Zero, 50f);

            Events.OnReenter += OnReenter;
            Events.OnReenterCompleted += OnReenterCompleted;
        }

        public void OnReenter()
        {
            Properties.IsReentering = true;
            Properties.IsTimeTravelling = false;
        }

        public override void Process()
        {
            _reentryTimer += Game.LastFrameTime;

            if (_reentryTimer > 2)
            {
                if (Vehicle.Driver == null)
                {
                    Vehicle.IsHandbrakeForcedOn = false;
                    Vehicle.SteeringAngle = 0;
                }
            }

            if (!Properties.IsReentering) return;
            if (Game.GameTime < _gameTimer) return;

            // Time will be fixed to your destination time until reentry is completed.
            Utils.SetWorldTime(Properties.DestinationTime);

            switch (_currentStep)
            {
                case 0:
                    Sounds.Reenter.Play();

                    SFX.Flash.Play();

                    Function.Call(Hash.ADD_SHOCKING_EVENT_AT_POSITION, 88, Vehicle.Position.X, Vehicle.Position.Y, Vehicle.Position.Z, 1f);

                    var timeToAdd = 500;

                    switch (Mods.WormholeType)
                    {
                        case WormholeType.BTTF1:
                            timeToAdd = 100;
                            break;
                        case WormholeType.BTTF2:
                        case WormholeType.BTTF3:
                            timeToAdd = 600;
                            break;
                    }

                    _gameTimer = Game.GameTime + timeToAdd;
                    _currentStep++;
                    break;

                case 1:

                    SFX.Flash.Play();

                    timeToAdd = 500;

                    switch (Mods.WormholeType)
                    {
                        case WormholeType.BTTF1:
                            timeToAdd = 300;
                            break;
                        case WormholeType.BTTF2:
                        case WormholeType.BTTF3:
                            timeToAdd = 600;
                            break;
                    }

                    _gameTimer = Game.GameTime + timeToAdd;
                    _currentStep++;
                    break;

                case 2:

                    SFX.Flash.Play();

                    _currentStep++;
                    break;

                case 3:
                    Stop();

                    Events.OnReenterCompleted?.Invoke();

                    break;
            }
        }

        private void OnReenterCompleted()
        {
            _reentryTimer = 0;

            if (Properties.WasOnTracks)
                Events.SetRailroadMode?.Invoke(true, true);
            else
            {
                Vehicle.Velocity = Properties.LastVelocity;

                if (Vehicle.GetMPHSpeed() == 0)
                    Vehicle.SetMPHSpeed(88);
            }

            if (!Properties.HasBeenStruckByLightning && Mods.IsDMC12)
                Properties.IsFueled = false;

            if (!Properties.IsOnTracks && Vehicle.Driver == null)
            {
                Vehicle.SteeringAngle = Utils.Random.NextDouble() >= 0.5f ? 35 : -35;
                Vehicle.IsHandbrakeForcedOn = true;
                Vehicle.Speed = Vehicle.Speed / 2;

                VehicleControl.SetBrake(Vehicle, 1f);
            }

            Events.SetFreeze?.Invoke(!Properties.HasBeenStruckByLightning);

            //Function.Call(Hash.SPECIAL_ABILITY_UNLOCK, Main.PlayerPed.Model);
            Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, true);
        }


        public override void Stop()
        {
            _currentStep = 0;
            _gameTimer = 0;
            Properties.IsReentering = false;

            if (Mods.HoverUnderbody == ModState.On)
                Properties.CanConvert = true;

            if (Mods.Hook == HookState.On)
                Mods.Hook = HookState.Removed;

            if (Mods.Plate == PlateType.Outatime)
                Mods.Plate = PlateType.Empty;

            Utils.HideVehicle(Vehicle, false);

            Main.HideGui = false;

            Main.DisablePlayerSwitching = false;

            Game.Player.IgnoredByPolice = false;
        }

        public override void Dispose()
        {
            Sounds.Reenter?.Dispose();
        }

        public override void KeyPress(Keys key)
        {

        }
    }
}