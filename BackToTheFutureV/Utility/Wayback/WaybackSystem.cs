using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using KlangRageAudioLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    [ScriptAttributes(NoDefaultInstance = true)]
    internal class WaybackSystem : Script
    {
        private static readonly List<WaybackMachine> Machines = new List<WaybackMachine>();

        public static WaybackMachine CurrentPlayerRecording => Machines.SingleOrDefault(x => x.Status == WaybackStatus.Recording && x.IsPlayer);

        public static bool Paradox = false;

        private static bool ParadoxText = false;

        public static float paradoxDelay;

        private static float opacityTimer;

        private static int _opacityStep;

        private static readonly AudioPlayer timeParadox = Main.CommonAudioEngine.Create("story/bttf_subtitle2.wav", Presets.No3D);

        static WaybackSystem()
        {
            TimeHandler.OnTimeChanged += (DateTime dateTime) => Stop();
        }

        public static new void Tick()
        {
            if (ParadoxText == true)
            {
                TimeText.DisplayText(Main.ResetDate);
                Game.Player.CanControlCharacter = true;
                ParadoxText = false;
            }

            //GTA.UI.Screen.ShowSubtitle(Machines.Count.ToString());

            if (Paradox == true && FusionUtils.PlayerPed.IsAlive)
            {
                if (FusionUtils.PlayerPed.Model != Main.ResetPed.Model)
                {
                    Game.Player.ChangeModel(Main.ResetPed.Model);
                }
                if (FusionUtils.PlayerPed.Position != Main.ResetPed.Position)
                {
                    FusionUtils.PlayerPed.Position = Main.ResetPed.Position;
                    FusionUtils.PlayerPed.Rotation = Main.ResetPed.Rotation;
                }
                Function.Call(Hash.FORCE_GAME_STATE_PLAYING);
                Function.Call(Hash.SET_PLAYER_INVINCIBLE, FusionUtils.PlayerPed, false);
                Function.Call(Hash.CLEAR_PLAYER_WANTED_LEVEL, FusionUtils.PlayerPed);
                Function.Call(Hash.RESET_PLAYER_ARREST_STATE, FusionUtils.PlayerPed);
                Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, FusionUtils.PlayerPed);
                Function.Call(Hash.CLEAR_PED_WETNESS, FusionUtils.PlayerPed);
                Function.Call(Hash.CLEAR_PED_ENV_DIRT, FusionUtils.PlayerPed);
                Function.Call(Hash.DISPLAY_HUD, true);
                FusionUtils.PlayerPed.SetAlpha(FusionEnums.AlphaLevel.L5);
                FusionUtils.PlayerPed.HealthFloat = Main.ResetPed.Health;
                FusionUtils.PlayerPed.ArmorFloat = Main.ResetPed.Armor;
                FusionUtils.PlayerPed.Money = Main.ResetPed.Money;
                for (int x = 0; x <= 11; x++)
                {
                    Function.Call(Hash.SET_PED_COMPONENT_VARIATION, FusionUtils.PlayerPed, x, Main.ResetPed.Components[x, 0], Main.ResetPed.Components[x, 1], Main.ResetPed.Components[x, 2]);
                }
                for (int x = 0; x <= 12; x++)
                {
                    Function.Call(Hash.SET_PED_PROP_INDEX, FusionUtils.PlayerPed, x, Main.ResetPed.Props[x, 0], Main.ResetPed.Props[x, 1], true);
                }
                foreach (TimeMachine x in TimeMachineHandler.TimeMachines)
                {
                    TimeMachineHandler.RemoveInstantlyTimeMachine(x);
                }
                RemoteTimeMachineHandler.DeleteAll();
                Game.TimeScale = 1.0f;
                TimeHandler.TimeTravelTo(Main.ResetDate);
                timeParadox.SourceEntity = FusionUtils.PlayerPed;
                timeParadox.Volume = 0.2f;
                timeParadox.Play();
                ParadoxText = true;
                if (Main.ResetVehicle != null)
                {
                    Vehicle vehicle = Main.ResetVehicle.Spawn(FusionEnums.SpawnFlags.NoOccupants, Main.ResetVehicle.Position, Main.ResetVehicle.Heading);
                    FusionUtils.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                    if (Main.IsTimeMachine)
                    {
                        TimeMachine timeMachine = FusionUtils.PlayerVehicle.TransformIntoTimeMachine();
                        Main.Mods.ApplyTo(timeMachine);
                        Main.Properties.ApplyTo(timeMachine);
                    }
                }
                _opacityStep = 0;
                Machines.Clear();
                if (Game.Player.WantedLevel != 0)
                {
                    Game.Player.WantedLevel = 0;
                }
                Paradox = false;
            }

            if (Paradox == true && FusionUtils.PlayerPed.IsDead)
            {
                if (Game.GameTime < paradoxDelay)
                {
                    Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "respawn_controller");
                    Function.Call(Hash.IGNORE_NEXT_RESTART, true);
                    Function.Call(Hash.PAUSE_DEATH_ARREST_RESTART, true);
                    Game.TimeScale = 0.4f;
                    Function.Call(Hash.SET_NO_LOADING_SCREEN, true);
                    Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, false);

                    if (Game.GameTime < opacityTimer)
                    {
                        return;
                    }

                    switch (_opacityStep)
                    {
                        case 0:

                            FusionUtils.PlayerPed.SetAlpha(FusionEnums.AlphaLevel.L4);
                            opacityTimer = Game.GameTime + 350;
                            _opacityStep++;
                             break;

                        case 1:

                            FusionUtils.PlayerPed.SetAlpha(FusionEnums.AlphaLevel.L3);
                            opacityTimer = Game.GameTime + 350;
                            _opacityStep++;
                            break;

                        case 2:

                            FusionUtils.PlayerPed.SetAlpha(FusionEnums.AlphaLevel.L2);
                            opacityTimer = Game.GameTime + 350;
                            _opacityStep++;
                            break;

                        case 3:
                            FusionUtils.PlayerPed.SetAlpha(FusionEnums.AlphaLevel.L1);
                            opacityTimer = Game.GameTime + 350;
                            _opacityStep++;
                            break;

                        case 4:
                            FusionUtils.PlayerPed.SetAlpha(FusionEnums.AlphaLevel.L0);
                            opacityTimer = Game.GameTime + 350;
                            break;
                    }

                    return;
                }

                Function.Call(Hash.NETWORK_REQUEST_CONTROL_OF_ENTITY, FusionUtils.PlayerPed);
                Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, Main.ResetPed.Position.X, Main.ResetPed.Position.Y, Main.ResetPed.Position.Z, Main.ResetPed.Heading, false, false);
            }

            if (CurrentPlayerRecording == default)
            {
                if (!TimeMachineHandler.CurrentTimeMachine.IsFunctioning() || (TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase != TimeTravelPhase.InTime && TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase != TimeTravelPhase.Reentering))
                {
                    Create(FusionUtils.PlayerPed, Guid.NewGuid());
                }
            }

            Machines.ForEach(x => x.Tick());
        }

        public static void Stop()
        {
            Machines.ForEach(x => x.Stop());
        }

        public static new void Abort()
        {
            Stop();
            Machines.Clear();
        }

        public static WaybackMachine Create(Ped ped, Guid guid)
        {
            WaybackMachine wayback = new WaybackMachine(ped, guid);

            wayback.Tick();

            if (wayback.Status == WaybackStatus.Recording)
            {
                Machines.Add(wayback);
                return wayback;
            }
            else
            {
                return null;
            }
        }

        public static WaybackMachine GetFromGUID(Guid guid)
        {
            return Machines.SingleOrDefault(x => x.GUID == guid);
        }
    }
}
