using System.Collections.Generic;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Windows.Forms;
using BackToTheFutureV.Utility;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean.Handlers
{
    public enum RcModes
    {
        FromCarCamera,
        FromPlayerCamera
    }

    public class RcHandler : Handler
    {
        public Ped Clone { get; private set; }
        public Ped OriginalPed;

        public RcModes CurrentMode { get; set; }

        private Camera _camera;
        private Blip _blip;

        private AudioPlayer rcOn;
        private AudioPlayer rcOff;
        private AudioPlayer rcBrake;
        private AudioPlayer rcAcceleration;
        private AudioPlayer rcSomeSerious;
        private List<AudioPlayer> _rcSounds;

        private NativeInput rcHandbrake;

        private bool _forcedHandbrake = false;

        public RcHandler(TimeCircuits circuits) : base(circuits)
        {
            PlayerSwitch.OnSwitchingComplete += OnSwitchingComplete;

            rcOn = circuits.AudioEngine.Create("general/rc/on.wav", Presets.Exterior);
            rcOff = circuits.AudioEngine.Create("general/rc/off.wav", Presets.Exterior);
            rcBrake = circuits.AudioEngine.Create("general/rc/brake.wav", Presets.Exterior);
            rcAcceleration = circuits.AudioEngine.Create("general/rc/acceleration.wav", Presets.Exterior);
            rcSomeSerious = circuits.AudioEngine.Create("general/rc/someSerious.wav", Presets.Exterior);            
            
            _rcSounds = new List<AudioPlayer>
            {
                rcOn, rcOff, rcBrake, rcAcceleration, rcSomeSerious
            };

            foreach (var sound in _rcSounds)
            {                
                sound.Volume = 0.4f;
                sound.MinimumDistance = 1f;
            }
            
            rcHandbrake = new NativeInput(GTA.Control.VehicleHandbrake);
            rcHandbrake.OnControlJustPressed += RcHandbrake_OnControlJustPressed;
        }

        private void RcHandbrake_OnControlJustPressed()
        {
            if (Mods.HoverUnderbody == ModState.On && TimeCircuits.GetHandler<FlyingHandler>().IsFlying)
                return;

            if (_forcedHandbrake || Game.IsControlPressed(GTA.Control.CharacterWheel))
            {
                rcBrake.Play();
                SetForcedHandbrake();

                if (_forcedHandbrake && Mods.Reactor == ReactorType.Nuclear && Mods.Plate == PlateType.Outatime && IsFueled)
                    rcSomeSerious.Play();
            }            
        }

        private void SetForcedHandbrake()
        {            
            _forcedHandbrake = !_forcedHandbrake;
            Vehicle.IsBurnoutForced = _forcedHandbrake;
            Vehicle.CanTiresBurst = !_forcedHandbrake;
        }

        private void OnSwitchingComplete()
        {
            if (!IsRemoteControlled)
            {
                Clone?.Delete();
                Clone = null;
            }
        }

        public void StartRC()
        {
            if (Vehicle == null) 
                return;
          
            IsRemoteControlled = true;

            Vehicle.LockStatus = VehicleLockStatus.StickPlayerInside;

            Clone = PlayerSwitch.CreatePedAndSwitch(out OriginalPed, Main.PlayerPed.Position, Main.PlayerPed.Heading, true);

            Clone.SetIntoVehicle(Vehicle, VehicleSeat.Driver);

            Clone.CanFlyThroughWindscreen = false;
            Clone.CanBeDraggedOutOfVehicle = false;
            Clone.BlockPermanentEvents = true;
            Clone.AlwaysKeepTask = true;
            Clone.IsVisible = false;
            
            _blip = OriginalPed.AddBlip();
            _blip.Sprite = (BlipSprite)480;
            _blip.Color = BlipColor.White;

            foreach (var sound in _rcSounds)
                sound.SourceEntity = OriginalPed;

            rcOn.Play();

            OriginalPed.Task.TurnTo(Vehicle);

            if (CurrentMode == RcModes.FromPlayerCamera)
            {
                _camera = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                _camera.PointAt(Vehicle);
                World.RenderingCamera = _camera;
            }
        }

        public void StopRC(bool instant = false)
        {
            if (IsRemoteControlled)
            {
                IsRemoteControlled = false;

                if (OriginalPed == null)
                    return;

                OriginalPed.Task.ClearAll();

                PlayerSwitch.Switch(OriginalPed, true, instant);

                if (!instant)
                    rcOff.Play();

                if (rcSomeSerious.IsAnyInstancePlaying)
                    rcSomeSerious?.Stop();

                if (_forcedHandbrake)
                    SetForcedHandbrake();

                Vehicle.LockStatus = VehicleLockStatus.None;

                Function.Call(Hash.CLEAR_FOCUS);

                _blip?.Delete();

                _camera?.Delete();
                _camera = null;
                World.RenderingCamera = null;                
            }
        }

        public override void Process()
        {
            if (!IsRemoteControlled)
                return;

            if (Main.IsManualPlayerSwitchInProgress)
                return;

            if (OriginalPed == null)
                return;

            if (OriginalPed.HasCollided)
            {
                RCManager.StopRemoteControl();
                return;
            }

            if (Game.IsControlJustPressed(GTA.Control.VehicleExit))
                RCManager.StopRemoteControl();

            rcHandbrake.Process();

            //// When u go too far from clone ped, game removes collision under him and 
            ////  he falls through the ground, so if player is 50 we freeze clone
            //var isCloneFreezed = Main.PlayerPed.Position.DistanceToSquared(OriginalPed.Position) >= 50*50;
            //Function.Call(Hash.FREEZE_ENTITY_POSITION, OriginalPed, isCloneFreezed);

            var origPos = OriginalPed.Position;
            var carPos = Vehicle.Position;
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, origPos.X, origPos.Y, origPos.Z);
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, carPos.X, carPos.Y, carPos.Z);

            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, Main.PlayerPed);
            Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, Main.PlayerPed);
            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, OriginalPed);
            Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, OriginalPed);

            if (Game.IsControlJustPressed(GTA.Control.VehicleAccelerate))
                rcAcceleration.Play();

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

                    Function.Call(Hash.SET_FOCUS_ENTITY, OriginalPed);

                    _camera = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                    _camera.PointAt(Vehicle);
                    World.RenderingCamera = _camera;                    
                }
            }

            if (CurrentMode == RcModes.FromPlayerCamera && _camera != null && _camera.Exists())
                _camera.Position = OriginalPed.Bones[Bone.SkelHead].GetOffsetPosition(new Vector3(0,0.1f,0));
        }

        public override void Stop() => StopRC();
        public void Stop(bool instant = false) => StopRC(instant);

        public override void Dispose()
        {
            rcOn?.Dispose();
            rcOff?.Dispose();
            rcBrake?.Dispose();
            rcAcceleration?.Dispose();
            Stop(true);
        }

        public override void KeyPress(Keys key) { }
    }
}