using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using KlangRageAudioLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RogersSierraRailway;
using static RogersSierraRailway.Commons;
using BackToTheFutureV.TimeMachineClasses;
using GTA.UI;

namespace BackToTheFutureV.Story
{
    public delegate void OnVehicleAttachedToRogersSierra(TimeMachine timeMachine);
    public delegate void OnVehicleDetachedFromRogersSierra(TimeMachine timeMachine);

    public class TrainMission : Mission
    {
        public OnVehicleAttachedToRogersSierra OnVehicleAttachedToRogersSierra;
        public OnVehicleDetachedFromRogersSierra OnVehicleDetachedFromRogersSierra;

        public bool OnlyMusic { get; set; } = true;

        public TimeMachine TimeMachine { get; private set; }
        public RogersSierra RogersSierra { get; private set; }

        public float TimeMultiplier = 1f;
        public bool PlayMusic = true;
        
        public AudioPlayer MissionMusic { get; set; }

        private AudioPlayer TrainApproachingSound { get; set; }

        private List<PtfxEntityBonePlayer> _wheelPtfxes = null;

        public TimedEventManager VehicleTimedEventManager = new TimedEventManager();

        public TrainMission()
        {
            OnVehicleAttachedToRogersSierra += OnVehicleAttached;
            OnVehicleDetachedFromRogersSierra += OnVehicleDetached;
            
            TrainApproachingSound = Main.CommonAudioEngine.Create($"story/trainMission/trainApproaching.wav", Presets.No3DLoop);

            VehicleTimedEventManager.ManageCamera = true;
        }

        private void Camera_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            if (TimeMachine == null && TimeMachineHandler.ClosestTimeMachine == null)
                return;

            TimeMachine timeMachine = TimeMachine;

            if (timeMachine == null)
                timeMachine = TimeMachineHandler.ClosestTimeMachine;

            switch (timedEvent.Step)
            {
                case 0:
                    timeMachine.CustomCamera = TimeMachineCamera.FrontPassengerWheelLookAtRear;
                    break;
                case 1:
                    timeMachine.CustomCamera = TimeMachineCamera.TrainApproaching;
                    break;
                case 3:
                case 5:
                    timeMachine.CustomCamera = TimeMachineCamera.DigitalSpeedo;
                    break;
                default:
                    if (timeMachine.CustomCamera != TimeMachineCamera.Default)
                        timeMachine.CustomCamera = TimeMachineCamera.Default;
                    break;
            }
        }

        private void OnVehicleDetached(TimeMachine timeMachine)
        {
            if (timeMachine != TimeMachine || !IsPlaying)
                return;

            if (VehicleTimedEventManager.ManageCamera)
                VehicleTimedEventManager.ResetCamera();

            VehicleTimedEventManager.ClearEvents();

            TimeMachine.Events.OnTimeTravelStarted -= StartExplodingScene;
            TimeMachine.Properties.BlockSparks = false;

            TimeMachine = null;
        }

        private void OnVehicleAttached(TimeMachine timeMachine)
        {
            if (timeMachine == null || TimeMachine != null || !IsPlaying)
                return;

            TimeMachine = timeMachine;

            TimeMachine.Properties.DestinationTime = new DateTime(1885, 9, 2, 08, 0, 0);
            TimeMachine.Properties.PreviousTime = new DateTime(1955, 11, 15, 09, 14, 0);

            TimeMachine.Events.OnTimeTravelStarted += StartExplodingScene;            
            TimeMachine.Properties.BlockSparks = true;

            VehicleTimedEventManager.Add(0, 31, 371, 0, 40, 0, TimeMultiplier); //turn on tc
            VehicleTimedEventManager.Last.OnExecute += TurnOnTC_OnExecute;

            VehicleTimedEventManager.Add(0, 38, 700, 0, 42, 0, TimeMultiplier); //insert date
            VehicleTimedEventManager.Last.OnExecute += InsertDate_OnExecute;

            VehicleTimedEventManager.Add(1, 38, 937, 1, 40, 137, TimeMultiplier); //reach 35mph
            VehicleTimedEventManager.Last.SetCamera(TimeMachine, new Vector3(-0.12f, 0.06f, 0.8f), CameraType.Entity, TimeMachine, new Vector3(-0.48f, 0.98f, 0.656f), CameraType.Entity, 50);

            VehicleTimedEventManager.Add(2, 24, 264, 2, 25, 464, TimeMultiplier); //reach 40mph
            VehicleTimedEventManager.Last.SetCamera(TimeMachine, new Vector3(-0.12f, 0.06f, 0.8f), CameraType.Entity, TimeMachine, new Vector3(-0.48f, 0.98f, 0.656f), CameraType.Entity, 50);

            VehicleTimedEventManager.Add(3, 15, 300, 3, 17, 500, TimeMultiplier); //reach 50mph
            VehicleTimedEventManager.Last.SetCamera(TimeMachine, new Vector3(-0.12f, 0.06f, 0.8f), CameraType.Entity, TimeMachine, new Vector3(-0.48f, 0.98f, 0.656f), CameraType.Entity, 50);

            VehicleTimedEventManager.Add(4, 25, 800, 4, 27, 0, TimeMultiplier); //reach 60mph
            VehicleTimedEventManager.Last.SetCamera(TimeMachine, new Vector3(-0.12f, 0.06f, 0.8f), CameraType.Entity, TimeMachine, new Vector3(-0.48f, 0.98f, 0.656f), CameraType.Entity, 50);

            VehicleTimedEventManager.Add(4, 44, 800, 4, 46, 0, TimeMultiplier); //reach 70mph
            VehicleTimedEventManager.Last.SetCamera(TimeMachine, new Vector3(-0.12f, 0.06f, 0.8f), CameraType.Entity, TimeMachine, new Vector3(-0.48f, 0.98f, 0.656f), CameraType.Entity, 50);

            VehicleTimedEventManager.Add(5, 4, 300, 5, 5, 500, TimeMultiplier); //reach 72mph
            VehicleTimedEventManager.Last.SetCamera(TimeMachine, new Vector3(-0.12f, 0.06f, 0.8f), CameraType.Entity, TimeMachine, new Vector3(-0.48f, 0.98f, 0.656f), CameraType.Entity, 50);

            VehicleTimedEventManager.Add(5, 54, 100, 5, 55, 100, TimeMultiplier); //reach 80mph
            VehicleTimedEventManager.Last.SetCamera(TimeMachine, new Vector3(-0.12f, 0.06f, 0.8f), CameraType.Entity, TimeMachine, new Vector3(-0.48f, 0.98f, 0.656f), CameraType.Entity, 50);

            VehicleTimedEventManager.Add(6, 8, 0, 6, 9, 0, TimeMultiplier); //reach 82mph
            VehicleTimedEventManager.Last.SetCamera(TimeMachine, new Vector3(-0.12f, 0.06f, 0.8f), CameraType.Entity, TimeMachine, new Vector3(-0.48f, 0.98f, 0.656f), CameraType.Entity, 50);

            VehicleTimedEventManager.Add(7, 3, 0, 7, 3, 500, TimeMultiplier); //reach 88mph
            VehicleTimedEventManager.Last.SetCamera(TimeMachine, new Vector3(-0.1f, 0.16f, 0.7f), CameraType.Entity, TimeMachine, new Vector3(0.12f, 1.1f, 0.45f), CameraType.Entity, 28);

            VehicleTimedEventManager.Add(5, 18, 250, 5, 19, 0, TimeMultiplier); //wheelie up
            VehicleTimedEventManager.Last.OnExecute += WheelieUp_OnExecute;

            VehicleTimedEventManager.Add(5, 26, 0, 5, 27, 0, TimeMultiplier); //wheelie down
            VehicleTimedEventManager.Last.OnExecute += WheelieDown_OnExecute;

            VehicleTimedEventManager.Add(5, 27, 0, 5, 28, 0, TimeMultiplier); //add ptfx wheels on front
            VehicleTimedEventManager.Last.OnExecute += GlowingWheelsFront_OnExecute;

            VehicleTimedEventManager.Add(5, 28, 500, 5, 30, 0, TimeMultiplier); //delete wheelie effects
            VehicleTimedEventManager.Last.OnExecute += DeleteEffects_OnExecute;

            VehicleTimedEventManager.Add(6, 55, 0, 6, 56, 0, TimeMultiplier); //start sparks
            VehicleTimedEventManager.Last.OnExecute += StartSparks_OnExecute;

            TrainApproachingSound.Stop();

            if (PlayMusic)
            {
                MissionMusic.SourceEntity = Main.PlayerPed;
                MissionMusic.Play();
            }
        }

        private void InsertDate_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            TimeMachine.Events.SimulateInputDate.Invoke(new DateTime(1985,10,27,11,0,0));
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

        public override void Process()
        {
            if (!IsPlaying)
                return;

            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, Main.PlayerPed);
            Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, Main.PlayerPed);           

            if (TimeMachine != null && (TimeMachine.Properties.IsAttachedToRogersSierra || TimeMachine.Properties.TimeTravelPhase == TimeTravelPhase.InTime))
            {
                TimedEventManager.RunEvents();
                
                VehicleTimedEventManager.RunEvents(TimedEventManager.CurrentTime);

                if (_wheelPtfxes != null)
                    foreach (var wheelPTFX in _wheelPtfxes)
                        wheelPTFX.Process();
            }

            if (TimedEventManager.AllExecuted())
                StartExplodingScene();
        }

        protected override void OnEnd()
        {
            if (!IsPlaying)
                return;

            if (TimedEventManager.IsCustomCameraActive)
                TimedEventManager.ResetCamera();

            if (PlayMusic && MissionMusic.IsAnyInstancePlaying)
                MissionMusic.Stop();

            _wheelPtfxes?.ForEach(x => x?.Stop());
            _wheelPtfxes.Clear();

            RogersSierra.IsOnTrainMission = false;

            if (RogersSierra.IsExploded == false)
                RogersSierra.FunnelSmoke = SmokeColor.Default;

            if (TimeMachine != null)
                OnVehicleDetached(TimeMachine);

            RogersSierra = null;            
        }

        public void StartExplodingScene()
        {
            TimedEventManager.ClearEvents();

            TimedEventManager.Add(TimedEventManager.CurrentTime.Add(TimeSpan.FromSeconds(2)), TimedEventManager.CurrentTime.Add(TimeSpan.FromSeconds(3)));
            TimedEventManager.Last.OnExecute += TrainExplosion_OnExecute;
        }

        private void TrainExplosion_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            RogersSierra.Explode();
            End();
        }

        protected override void OnStart()
        {
            if (!(RogersSierra is null))
                return;

            RogersSierra = TrainManager.ClosestRogersSierra;

            if (RogersSierra is null || RogersSierra.IsExploded)
            {
                RogersSierra = null;
                IsPlaying = false;
                return;
            }

            TimedEventManager.ResetExecution();
            TimedEventManager.ClearEvents();

            TimedEventManager.ManageCamera = true;

            TimedEventManager.Add(0, 0, 0, 0, 25, 0, TimeMultiplier); //reach 25mph
            TimedEventManager.Last.SetSpeed(2, 25);            
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(1, 29, 86, 1, 40, 137, TimeMultiplier); //green log explosion and reach 35mph
            TimedEventManager.Last.SetSpeed(25, 35);
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(2, 16, 66, 2, 25, 364, TimeMultiplier); //yellow log explosion and reach 40mph
            TimedEventManager.Last.SetSpeed(35, 41);
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(2, 25, 364, 3, 3, 406, TimeMultiplier); //reach 45mph
            TimedEventManager.Last.SetSpeed(41, 45);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 3, 406, 3, 17, 300, TimeMultiplier); //reach 50mph
            TimedEventManager.Last.SetSpeed(45, 50);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 20, 180, 3, 21, 240, TimeMultiplier); //first whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 21, 240, 3, 21, 740, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 21, 490, 3, 22, 900, TimeMultiplier); //second whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 22, 900, 3, 23, 400, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 23, 315, 3, 24, 511, TimeMultiplier); //third whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 24, 11, 3, 24, 511, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 24, 511, 3, 25, 529, TimeMultiplier); //fourth whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 25, 529, 4, 26, 800, TimeMultiplier); //reach 60mph
            TimedEventManager.Last.SetSpeed(50, 60);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(4, 26, 800, 4, 44, 0, TimeMultiplier); //reach 68mph
            TimedEventManager.Last.SetSpeed(60, 68);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(4, 44, 0, 4, 46, 0, TimeMultiplier); //reach 70mph
            TimedEventManager.Last.SetSpeed(68, 70);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(4, 46, 0, 5, 5, 500, TimeMultiplier); //reach 72mph
            TimedEventManager.Last.SetSpeed(70, 72);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 10, 200, 5, 13, 603, TimeMultiplier); //red log explosion            
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(5, 19, 0, 5, 21, 0, TimeMultiplier); //reach 75mph
            TimedEventManager.Last.SetSpeed(72, 75);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

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

            TimedEventManager.Add(6, 58, 0, 7, 0, 0, TimeMultiplier); //reach 86mph
            TimedEventManager.Last.SetSpeed(84, 86);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(7, 1, 0, 7, 3, 0, TimeMultiplier); //reach 87mph
            TimedEventManager.Last.SetSpeed(86, 87);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(7, 3, 0, 7, 3, 500, TimeMultiplier); //reach 88mph
            TimedEventManager.Last.SetSpeed(87, 88);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(7, 6, 0, 9, 12, 627, TimeMultiplier);

            RogersSierra.IsOnTrainMission = true;
            RogersSierra.RandomTrain = false;
            RogersSierra.UnlockSpeed = true;

            _wheelPtfxes = new List<PtfxEntityBonePlayer>();

            RogersSierra.LocomotiveSpeed = Utils.MphToMs(2);

            if (RogersSierra.AttachedVehicle != null)
                OnVehicleAttached(TimeMachineHandler.GetTimeMachineFromVehicle(RogersSierra.AttachedVehicle));
            else
            {
                TrainApproachingSound.SourceEntity = Main.PlayerPed;
                TrainApproachingSound.Play();
            }
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
            var ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_lr", wheelOffset, wheelRot, size, true, doLoopHandling);

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
            var ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_lf", wheelOffset, wheelRot, size, true, doLoopHandling);

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
