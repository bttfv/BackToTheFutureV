using BackToTheFutureV.Settings;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class SpeedoHandler : Handler
    {
        private int nextCheck;
        private int currentSpeed = -1;

        private bool simulateSpeed;
        private int maxSpeed;
        private int maxSeconds;
        private float currentSimSpeed;

        public SpeedoHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.SetSimulateSpeed += SetSimulateSpeed;
        }

        public override void Dispose()
        {
                        
        }

        public override void KeyDown(Keys key)
        {
        }

        public void SetSimulateSpeed(int maxSpeed, int seconds)
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

        public override void Process()
        {
            if (!Properties.IsGivenScaleformPriority || Utils.PlayerPed.Position.DistanceToSquared(Vehicle.Position) > 6f * 6f)
                return;

            Scaleforms.SpeedoRT.Draw();

            if (Game.GameTime < nextCheck) return;

            if (Properties.TimeTravelPhase < TimeTravelPhase.InTime)
            {
                int mphSpeed = ((int) Vehicle.GetMPHSpeed());

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
                        
                    mphSpeed = (int)currentSimSpeed;

                    if (mphSpeed >= maxSpeed)
                    {
                        simulateSpeed = false;
                        Events.OnSimulateSpeedReached?.Invoke();
                    }
                }

                if (mphSpeed > 88)
                    mphSpeed = 88;

                string mphSpeedStr = mphSpeed.ToString("00");

                string speedDigit1 = mphSpeedStr.Substring(0, 1);
                string speedDigit2 = mphSpeedStr.Substring(1, 1);

                if (mphSpeed > currentSpeed || mphSpeed < currentSpeed)
                {
                    currentSpeed = mphSpeed;

                    if (ModSettings.PlaySpeedoBeep && Vehicle.IsVisible)
                        Sounds.Speedo?.Play();

                    UpdateGUI(ScaleformsHandler.Speedo, speedDigit1, speedDigit2);

                    ExternalHUD.Speed = mphSpeed;
                    RemoteHUD.Speed = mphSpeed;
                    
                    if (!TcdEditer.IsEditing)
                        UpdateGUI(ScaleformsHandler.GUI, speedDigit1, speedDigit2);
                }
            }

            nextCheck = Game.GameTime + 20;
        }

        private void UpdateGUI(ScaleformGui scaleform, string digit1, string digit2)
        {
            if (digit1 != "0")
                scaleform.CallFunction("SET_DIGIT_1", int.Parse(digit1));
            else
                scaleform.CallFunction("SET_DIGIT_1", 15);

            scaleform.CallFunction("SET_DIGIT_2", int.Parse(digit2));
        }

        public override void Stop()
        {

        }
    }
}