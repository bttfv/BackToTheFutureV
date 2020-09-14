using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class SIDHandler : Handler
    {

        private Dictionary<int, AnimateProp> sidProps = new Dictionary<int, AnimateProp>();

        public SIDHandler(TimeCircuits circuits) : base(circuits)
        {
            for(int i = 1; i < 89; i++)
            {
                //if(ModelHandler.SIDModels.TryGetValue(i, out Model model))
                //{
                //    ModelHandler.RequestModel(model);
                //    var animProp = new AnimateProp(Vehicle, model, Vector3.Zero, Vector3.Zero);
                //    sidProps.Add(i, animProp);
                //}
            }
        }

        public override void KeyPress(Keys key)
        {
        }

        public override void Process()
        {
            if(Vehicle == null || !Vehicle.IsVisible)
            {
                foreach(var sidProp in sidProps)
                {
                    sidProp.Value.DeleteProp();
                }

                return;
            }

            int speed = (int)MPHSpeed;
            int chosenKey = sidProps.Keys.OrderBy(item => Math.Abs(speed - item)).First();

            foreach(var prop in sidProps)
            {
                if (prop.Key != chosenKey)
                    prop.Value?.DeleteProp();
                else
                    prop.Value?.SpawnProp();
            }
        }

        public override void Stop()
        {
        }

        public override void Dispose()
        {
            foreach (var sidProp in sidProps)
            {
                sidProp.Value.DeleteProp();
            }
        }
    }
}
