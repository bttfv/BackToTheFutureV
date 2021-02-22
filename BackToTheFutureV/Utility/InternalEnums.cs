namespace BackToTheFutureV.Utility
{
    public class InternalEnums
    {
        public enum RcModes
        {
            FromCarCamera,
            FromPlayerCamera
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
            RC,
            Wayback
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
            FrontToBackRightSide,
            LicensePlate,
            TimeTravelOnTracks,
            DigitalSpeedoTowardsFront,
            RearCarTowardsFront
        }
    }
}
