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

        private readonly NativeInput rcHandbrake;

        private bool _forcedHandbrake = false;
        private bool _boostStarted = false;
        private bool _handleBoost = false;

        private bool simulateSpeed;
        private float maxSpeed;
        private int maxSeconds;
        private float currentSimSpeed;

        //private static AnimateProp RCProp;

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
            {
                StartRC();
            }
            else
            {
                StopRC(instant);
            }
        }

        private void RcHandbrake_OnControlJustPressed()
        {
            if (!Properties.IsRemoteControlled || Properties.IsFlying || !Vehicle.IsAutomobile || Properties.IsWayback)
            {
                return;
            }

            if (!Game.IsControlPressed(GTA.Control.VehicleDuck) && !_forcedHandbrake)
            {
                TextHandler.Me.ShowHelp("RCBrakeTip", 3000, true, false, new ControlInfo(ModControls.HoverBoost).Button);
            }

            if (_forcedHandbrake || Game.IsControlPressed(GTA.Control.VehicleDuck))
            {
                Sounds.RCBrake?.Play();
                SetForcedHandbrake();

                if (_forcedHandbrake && Mods.IsDMC12 && Mods.Reactor == ReactorType.Nuclear && Mods.Plate == PlateType.Outatime && Mods.Hoodbox == ModState.Off && Mods.Hook == HookState.Off && Mods.HoverUnderbody == ModState.Off && Mods.WormholeType == WormholeType.BTTF1 && Properties.IsFueled && Properties.AreTimeCircuitsOn)
                {
                    Sounds.RCSomeSerious?.Stop();
                    Sounds.RCSomeSerious?.Play();
                }
            }
        }

        private void StopForcedHandbrake()
        {
            if (Properties.IsRemoteControlled && _forcedHandbrake)
            {
                SetForcedHandbrake();
            }
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

                if (ModSettings.WaybackSystem && TimeMachineHandler.CurrentTimeMachine == TimeMachine && WaybackSystem.CurrentPlayerRecording != default)
                {
                    WaybackSystem.CurrentPlayerRecording.LastRecord.Vehicle.Event = WaybackVehicleEvent.RcHandbrakeOn;
                }
            }
            else
            {
                SetSimulateSpeed(0, 0);

                if (ModSettings.WaybackSystem && TimeMachineHandler.CurrentTimeMachine == TimeMachine && WaybackSystem.CurrentPlayerRecording != default)
                {
                    WaybackSystem.CurrentPlayerRecording.LastRecord.Vehicle.Event = WaybackVehicleEvent.RcHandbrakeOff;
                }
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
            {
                return;
            }

            Properties.IsRemoteControlled = true;

            if (_handleBoost)
            {
                SetSimulateSpeed(0, 0);
                _boostStarted = false;
                _handleBoost = false;
            }

            int StartHealth = FusionUtils.PlayerPed.Health;

            Clone = PlayerSwitch.CreatePedAndSwitch(out TimeMachine.OriginalPed, FusionUtils.PlayerPed.Position, FusionUtils.PlayerPed.Heading, true);
            Clone.SetIntoVehicle(Vehicle, VehicleSeat.Driver);
            Clone.CanFlyThroughWindscreen = false;
            Clone.CanBeDraggedOutOfVehicle = false;
            Clone.BlockPermanentEvents = true;
            Clone.AlwaysKeepTask = true;
            Clone.IsVisible = false;
            Clone.Health = StartHealth;
            TimeMachine.OriginalPed.Health = StartHealth;
            Clone.IsInvincible = true;
            Clone.IsExplosionProof = true;
            Clone.CanBeKnockedOffBike = false;
            Clone.CanWearHelmet = false;

            _blip = TimeMachine.OriginalPed.AddBlip();
            _blip.Sprite = (BlipSprite)480;
            _blip.Color = BlipColor.White;

            foreach (KlangRageAudioLibrary.AudioPlayer sound in Sounds.RCSounds)
            {
                sound.SourceEntity = TimeMachine.OriginalPed;
            }

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
                if (TimeMachine.OriginalPed == null)
                {
                    return;
                }

                TimeMachine.OriginalPed.Task.ClearAll();

                PlayerSwitch.Switch(TimeMachine.OriginalPed, true, instant);

                if (!instant)
                {
                    Sounds.RCOff?.Play();
                }
                else
                {
                    Clone?.Delete();
                }

                if (Sounds.RCSomeSerious.IsAnyInstancePlaying)
                {
                    Sounds.RCSomeSerious?.Stop();
                }

                if (_forcedHandbrake)
                {
                    SetForcedHandbrake();
                }

                Function.Call(Hash.CLEAR_FOCUS);

                _blip?.Delete();

                _camera?.Delete();
                _camera = null;
                World.RenderingCamera = null;

                if (CurrentMode == RcModes.FromPlayerCamera)
                {
                    Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, 4);
                }

                Properties.IsRemoteControlled = false;
                //RCProp?.Dispose();
            }
        }

        public void DrawGUI()
        {
            if (FusionUtils.HideGUI || FusionUtils.PlayerVehicle != Vehicle || !Constants.HasScaleformPriority || FusionUtils.IsCameraInFirstPerson() || TcdEditer.IsEditing || RCGUIEditer.IsEditing)
            {
                return;
            }

            float mphSpeed = Vehicle.GetMPHSpeed();

            if (simulateSpeed)
            {
                if (Game.IsControlPressed(GTA.Control.VehicleAccelerate))
                {
                    currentSimSpeed += maxSpeed / maxSeconds * Game.LastFrameTime;
                }
                else
                {
                    currentSimSpeed -= maxSpeed / (maxSeconds / 2) * Game.LastFrameTime;

                    if (currentSimSpeed < 0)
                    {
                        currentSimSpeed = 0;
                    }
                }

                mphSpeed = currentSimSpeed;

                if (mphSpeed >= maxSpeed)
                {
                    simulateSpeed = false;
                    _boostStarted = true;
                    StopForcedHandbrake();
                }
            }

            ScaleformsHandler.RCGUI?.SetSpeed(mphSpeed);
            ScaleformsHandler.RCGUI?.SetStop(_handleBoost && _forcedHandbrake);
            ScaleformsHandler.RCGUI?.Draw2D();
        }

        public override void Tick()
        {
            if (Properties.IsWayback)
                return;

            if (!Properties.IsRemoteControlled)
            {
                if (_handleBoost)
                {
                    _boostStarted = false;
                    _handleBoost = false;
                }

                return;
            }

            Game.DisableControlThisFrame(GTA.Control.Phone);

            DrawGUI();

            if (_handleBoost && _boostStarted && !Game.IsControlPressed(GTA.Control.VehicleAccelerate))
            {
                _handleBoost = false;
                _boostStarted = false;
            }

            if (_handleBoost && _boostStarted && !_forcedHandbrake && Game.IsControlPressed(GTA.Control.VehicleAccelerate) && Vehicle.GetMPHSpeed() <= 90)
            {
                Properties.Boost = 0.3f;
            }
            else
            {
                Properties.Boost = 0.0f;
            }

            if (PlayerSwitch.IsManualInProgress)
            {
                return;
            }

            if (TimeMachine.OriginalPed == null)
            {
                return;
            }

            if (TimeMachine.OriginalPed.HasCollided || TimeMachine.Vehicle.Health <= 100 || TimeMachine.Vehicle.IsConsideredDestroyed || TimeMachine.Vehicle.IsDead)
            {
                RemoteTimeMachineHandler.StopRemoteControl();
                return;
            }

            if (Game.IsControlJustPressed(GTA.Control.VehicleExit))
            {
                RemoteTimeMachineHandler.StopRemoteControl();
            }

            Vector3 origPos = TimeMachine.OriginalPed.Position;
            Vector3 carPos = Vehicle.Position;
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, origPos.X, origPos.Y, origPos.Z);
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, carPos.X, carPos.Y, carPos.Z);
            if (!Driver.NotNullAndExists())
            {
                Clone.SetIntoVehicle(Vehicle, VehicleSeat.Driver);
            }
            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, Driver);
            Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, Driver);
            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, TimeMachine.OriginalPed);
            Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, TimeMachine.OriginalPed);

            if (Game.IsControlJustPressed(GTA.Control.VehicleAccelerate))
            {
                Sounds.RCAcceleration?.Play();
            }

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
            {
                RemoteTimeMachineHandler.StopRemoteControl();
            }
        }

        public override void Dispose()
        {
            if (Properties.IsRemoteControlled && !Properties.IsWayback)
            {
                RemoteTimeMachineHandler.StopRemoteControl(true);
            }
        }

        public override void KeyDown(KeyEventArgs e) { }
    }
}
