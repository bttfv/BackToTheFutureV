using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
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
            Decorator.Register(BTTFVDecors.TorqueMultiplier, DecorType.Float);
            Decorator.Register(BTTFVDecors.TimeMachine, DecorType.Bool);
            Decorator.Register(BTTFVDecors.TimeCircuitsOn, DecorType.Bool);
            Decorator.Register(BTTFVDecors.CutsceneMode, DecorType.Bool);
            Decorator.Register(BTTFVDecors.WormholeType, DecorType.Int);
            Decorator.Register(BTTFVDecors.DestDate, DecorType.DateTime);
            Decorator.Register(BTTFVDecors.LastDate, DecorType.DateTime);
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
                Properties.DestinationTime = Decorator.GetDateTime(BTTFVDecors.DestDate);
                Properties.PreviousTime = Decorator.GetDateTime(BTTFVDecors.LastDate);

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
            Decorator.SetDateTime(BTTFVDecors.DestDate, Properties.DestinationTime);
        }

        private void OnTimeTravelStarted()
        {
            Decorator.SetDateTime(BTTFVDecors.LastDate, Properties.PreviousTime);
        }

        private void OnTimeCircuitsToggle()
        {
            Decorator.SetBool(BTTFVDecors.TimeCircuitsOn, Properties.AreTimeCircuitsOn);
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
