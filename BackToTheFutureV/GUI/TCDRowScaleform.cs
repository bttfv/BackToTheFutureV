using FusionLibrary;
using System;

namespace BackToTheFutureV.GUI
{
    public class TCDRowScaleform : ScaleformGui
    {
        public TCDRowScaleform(string type) : base("bttf_3d_gui_" + type)
        {
            Type = type;
            date = new DateTime();
        }

        private DateTime date;

        public string Type { get; }

        public void SetDate(DateTime date)
        {
            this.date = date;

            CallFunction("SET_MONTH", date.Month);
            CallFunction("SET_DAY", date.Day);
            CallFunction("SET_YEAR", date.Year);
            CallFunction("SET_HOUR", ((date.Hour + 11) % 12) + 1);
            CallFunction("SET_MINUTE", date.Minute);
        }

        public void SetVisible(bool toggle, bool month = true, bool day = true, bool year = true, bool hour = true, bool minute = true)
        {
            if (toggle)
            {
                SetDate(date);

                if (!month)
                    CallFunction("SET_MONTH", -1);

                if (!day)
                    CallFunction("SET_DAY", -1);

                if (!year)
                    CallFunction("SET_YEAR", -1);

                if (!hour)
                    CallFunction("SET_HOUR", -1);

                if (!minute)
                    CallFunction("SET_MINUTE", -1);
            }
            else
            {
                if (month)
                    CallFunction("SET_MONTH", -1);

                if (day)
                    CallFunction("SET_DAY", -1);

                if (year)
                    CallFunction("SET_YEAR", -1);

                if (hour)
                    CallFunction("SET_HOUR", -1);

                if (minute)
                    CallFunction("SET_MINUTE", -1);
            }
        }
    }
}
