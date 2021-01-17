using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class SIDHandler : Handler
    {
        private bool _sidMax;
        private int _playDiodeSoundAt;

        public SIDHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnSIDReachMax += OnSIDReachMax;
        }

        public override void Dispose()
        {
            
        }

        public override void KeyDown(Keys key)
        {
            
        }

        private void OnSIDReachMax(int playDiodeSoundAt)
        {
            _sidMax = true;
            _playDiodeSoundAt = playDiodeSoundAt;
        }

        private void DrawRT()
        {
            if (!Properties.IsGivenScaleformPriority || Utils.PlayerPed.Position.DistanceToSquared(Vehicle.Position) > 6f * 6f || ModSettings.HideSID)
                return;

            Scaleforms.SIDRT?.Draw();
        }

        public override void Process()
        {
            DrawRT();

            if (Utils.PlayerVehicle != Vehicle || !Properties.AreTimeCircuitsOn || Properties.TimeTravelPhase > Enums.TimeTravelPhase.OpeningWormhole)
                return;

            if (_sidMax)
            {
                if (Vehicle.GetMPHSpeed() < _playDiodeSoundAt)
                {
                    _sidMax = false;
                    return;
                }

                ScaleformsHandler.SID2D?.SetAllState(true);
                ScaleformsHandler.SID3D?.SetAllState(true);

                if (!Properties.IsGivenScaleformPriority || Utils.PlayerPed.Position.DistanceToSquared(Vehicle.Position) > 6f * 6f || ModSettings.HideSID)
                    return;

                Scaleforms.SIDRT?.Draw();

                return;
            }

            float speed = Vehicle.GetMPHSpeed();

            if (speed == 0)
            {
                ScaleformsHandler.SID2D?.Random(0, 0);
                ScaleformsHandler.SID3D?.Random(0, 0);

                if (!Properties.IsGivenScaleformPriority || Utils.PlayerPed.Position.DistanceToSquared(Vehicle.Position) > 6f * 6f || ModSettings.HideSID)
                    return;

                Scaleforms.SIDRT?.Draw();

                return;
            }

            if (speed > 88)
                speed = 88;

            int height = (int)((speed / 88) * 15);

            int min = height - 2;
            int max = height + 5;

            if (min < 0)
                min = 0;

            if (max > 20)
                max = 20;

            ScaleformsHandler.SID2D?.Random(min, max);
            ScaleformsHandler.SID3D?.Random(min, max);

        }

        public override void Stop()
        {
            
        }
    }
}
