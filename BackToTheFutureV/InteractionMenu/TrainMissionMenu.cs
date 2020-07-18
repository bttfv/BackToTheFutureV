using GTA;
using GTA.Math;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.Story;

namespace BackToTheFutureV.InteractionMenu
{
    public class TrainMissionMenu : UIMenu
    {

        public UIMenuCheckboxItem TrainMission { get; }
        public UIMenuCheckboxItem PlayMusic { get; }
        public UIMenuDynamicListItem TimeSpeed { get; }

        private float _speed = 1.0f;

        public TrainMissionMenu() : base("Train Mission", "SELECT AN OPTION")
        {            
            //AddItem(TrainMission = new UIMenuCheckboxItem("Toggle train mission", MissionHandler.TrainMission.IsPlaying, "Start (or stop) train mission."));
            //AddItem(PlayMusic = new UIMenuCheckboxItem("Play music", MissionHandler.TrainMission.PlayMusic, "If true, movie's soundtrack will be played in background."));
            //AddItem(TimeSpeed = new UIMenuDynamicListItem("Speed", "Set speed multipier of the mission. Lower the value, faster the speed and vice versa.", _speed.ToString(), ChangeCallback));
            
            //OnCheckboxChange += TrainMissionMenu_OnCheckboxChange;
            //OnMenuOpen += TrainMissionMenu_OnMenuOpen;
        }

        private void TrainMissionMenu_OnMenuOpen(UIMenu sender)
        {
            //if (Main.ExperimentalFeatures)
            //    TrainMission.Enabled = RogersSierra.Manager.RogersSierra != null;
            TrainMission.Checked = MissionHandler.TrainMission.IsPlaying;
        }

        private void TrainMissionMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (checkboxItem == TrainMission)
            {
                if (Checked)
                    MissionHandler.TrainMission.Start();
                else
                    MissionHandler.TrainMission.End();

                //if (RogersSierra.TrainMission.NoStop)
                //    checkboxItem.Checked = !checkboxItem.Checked;

                //RogersSierra.TrainMission.ToggleMission();                
            }
            else if (checkboxItem == PlayMusic)
            {
                MissionHandler.TrainMission.PlayMusic = Checked;
            }
        }

        private string ChangeCallback(UIMenuDynamicListItem sender, UIMenuDynamicListItem.ChangeDirection direction)
        {
            switch (direction)
            {
                case UIMenuDynamicListItem.ChangeDirection.Left:
                    if(_speed>0.1f)
                        _speed -= 0.1f;
                    break;
                case UIMenuDynamicListItem.ChangeDirection.Right:
                    _speed += 0.1f;
                    break;
            }

            _speed = Convert.ToSingle(Math.Round(_speed, 1));

            MissionHandler.TrainMission.TimeMultiplier = _speed;

            return _speed.ToString();
        }
    }
}
