using System.Windows.Forms;
using GTA;
using GTA.Math;
using BackToTheFutureV.Utility;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class HookHandler : Handler
    {
        private Vector3 hookPosition = new Vector3(0.75f, 0f, 0f);

        public HookHandler(TimeCircuits circuits) : base(circuits)
        {

        }

        public override void KeyPress(Keys key)
        {
        }

        public override void Process()
        {
            if (Main.PlayerPed.IsInVehicle()) 
                return;

            if (Mods.Hook != HookState.OnDoor) 
                return;

            Vector3 worldPos = Vehicle.GetOffsetPosition(hookPosition);

            float dist = Main.PlayerPed.Position.DistanceToSquared(worldPos);

            if(dist <= 2f * 2f)
            {
                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Apply_Hook"));

                if (Game.IsControlJustPressed(GTA.Control.Context))
                    Mods.Hook = HookState.On;
            }
        }

        public override void Stop()
        {
        }

        public override void Dispose()
        {
        }
    }
}
