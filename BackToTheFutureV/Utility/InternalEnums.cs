namespace BackToTheFutureV.Utility
{
    public class InternalEnums
    {
        public enum WaybackEventType
        {
            None,
            Refuel,
            Door
        }

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

        public enum WormholeType
        {
            Unknown = -1,
            DMC12,
            BTTF1,
            BTTF2,
            BTTF3
        }

        public enum ModState
        {
            Off = -1,
            On = 0
        }

        public enum HookState
        {
            Off,
            OnDoor,
            On,
            Removed,
            Unknown
        }

        public enum PlateType
        {
            Empty = -1,
            Outatime = 0,
            BTTF2 = 1,
            NOTIME = 2,
            TIMELESS = 3,
            TIMELESS2 = 4,
            DMCFACTORY = 5,
            DMCFACTORY2 = 6
        }

        public enum ReactorType
        {
            None = -1,
            MrFusion = 0,
            Nuclear = 1
        }

        public enum ExhaustType
        {
            Stock = -1,
            BTTF = 0,
            None = 1
        }

        public enum WheelType
        {
            Stock = -1,
            StockInvisible = 0,
            RailroadInvisible = 1,
            RedInvisible = 2,
            Red = 3
        }

        public enum SuspensionsType
        {
            Unknown = -1,
            Stock = 0,
            LiftFrontLowerRear = 1,
            LiftFront = 2,
            LiftRear = 3,
            LiftFrontAndRear = 4,
            LowerFrontLiftRear = 5,
            LowerFront = 6,
            LowerRear = 7,
            LowerFrontAndRear = 8
        }

        public enum HoodType
        {
            Stock = -1,
            H1983 = 0,
            H1981 = 1
        }
    }
}
