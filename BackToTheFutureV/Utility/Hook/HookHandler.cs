using GTA;
using MinHook;
using System;

namespace BackToTheFutureV
{
    internal static class HookHandler
    {
        public static HookEngine Engine { get; } = new HookEngine();

        delegate void DeluxoNullsubDelegate1(long vehicle, uint a2, float a3, float a4, float a5, float a6);
        delegate void DeluxoNullsubDelegate2(long vehicle, long a2, float a3, float a4, int a5, float a6);
        delegate float DeluxoNullsubDelegate3(long vehicle, long a2, float a3, float a4, float a5, float a6, float a7, float a8, uint a9, char a10);

        static void DeluxoNullsub1(long vehicle, uint a2, float a3, float a4, float a5, float a6) { }
        static void DeluxoNullsub2(long vehicle, long a2, float a3, float a4, int a5, float a6) { }
        static float DeluxoNullsub3(long vehicle, long a2, float a3, float a4, float a5, float a6, float a7, float a8, uint a9, char a10) { return 1f; }

        private static IntPtr deluxo_1;
        private static IntPtr deluxo_2;
        private static IntPtr deluxo_3;

        public static void Setup()
        {
            deluxo_1 = Game.FindPattern("48 8B C4 48 89 58 08 48 89 68 10 48 89 70 18 48 89 78 20 41 56 48 83 EC 40 0F 29 70 E8 48 8B 01 4D 8B F1 0F 28 F2 8B EA 48 8B F9 FF 50 ?? 33 DB 48 85 C0 74 18 48 8B 48 68 48 85 C9 74 1E 48 39 58 78 74 18 48 8B 89 78 01 00 00 EB 12 48 8B 4F 50 48 85 C9 74 06 48 8B 49 28 EB 03 48 8B CB 48 63 C5 48 8D 34 80 48 8B 01 48 C1 E6 04 48 03 70 20 0F 84 F1");
            deluxo_2 = Game.FindPattern("48 83 EC 48 83 FA");
            deluxo_3 = Game.FindPattern("48 8B C4 48 89 58 08 48 89 68 10 48 89 70 18 48 89 78 20 41 56 48 81 EC C0 00 00 00 0F 29 70 E8 0F 29 78 D8 44 0F 29 40 C8 44 0F 29 48 B8 44 0F 29 50 A8 44 0F 29 58 98 44 0F 29 60 88 44 0F 29 6C 24 40 66");

            Engine.CreateHook(deluxo_1, new DeluxoNullsubDelegate1(DeluxoNullsub1));
            Engine.CreateHook(deluxo_2, new DeluxoNullsubDelegate2(DeluxoNullsub2));
            Engine.CreateHook(deluxo_3, new DeluxoNullsubDelegate3(DeluxoNullsub3));

            Engine.EnableHooks();
        }

        public static void Abort()
        {
            Engine.DisableHooks();
        }
    }
}
