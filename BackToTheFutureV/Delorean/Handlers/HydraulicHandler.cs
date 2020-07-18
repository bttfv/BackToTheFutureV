using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Native;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class Hydraulic : Handler
    {
        public Hydraulic(TimeCircuits circuits) : base(circuits) {}

        public enum WheelId
        {
            FrontLeft = 0,
            FrontRight = 1,
            RearLeft = 4,
            RearRight = 5
        }

        private List<WheelId> _wheels = new List<WheelId>();
        public float Height { get; set; }

        private static void LiftUpWheel(Vehicle vehicle, WheelId id, float height)
        {
            //_SET_HYDRAULIC_STATE
            Function.Call((Hash) 0x84EA99C62CB3EF0C, vehicle, id, height);
        }

        public void LiftUpWheel(WheelId id)
        {
            _wheels.Add(id);
        }

        public override void KeyPress(Keys key) {}

        public override void Process()
        {
            if (_wheels.Count == 0)
                return;

            _wheels.ForEach(x => LiftUpWheel(Vehicle, x, Height));
        }

        public override void Stop()
        {
            _wheels.ForEach(x => LiftUpWheel(Vehicle, x, 0));
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}