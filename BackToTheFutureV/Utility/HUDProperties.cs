using System;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class HUDProperties
    {
        public int[] CurrentHeight = new int[10];

        public DateTime[] Date = new DateTime[3];

        public bool IsHUDVisible { get; set; } = false;
        public bool IsTickVisible { get; set; } = false;
        public EmptyType Empty { get; set; } = EmptyType.Hide;
        public int Speed { get; set; } = 0;

        public bool[] MonthVisible { get; set; } = new bool[3];
        public bool[] DayVisible { get; set; } = new bool[3];
        public bool[] YearVisible { get; set; } = new bool[3];
        public bool[] HourVisible { get; set; } = new bool[3];
        public bool[] MinuteVisible { get; set; } = new bool[3];
        public bool[] AmPmVisible { get; set; } = new bool[3];

        private int RowNameToInt(string name)
        {
            name = name.ToLower();

            switch (name)
            {
                case "red":
                    return 0;
                case "green":
                    return 1;
                default:
                    return 2;
            }
        }

        public void SetDate(string type, DateTime date)
        {
            int row = RowNameToInt(type);

            Date[row] = date;

            MonthVisible[row] = true;
            DayVisible[row] = true;
            YearVisible[row] = true;
            HourVisible[row] = true;
            MinuteVisible[row] = true;
            AmPmVisible[row] = true;
        }

        public void SetVisible(string type, bool toggle, bool month = true, bool day = true, bool year = true, bool hour = true, bool minute = true, bool amPm = true)
        {
            int row = RowNameToInt(type);

            if (toggle)
            {
                if (!month)
                    MonthVisible[row] = false;

                if (!day)
                    DayVisible[row] = false;

                if (!year)
                    YearVisible[row] = false;

                if (!hour)
                    HourVisible[row] = false;

                if (!minute)
                    MinuteVisible[row] = false;

                if (!amPm)
                    AmPmVisible[row] = false;
            }
            else
            {
                if (month)
                    MonthVisible[row] = false;

                if (day)
                    DayVisible[row] = false;

                if (year)
                    YearVisible[row] = false;

                if (hour)
                    HourVisible[row] = false;

                if (minute)
                    MinuteVisible[row] = false;

                if (amPm)
                    AmPmVisible[row] = false;
            }
        }
    }
}
