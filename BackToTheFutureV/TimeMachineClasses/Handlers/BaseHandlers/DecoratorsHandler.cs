using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class DecoratorsHandler : HandlerPrimitive
    {
        public Decorator Decorator { get; }

        public float TorqueMultiplier
        {
            get => Decorator.GetFloat(BTTFVDecors.TorqueMultiplier);

            set => Decorator.SetFloat(BTTFVDecors.TorqueMultiplier, value);
        }

        public static void Register()
        {
            Decorator.Register(BTTFVDecors.TimeMachine, DecorType.Bool);
            Decorator.Register(BTTFVDecors.DestDate1, DecorType.Int);
            Decorator.Register(BTTFVDecors.DestDate2, DecorType.Int);
            Decorator.Register(BTTFVDecors.LastDate1, DecorType.Int);
            Decorator.Register(BTTFVDecors.LastDate2, DecorType.Int);
            Decorator.Register(BTTFVDecors.WormholeType, DecorType.Int);
            Decorator.Register(BTTFVDecors.TimeCircuitsOn, DecorType.Bool);
            Decorator.Register(BTTFVDecors.CutsceneMode, DecorType.Bool);
            Decorator.Register(BTTFVDecors.TorqueMultiplier, DecorType.Float);
            Decorator.Lock();
        }

        public DecoratorsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Decorator = Vehicle.Decorator();

            Events.OnWormholeTypeChanged += OnWormholeTypeChanged;
            Events.OnDestinationDateChange += OnDestinationDateChange;
            Events.OnTimeTravelStarted += OnTimeTravelStarted;
            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;

            if (!Mods.IsDMC12 && CheckVehicle())
            {
                Mods.WormholeType = (WormholeType)Decorator.GetInt(BTTFVDecors.WormholeType);
                Properties.DestinationTime = ConvertIntToDate(Decorator.GetInt(BTTFVDecors.DestDate1), Decorator.GetInt(BTTFVDecors.DestDate2));
                Properties.PreviousTime = ConvertIntToDate(Decorator.GetInt(BTTFVDecors.LastDate1), Decorator.GetInt(BTTFVDecors.LastDate2));

                if (Properties.AreTimeCircuitsOn != Decorator.GetBool(BTTFVDecors.TimeCircuitsOn))
                {
                    Events.SetTimeCircuits?.Invoke(!Properties.AreTimeCircuitsOn);
                }

                return;
            }

            Decorator.SetBool(BTTFVDecors.TimeMachine, true);

            OnWormholeTypeChanged();
            OnDestinationDateChange(InputType.Full);
            OnTimeTravelStarted();
            OnTimeCircuitsToggle();

            if (!Decorator.Exists(BTTFVDecors.TorqueMultiplier))
            {
                TorqueMultiplier = 1f;
            }
        }

        public bool CheckVehicle()
        {
            return CheckVehicle(Vehicle);
        }

        public static bool CheckVehicle(Vehicle vehicle)
        {
            return vehicle.Decorator().Exists(BTTFVDecors.TimeMachine) && vehicle.Decorator().GetBool(BTTFVDecors.TimeMachine);
        }

        public void OnWormholeTypeChanged()
        {
            Decorator.SetInt(BTTFVDecors.WormholeType, (int)Mods.WormholeType);
        }

        public void OnDestinationDateChange(InputType inputType)
        {
            int[] tmp = ConvertDateToInt(Properties.DestinationTime);

            Decorator.SetInt(BTTFVDecors.DestDate1, tmp[0]);
            Decorator.SetInt(BTTFVDecors.DestDate2, tmp[1]);
        }

        private void OnTimeTravelStarted()
        {
            int[] tmp = ConvertDateToInt(Properties.PreviousTime);

            Decorator.SetInt(BTTFVDecors.LastDate1, tmp[0]);
            Decorator.SetInt(BTTFVDecors.LastDate2, tmp[1]);
        }

        private void OnTimeCircuitsToggle()
        {
            Decorator.SetBool(BTTFVDecors.TimeCircuitsOn, Properties.AreTimeCircuitsOn);
        }

        public static int[] ConvertDateToInt(DateTime dateTime)
        {
            return new int[] { Convert.ToInt32($"9{dateTime:MMyyyy}"), Convert.ToInt32($"9{dateTime:ddhhmm}") };
        }

        public static DateTime ConvertIntToDate(int dateTime1, int dateTime2)
        {
            string tmp1 = dateTime1.ToString().Substring(1);
            IEnumerable<string> tmp2 = dateTime2.ToString().Substring(1).SplitInParts(2);

            return new DateTime(Convert.ToInt32(tmp1.Substring(2)), Convert.ToInt32(tmp1.Substring(0, 2)), Convert.ToInt32(tmp2.ElementAt(0)), Convert.ToInt32(tmp2.ElementAt(1)), Convert.ToInt32(tmp2.ElementAt(2)), 0);
        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Stop()
        {

        }

        public override void Tick()
        {

        }
    }
}
