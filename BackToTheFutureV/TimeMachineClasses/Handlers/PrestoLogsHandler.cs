using System.Collections.Generic;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System.Windows.Forms;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class PrestoLogsHandler : Handler
    {
        private Vector3 logsRotation = new Vector3(10f, 0, 0);

        private AnimateProp greenLog;
        private Vector3 greenOffset = new Vector3(-0.08f, -0.07f, 0.07f);

        private AnimateProp yellowLog;
        private Vector3 yellowOffset = new Vector3(0.08f, -0.07f, 0.07f);

        private AnimateProp redLog;
        private Vector3 redOffset = new Vector3(0, -0.08f, 0.16f);

        public PrestoLogsHandler(TimeCircuits circuits) : base(circuits)
        {
            ModelHandler.RequestModel(ModelHandler.GreenPrestoLogProp);
            ModelHandler.RequestModel(ModelHandler.YellowPrestoLogProp);
            ModelHandler.RequestModel(ModelHandler.RedPrestoLogProp);

            greenLog = new AnimateProp(Vehicle, ModelHandler.GreenPrestoLogProp, "seat_pside_f");
            greenLog.SpawnProp(greenOffset, logsRotation);

            yellowLog = new AnimateProp(Vehicle, ModelHandler.YellowPrestoLogProp, "seat_pside_f");
            yellowLog.SpawnProp(yellowOffset, logsRotation);

            redLog = new AnimateProp(Vehicle, ModelHandler.RedPrestoLogProp, "seat_pside_f");
            redLog.SpawnProp(redOffset, logsRotation);
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyPress(Keys key)
        {
            
        }

        public override void Process()
        {
            if (Game.IsControlJustPressed(GTA.Control.Sprint))
            {

            }
        }

        public override void Stop()
        {
            redLog?.DeleteProp();
            greenLog?.DeleteProp();
            yellowLog?.DeleteProp();
        }
    }
}
