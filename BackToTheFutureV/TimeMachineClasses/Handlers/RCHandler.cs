using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class RcHandler : HandlerPrimitive
    {
        public Ped Clone { get; private set; }

        public RcModes CurrentMode { get; set; }

        private Camera _camera;
        private Blip _blip;

        private NativeInput rcHandbrake;

        private bool _forcedHandbrake = false;
        private bool _boostStarted = false;
        private bool _handleBoost = false;

        private bool simulateSpeed;
        private float maxSpeed;
        private int maxSeconds;
        private float currentSimSpeed;

        //private static AnimateProp RCProp;

        private bool switchCall = false;

        public RcHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            PlayerSwitch.OnSwitchingComplete += OnSwitchingComplete;

            rcHandbrake = new NativeInput(GTA.Control.VehicleHandbrake);
            rcHandbrake.OnControlJustPressed += RcHandbrake_OnControlJustPressed;

            Events.SetRCMode += SetRCMode;
        }

        public void SetSimulateSpeed(float maxSpeed, int seconds)
        {
            if (maxSpeed == 0)
            {
                simulateSpeed = false;
                return;
            }

            this.maxSpeed = maxSpeed;
            maxSeconds = seconds;
            currentSimSpeed = 0;
            simulateSpeed = true;
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
            if (!Properties.IsRemoteControlled || Properties.IsFlying)
                return;

            if (_forcedHandbrake || Game.IsControlPressed(GTA.Control.VehicleDuck))
            {
                Sounds.RCBrake?.Play();
                SetForcedHandbrake();

                if (_forcedHandbrake && Mods.IsDMC12 && Mods.Reactor == ReactorType.Nuclear && Mods.Plate == PlateType.Outatime && Properties.IsFueled)
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
            _forcedHandbrake = !_forcedHandbrake;

            Vehicle.IsBurnoutForced = _forcedHandbrake;
            Vehicle.CanTiresBurst = !_forcedHandbrake;

            if (_forcedHandbrake)
            {
                _boostStarted = false;
                _handleBoost = true;

                SetSimulateSpeed(64.5f, 8);
            }
        }

        private void OnSwitchingComplete()
        {
            if (ModSettings.WaybackSystem && switchCall)
                WaybackSystem.CurrentPlayerRecording.Clone(FusionUtils.PlayerPed);

            if (switchCall)
                switchCall = false;

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
                SetSimulateSpeed(0, 0);
                _boostStarted = false;
                _handleBoost = false;
            }

            switchCall = true;
            Clone = PlayerSwitch.CreatePedAndSwitch(out TimeMachine.OriginalPed, FusionUtils.PlayerPed.Position, FusionUtils.PlayerPed.Heading, true);

            Clone.SetIntoVehicle(Vehicle, VehicleSeat.Driver);

            Clone.CanFlyThroughWindscreen = false;
            Clone.CanBeDraggedOutOfVehicle = false;
            Clone.BlockPermanentEvents = true;
            Clone.AlwaysKeepTask = true;
            Clone.IsVisible = false;

            _blip = TimeMachine.OriginalPed.AddBlip();
            _blip.Sprite = (BlipSprite)480;
            _blip.Color = BlipColor.White;

            foreach (KlangRageAudioLibrary.AudioPlayer sound in Sounds.RCSounds)
                sound.SourceEntity = TimeMachine.OriginalPed;

            Sounds.RCOn?.Play();

            //RCProp = new AnimateProp(ModelHandler.BTTFMrFusion, TimeMachine.OriginalPed, TimeMachine.OriginalPed.Bones[Bone.MHLeftHandSide]);
            //RCProp.SpawnProp();

            if (!TimeMachine.OriginalPed.IsInVehicle())
            {
                TimeMachine.OriginalPed.AlwaysKeepTask = true;
                //TimeMachine.OriginalPed.Task.PlayAnimation("amb@world_human_seat_wall_eating@male@both_hands@idle_a", "idle_a", 8f, -1, AnimationFlags.UpperBodyOnly | AnimationFlags.Loop);
                TimeMachine.OriginalPed.Task.TurnTo(Vehicle);
            }

            if (CurrentMode == RcModes.FromPlayerCamera || FusionUtils.IsCameraInFirstPerson())
            {
                CurrentMode = RcModes.FromPlayerCamera;

                _camera = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                Function.Call(Hash.ATTACH_CAM_TO_PED_BONE, _camera, TimeMachine.OriginalPed, Bone.SkelHead, 0, 0, 0, true);
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

                switchCall = true;
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

                if (CurrentMode == RcModes.FromPlayerCamera)
                    Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, 4);

                //RCProp?.Dispose();
            }
        }

        public void DrawGUI()
        {
            if (FusionUtils.HideGUI || FusionUtils.PlayerVehicle != Vehicle || !Properties.IsGivenScaleformPriority || FusionUtils.IsCameraInFirstPerson() || TcdEditer.IsEditing || RCGUIEditer.IsEditing)
                return;

            float mphSpeed = Vehicle.GetMPHSpeed();

            if (simulateSpeed)
            {
                if (Game.IsControlPressed(GTA.Control.VehicleAccelerate))
                    currentSimSpeed += (maxSpeed / maxSeconds) * Game.LastFrameTime;
                else
                {
                    currentSimSpeed -= (maxSpeed / (maxSeconds / 2)) * Game.LastFrameTime;

                    if (currentSimSpeed < 0)
                        currentSimSpeed = 0;
                }

                mphSpeed = currentSimSpeed;

                if (mphSpeed >= maxSpeed)
                {
                    simulateSpeed = false;
                    StopForcedHandbrake();
                }
            }

            ScaleformsHandler.RCGUI?.SetSpeed(mphSpeed);
            ScaleformsHandler.RCGUI?.SetStop(_handleBoost && _forcedHandbrake);
            ScaleformsHandler.RCGUI?.Draw2D();
        }

        public override void Tick()
        {
            if (!Properties.IsRemoteControlled)
            {
                if (_handleBoost)
                {
                    _boostStarted = false;
                    _handleBoost = false;
                }

                return;
            }

            DrawGUI();

            if (_handleBoost && !_boostStarted && Game.IsControlPressed(GTA.Control.VehicleAccelerate))
                _boostStarted = true;

            if (_handleBoost && _boostStarted && !Game.IsControlPressed(GTA.Control.VehicleAccelerate))
            {
                StopForcedHandbrake();
                _handleBoost = false;
            }

            if (_handleBoost && _boostStarted && !_forcedHandbrake && Game.IsControlPressed(GTA.Control.VehicleAccelerate) && Vehicle.GetMPHSpeed() <= 90)
                Properties.Boost = 0.3f;

            if (PlayerSwitch.IsManualInProgress)
                return;

            if (TimeMachine.OriginalPed == null)
                return;

            if (TimeMachine.OriginalPed.HasCollided)
            {
                RemoteTimeMachineHandler.StopRemoteControl();
                return;
            }

            if (Game.IsControlJustPressed(GTA.Control.VehicleExit))
                RemoteTimeMachineHandler.StopRemoteControl();

            //// When u go too far from clone ped, game removes collision under him and 
            ////  he falls through the ground, so if player is 50 we freeze clone
            //var isCloneFreezed = CommonSettings.PlayerPed.Position.DistanceToSquared(OriginalPed.Position) >= 50*50;
            //Function.Call(Hash.FREEZE_ENTITY_POSITION, OriginalPed, isCloneFreezed);

            Vector3 origPos = TimeMachine.OriginalPed.Position;
            Vector3 carPos = Vehicle.Position;
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, origPos.X, origPos.Y, origPos.Z);
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, carPos.X, carPos.Y, carPos.Z);

            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, Driver);
            Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, Driver);
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
                    Function.Call(Hash.ATTACH_CAM_TO_PED_BONE, _camera, TimeMachine.OriginalPed, Bone.SkelHead, 0, 0, 0, true);
                    _camera.PointAt(Vehicle);
                    World.RenderingCamera = _camera;
                }
            }

            //if (CurrentMode == RcModes.FromPlayerCamera && _camera != null && _camera.Exists())
            //    TimeMachine.OriginalPed.Rotation = _camera.Rotation;
        }

        public override void Stop()
        {
            if (Properties.IsRemoteControlled)
                RemoteTimeMachineHandler.StopRemoteControl();
        }

        public override void Dispose()
        {
            if (Properties.IsRemoteControlled)
                RemoteTimeMachineHandler.StopRemoteControl(true);
        }

        public override void KeyDown(KeyEventArgs e) { }
    }
}