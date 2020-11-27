using GTA;
using GTA.Math;
using GTA.Native;
using System;

namespace FusionLibrary
{
    public delegate void OnSwitchingComplete();
    public delegate void OnSwitchingStart();

    public class PlayerSwitch
    {
        public static bool IsInProgress => Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS);
        public static bool IsManualInProgress => IsInProgress && PlayerSwitch.IsSwitching;
        public static bool Disable { get; set; } = false;

        public enum SwitchTypes
        {
            SWITCH_TYPE_AUTO,
            SWITCH_TYPE_LONG,
            SWITCH_TYPE_MEDIUM,
            SWITCH_TYPE_SHORT
        };

        public static SwitchTypes SwitchType { get; private set; }
        public static bool IsSwitching { get; private set; }        
        public static Ped To { get; private set; }
        public static bool ForceShort { get; set; }
        public static OnSwitchingComplete OnSwitchingComplete { get; set; }
        public static OnSwitchingStart OnSwitchingStart { get; set; }

        private static int _health;
        private static bool _ragdoll;

        public static void Switch(Ped to, bool forceShort, bool instant = false)
        {
            _health = Function.Call<int>(Hash.GET_ENTITY_HEALTH, to);
            _ragdoll = to.IsRagdoll;

            if (instant)
            {               
                Function.Call(Hash.CHANGE_PLAYER_PED, Game.Player, to, false, false);
                Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character, _health);

                if (_ragdoll)
                    Game.Player.Character.Ragdoll(1);

                OnSwitchingComplete?.Invoke();
                return;
            }

            To = to;
            ForceShort = forceShort;
            IsSwitching = true;

            if(ForceShort)
                SwitchType = SwitchTypes.SWITCH_TYPE_SHORT;
            else
                SwitchType = (SwitchTypes)Function.Call<int>(Hash.GET_IDEAL_PLAYER_SWITCH_TYPE, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, to.Position.X, to.Position.Y, to.Position.Z);
            
            Function.Call(Hash.START_PLAYER_SWITCH, Game.Player.Character, To, 1024, SwitchType);
            Function.Call(Hash.CHANGE_PLAYER_PED, Game.Player, To, false, false);

            OnSwitchingStart?.Invoke();
        }

        public static Ped CreatePedAndSwitch(out Ped originalPed, Vector3 pos, float heading, bool forceShort, bool instant = false)
        {
            originalPed = Game.Player.Character;

            Ped clone = World.CreatePed(Game.Player.Character.Model, pos, heading);

            Function.Call(Hash.CLONE_PED_TO_TARGET, Game.Player.Character, clone);

            Switch(clone, forceShort, instant);

            return clone;
        }

        internal static void Process()
        {
            if (!IsSwitching)
                return;

            if(!Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS))
            {
                Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character, _health);

                if (_ragdoll)
                    Game.Player.Character.Ragdoll(1);

                OnSwitchingComplete?.Invoke();
                IsSwitching = false;
                To = null;
            }
        }
    }
}