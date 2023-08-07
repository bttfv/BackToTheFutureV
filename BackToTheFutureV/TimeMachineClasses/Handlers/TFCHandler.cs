using FusionLibrary;
using GTA;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{

    internal class TFCHandler : HandlerPrimitive
    {
        private int playAt;
        private bool finishedAnimating = true;
        private bool hasNuclearTimeTraveled = false;
        private TimedEventHandler emptyTimer { get; } = new TimedEventHandler();

        public TFCHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Props.Gauges.SpawnProp();

            Props.FuelGauge.OnAnimCompleted += FuelGauge_OnAnimCompleted;

            Props.TFCOff?.SpawnProp();

            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            Events.StartFuelGaugeGoDown += StartFuelGaugeGoDown;

            emptyTimer.Add(0, 0, 0, 3, 0, 0);
            emptyTimer.Last.SetFloat(90f, 0f);
        }

        private void StartFuelGaugeGoDown(bool doAnim)
        {
            if (doAnim)
            {
                Events.StartFuelBlink?.Invoke();
                finishedAnimating = true;
                hasNuclearTimeTraveled = true;
            }
        }

        private void FuelGauge_OnAnimCompleted(AnimationStep animationStep)
        {
            finishedAnimating = true;
        }

        private void OnTimeCircuitsToggle(bool instant = false)
        {
            finishedAnimating = false;

            if (instant)
            {
                if (Properties.AreTimeCircuitsOn)
                {
                    Props.TFCOn?.SpawnProp();
                    Props.TFCOff?.Delete();

                    Props.GaugeGlow?.SpawnProp();

                    Props.Gauges.Play(true);

                    if (!Properties.IsFueled || Mods.Reactor != ReactorType.Nuclear)
                    {
                        Props.FuelGauge?.SetRotation(Coordinate.Y, 0f, true);
                        Props.FuelGauge?.Stop();
                    }
                }
                else
                {
                    Props.TFCOn?.Delete();
                    Props.TFCOff?.SpawnProp();

                    Props.GaugeGlow?.Delete();

                    Props.Gauges.Play(true);

                    if (Props.FuelGauge?.CurrentRotation.Y > 0)
                    {
                        Props.FuelGauge?.SetRotation(Coordinate.Y, 0f, true);
                        Props.FuelGauge?.Stop();
                    }
                }

                finishedAnimating = true;
                return;
            }

            for (int i = 0; i < 3; i++)
                Props.Gauges[i].Stop();

            if (Props.FuelGauge.IsPlaying && !hasNuclearTimeTraveled)
            {
                Props.FuelGauge?.Stop(!hasNuclearTimeTraveled);
            }

            if (Properties.AreTimeCircuitsOn)
            {
                Props.TFCOn?.SpawnProp();
                Props.TFCOff?.Delete();

                Props.TFCHandle?.Play();

                if (Mods.Reactor == ReactorType.Nuclear)
                {
                    playAt = Game.GameTime + 2000;
                }
                else
                {
                    playAt = Game.GameTime;
                }

                IsPlaying = true;
            }
            else
            {
                IsPlaying = false;
                Props.TFCOff?.SpawnProp();
                Props.TFCOn?.Delete();

                Props.TFCHandle?.Play();

                Props.Gauge1?.Play();
                Props.Gauge2?.Play();

                if (Props.FuelGauge?.CurrentRotation.Y > 0)
                    Props.FuelGauge?.Play();

                Props.GaugeGlow?.Delete();
            }
        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Tick()
        {
            if (IsPlaying && Game.GameTime >= playAt)
            {
                if (Mods.Reactor == ReactorType.Nuclear)
                {
                    Sounds.PlutoniumGauge?.Play();
                }

                Props.Gauge1?.Play();
                Props.Gauge2?.Play();

                if (Mods.Reactor == ReactorType.Nuclear && (Properties.IsFueled || hasNuclearTimeTraveled))
                {
                    Props.FuelGauge?.Play();
                }

                Props.GaugeGlow?.SpawnProp();

                IsPlaying = false;
            }

            if (Properties.IsFueled && Properties.AreTimeCircuitsOn && Mods.Reactor == ReactorType.Nuclear && finishedAnimating && Props.FuelGauge.CurrentRotation.Y == 0)
            {
                Props.FuelGauge?.Play();
            }

            if (hasNuclearTimeTraveled)
            {
                emptyTimer.RunEvents();
            }

            if (!Properties.IsFueled && hasNuclearTimeTraveled && finishedAnimating)
            {
                Props.FuelGauge[AnimationType.Rotation][AnimationStep.First][Coordinate.Y].Setup(true, true, 0, emptyTimer.Last.CurrentFloat, 1, 50f, 1, true);
                if (Properties.AreTimeCircuitsOn)
                {
                    Props.FuelGauge?.Play(true);
                }
            }

            if (emptyTimer.AllExecuted() && hasNuclearTimeTraveled)
            {
                Props.FuelGauge[AnimationType.Rotation][AnimationStep.First][Coordinate.Y].Setup(true, true, 0, 90f, 1, 50f, 1, true);
                Props.FuelGauge?.SetRotation(Coordinate.Y, 0f, true);
                Props.FuelGauge?.Stop();
                emptyTimer.ResetExecution();
                hasNuclearTimeTraveled = false;
            }
        }

        public override void Stop()
        {
        }

        public override void Dispose()
        {
        }
    }
}
