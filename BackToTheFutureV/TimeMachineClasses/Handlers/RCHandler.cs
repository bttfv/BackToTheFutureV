using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public enum RcModes
    {
        FromCarCamera,
        FromPlayerCamera
    }

    public class RcHandler : Handler
    {
        public Ped Clone { get; private set; }

        public RcModes CurrentMode { get; set; }

        private Camera _camera;
        private Blip _blip;

        private NativeInput rcHandbrake;

        private bool _forcedHandbrake = false;

        private bool _handleBoost = false;

        public RcHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            PlayerSwitch.OnSwitchingComplete += OnSwitchingComplete;
           
            rcHandbrake = new NativeInput(GTA.Control.VehicleHandbrake);
            rcHandbrake.OnControlJustPressed += RcHandbrake_OnControlJustPressed;

            Events.SetRCMode += SetRCMode;
            Events.OnSimulateSpeedReached += StopForcedHandbrake;
        }

        public void SetRCMode(bool state, bool instant = false)
        {
            if (state)
                StartRC();
            else
                StopRC(instant);
        }

        private void RcHandbrake_OnControlJustPressed()
        {
            if (!Properties.IsRemoteControlled || !Mods.IsDMC12)
                return;

            if (Mods.HoverUnderbody == ModState.On && Properties.IsFlying)
                return;

            if (_forcedHandbrake || Game.IsControlPressed(GTA.Control.VehicleDuck))
            {
                Sounds.RCBrake?.Play();
                SetForcedHandbrake();

                if (_forcedHandbrake && Mods.Reactor == ReactorType.Nuclear && Mods.Plate == PlateType.Outatime && Properties.IsFueled)
                    Sounds.RCSomeSerious?.Play();
            }            
        }

        private void StopForcedHandbrake()
        {
            if (Properties.IsRemoteControlled && _forcedHandbrake)
                SetForcedHandbrake();
        }

        private void SetForcedHandbrake()
        {
            if (!Mods.IsDMC12)
                return;

            _forcedHandbrake = !_forcedHandbrake;

            Vehicle.IsBurnoutForced = _forcedHandbrake;
            Vehicle.CanTiresBurst = !_forcedHandbrake;

            if (_forcedHandbrake)
            {
                Properties.TorqueMultiplier *= 4;
                _handleBoost = true;

                Events.SetSimulateSpeed?.Invoke(64, 8);
            }
        }

        private void OnSwitchingComplete()
        {
            if (!Properties.IsRemoteControlled)
            {
                Clone?.Delete();
                Clone = null;
            }
        }

        public void StartRC()
        {
            if (Vehicle == null) 
                return;

            Properties.IsRemoteControlled = true;

            if (_handleBoost)
            {                
                Events.SetSimulateSpeed?.Invoke(0, 0);
                _handleBoost = false;
            }

            Clone = PlayerSwitch.CreatePedAndSwitch(out TimeMachine.OriginalPed, Utils.PlayerPed.Position, Utils.PlayerPed.Heading, true);

            Clone.SetIntoVehicle(Vehicle, VehicleSeat.Driver);

            Clone.CanFlyThroughWindscreen = false;
            Clone.CanBeDraggedOutOfVehicle = false;
            Clone.BlockPermanentEvents = true;
            Clone.AlwaysKeepTask = true;
            Clone.IsVisible = false;
            
            _blip = TimeMachine.OriginalPed.AddBlip();
            _blip.Sprite = (BlipSprite)480;
            _blip.Color = BlipColor.White;

            foreach (var sound in Sounds.RCSounds)
                sound.SourceEntity = TimeMachine.OriginalPed;

            Sounds.RCOn?.Play();

            TimeMachine.OriginalPed.Task.TurnTo(Vehicle);

            if (CurrentMode == RcModes.FromPlayerCamera)
            {
                _camera = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                _camera.PointAt(Vehicle);
                World.RenderingCamera = _camera;
            }
        }

        public void StopRC(bool instant = false)
        {
            if (Properties.IsRemoteControlled)
            {
                Properties.IsRemoteControlled = false;

                if (TimeMachine.OriginalPed == null)
                    return;

                TimeMachine.OriginalPed.Task.ClearAll();

                PlayerSwitch.Switch(TimeMachine.OriginalPed, true, instant);

                if (!instant)
                    Sounds.RCOff?.Play();
                else
                    Clone?.Delete();

                if (Sounds.RCSomeSerious.IsAnyInstancePlaying)
                    Sounds.RCSomeSerious?.Stop();

                if (_forcedHandbrake)
                    SetForcedHandbrake();

                Function.Call(Hash.CLEAR_FOCUS);

                _blip?.Delete();

                _camera?.Delete();
                _camera = null;
                World.RenderingCamera = null;                          
            }
        }

        public override void Process()
        {
            if (!Properties.IsRemoteControlled)
            {
                if (_handleBoost)
                {
                    Properties.TorqueMultiplier /= 4;
                    _handleBoost = false;
                }

                return;
            }

            if (_handleBoost && Game.IsControlPressed(GTA.Control.VehicleBrake))
            {
                Properties.TorqueMultiplier /= 4;
                _handleBoost = false;
            }

            if (PlayerSwitch.IsManualInProgress)
                return;

            if (TimeMachine.OriginalPed == null)
                return;

            if (TimeMachine.OriginalPed.HasCollided)
            {
                RCManager.StopRemoteControl();
                return;
            }

            if (Game.IsControlJustPressed(GTA.Control.VehicleExit))
                RCManager.StopRemoteControl();
            
            //// When u go too far from clone ped, game removes collision under him and 
            ////  he falls through the ground, so if player is 50 we freeze clone
            //var isCloneFreezed = CommonSettings.PlayerPed.Position.DistanceToSquared(OriginalPed.Position) >= 50*50;
            //Function.Call(Hash.FREEZE_ENTITY_POSITION, OriginalPed, isCloneFreezed);

            var origPos = TimeMachine.OriginalPed.Position;
            var carPos = Vehicle.Position;
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, origPos.X, origPos.Y, origPos.Z);
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, carPos.X, carPos.Y, carPos.Z);

            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, Utils.PlayerPed);
            Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, Utils.PlayerPed);
            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, TimeMachine.OriginalPed);
            Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, TimeMachine.OriginalPed);

            if (Game.IsControlJustPressed(GTA.Control.VehicleAccelerate))
                Sounds.RCAcceleration?.Play();

            if (Game.IsControlJustPressed(GTA.Control.NextCamera))
            {
                if (CurrentMode == RcModes.FromPlayerCamera)
                {
                    CurrentMode = RcModes.FromCarCamera;

                    Function.Call(Hash.CLEAR_FOCUS);

                    _camera?.Delete();
                    _camera = null;
                    World.RenderingCamera = null;

                    Function.Call(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, 0);                    
                }
                else if (Function.Call<int>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE) == 1)
                {
                    CurrentMode = RcModes.FromPlayerCamera;

                    Function.Call(Hash.SET_FOCUS_ENTITY, TimeMachine.OriginalPed);

                    _camera = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                    _camera.PointAt(Vehicle);
                    World.RenderingCamera = _camera;                    
                }
            }

            if (CurrentMode == RcModes.FromPlayerCamera && _camera != null && _camera.Exists())
                _camera.Position = TimeMachine.OriginalPed.Bones[Bone.SkelHead].GetOffsetPosition(new Vector3(0,0.1f,0));
        }

        public override void Stop()
        {
            if (Properties.IsRemoteControlled)
                RCManager.StopRemoteControl();
        }

        public override void Dispose()
        {
            if (Properties.IsRemoteControlled)
                RCManager.StopRemoteControl(true);
        }

        public override void KeyDown(Keys key) { }
    }
}