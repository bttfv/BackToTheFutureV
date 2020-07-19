using BackToTheFutureV.Delorean.Handlers;
using GTA;

namespace BackToTheFutureV.Delorean
{

    public delegate void OnAttachedDelorean(DeloreanTimeMachine deloreanTimeMachine);

    public delegate void OnDetachedDelorean(DeloreanTimeMachine deloreanTimeMachine);

    public class RogersSierraManager
    {

        private static DeloreanTimeMachine attachedDeLorean;

        public static event OnAttachedDelorean OnAttachedDelorean;
        public static event OnDetachedDelorean OnDetachedDelorean;

        public static void Process()
        {            
            if (RogersSierra.Manager.RogersSierra != null)
            {
                if (RogersSierra.Manager.RogersSierra.isDeLoreanAttached == false)
                {
                    Vehicle tDel = RogersSierra.Manager.RogersSierra.CheckDeLorean();

                    if (tDel != null)
                    {
                        attachedDeLorean = DeloreanHandler.GetTimeMachineFromVehicle(tDel);

                        if (attachedDeLorean != null && attachedDeLorean.Mods.Wheel == WheelType.RailroadInvisible)
                        {
                            if (attachedDeLorean.Circuits.IsOnTracks)
                                attachedDeLorean.Circuits.GetHandler<RailroadHandler>().Stop();

                            attachedDeLorean.Circuits.IsAttachedToRogersSierra = true;
                            attachedDeLorean.Circuits.IsOnTracks = true;

                            RogersSierra.Manager.RogersSierra.AttachDeLorean(tDel);
                            
                            OnAttachedDelorean?.Invoke(attachedDeLorean);
                        }
                    }
                } else
                {
                    if (RogersSierra.Manager.RogersSierra.Locomotive.Speed == 0 && attachedDeLorean.Circuits.IsOnTracks == true)
                    {
                        RogersSierra.Manager.RogersSierra.DetachDeLorean();

                        OnDetachedDelorean?.Invoke(attachedDeLorean);

                        attachedDeLorean.Circuits.IsOnTracks = false;
                        attachedDeLorean.Circuits.IsAttachedToRogersSierra = false;
                        attachedDeLorean = null;
                    }
                }
            } else
                DetachDeLorean();
        }

        public static void DetachDeLorean()
        {
            if (attachedDeLorean != null)
            {
                RogersSierra.Manager.RogersSierra?.DetachDeLorean();
                
                attachedDeLorean.Circuits.IsOnTracks = false;
                attachedDeLorean.Circuits.IsAttachedToRogersSierra = false;
                attachedDeLorean = null;
            }
        }
    }
}
