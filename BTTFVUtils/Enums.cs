using System;

namespace FusionLibrary
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

        public enum SmokeColor
        {
            Off,
            Default,
            Green,
            Yellow,
            Red
        }

        public enum TrainCamera
        {
            Off = -1,
            TowardsRail,
            Pilot,
            Front,
            RightFunnel,
            RightWheels,
            RightFrontWheels,
            RightFront2Wheels,
            RightSide,
            TopCabin,
            LeftSide,
            LeftFunnel,
            LeftWheels,
            LeftFrontWheels,
            LeftFront2Wheels,
            Inside,
            WheelieUp,
            WheelieDown
        }

        public struct CoordinateSetting
        {
            public bool Update;
            public bool isIncreasing;
            public float Minimum;
            public float Maximum;
            public float MaxMinRatio;
            public float Step;
            public float StepRatio;
            public bool isFullCircle;
            public bool Stop;
        }

        public enum AnimationStep
        {
            Off,
            First,
            Second,
            Third,
            Fourth,
            Fifth
        }
    }
}
