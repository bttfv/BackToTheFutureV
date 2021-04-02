using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using KlangRageAudioLibrary;
using RogersSierraRailway;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static RogersSierraRailway.TrainExtensions;

namespace BackToTheFutureV
{
    internal delegate void OnVehicleAttachedToRogersSierra(TimeMachine timeMachine);
    internal delegate void OnVehicleDetachedFromRogersSierra(TimeMachine timeMachine);

    internal class TrainMission : Mission
    {
        public OnVehicleAttachedToRogersSierra OnVehicleAttachedToRogersSierra;
        public OnVehicleDetachedFromRogersSierra OnVehicleDetachedFromRogersSierra;

        public TimeMachine TimeMachine { get; private set; }
        public RogersSierra RogersSierra { get; private set; }

        public float TimeMultiplier { get; set; } = 1f;

        public AudioPlayer MissionMusic { get; set; }
        public bool Mute { get; set; } = false;
        public float OriginalVolume { get; set; }

        private AudioPlayer TrainApproachingSound { get; set; }
        private bool _useInternalTime = false;
        private List<PtfxEntityBonePlayer> _wheelPtfxes = new List<PtfxEntityBonePlayer>();

        public TrainMission()
        {
            OnVehicleAttachedToRogersSierra += OnVehicleAttached;
            OnVehicleDetachedFromRogersSierra += OnVehicleDetached;

            TrainApproachingSound = Main.CommonAudioEngine.Create($"story/trainMission/trainApproaching.wav", Presets.No3DLoop);
        }

        private void OnVehicleDetached(TimeMachine timeMachine)
        {
            if (!IsPlaying)
                return;

            TimeMachine.Events.OnTimeTravelStarted -= StartExplodingScene;
            TimeMachine.Properties.BlockSparks = false;

            if (TimeMachine.Properties.TimeTravelPhase != TimeTravelPhase.InTime)
                Utils.HideGUI = false;
        }

        private void OnVehicleAttached(TimeMachine timeMachine)
        {
            if (!IsPlaying)
                return;

            TimeMachine.CustomCamera = TimeMachineCamera.Default;
            TimeMachine.Events.OnTimeTravelStarted += StartExplodingScene;

            TrainApproachingSound.Stop(true);

            TimedEventManager.Pause = false;

            MissionMusic.SourceEntity = Utils.PlayerPed;
            MissionMusic.Play();

            if (Mute)
            {
                OriginalVolume = MissionMusic.Volume;
                MissionMusic.Volume = 0;
            }

            Utils.HideGUI = true;
        }

        private void InsertDate_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            TimeMachine.Events.SimulateInputDate.Invoke(new DateTime(1985, 10, 27, 11, 0, 0));
        }

        private void TurnOnTC_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            TimeMachine.Events.SetTimeCircuits.Invoke(true);
        }

        private void StartSparks_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            TimeMachine.Properties.BlockSparks = false;
        }

        public override void Tick()
        {
            if (!IsPlaying)
                return;

            if (!RogersSierra.Locomotive.NotNullAndExists())
            {
                End();
                return;
            }

            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, Utils.PlayerPed);
            Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, Utils.PlayerPed);

            if (MissionMusic.IsAnyInstancePlaying && !_useInternalTime)
                TimedEventManager.RunEvents(MissionMusic.Last.PlayPosition);
            else
                TimedEventManager.RunEvents();

            if (TimedEventManager.Pause && TrainApproachingSound.IsAnyInstancePlaying && RogersSierra.Locomotive.DistanceToSquared2D(TimeMachine, 8) && TimeMachine.CustomCamera != TimeMachineCamera.TrainApproaching)
                TimeMachine.CustomCamera = TimeMachineCamera.TrainApproaching;

            if (_wheelPtfxes != null)
                foreach (PtfxEntityBonePlayer wheelPTFX in _wheelPtfxes)
                    wheelPTFX.Tick();
        }

        private void StartExplodingScene()
        {
            TimedEventManager.ClearEvents();
            TimedEventManager.ResetExecution();

            TimedEventManager.Add(TimedEventManager.CurrentTime.Add(TimeSpan.FromSeconds(2)), TimedEventManager.CurrentTime.Add(TimeSpan.FromSeconds(3)));
            TimedEventManager.Last.OnExecute += TrainExplosion_OnExecute;

            _useInternalTime = true;
        }

        private void TrainExplosion_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            RogersSierra.Explode();
            End();
        }

        protected override void OnEnd()
        {
            if (!IsPlaying)
                return;

            if (TimedEventManager.IsCustomCameraActive)
                TimedEventManager.ResetCamera();

            if (TimeMachine.NotNullAndExists())
                TimeMachine.CustomCamera = TimeMachineCamera.Default;

            if (MissionMusic.IsAnyInstancePlaying)
                MissionMusic.Stop();

            if (TrainApproachingSound.IsAnyInstancePlaying)
                TrainApproachingSound.Stop();

            _wheelPtfxes?.ForEach(x => x?.Stop());
            _wheelPtfxes.Clear();

            if (RogersSierra.Locomotive.NotNullAndExists())
            {
                RogersSierra.IsOnTrainMission = false;

                if (RogersSierra.IsExploded == false)
                    RogersSierra.FunnelSmoke = SmokeColor.Default;

                if (TimeMachine != null)
                    OnVehicleDetached(TimeMachine);
            }

            _useInternalTime = false;
            TimeMachine = null;
            RogersSierra = null;
        }

        protected override void OnStart()
        {
            RogersSierra = TrainManager.ClosestRogersSierra;
            TimeMachine = TimeMachineHandler.ClosestTimeMachine;

            if (RogersSierra is null || RogersSierra.IsExploded || !RogersSierra.WheelsOnPilot || TimeMachine == null || !TimeMachine.Properties.IsOnTracks || !RogersSierra.Locomotive.DistanceToSquared2D(TimeMachine, 15))
            {
                RogersSierra = null;
                TimeMachine = null;
                IsPlaying = false;
                return;
            }

            TimeMachine.Properties.DestinationTime = new DateTime(1885, 9, 2, 08, 0, 0);
            TimeMachine.Properties.PreviousTime = new DateTime(1955, 11, 15, 09, 14, 0);

            TimeMachine.Properties.BlockSparks = true;

            TimedEventManager.ResetExecution();
            TimedEventManager.ClearEvents();

            TimedEventManager.ManageCamera = true;


            TimedEventManager.Add(5, 54, 100, 5, 55, 100, TimeMultiplier); //reach 80mph
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DigitalSpeedo);

            TimedEventManager.Add(6, 8, 0, 6, 9, 0, TimeMultiplier); //reach 82mph
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DigitalSpeedo);


            TimedEventManager.Add(0, 0, 0, 0, 25, 0, TimeMultiplier); //reach 25mph
            TimedEventManager.Last.SetSpeed(2, 25);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(0, 25, 257, 0, 28, 381, TimeMultiplier); //reach 25mph
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.FrontToBack);

            TimedEventManager.Add(0, 31, 371, 0, 34, 137, TimeMultiplier); //turn on tc
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DriverSeat);
            TimedEventManager.Last.OnExecute += TurnOnTC_OnExecute;

            TimedEventManager.Add(0, 38, 700, 0, 42, 0, TimeMultiplier); //insert date
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DriverSeat);
            TimedEventManager.Last.OnExecute += InsertDate_OnExecute;

            TimedEventManager.Add(1, 29, 86, 1, 38, 937, TimeMultiplier); //green log explosion and reach 34mph
            TimedEventManager.Last.SetSpeed(25, 34);
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(1, 36, 800, 1, 38, 937, TimeMultiplier); //reach 35mph
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.FrontToBackRightSide);

            TimedEventManager.Add(1, 38, 937, 1, 40, 137, TimeMultiplier); //reach 35mph
            TimedEventManager.Last.SetSpeed(34, 35);
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.AnalogSpeedo);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(2, 16, 66, 2, 24, 264, TimeMultiplier); //yellow log explosion and reach 40mph
            TimedEventManager.Last.SetSpeed(35, 40);
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(2, 24, 264, 2, 25, 464, TimeMultiplier); //reach 41mph
            TimedEventManager.Last.SetSpeed(40, 41);
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DigitalSpeedo);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(2, 59, 700, TimeMultiplier); //move player to passenger seat            
            TimedEventManager.Last.OnExecute += MovePlayer_OnExecute;

            TimedEventManager.Add(3, 1, 700, 3, 3, 406, TimeMultiplier); //open door
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.RightSide);
            TimedEventManager.Last.OnExecute += OpenDoor_OnExecute;

            TimedEventManager.Add(2, 25, 464, 3, 3, 406, TimeMultiplier); //reach 46mph
            TimedEventManager.Last.SetSpeed(41, 46);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 3, 406, 3, 15, 300, TimeMultiplier); //reach 49mph
            TimedEventManager.Last.SetSpeed(46, 49);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 15, 300, 3, 17, 500, TimeMultiplier); //reach 50mph
            TimedEventManager.Last.SetSpeed(49, 50);
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DigitalSpeedo);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 20, 180, TimeMultiplier); //first whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 21, 240, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 21, 490, TimeMultiplier); //second whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 22, 900, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 23, 315, TimeMultiplier); //third whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 24, 11, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 24, 511, TimeMultiplier); //fourth whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 25, 529, 4, 25, 800, TimeMultiplier); //reach 59mph
            TimedEventManager.Last.SetSpeed(50, 59);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(4, 25, 800, 4, 27, 0, TimeMultiplier); //reach 60mph
            TimedEventManager.Last.SetSpeed(59, 60);
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DigitalSpeedo);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(4, 26, 800, 4, 44, 800, TimeMultiplier); //reach 68mph
            TimedEventManager.Last.SetSpeed(60, 69);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(4, 44, 800, 4, 46, 0, TimeMultiplier); //reach 71mph
            TimedEventManager.Last.SetSpeed(69, 71);
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DigitalSpeedo);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 4, 300, 5, 5, 500, TimeMultiplier); //reach 72mph
            TimedEventManager.Last.SetSpeed(71, 72);
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DigitalSpeedo);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 10, 200, TimeMultiplier); //red log explosion            
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(5, 19, 0, 5, 20, 0, TimeMultiplier); //wheelie up
            TimedEventManager.Last.SetCamera(RogersSierra.CustomCamera, (int)TrainCamera.WheelieUp);
            TimedEventManager.Last.OnExecute += WheelieUp_OnExecute;

            TimedEventManager.Add(5, 20, 0, 5, 21, 0, TimeMultiplier); //reach 75mph
            TimedEventManager.Last.SetSpeed(72, 75);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 26, 500, 5, 28, 500, TimeMultiplier); //wheelie down
            TimedEventManager.Last.SetCamera(RogersSierra.CustomCamera, (int)TrainCamera.WheelieDown);
            TimedEventManager.Last.OnExecute += WheelieDown_OnExecute;

            TimedEventManager.Add(5, 27, 0, TimeMultiplier); //add ptfx wheels on front
            TimedEventManager.Last.OnExecute += GlowingWheelsFront_OnExecute;

            TimedEventManager.Add(5, 28, 500, TimeMultiplier); //delete wheelie effects
            TimedEventManager.Last.OnExecute += DeleteEffects_OnExecute;

            TimedEventManager.Add(5, 21, 100, 5, 54, 500, TimeMultiplier); //reach 79mph
            TimedEventManager.Last.SetSpeed(75, 79);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 39, 0, 5, 40, 0, TimeMultiplier); //firebox door opens            
            TimedEventManager.Last.OnExecute += Firebox_OnExecute;

            TimedEventManager.Add(5, 54, 100, 5, 55, 100, TimeMultiplier); //reach 80mph
            TimedEventManager.Last.SetSpeed(79, 80);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 56, 100, 5, 57, 100, TimeMultiplier); //reach 81mph
            TimedEventManager.Last.SetSpeed(80, 81);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(6, 8, 0, 6, 9, 0, TimeMultiplier); //reach 82mph
            TimedEventManager.Last.SetSpeed(81, 82);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(6, 10, 0, 6, 57, 0, TimeMultiplier); //reach 84mph
            TimedEventManager.Last.SetSpeed(82, 84);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(6, 54, 0, 6, 55, 0, TimeMultiplier); //close door
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.RightSide);
            TimedEventManager.Last.OnExecute += CloseDoor_OnExecute;

            TimedEventManager.Add(6, 55, 0, TimeMultiplier); //move player to driver seat
            TimedEventManager.Last.OnExecute += MovePlayer_OnExecute;

            TimedEventManager.Add(6, 55, 0, TimeMultiplier); //start sparks
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.RightSide);
            TimedEventManager.Last.OnExecute += StartSparks_OnExecute;

            TimedEventManager.Add(6, 58, 0, 7, 0, 0, TimeMultiplier); //reach 86mph
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.TimeTravelOnTracks);
            TimedEventManager.Last.SetSpeed(84, 86);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(7, 1, 0, 7, 3, 0, TimeMultiplier); //reach 87mph
            TimedEventManager.Last.SetSpeed(86, 87);
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DigitalSpeedoTowardsFront);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(7, 3, 0, 7, 3, 500, TimeMultiplier); //reach 88mph
            TimedEventManager.Last.SetSpeed(87, 88);
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DigitalSpeedoTowardsFront);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(7, 6, 0, 7, 7, 200, TimeMultiplier); //show destination date            
            TimedEventManager.Last.SetCamera(TimeMachine.CustomCameraManager, (int)TimeMachineCamera.DestinationDate);

            TimedEventManager.Add(7, 8, 833, TimeMultiplier); //time travel
            TimedEventManager.Last.OnExecute += TimeTravel_OnExecute;

            RogersSierra.IsOnTrainMission = true;
            RogersSierra.RandomTrain = false;
            RogersSierra.UnlockSpeed = true;

            RogersSierra.LocomotiveSpeed = MathExtensions.ToMS(2);

            if (RogersSierra.AttachedVehicle == TimeMachine)
                OnVehicleAttached(TimeMachine);
            else
            {
                TimedEventManager.Pause = true;

                TrainApproachingSound.SourceEntity = Utils.PlayerPed;
                TrainApproachingSound.Play();

                TimeMachine.CustomCamera = TimeMachineCamera.FrontPassengerWheelLookAtRear;
            }
        }

        private void TimeTravel_OnExecute(TimedEvent timedEvent)
        {
            if (TimeMachine.Properties.IsFueled && TimeMachine.Properties.AreTimeCircuitsOn)
                TimeMachine.Events.OnSparksEnded?.Invoke();
        }

        private void MovePlayer_OnExecute(TimedEvent timedEvent)
        {
            if (Utils.PlayerVehicle != TimeMachine)
                return;

            Utils.PlayerPed.Task.ShuffleToNextVehicleSeat();
        }

        private void CloseDoor_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            TimeMachine.Vehicle.Doors[VehicleDoorIndex.FrontRightDoor].Close();
        }

        private void OpenDoor_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            TimeMachine.Vehicle.Doors[VehicleDoorIndex.FrontRightDoor].Open();
        }

        private void Firebox_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            RogersSierra.ExplodeFirebox();
        }

        private void DeleteEffects_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                _wheelPtfxes?.ForEach(x => x?.Stop());
        }

        private void GlowingWheelsFront_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {
                SetupFrontWheelsPTFXs("des_bigjobdrill", "ent_ray_big_drill_start_sparks", new Vector3(0, -0.3f, 0), new Vector3(0, 90f, 0), 1f, true);
                SetupFrontWheelsPTFXs("veh_impexp_rocket", "veh_rocket_boost", new Vector3(0.2f, 0, 0), new Vector3(0, 0, 90f), 2.5f);
            }
        }

        private void WheelieDown_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                TimeMachine.Events.SetWheelie?.Invoke(false);
        }

        private void WheelieUp_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {
                TimeMachine.Events.SetWheelie?.Invoke(true);
                SetupRearWheelsPTFXs("des_bigjobdrill", "ent_ray_big_drill_start_sparks", new Vector3(0, -0.3f, 0), new Vector3(0, 90f, 0), 1f, true);
                SetupRearWheelsPTFXs("veh_impexp_rocket", "veh_rocket_boost", new Vector3(0.2f, 0, 0), new Vector3(0, 0, 90f), 2.5f);
            }
        }

        private void Whistle_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                RogersSierra.Whistle = !RogersSierra.Whistle;
        }

        private void SetSpeed_OnExecute(TimedEvent timedEvent)
        {
            if (RogersSierra.Locomotive.GetMPHSpeed() < timedEvent.EndSpeed)
                RogersSierra.LocomotiveSpeed += Convert.ToSingle(timedEvent.CurrentSpeed);
        }

        private void Explosion_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {
                switch (RogersSierra.FunnelSmoke)
                {
                    case SmokeColor.Default:
                        RogersSierra.PrestoLogExplosion(SmokeColor.Green);
                        break;
                    case SmokeColor.Green:
                        RogersSierra.PrestoLogExplosion(SmokeColor.Yellow);
                        break;
                    case SmokeColor.Yellow:
                        RogersSierra.PrestoLogExplosion(SmokeColor.Red);
                        break;
                }
            }

            if (RogersSierra.Locomotive.GetMPHSpeed() < timedEvent.EndSpeed)
                RogersSierra.LocomotiveSpeed += Convert.ToSingle(timedEvent.CurrentSpeed);
        }

        private void SetupRearWheelsPTFXs(string particleAssetName, string particleName, Vector3 wheelOffset, Vector3 wheelRot, float size = 3f, bool doLoopHandling = false)
        {
            PtfxEntityBonePlayer ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_lr", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);

            ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_rr", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);
        }

        private void SetupFrontWheelsPTFXs(string particleAssetName, string particleName, Vector3 wheelOffset, Vector3 wheelRot, float size = 3f, bool doLoopHandling = false)
        {
            PtfxEntityBonePlayer ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_lf", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);

            ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_rf", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);
        }

        public override void KeyDown(KeyEventArgs key)
        {

        }
    }
}
