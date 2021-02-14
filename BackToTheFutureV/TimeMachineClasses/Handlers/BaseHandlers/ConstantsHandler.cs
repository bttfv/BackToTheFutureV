using BackToTheFutureV.Vehicles;
using FusionLibrary.Extensions;
using GTA;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    public class ConstantsHandler : Handler
    {
        public bool Over88MphSpeed { get; private set; }

        public bool OverStartTimeTravelSequenceAtSpeed { get; private set; }
        public int StartTimeTravelSequenceAtSpeed
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return 88;
                    case WormholeType.BTTF2:
                        return 88;
                    case WormholeType.BTTF3:
                        return Properties.IsOnTracks ? 82 : 65;
                    default:
                        return 88;
                }
            }
        }

        public int TimeTravelAtTime { get; private set; }
        public int StabilizationSoundAtTime { get; private set; }
        public float TimeTravelCooldown { get; private set; } = -1;

        public int WormholeLengthTime
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return 6000;
                    case WormholeType.BTTF2:
                        return 3900;
                    case WormholeType.BTTF3:
                        return 5200;
                    default:
                        return 5200;
                }
            }
        }

        public bool OverDiodeSoundAtSpeed { get; private set; }
        public int PlayDiodeSoundAtSpeed
        {
            get
            {
                switch (Mods.WormholeType)
                {
                    case WormholeType.BTTF1:
                        return 82;
                    case WormholeType.BTTF2:
                        return 82;
                    case WormholeType.BTTF3:
                        return Properties.IsOnTracks ? 80 : 60;
                    default:
                        return 82;
                }
            }
        }

        public int FireTrailsAppearTime => (FireTrailsIs99 || Properties.IsFlying) ? -1 : 0;

        public float FireTrailsDisappearTime => (FireTrailsIs99 || Properties.IsFlying) ? 1 : 15;

        public bool FireTrailsUseBlue => Mods.WormholeType == WormholeType.BTTF1;

        public bool FireTrailsIs99 => Properties.IsFlying && Properties.HasBeenStruckByLightning;

        public int FireTrailsLength => Properties.IsOnTracks ? 100 : 50;

        public ConstantsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnReenter += StartTimeTravelCooldown;
        }

        public void StartTimeTravelCooldown()
        {
            TimeTravelCooldown = 0;
            ResetAll();
        }

        public void ResetAll()
        {
            TimeTravelAtTime = 0;
            StabilizationSoundAtTime = 0;

            Over88MphSpeed = false;
            OverDiodeSoundAtSpeed = false;
            OverStartTimeTravelSequenceAtSpeed = false;
        }

        public override void Dispose()
        {

        }

        public override void KeyDown(Keys key)
        {

        }

        public override void Process()
        {
            if (TimeTravelCooldown > -1)
            {
                TimeTravelCooldown += Game.LastFrameTime;

                if (TimeTravelCooldown >= 30)
                    TimeTravelCooldown = -1;
                else
                    return;
            }

            if (Vehicle.GetMPHSpeed() >= PlayDiodeSoundAtSpeed && !OverDiodeSoundAtSpeed)
            {
                OverDiodeSoundAtSpeed = true;
                Events.OnPlayDiodeSoundAtSpeed?.Invoke(true);
            }

            if (Vehicle.GetMPHSpeed() < PlayDiodeSoundAtSpeed && OverDiodeSoundAtSpeed)
            {
                TimeTravelAtTime = 0;
                StabilizationSoundAtTime = 0;

                OverDiodeSoundAtSpeed = false;
                Events.OnPlayDiodeSoundAtSpeed?.Invoke(false);
            }

            if (Vehicle.GetMPHSpeed() >= StartTimeTravelSequenceAtSpeed && !OverStartTimeTravelSequenceAtSpeed && !Properties.BlockSparks)
            {
                OverStartTimeTravelSequenceAtSpeed = true;
                Events.OnStartTimeTravelSequenceAtSpeed?.Invoke(true);
            }

            if (Vehicle.GetMPHSpeed() < StartTimeTravelSequenceAtSpeed && OverStartTimeTravelSequenceAtSpeed)
            {
                OverStartTimeTravelSequenceAtSpeed = false;
                Events.OnStartTimeTravelSequenceAtSpeed?.Invoke(false);
            }

            if (Vehicle.GetMPHSpeed() >= 88 && !Over88MphSpeed)
            {
                TimeTravelAtTime = Game.GameTime + WormholeLengthTime;
                StabilizationSoundAtTime = Game.GameTime + 1000;

                Over88MphSpeed = true;
                Events.On88MphSpeed?.Invoke(true);
            }

            if (Vehicle.GetMPHSpeed() < 88 && Over88MphSpeed)
            {
                Over88MphSpeed = false;
                Events.On88MphSpeed?.Invoke(false);
            }
        }

        public override void Stop()
        {

        }
    }
}
