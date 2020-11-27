using System;

namespace BTTFVUtils
{
    public class Enums
    {
        public enum Coordinate
        {
            X,
            Y,
            Z
        }

        [Flags]
        public enum SpawnFlags
        {
            Default = 0,
            WarpPlayer = 1,
            ForcePosition = 2,
            ResetValues = 4,
            Broken = 8,
            ForceReentry = 16,
            CheckExists = 32,
            NoOccupants = 64,
            NoVelocity = 128,
        }

        public enum TimeTravelPhase
        {
            Completed = 0,
            OpeningWormhole = 1,
            InTime = 2,
            Reentering = 3
        }

        public enum ReenterType
        {
            Normal,
            Spawn,
            Forced
        }

        public enum TimeTravelType
        {
            Cutscene,
            Instant,
            RC
        }

        public enum LightsMode
        {
            Default,
            Disabled,
            AlwaysOn
        }

        public enum MapArea
        {
            County = 2072609373,
            City = -289320599
        }

        public enum WheelId
        {
            FrontLeft = 0,
            FrontRight = 1,
            RearLeft = 4,
            RearRight = 5
        }

        public enum MissionType
        {
            None,
            Escape,
            Train
        }

        public enum TimeMachineCamera
        {
            Default = -1,
            DestinationDate,
            DriverSeat,
            DigitalSpeedo,
            AnalogSpeedo,
            FrontPassengerWheelLookAtRear,
            TrainApproaching,
            RightSide,
            FrontToBack,
            FrontOnRail,
            FrontToBackRightSide
        }
    }
}
