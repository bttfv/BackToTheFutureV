using FusionLibrary;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal class TCD3DRowHandler : HandlerPrimitive
    {
        private static readonly Dictionary<string, Vector3> offsets = new Dictionary<string, Vector3>()
        {
            { "red", new Vector3(-0.01477456f, 0.3175744f, 0.6455771f) },
            { "yellow", new Vector3(-0.01964803f, 0.2769623f, 0.565388f) },
            { "green", new Vector3(-0.01737539f, 0.2979541f, 0.6045464f) }
        };
        private static Vector3 rotation = new Vector3(-5.01f, -0.46f, -7.26f);

        public string SlotType { get; private set; }

        private DateTime date;

        public bool IsDoingTimedVisible { get; private set; }
        private bool toggle;
        private int showPropsAt;
        private int showMonthAt;

        private readonly AnimateProp amProp;
        private readonly AnimateProp pmProp;

        public TCD3DRowHandler(string slotType, TimeMachine timeMachine) : base(timeMachine)
        {
            SlotType = slotType;

            if (Mods.IsDMC12)
            {
                Scaleforms.TCDRowsRT[slotType] = new RenderTarget(ModelHandler.TCDRTModels[slotType], "bttf_tcd_row_" + slotType, Vehicle, offsets[slotType], rotation);

                amProp = new AnimateProp(ModelHandler.TCDAMModels[slotType], Vehicle, Vector3.Zero, Vector3.Zero);
                pmProp = new AnimateProp(ModelHandler.TCDPMModels[slotType], Vehicle, Vector3.Zero, Vector3.Zero);
                amProp.SpawnProp();
                amProp.Visible = false;
                pmProp.SpawnProp();
                pmProp.Visible = false;

                Scaleforms.TCDRowsRT[slotType].OnRenderTargetDraw += OnRenderTargetDraw;
            }

            date = new DateTime();

            if (Mods.IsDMC12)
            {
                Events.OnScaleformPriority += OnScaleformPriority;
            }
        }

        private void OnScaleformPriority()
        {
            if (!Mods.IsDMC12)
            {
                return;
            }

            if (Constants.HasScaleformPriority)
            {
                Scaleforms.TCDRowsRT[SlotType]?.CreateProp();
            }
            else
            {
                Scaleforms.TCDRowsRT[SlotType]?.Dispose();
            }
        }

        public void SetDate(DateTime dateToSet)
        {
            if (!TcdEditer.IsEditing)
            {
                ScaleformsHandler.GUI.SetDate(SlotType, dateToSet);
                Properties.HUDProperties.SetDate(SlotType, dateToSet);
            }

            ScaleformsHandler.TCDRowsScaleforms[SlotType]?.SetDate(dateToSet);
            if (Mods.IsDMC12)
            {
                amProp.Visible = dateToSet.ToString("tt", CultureInfo.InvariantCulture) == "AM";
                pmProp.Visible = dateToSet.ToString("tt", CultureInfo.InvariantCulture) != "AM";
            }

            date = dateToSet;
            toggle = true;
        }

        public void SetVisible(bool toggleTo, bool month = true, bool day = true, bool year = true, bool hour = true, bool minute = true, bool amPm = true)
        {
            if (!TcdEditer.IsEditing)
            {
                ScaleformsHandler.GUI.SetVisible(SlotType, toggleTo, month, day, year, hour, minute, amPm);

                if (toggleTo)
                {
                    Properties.HUDProperties.SetDate(SlotType, date);
                }

                Properties.HUDProperties.SetVisible(SlotType, toggleTo, month, day, year, hour, minute, amPm);
            }

            ScaleformsHandler.TCDRowsScaleforms[SlotType]?.SetVisible(toggleTo, month, day, year, hour, minute);

            if (Mods.IsDMC12)
            {
                if ((!toggleTo && amPm) || (toggleTo && !amPm))
                {
                    amProp.Visible = false;
                    pmProp.Visible = false;
                }
                else if ((!toggleTo && !amPm) || (toggleTo && amPm))
                {
                    amProp.Visible = date.ToString("tt", CultureInfo.InvariantCulture) == "AM";
                    pmProp.Visible = date.ToString("tt", CultureInfo.InvariantCulture) != "AM";
                }
            }

            toggle = toggleTo;
        }

        public void SetVisibleAt(bool toggle, int showPropsAt, int showMonthAt)
        {
            this.toggle = toggle;
            this.showPropsAt = Game.GameTime + showPropsAt;
            this.showMonthAt = Game.GameTime + showMonthAt;
            IsDoingTimedVisible = true;
        }

        public override void Dispose()
        {
            if (Mods.IsDMC12)
            {
                Scaleforms.TCDRowsRT[SlotType]?.Dispose();
            }

            amProp?.Delete();
            pmProp?.Delete();
        }

        private void OnRenderTargetDraw()
        {
            ScaleformsHandler.TCDRowsScaleforms[SlotType]?.Render2D(new PointF(0.379f, 0.12f), new SizeF(0.75f, 0.27f));
        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Tick()
        {
            if (Mods.IsDMC12 && (toggle || IsDoingTimedVisible))
            {
                Scaleforms.TCDRowsRT[SlotType]?.Draw();
            }

            if (!IsDoingTimedVisible)
            {
                SetVisible(toggle);

                return;
            }

            if (Game.GameTime > showPropsAt)
            {
                SetVisible(toggle, false);
            }

            if (Game.GameTime > showMonthAt)
            {
                SetVisible(toggle);
            }

            if (Game.GameTime > showPropsAt && Game.GameTime > showMonthAt)
            {
                IsDoingTimedVisible = false;
            }
        }

        public override void Stop()
        {

        }
    }

}
