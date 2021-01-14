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

        public override void Process()
        {
            if (Utils.PlayerVehicle != Vehicle || !Properties.AreTimeCircuitsOn || Properties.TimeTravelPhase > Enums.TimeTravelPhase.OpeningWormhole)
                return;

            if (_sidMax)
            {
                if (Vehicle.GetMPHSpeed() < _playDiodeSoundAt)
                {
                    _sidMax = false;
                    return;
                }

                ScaleformsHandler.SID?.Random(20, 20);
                ScaleformsHandler.SID3D?.Random(20, 20);

                return;
            }

            float speed = Vehicle.GetMPHSpeed();

            if (speed == 0)
            {
                ScaleformsHandler.SID?.Random(0, 0);
                ScaleformsHandler.SID3D?.Random(0, 0);

                return;
            }

            if (speed > 88)
                speed = 88;

            int height = (int)((speed / 88) * 18);

            int min = height - 2;
            int max = height + 2;

            if (min < 0)
                min = 0;

            if (max > 20)
                max = 20;

            ScaleformsHandler.SID?.Random(min, max);
            ScaleformsHandler.SID3D?.Random(min, max);
        }

        public override void Stop()
        {
            
        }
    }
}
