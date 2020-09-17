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

namespace BackToTheFutureV.Story
{
    public class TrainMission : Mission
    {
        public float TimeMultiplier = 0.1f;
        public bool PlayMusic = true;

        private AudioPlayer _funnelExpl;
        private AudioPlayer _missionMusic;
        //private AudioPlayer _missionMusic = new AudioPlayer($"TrainMissionWithVoices.wav", false, 0.8f);

        private PtfxEntityBonePlayer _funnelExplPtfx;
        private List<PtfxEntityBonePlayer> _wheelPtfxes = null;

        public TimedEventManager DeloreanTimedEventManager = new TimedEventManager();

        private bool _justAttached = false;

        public TrainMission()
        {
            //RogersSierraManager.OnAttachedDelorean += RogersSierraManager_OnAttachedDelorean;
            //RogersSierraManager.OnDetachedDelorean += RogersSierraManager_OnDetachedDelorean;
        }

        //private void RogersSierraManager_OnDetachedDelorean(DeloreanTimeMachine deloreanTimeMachine)
        //{
        //    if (IsPlaying && _justAttached)
        //    {
        //        if (DeloreanTimedEventManager.ManageCamera)
        //            DeloreanTimedEventManager.ResetCamera();

        //        DeloreanTimedEventManager.ClearEvents();

        //        _justAttached = false;
        //    }
        //}

        //private void RogersSierraManager_OnAttachedDelorean(DeloreanTimeMachine deloreanTimeMachine)
        //{
        //    if (IsPlaying && !_justAttached)
        //    {
        //        DeloreanTimedEventManager.ManageCamera = false;

        //        DeloreanTimedEventManager.Add(1, 31, 386, 1, 40, 137, TimeMultiplier);
        //        DeloreanTimedEventManager.Last.SetCamera(deloreanTimeMachine.Vehicle, new Vector3(1f, 3f, 0), deloreanTimeMachine.Vehicle, Vector3.Zero);

        //        DeloreanTimedEventManager.Add(5, 19, 0, 5, 21, 0, TimeMultiplier); //wheelie up
        //        DeloreanTimedEventManager.Last.OnExecute += WheelieUp_OnExecute;

        //        DeloreanTimedEventManager.Add(5, 26, 0, 5, 27, 0, TimeMultiplier); //wheelie down
        //        DeloreanTimedEventManager.Last.OnExecute += WheelieDown_OnExecute;

        //        DeloreanTimedEventManager.Add(5, 27, 500, 5, 28, 500, TimeMultiplier); //add ptfx wheels on front
        //        DeloreanTimedEventManager.Last.OnExecute += GlowingWheelsFront_OnExecute;

        //        DeloreanTimedEventManager.Add(5, 29, 500, 5, 30, 500, TimeMultiplier); //delete wheelie effects
        //        DeloreanTimedEventManager.Last.OnExecute += DeleteEffects_OnExecute;

        //        _justAttached = true;
        //    }
        //}

        private RogersSierra.cRogersSierra tRogersSierra => Main.CurrentRogersSierra;

        public override void Process()
        {
            if (IsPlaying)
            {
                CurrentTime = CurrentTime.Add(TimeSpan.FromSeconds(Game.LastFrameTime));

                TimedEventManager.RunEvents(CurrentTime);

                if (TimedEventManager.AllExecuted(CurrentTime))
                    StartExplodingScene();

                if (_justAttached)
                {
                    DeloreanTimedEventManager.RunEvents(CurrentTime);

                    if (_wheelPtfxes != null)
                        foreach (var wheelPTFX in _wheelPtfxes)
                            wheelPTFX.Process();
                }                                   
            }
        }

        public override void OnEnd()
        {
            if (IsPlaying)
            {
                if (TimedEventManager.IsCustomCameraActive)
                    TimedEventManager.ResetCamera();

                if (_missionMusic.IsAnyInstancePlaying)
                    _missionMusic.Stop();

                _wheelPtfxes?.ForEach(x => x?.Stop());
                _wheelPtfxes.Clear();

                tRogersSierra.isOnTrainMission = false;

                if (tRogersSierra.isExploded == false)
                    tRogersSierra.FunnelSmoke = RogersSierra.Commons.SmokeColor.Default;
            }
        }

        public void StartExplodingScene()
        {
            //if (tRogersSierra.isDeLoreanAttached)
            //    RogersSierraManager.DetachDeLorean();

            TimedEventManager.ClearEvents();

            TimedEventManager.Add(CurrentTime.Add(TimeSpan.FromSeconds(2)), CurrentTime.Add(TimeSpan.FromSeconds(3)));
            TimedEventManager.Last.OnExecute += TrainExplosion_OnExecute;
        }

        private void TrainExplosion_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {
                tRogersSierra.Explode();
                End();
            }                
        }

        public override void OnStart()
        {
            if ((tRogersSierra?.isExploded).HasValue ? tRogersSierra.isExploded : true)
            {
                IsPlaying = false;
                return;
            }

            tRogersSierra.AudioEngine.BaseSoundFolder = "BackToTheFutureV\\Sounds";

            _funnelExpl = tRogersSierra.AudioEngine.Create($"story/trainMission/funnelExplosion.wav", Presets.ExteriorLoud);
            _funnelExpl.SourceBone = "funnel_dummy";
            
            _missionMusic = tRogersSierra.AudioEngine.Create($"story/trainMission/music.wav", Presets.No3D);

            CurrentTime = TimeSpan.Zero;

            TimedEventManager.ClearEvents();

            TimedEventManager.ManageCamera = false;

            TimedEventManager.Add(0, 0, 0, 0, 10, 0, TimeMultiplier);
            TimedEventManager.Last.SetCamera(tRogersSierra.Locomotive, new Vector3(2.5f, 13f, -1f), tRogersSierra.Locomotive, Vector3.Zero);

            TimedEventManager.Add(0, 0, 0, 0, 25, 0, TimeMultiplier); //reach 25mph
            TimedEventManager.Last.SetSpeed(0, 25);            
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(1, 29, 386, 1, 31, 386, TimeMultiplier);
            TimedEventManager.Last.SetCamera(tRogersSierra.Locomotive, new Vector3(0, 15f, 2f), tRogersSierra.Locomotive, Vector3.Zero);

            TimedEventManager.Add(1, 29, 386, 1, 40, 137, TimeMultiplier); //green log explosion and reach 35mph
            TimedEventManager.Last.SetSpeed(25, 35);
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(2, 16, 266, 2, 25, 364, TimeMultiplier); //yellow log explosion and reach 40mph
            TimedEventManager.Last.SetSpeed(35, 40);
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(2, 25, 364, 3, 3, 406, TimeMultiplier); //reach 45mph
            TimedEventManager.Last.SetSpeed(40, 45);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 3, 406, 3, 16, 710, TimeMultiplier); //reach 50mph
            TimedEventManager.Last.SetSpeed(45, 50);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 20, 321, 3, 21, 587, TimeMultiplier); //first whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 21, 87, 3, 21, 587, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 21, 587, 3, 23, 315, TimeMultiplier); //second whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 22, 815, 3, 23, 315, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 23, 315, 3, 24, 511, TimeMultiplier); //third whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 24, 11, 3, 24, 511, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 24, 511, 3, 25, 529, TimeMultiplier); //fourth whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 25, 529, 3, 47, 111, TimeMultiplier); //reach 55mph
            TimedEventManager.Last.SetSpeed(50, 55);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 47, 111, 4, 26, 239, TimeMultiplier); //reach 60mph
            TimedEventManager.Last.SetSpeed(55, 60);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(4, 26, 239, 4, 46, 395, TimeMultiplier); //reach 70mph
            TimedEventManager.Last.SetSpeed(60, 70);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 10, 603, 5, 13, 603, TimeMultiplier); //red log explosion and reach 72mph
            TimedEventManager.Last.SetSpeed(70, 72);
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(5, 19, 0, 5, 21, 0, TimeMultiplier); //reach 75mph
            TimedEventManager.Last.SetSpeed(72, 75);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 25, 0, 5, 40, 137, TimeMultiplier); //reach 80mph
            TimedEventManager.Last.SetSpeed(75, 80);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            //TimedEventManager.Add(5, 56, 450, 7, 12, 627, TimeMultiplier); //reach 88mph
            TimedEventManager.Add(5, 56, 450, 7, 4, 627, TimeMultiplier); //reach 88mph
            TimedEventManager.Last.SetSpeed(80, 88);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 56, 450, 9, 12, 627, TimeMultiplier); //reach 88mph

            tRogersSierra.isOnTrainMission = true;

            _wheelPtfxes = new List<PtfxEntityBonePlayer>();
            _funnelExplPtfx = new PtfxEntityBonePlayer("scr_josh3", "scr_josh3_explosion", tRogersSierra.Locomotive, "funnel_dummy", Vector3.Zero, Vector3.Zero);

            _missionMusic.Play();

            if (!PlayMusic)
                _missionMusic.Volume = 0f;
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
            //if (timedEvent.FirstExecution)           
            //    tRogersSierra.DeLoreanWheelie = RogersSierra.Commons.AnimationStep.Second;                            
        }

        private void WheelieUp_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {
                //tRogersSierra.DeLoreanWheelie = RogersSierra.Commons.AnimationStep.First;
                SetupRearWheelsPTFXs("des_bigjobdrill", "ent_ray_big_drill_start_sparks", new Vector3(0, -0.3f, 0), new Vector3(0, 90f, 0), 1f, true);
                SetupRearWheelsPTFXs("veh_impexp_rocket", "veh_rocket_boost", new Vector3(0.2f, 0, 0), new Vector3(0, 0, 90f), 2.5f);
            }                
        }

        private void Whistle_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                tRogersSierra.Whistle = !tRogersSierra.Whistle;
        }

        private void SetSpeed_OnExecute(TimedEvent timedEvent)
        {
            if (tRogersSierra.Locomotive.GetMPHSpeed() <= timedEvent.EndSpeed)
                tRogersSierra.LocomotiveSpeed += Convert.ToSingle(timedEvent.CurrentSpeed);
        }

        private void Explosion_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {               
                _funnelExplPtfx.Play();
                _funnelExpl.Play();

                switch (tRogersSierra.FunnelSmoke)
                {
                    case RogersSierra.Commons.SmokeColor.Default:
                        tRogersSierra.FunnelSmoke = RogersSierra.Commons.SmokeColor.Green;
                        break;
                    case RogersSierra.Commons.SmokeColor.Green:
                        tRogersSierra.FunnelSmoke = RogersSierra.Commons.SmokeColor.Yellow;
                        break;
                    case RogersSierra.Commons.SmokeColor.Yellow:
                        tRogersSierra.FunnelSmoke = RogersSierra.Commons.SmokeColor.Red;
                        tRogersSierra.FunnelFire = true;
                        break;
                }
            }

            if (tRogersSierra.Locomotive.GetMPHSpeed() <= timedEvent.EndSpeed)
                tRogersSierra.LocomotiveSpeed += Convert.ToSingle(timedEvent.CurrentSpeed);
        }

        private void SetupRearWheelsPTFXs(string particleAssetName, string particleName, Vector3 wheelOffset, Vector3 wheelRot, float size = 3f, bool doLoopHandling = false)
        {
            var ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, tRogersSierra.AttachedDeLorean, "wheel_lr", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);

            ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, tRogersSierra.AttachedDeLorean, "wheel_rr", wheelOffset, wheelRot, size, true, doLoopHandling);

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
            var ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, tRogersSierra.AttachedDeLorean, "wheel_lf", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);

            ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, tRogersSierra.AttachedDeLorean, "wheel_rf", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);
        }
    }
}
