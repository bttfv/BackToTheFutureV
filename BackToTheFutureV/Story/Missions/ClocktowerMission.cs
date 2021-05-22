using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using KlangRageAudioLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class ClocktowerMission : Mission
    {
        private static Vector3 lightningOffset = new Vector3(0, -1.548199f, 0.994037f);

        private static Vector3 polePosition = new Vector3(63.4558f, 6581.7065f, 46.8226f);

        private static Model streetPoleModel = new Model("prop_streetlight_09");
        private static Model mastModel = new Model("prop_air_mast_01");
        private static Model poleModel = new Model("prop_flagpole_1a");

        private Prop LeftStreetPole;
        private Prop RightStreetPole;
        private Prop Mast;
        private Prop Pole;

        private Rope StreetRope;
        private Rope MastRope;

        private AnimatePropsHandler Lightnings;
        private PtfxEntityPlayer Spark;
        private List<PtfxPlayer> sparkRope = new List<PtfxPlayer>();
        private List<PtfxPlayer> fireRope = new List<PtfxPlayer>();

        private int step = 0;
        private int currentIndex = 0;
        private int gameTime;

        private TimeMachine CurrentTimeMachine => TimeMachineHandler.CurrentTimeMachine;

        private CustomCamera CustomCamera;

        private AudioPlayer Thunder;
        
        public const Hash LightningRunStreet = unchecked((Hash)(-119993883));

        static ClocktowerMission()
        {
            lightningOffset = (lightningOffset * -1).GetSingleOffset(Coordinate.Z, poleModel.Dimensions.frontTopRight.Z);
        }

        public ClocktowerMission()
        {
            TimeHandler.OnTimeChanged += (DateTime time) =>
            {
                OnEnd();
            };
        }

        public override void Abort()
        {
            OnEnd();
        }

        public override void KeyDown(KeyEventArgs key)
        {
            //if (key.KeyCode == Keys.L)
            //    IsPlaying = true;

            //if (key.KeyCode == Keys.O)
            //    FusionUtils.CurrentTime = new DateTime(1955, 11, 12, 22, 3, 0);
        }

        public override void Tick()
        {
            if (!TimeHandler.RealTime || FusionUtils.PlayerPed.Position.DistanceToSquared2D(new Vector3(41.5676f, 6585.7378f, 30.3686f)) > 197480)
                return;

            if (!LeftStreetPole.NotNullAndExists() | !RightStreetPole.NotNullAndExists() | !Mast.NotNullAndExists() | !Pole.NotNullAndExists())
            {
                OnEnd();
                Setup();
            }

            if (FusionUtils.CurrentTime == new DateTime(1955, 11, 12, 22, 4, 0) && !IsPlaying)
                IsPlaying = true;

            if (IsPlaying && TimeMachineHandler.CurrentTimeMachine.NotNullAndExists() && LeftStreetPole.NotNullAndExists() && RightStreetPole.NotNullAndExists())
            {
                RaycastResult raycastResult = World.Raycast(LeftStreetPole.Position.GetSingleOffset(Coordinate.Z, 0.75f), RightStreetPole.Position.GetSingleOffset(Coordinate.Z, 0.75f), IntersectFlags.MissionEntities);

                if (raycastResult.DidHit && raycastResult.HitEntity == CurrentTimeMachine)
                {
                    if (CurrentTimeMachine.Mods.Hook == HookState.On && CurrentTimeMachine.Properties.AreTimeCircuitsOn && CurrentTimeMachine.Constants.Over88MphSpeed && !CurrentTimeMachine.Properties.HasBeenStruckByLightning && sparkRope.Count(x => x.IsPlaying) >= 100)
                        CurrentTimeMachine.Events.StartLightningStrike?.Invoke(-1);
                }
            }

            if (!IsPlaying || Game.GameTime < gameTime)
                return;

            switch (step)
            {
                case 0:
                    if (CurrentTimeMachine.NotNullAndExists() && CurrentTimeMachine.Vehicle.GetMPHSpeed() >= 80)
                        CustomCamera?.Show();

                    Thunder.SourceEntity = FusionUtils.PlayerPed;
                    Thunder.Play();

                    Lightnings.Play();
                    Spark.Play();

                    step++;
                    gameTime = Game.GameTime + 100;
                    break;
                case 1:
                    if (currentIndex == sparkRope.Count)
                    {
                        currentIndex = 0;
                        step++;
                        gameTime = Game.GameTime + 1000;
                        break;
                    }

                    if (Lightnings.IsSequencePlaying)
                        Lightnings.Delete();

                    if (Spark.IsPlaying)
                        Spark.Stop();

                    sparkRope[currentIndex].Play();
                    currentIndex++;
                    gameTime = Game.GameTime + 10;
                    break;
                case 2:
                    sparkRope.ForEach(x => x.StopNaturally());

                    step++;
                    gameTime = Game.GameTime + 250;
                    break;
                case 3:
                    if (currentIndex == fireRope.Count)
                    {
                        currentIndex = 0;
                        step++;
                        gameTime = Game.GameTime + 5000;
                        break;
                    }

                    if (FusionUtils.Random.NextDouble() >= 0.5f)
                        fireRope[currentIndex].Play();

                    currentIndex++;
                    gameTime = Game.GameTime + 10;
                    break;
                case 4:
                    if (currentIndex == fireRope.Count)
                    {
                        CustomCamera?.Stop();

                        currentIndex = 0;
                        step = 0;
                        IsPlaying = false;
                        break;
                    }

                    fireRope[currentIndex].StopNaturally();
                    currentIndex++;
                    gameTime = Game.GameTime + 20;
                    break;
            }

            Spark?.Tick();
        }

        private void Setup()
        {
            LeftStreetPole = World.CreateProp(streetPoleModel, new Vector3(50.4339f, 6576.8843f, 30.3620f), true, false);
            RightStreetPole = World.CreateProp(streetPoleModel, new Vector3(41.5676f, 6585.7378f, 30.3686f), true, false);

            Vector3 leftRope = LeftStreetPole.GetOffsetPosition(Vector3.Zero.GetSingleOffset(Coordinate.Z, 3.42f));
            Vector3 rightRope = RightStreetPole.GetOffsetPosition(Vector3.Zero.GetSingleOffset(Coordinate.Z, 3.42f));

            float distance = leftRope.DistanceTo(rightRope);

            StreetRope = World.AddRope((RopeType)6, leftRope, leftRope.GetDirectionTo(rightRope).DirectionToRotation(0), distance, distance, false);
            StreetRope.Connect(LeftStreetPole, leftRope, RightStreetPole, rightRope, distance);

            Mast = World.CreateProp(mastModel, new Vector3(63.0749f, 6582.1401f, 30.5130f), true, false);
            Mast.IsPositionFrozen = true;

            distance = leftRope.DistanceTo(polePosition);

            MastRope = World.AddRope((RopeType)6, leftRope, leftRope.GetDirectionTo(polePosition).DirectionToRotation(0), distance, distance, false);
            MastRope.Connect(LeftStreetPole, leftRope, Mast, polePosition, distance);

            Pole = World.CreateProp(poleModel, polePosition, true, false);
            Pole.IsPositionFrozen = true;

            Lightnings = new AnimatePropsHandler() { SequenceSpawn = true, SequenceInterval = 100, IsSequenceRandom = true, IsSequenceLooped = true };
            foreach (CustomModel x in ModelHandler.Lightnings)
                Lightnings.Add(new AnimateProp(x, Pole, lightningOffset, Vector3.Zero));

            Spark = new PtfxEntityPlayer("core", "ent_brk_sparking_wires_sp", Pole, Vector3.Zero, Vector3.Zero, 8f, true, true, 500);

            CustomCamera = new CustomCamera(RightStreetPole, new Vector3(11.93889f, 11.07275f, 4.756693f), new Vector3(11.65637f, 10.13232f, 4.56657f), 64);

            Thunder = Main.CommonAudioEngine.Create("general/thunder.wav", Presets.No3D);

            Vector3 curPos = polePosition;

            sparkRope.Add(new PtfxPlayer("scr_reconstructionaccident", "scr_sparking_generator", curPos, Vector3.Zero, 2, true));

            do
            {
                curPos += curPos.GetDirectionTo(leftRope) * 0.25f;
                sparkRope.Add(new PtfxPlayer("scr_reconstructionaccident", "scr_sparking_generator", curPos, Vector3.Zero, 2, true));
            } while (curPos.DistanceTo(leftRope) > 0.1f);

            do
            {
                curPos += curPos.GetDirectionTo(rightRope) * 0.25f;
                sparkRope.Add(new PtfxPlayer("scr_reconstructionaccident", "scr_sparking_generator", curPos, Vector3.Zero, 2, true));
            } while (curPos.DistanceTo(rightRope) > 0.1f);

            curPos = polePosition.GetSingleOffset(Coordinate.Z, -0.1f);

            leftRope = leftRope.GetSingleOffset(Coordinate.Z, -0.1f);
            rightRope = rightRope.GetSingleOffset(Coordinate.Z, -0.1f);

            fireRope.Add(new PtfxPlayer("core", "fire_petroltank_heli", curPos, curPos.GetDirectionTo(leftRope).DirectionToRotation(0), 0.4f, true));
            fireRope.Last().SetEvolutionParam("strength", 1);
            fireRope.Last().SetEvolutionParam("dist", 0);
            fireRope.Last().SetEvolutionParam("fadein", 0);

            do
            {
                curPos += curPos.GetDirectionTo(leftRope) * 0.5f;
                fireRope.Add(new PtfxPlayer("core", "fire_petroltank_heli", curPos, curPos.GetDirectionTo(leftRope).DirectionToRotation(0), 0.4f, true));
                fireRope.Last().SetEvolutionParam("strength", 1);
                fireRope.Last().SetEvolutionParam("dist", 0);
                fireRope.Last().SetEvolutionParam("fadein", 0);
            } while (curPos.DistanceTo(leftRope) > 0.1f);

            do
            {
                curPos += curPos.GetDirectionTo(rightRope) * 0.5f;
                fireRope.Add(new PtfxPlayer("core", "fire_petroltank_heli", curPos, rightRope.GetDirectionTo(curPos).DirectionToRotation(0), 0.4f, true));
                fireRope.Last().SetEvolutionParam("strength", 1);
                fireRope.Last().SetEvolutionParam("dist", 0);
                fireRope.Last().SetEvolutionParam("fadein", 0);
            } while (curPos.DistanceTo(rightRope) > 0.1f);
        }

        protected override void OnEnd()
        {
            CustomCamera?.Stop();

            Lightnings?.Delete();
            LeftStreetPole?.Delete();
            RightStreetPole?.Delete();
            Pole?.Delete();
            StreetRope?.Delete();
            MastRope?.Delete();

            sparkRope?.ForEach(x => x?.Stop());
            fireRope?.ForEach(x => x?.Stop());

            IsPlaying = false;
        }

        protected override void OnStart()
        {

        }
    }
}
