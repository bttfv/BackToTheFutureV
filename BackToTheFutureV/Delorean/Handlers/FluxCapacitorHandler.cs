using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class FluxCapacitorHandler : Handler
    {
        public FluxCapacitorHandler(TimeCircuits circuits) : base(circuits)
        {
            _fluxScaleform = new ScaleformGui("bttf_flux_scaleform");
            _fluxRenderTarget = new RenderTarget(ModelHandler.FluxModel, "bttf_flux", Vehicle, "flux_capacitor");
            _fluxScaleform.DrawInPauseMenu = true;

            _fluxRenderTarget.OnRenderTargetDraw += () =>
            {
                _fluxScaleform.Render2D(new PointF(0.5f, 0.5f), 1f);
            };
            _fluxRenderTarget.CreateProp();

            TimeCircuits.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            TimeCircuits.OnScaleformPriority += OnScaleformPriority;
        }

        private ScaleformGui _fluxScaleform;
        private RenderTarget _fluxRenderTarget;

        public void StartTimeTravelEffect()
        {
            _fluxScaleform.CallFunction("START_BLUE_ANIMATION");
        }

        public void StartNormalFluxing()
        {
            _fluxScaleform.CallFunction("START_ANIMATION");
        }

        private void OnScaleformPriority()
        {
            Update();
        }

        private void OnTimeCircuitsToggle()
        {
            if (TimeCircuits.Delorean.IsGivenScaleformPriority)
                Update();
        }

        public override void Process()
        {
            if(!Vehicle.IsVisible)
                return;

            if(IsOn && TimeCircuits.Delorean.IsGivenScaleformPriority)
                _fluxRenderTarget.Draw();
        }

        public void Update()
        {
            if (!IsOn)
            {
                _fluxScaleform.CallFunction("STOP_ANIMATION");
            }
            else
            {
                _fluxScaleform.CallFunction("START_ANIMATION");
            }
        }

        public override void Stop()
        {
        }

        public override void Dispose()
        {
            _fluxRenderTarget?.Dispose();
        }

        public override void KeyPress(Keys key)
        {
        }
    }
}
