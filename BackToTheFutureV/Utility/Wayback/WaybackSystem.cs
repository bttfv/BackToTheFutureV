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
    internal class WaybackSystem
    {
        private static readonly List<WaybackMachine> Machines = new List<WaybackMachine>();

        public static WaybackMachine CurrentPlayerRecording => Machines.SingleOrDefault(x => x.Status == WaybackStatus.Recording && x.IsPlayer);

        public static bool Paradox = false;

        private static bool ParadoxText = false;

        private static int textAlpha1 = 0;

        private static int textAlpha2 = 0;

        private static int textAlpha3 = 0;

        public static float paradoxDelay;

        private static float opacityTimer;

        private static int _opacityStep;

        private static readonly AudioPlayer timeParadox = Main.CommonAudioEngine.Create("story/bttf_subtitle2.wav", Presets.No3D);

        static WaybackSystem()
        {
            TimeHandler.OnTimeChanged += (DateTime dateTime) => Stop();
        }

        public static void Tick()
        {
            if (!ModSettings.WaybackSystem)
            {
                return;
            }

            if (ParadoxText == true)
            {
                Function.Call(Hash.SETTIMERA, 0);
                while (true)
                {
                    Function.Call(Hash.HIDE_LOADING_ON_FADE_THIS_FRAME);
                    Function.Call(Hash.SET_TEXT_SCALE, 1.25f, 1.25f); // scale, size
                    var fVar1 = Function.Call<float>(Hash.GET_RENDERED_CHARACTER_HEIGHT, 1.25f, 0);
                    if (textAlpha1 * 8 < 252)
                    {
                        Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, textAlpha1 * 8);
                        textAlpha1++;
                    }
                    else
                    {
                        textAlpha1 = 63;
                        Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
                    }
                    Function.Call(Hash.SET_TEXT_CENTRE, true);
                    Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 7);
                    Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING"); // draw crap on screen
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, $"{Main.ResetDate:dddd}");
                    Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, 0.62f - fVar1 / 2, 0);
                    // -1f to 1f.
                    // X - Horizontal
                    // Y - Vertical
                    Script.Wait(1);
                    if (Function.Call<int>(Hash.TIMERA) > 1500)
                    {
                        Function.Call(Hash.SET_TEXT_SCALE, 1.25f, 1.25f); // scale, size
                        if (textAlpha2 * 8 < 252)
                        {
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, textAlpha2 * 8);
                            textAlpha2++;
                        }
                        else
                        {
                            textAlpha2 = 63;
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
                        }
                        Function.Call(Hash.SET_TEXT_CENTRE, true);
                        Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 7);
                        Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING"); // draw crap on screen
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, $"\n{Main.ResetDate:MMMM d, yyyy}");
                        Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, 0.62f - fVar1 / 2, 0);
                    }
                    if (Function.Call<int>(Hash.TIMERA) > 3000)
                    {
                        Function.Call(Hash.SET_TEXT_SCALE, 1.25f, 1.25f); // scale, size
                        if (textAlpha3 * 8 < 252)
                        {
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, textAlpha3 * 8);
                            textAlpha3++;
                        }
                        else
                        {
                            textAlpha3 = 63;
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
                        }
                        Function.Call(Hash.SET_TEXT_CENTRE, true);
                        Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 7);
                        Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING"); // draw crap on screen
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, $"\n\n{Main.ResetDate:hh:mm tt}");
                        Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, 0.62f - fVar1 / 2, 0);
                    }
                    if (Function.Call<int>(Hash.TIMERA) > 6000)
                    {
                        break;
                    }
                }
                while (true)
                {
                    Function.Call(Hash.HIDE_LOADING_ON_FADE_THIS_FRAME);
                    Function.Call(Hash.SET_TEXT_SCALE, 1.25f, 1.25f); // scale, size
                    var fVar1 = Function.Call<float>(Hash.GET_RENDERED_CHARACTER_HEIGHT, 1.25f, 0);
                    if (textAlpha1 * 4 > 0)
                    {
                        Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, textAlpha1 * 4);
                        textAlpha1--;
                    }
                    else
                    {
                        Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 0);
                        textAlpha1 = 0;
                        textAlpha2 = 0;
                        textAlpha3 = 0;
                    }
                    Function.Call(Hash.SET_TEXT_CENTRE, true);
                    Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 7);
                    Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING"); // draw crap on screen
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, $"{Main.ResetDate:dddd}\n{Main.ResetDate:MMMM d, yyyy}\n{Main.ResetDate:hh:mm tt}");
                    Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, 0.62f - fVar1 / 2, 0);
                    // -1f to 1f.
                    // X - Horizontal
                    // Y - Vertical
                    Script.Wait(1);
                    if (Function.Call<int>(Hash.TIMERA) > 8000)
                    {
                        break;
                    }
                }
                Function.Call(Hash.RESET_SCRIPT_GFX_ALIGN);
                Function.Call(Hash.HIDE_LOADING_ON_FADE_THIS_FRAME);
                if (GTA.UI.Screen.IsFadedOut && !GTA.UI.Screen.IsFadingIn)
                {
                    GTA.UI.Screen.FadeIn(1000);
                }
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
                    vehicle.Mods.LicensePlate = Main.ResetVehiclePlate;
                    vehicle.Mods.LicensePlateStyle = Main.ResetVehiclePlateStyle;
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

        public static void Abort()
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
