using BackToTheFutureV.Story;
using FusionLibrary;
using KlangRageAudioLibrary;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    public class TrainMissionMenu : CustomNativeMenu
    {
        private NativeCheckboxItem MissionToggle;
        private NativeSliderItem Speed;
        private NativeCheckboxItem Mute;
        private NativeSliderItem Volume;
        private NativeCheckboxItem MusicOnly;

        public TrainMissionMenu() : base("", "Train Mission")
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            OnItemCheckboxChanged += TrainMissionMenu_OnItemCheckboxChanged;
            Shown += TrainMissionMenu_Shown;

            Add(MissionToggle = new NativeCheckboxItem("Mission toggle"));
            Add(Speed = new NativeSliderItem("Speed"));
            Speed.ValueChanged += Speed_ValueChanged;
            Add(Volume = new NativeSliderItem("Volume"));
            Add(Mute = new NativeCheckboxItem("Mute"));
            Add(MusicOnly = new NativeCheckboxItem("Music only", false));
            Volume.ValueChanged += MusicVolume_ValueChanged;
        }

        private void MusicVolume_ValueChanged(object sender, EventArgs e)
        {
            MissionHandler.TrainMission.MissionMusic.Volume = Volume.Value / 100.0f;
            Volume.Title = "Music volume: " + Volume.Value.ToString();
        }

        private void TrainMissionMenu_Shown(object sender, EventArgs e)
        {
            if (MissionHandler.TrainMission.MissionMusic == null)
            {
                if (MusicOnly.Checked)
                    MissionHandler.TrainMission.MissionMusic = Main.CommonAudioEngine.Create($"story/trainMission/music.wav", Presets.No3D);
                else
                    MissionHandler.TrainMission.MissionMusic = Main.CommonAudioEngine.Create($"story/trainMission/musicWithVoices.wav", Presets.No3D);

                MissionHandler.TrainMission.MissionMusic.Volume = 0.7f;
            }

            MissionToggle.Checked = MissionHandler.TrainMission.IsPlaying;
            Mute.Checked = MissionHandler.TrainMission.Mute;
            Speed.Value = (int)(MissionHandler.TrainMission.TimeMultiplier * 100);
            Volume.Value = (int)(MissionHandler.TrainMission.MissionMusic.Volume * 100);
        }

        public override void Tick()
        {
            Speed.Enabled = !MissionToggle.Checked;
            MusicOnly.Enabled = !MissionHandler.TrainMission.IsPlaying;
            Volume.Enabled = !Mute.Checked;
        }

        private void Speed_ValueChanged(object sender, EventArgs e)
        {
            if (Speed.Value < 10)
                Speed.Value = 10;

            MissionHandler.TrainMission.TimeMultiplier = Speed.Value / 100.0f;
            Speed.Title = "Speed: " + Speed.Value.ToString();
        }

        private void TrainMissionMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == MissionToggle)
            {
                if (Checked)
                    MissionHandler.TrainMission.Start();
                else
                    MissionHandler.TrainMission.End();
            }

            if (sender == Mute)
            {
                MissionHandler.TrainMission.Mute = Checked;

                if (MissionHandler.TrainMission.IsPlaying)
                {
                    if (Checked)
                    {
                        MissionHandler.TrainMission.OriginalVolume = MissionHandler.TrainMission.MissionMusic.Volume;
                        MissionHandler.TrainMission.MissionMusic.Volume = 0;
                    }
                    else
                    {
                        MissionHandler.TrainMission.MissionMusic.Volume = MissionHandler.TrainMission.OriginalVolume;
                    }

                    Volume.Value = (int)(MissionHandler.TrainMission.MissionMusic.Volume * 100);
                }
            }

            if (sender == MusicOnly)
            {
                MissionHandler.TrainMission.MissionMusic.Dispose();

                if (Checked)
                    MissionHandler.TrainMission.MissionMusic = Main.CommonAudioEngine.Create($"story/trainMission/music.wav", Presets.No3D);
                else
                    MissionHandler.TrainMission.MissionMusic = Main.CommonAudioEngine.Create($"story/trainMission/musicWithVoices.wav", Presets.No3D);
            }
        }
    }
}
