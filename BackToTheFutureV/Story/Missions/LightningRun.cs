using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using KlangRageAudioLibrary;
using System;
using System.Linq;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class LightningRun : Mission
    {
        private static Vector3 lightningOffset = new Vector3(0, -1.548199f, 0.994037f);

        private static Vector3 polePosition = new Vector3(63.4558f, 6581.7065f, 46.8226f);

        private static Model streetPoleModel = new Model("prop_streetlight_09");
        private static Model mastModel = new Model("prop_air_mast_01");
        private static Model poleModel = new Model("prop_flagpole_1a");
        private static Model hookModel = new Model("bttf_hook_prop");

        private Prop LeftStreetPole;
        private Prop RightStreetPole;
        private Prop Mast;
        private Prop Pole;
        private Prop Hook;

        private Rope StreetRope;
        private Rope MastRope;

        private AnimatePropsHandler Lightnings;
        private ParticlePlayer Spark;
        private readonly ParticlePlayerHandler sparkRope = new ParticlePlayerHandler();
        private readonly ParticlePlayerHandler fireRope = new ParticlePlayerHandler();

        private Vector3 _springBase;
        private Vector3 _springOrigin;
        private Vector3 _springPosition;
        private Vector3 _springVelocity;
        private Vector3 _springDirection;
        private const float _springStiffness = 0.3f; // Smaller values make spring harder to bend
        private const float _springDamping = 0.3f; // Friction, resistance. Smaller values will make spring stall faster
        private const float _springWindFactor = 0.01f; // How much of the game wind will affect spring
        private const float _springLength = 2.54f; // Matched to prop height for accuracy
        private const int _springIterations = 4; // More iterations will make simulation look faster
        private bool _firstTick = true;

        private int step = 0;
        private int gameTime;
        private bool struck;

        private TimeMachine CurrentTimeMachine => TimeMachineHandler.CurrentTimeMachine;

        private readonly CustomCameraHandler CustomCamera = new CustomCameraHandler();

        private AudioPlayer Thunder;

        public const Hash LightningRunStreet = unchecked((Hash)4174973413);

        private Vector3 checkPos = new Vector3(-143.6626f, 6390.0047f, 30.7007f);

        public static DateTime StartTime { get; } = new DateTime(1955, 11, 12, 20, 0, 0);
        public static DateTime EndTime { get; } = new DateTime(1955, 11, 12, 22, 4, 30);
        public static DateTime StrikeTime { get; } = new DateTime(1955, 11, 12, 22, 4, 0);

        private bool setup;

        static LightningRun()
        {
            lightningOffset = (lightningOffset * -1).GetSingleOffset(Coordinate.Z, poleModel.Dimensions.frontTopRight.Z);
        }

        public LightningRun()
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
            if (key.KeyCode == Keys.U && !struck)
            {
                /*HookSetup(CurrentTimeMachine.Vehicle.Position);
                struck = true;*/
            }
            else if (key.KeyCode == Keys.U && struck)
            {
                /*Hook?.Delete();
                _firstTick = true;
                struck = false;*/
            }

            /*if (key.KeyCode == Keys.O)
            {
                FusionUtils.CurrentTime = StrikeTime.AddMinutes(-1);
            }*/
        }

        private void HookSetup(Vector3 position)
        {
            Hook = World.CreateProp(hookModel, new Vector3(position.X, position.Y, LeftStreetPole.GetOffsetPosition(Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.88f)).Z), false, false);
            Hook.IsCollisionEnabled = false;
            Hook.IsPositionFrozen = true;
            Hook.HasGravity = false;
            Hook.IsInvincible = true;
        }

        private Vector3 GetWindForce()
        {
            Vector3 dir = Function.Call<Vector3>(Hash.GET_WIND_DIRECTION);
            float scalar = Function.Call<float>(Hash.GET_WIND_SPEED);

            return dir * scalar;
        }

        private void ResetSpringPosition()
        {
            _springPosition = _springOrigin;
            _springVelocity = CurrentTimeMachine.Vehicle.Velocity * 0.1f;
        }

        private void UpdateSpringPosition()
        {
            _springBase = Hook.Position;
            _springOrigin = _springBase - (Vector3.WorldUp * _springLength);
        }

        private void UpdateSpringPhysics()
        {
            for (int i = 0; i < _springIterations; i++)
            {
                // Making simulation less artifical
                float randomScalar = FusionUtils.Random
                    .Next(-3, 10) / 10f;

                Vector3 acceleration = Vector3.Zero;
                acceleration += (_springOrigin - _springPosition) * _springStiffness;
                acceleration -= _springVelocity * _springDamping * randomScalar;
                acceleration += GetWindForce() * _springWindFactor * randomScalar;

                _springVelocity += acceleration * Game.LastFrameTime;
                _springPosition += _springVelocity * Game.LastFrameTime;
            }
            _springDirection = (_springPosition - _springBase).Normalized;
        }

        public override void Tick()
        {
            if (!TimeHandler.RealTime || FusionUtils.PlayerPed.Position.DistanceToSquared2D(checkPos) > 150000)
            {
                if (setup)
                {
                    OnEnd();
                }

                return;
            }

            if (!setup || (FusionUtils.CurrentTime >= StartTime && FusionUtils.CurrentTime <= EndTime && Thunder == null))
            {
                OnEnd();
                Setup();
            }

            if (Hook != null && Hook.IsVisible && IsPlaying)
            {

                UpdateSpringPosition();
                if (_firstTick)
                {
                    _firstTick = false;
                    ResetSpringPosition();
                }
                UpdateSpringPhysics();

                Vector3 springUp = _springDirection;
                Vector3 springForward = CurrentTimeMachine.Vehicle.ForwardVector;

                // Align spring forward with spring up to make right angle
                springForward = Vector3.Cross(springForward, Vector3.WorldUp);
                springForward = Vector3.Cross(springForward, springUp);
                Hook.Position = _springBase;
                Hook.Quaternion = Quaternion.LookRotation(springForward, -springUp);
            }

            if (FusionUtils.CurrentTime == StrikeTime && !IsPlaying)
            {
                IsPlaying = true;
            }

            if (IsPlaying && TimeMachineHandler.CurrentTimeMachine.NotNullAndExists() && LeftStreetPole.NotNullAndExists() && RightStreetPole.NotNullAndExists())
            {
                RaycastResult raycastResult = World.Raycast(LeftStreetPole.Position.GetSingleOffset(Coordinate.Z, 0.75f), RightStreetPole.Position.GetSingleOffset(Coordinate.Z, 0.75f), IntersectFlags.Vehicles);

                if (raycastResult.DidHit && raycastResult.HitEntity == CurrentTimeMachine)
                {
                    if ((CurrentTimeMachine.Mods.Hook == HookState.On /*|| (CurrentTimeMachine.Constants.DeluxoProto && CurrentTimeMachine.Vehicle.IsExtraOn(1))*/) && CurrentTimeMachine.Properties.AreTimeCircuitsOn && CurrentTimeMachine.Constants.OverTimeTravelAtSpeed && !CurrentTimeMachine.Properties.HasBeenStruckByLightning && sparkRope.ParticlePlayers.Count(x => x.IsPlaying) >= 100)
                    {
                        CurrentTimeMachine.Events.StartLightningStrike?.Invoke(-1);

                        if (!struck && sparkRope.SequenceComplete)
                        {
                            if (CurrentTimeMachine.Mods.IsDMC12)
                            {
                                HookSetup(CurrentTimeMachine.Vehicle.Position);
                            }

                            struck = true;
                        }
                    }
                }
            }

            if (!IsPlaying || Game.GameTime < gameTime)
            {
                return;
            }

            switch (step)
            {
                case 0:
                    if (CurrentTimeMachine.NotNullAndExists() && CurrentTimeMachine.Constants.ReadyForLightningRun && CurrentTimeMachine.Vehicle.GetMPHSpeed() >= 80)
                    {
                        CustomCamera.Show(0);
                    }

                    Thunder.SourceEntity = FusionUtils.PlayerPed;
                    Thunder.Play();

                    Lightnings.Play();
                    Spark.Play();

                    step++;
                    gameTime = Game.GameTime + 100;
                    break;
                case 1:
                    if (Lightnings.IsSequencePlaying)
                    {
                        Lightnings.Delete();
                    }

                    if (Spark.IsPlaying)
                    {
                        Spark.Stop();
                    }

                    if (!sparkRope.IsPlaying)
                    {
                        sparkRope.Play();
                    }
                    else if (sparkRope.SequenceComplete)
                    {
                        step++;
                        gameTime = Game.GameTime + 1000;
                    }

                    break;
                case 2:
                    sparkRope.Stop();

                    step++;
                    gameTime = Game.GameTime + 250;
                    break;
                case 3:
                    if (!fireRope.IsPlaying)
                    {
                        fireRope.Play();
                    }
                    else if (fireRope.SequenceComplete)
                    {
                        step++;
                        gameTime = Game.GameTime + 1000;
                    }

                    break;
                case 4:
                    fireRope.StopInSequence();

                    step++;
                    break;
                case 5:
                    if (fireRope.IsPlaying)
                    {
                        break;
                    }

                    IsPlaying = false;

                    CustomCamera.Stop();

                    step = 0;
                    struck = false;
                    break;
            }
        }

        private void Setup()
        {
            LeftStreetPole = World.CreateProp(streetPoleModel, new Vector3(50.4339f, 6576.8843f, 30.3620f), true, false);
            RightStreetPole = World.CreateProp(streetPoleModel, new Vector3(41.5676f, 6585.7378f, 30.3686f), true, false);

            Mast = World.CreateProp(mastModel, new Vector3(63.0749f, 6582.1401f, 30.5130f), true, false);
            Mast.IsPositionFrozen = true;

            Pole = World.CreateProp(poleModel, polePosition, true, false);
            Pole.IsPositionFrozen = true;

            if (FusionUtils.CurrentTime >= StartTime && FusionUtils.CurrentTime <= EndTime)
            {
                if (FusionUtils.IsTrafficAlive)
                {
                    TimeHandler.MissionTraffic = true;
                    FusionUtils.IsTrafficAlive = false;
                    Function.Call(Hash.SET_PED_PATHS_IN_AREA, -800.0f, 5500.0f, -1000.0f, 500.0f, 7000.0f, 1000.0f, 1);
                    Function.Call(Hash.SET_PED_POPULATION_BUDGET, 0);
                }

                Vector3 leftRope = LeftStreetPole.GetOffsetPosition(Vector3.Zero.GetSingleOffset(Coordinate.Z, 3.42f));
                Vector3 rightRope = RightStreetPole.GetOffsetPosition(Vector3.Zero.GetSingleOffset(Coordinate.Z, 3.42f));

                float distance = leftRope.DistanceTo(rightRope);

                StreetRope = World.AddRope((RopeType)6, leftRope, leftRope.GetDirectionTo(rightRope).DirectionToRotation(0), distance, distance, false);
                StreetRope.Connect(LeftStreetPole, leftRope, RightStreetPole, rightRope, distance);

                distance = leftRope.DistanceTo(polePosition);

                MastRope = World.AddRope((RopeType)6, leftRope, leftRope.GetDirectionTo(polePosition).DirectionToRotation(0), distance, distance, false);
                MastRope.Connect(LeftStreetPole, leftRope, Mast, polePosition, distance);

                Lightnings = new AnimatePropsHandler() { SequenceSpawn = true, SequenceInterval = 100, IsSequenceRandom = true, IsSequenceLooped = true };
                foreach (CustomModel x in ModelHandler.Lightnings)
                {
                    Lightnings.Add(new AnimateProp(x, Pole, lightningOffset, Vector3.Zero));
                }

                Spark = new ParticlePlayer("core", "ent_brk_sparking_wires_sp", ParticleType.ForceLooped, Pole, Vector3.Zero, Vector3.Zero, 8f) { Interval = 500 };

                CustomCamera.Add(RightStreetPole, new Vector3(11.93889f, 11.07275f, 4.756693f), new Vector3(11.65637f, 10.13232f, 4.56657f), 64);

                Thunder = Main.CommonAudioEngine.Create("general/thunder.wav", Presets.No3D);

                Vector3 curPos = polePosition;

                sparkRope.Add("scr_reconstructionaccident", "scr_sparking_generator", ParticleType.Looped, curPos, Vector3.Zero, 2);

                do
                {
                    curPos += curPos.GetDirectionTo(leftRope) * 0.25f;
                    sparkRope.Add("scr_reconstructionaccident", "scr_sparking_generator", ParticleType.Looped, curPos, Vector3.Zero, 2);
                } while (curPos.DistanceTo(leftRope) > 0.1f);

                do
                {
                    curPos += curPos.GetDirectionTo(rightRope) * 0.25f;
                    sparkRope.Add("scr_reconstructionaccident", "scr_sparking_generator", ParticleType.Looped, curPos, Vector3.Zero, 2);
                } while (curPos.DistanceTo(rightRope) > 0.1f);

                sparkRope.UseFrameTimeHelper = true;

                curPos = polePosition.GetSingleOffset(Coordinate.Z, -0.1f);

                leftRope = leftRope.GetSingleOffset(Coordinate.Z, -0.1f);
                rightRope = rightRope.GetSingleOffset(Coordinate.Z, -0.1f);

                fireRope.Add("core", "fire_petroltank_heli", ParticleType.Looped, curPos, curPos.GetDirectionTo(leftRope).DirectionToRotation(0), 0.4f);

                do
                {
                    curPos += curPos.GetDirectionTo(leftRope) * 0.5f;
                    fireRope.Add("core", "fire_petroltank_heli", ParticleType.Looped, curPos, curPos.GetDirectionTo(leftRope).DirectionToRotation(0), 0.4f);
                } while (curPos.DistanceTo(leftRope) > 0.1f);

                do
                {
                    curPos += curPos.GetDirectionTo(rightRope) * 0.5f;
                    fireRope.Add("core", "fire_petroltank_heli", ParticleType.Looped, curPos, rightRope.GetDirectionTo(curPos).DirectionToRotation(0), 0.4f);
                } while (curPos.DistanceTo(rightRope) > 0.1f);

                fireRope.SetEvolutionParam("strength", 1);
                fireRope.SetEvolutionParam("dist", 0);
                fireRope.SetEvolutionParam("fadein", 0);

                fireRope.UseFrameTimeHelper = true;
                fireRope.ChanceOfSpawn = 0.5f;
            }
            setup = true;
        }

        protected override void OnEnd()
        {
            CustomCamera.Stop();
            CustomCamera.Cameras.Clear();

            Lightnings?.Delete();
            LeftStreetPole?.Delete();
            RightStreetPole?.Delete();
            Mast?.Delete();
            Pole?.Delete();
            StreetRope?.Delete();
            MastRope?.Delete();
            Hook?.Delete();

            sparkRope?.Stop();
            fireRope?.Stop();

            if (!FusionUtils.IsTrafficAlive)
            {
                FusionUtils.IsTrafficAlive = true;
                TimeHandler.MissionTraffic = false;
                Function.Call(Hash.SET_PED_PATHS_BACK_TO_ORIGINAL, -800.0f, 5500.0f, -1000.0f, 500.0f, 7000.0f, 1000.0f);
                Function.Call(Hash.SET_PED_POPULATION_BUDGET, 3);
            }

            IsPlaying = false;
            struck = false;
            _firstTick = true;
            setup = false;
        }

        protected override void OnStart()
        {

        }
    }
}
