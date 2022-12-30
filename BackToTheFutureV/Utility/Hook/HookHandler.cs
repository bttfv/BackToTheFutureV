using GTA;
using MinHook;
using System;
using System.Runtime.InteropServices;

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
        private static IntPtr handling_GetSubHandling;

        private static CSpecialFlightHandlingData m_CustomFlightData = new CSpecialFlightHandlingData();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        delegate long GetSubHandlingByTypeDelegate(long inst, int type);
        static GetSubHandlingByTypeDelegate GetSubHandlingByType_Original;

        unsafe static long GetSubHandlingByType_Hook(long inst, int type)
        {
            if (type == 10) // rage::par::SUB_HANDLING_SPECIAL_FLIGHT
            {
                fixed (CSpecialFlightHandlingData* i = &m_CustomFlightData) { return (long)i; }
            }

            return GetSubHandlingByType_Original(inst, type);
        }

        public static void Setup()
        {
            deluxo_1 = Game.FindPattern("48 8B C4 48 89 58 08 48 89 68 10 48 89 70 18 48 89 78 20 41 56 48 83 EC 40 0F 29 70 E8 48 8B 01 4D 8B F1 0F 28 F2 8B EA 48 8B F9 FF 50 ?? 33 DB 48 85 C0 74 18 48 8B 48 68 48 85 C9 74 1E 48 39 58 78 74 18 48 8B 89 78 01 00 00 EB 12 48 8B 4F 50 48 85 C9 74 06 48 8B 49 28 EB 03 48 8B CB 48 63 C5 48 8D 34 80 48 8B 01 48 C1 E6 04 48 03 70 20 0F 84 F1");
            deluxo_2 = Game.FindPattern("48 83 EC 48 83 FA");
            deluxo_3 = Game.FindPattern("48 8B C4 48 89 58 08 48 89 68 10 48 89 70 18 48 89 78 20 41 56 48 81 EC C0 00 00 00 0F 29 70 E8 0F 29 78 D8 44 0F 29 40 C8 44 0F 29 48 B8 44 0F 29 50 A8 44 0F 29 58 98 44 0F 29 60 88 44 0F 29 6C 24 40 66");

            handling_GetSubHandling = Game.FindPattern("E8 ?? ?? ?? ?? 66 3B 70 48");

            handling_GetSubHandling = handling_GetSubHandling + 0x1;
            unsafe
            {
                handling_GetSubHandling = handling_GetSubHandling + 4 + *(int*)handling_GetSubHandling;
            }

            // From handling.meta
            m_CustomFlightData.mode = 1;
            m_CustomFlightData.fLiftCoefficient = 50.0f;
            m_CustomFlightData.fMinLiftVelocity = 0.0f;
            m_CustomFlightData.fDragCoefficient = 0.0f;
            m_CustomFlightData.fMaxPitchTorque = 500.0f;
            m_CustomFlightData.fMaxSteeringRollTorque = 50.0f;
            m_CustomFlightData.fMaxThrust = 20.0f;
            m_CustomFlightData.fYawTorqueScale = -4.0f;
            m_CustomFlightData.fRollTorqueScale = 7.5f;
            m_CustomFlightData.fTransitionDuration = 1.0f;
            m_CustomFlightData.fPitchTorqueScale = 8.0f;
            m_CustomFlightData.vecAngularDamping = new Vec3V { x = 3.0f, y = 2.0f, z = 1.2f, w = 3.0f }; // W component repeats X
            m_CustomFlightData.vecLinearDamping = new Vec3V { x = 0.9f, y = 0.1f, z = 0.7f, w = 0.9f }; // W component repeats X

            // From in game (defaults)
            m_CustomFlightData.fCriticalLiftAngle = 45.0f;
            m_CustomFlightData.fInitialLiftAngle = 1.5f;
            m_CustomFlightData.fMaxLiftAngle = 25.0f;
            m_CustomFlightData.fBrakingDrag = 10.0f;
            m_CustomFlightData.fMaxLiftVelocity = 2000.0f;
            m_CustomFlightData.fRollTorqueScale = 7.5f;
            m_CustomFlightData.fMaxTorqueVelocity = 100.0f;
            m_CustomFlightData.fMinTorqueVelocity = 40000.0f;
            m_CustomFlightData.fYawTorqueScale = -4.0f;
            m_CustomFlightData.fSelfLevelingPitchTorqueScale = -5.0f;
            m_CustomFlightData.fInitalOverheadAssist = -5.0f;
            m_CustomFlightData.fSteeringTorqueScale = 1000.0f;
            m_CustomFlightData.fMaxThrust = 20.0f;
            m_CustomFlightData.fHoverVelocityScale = 1.0f;
            m_CustomFlightData.fStabilityAssist = 10.0f;
            m_CustomFlightData.fMinSpeedForThrustFalloff = 0.0f;
            m_CustomFlightData.fBrakingThrustScale = 0.0f;

            Engine.CreateHook(deluxo_1, new DeluxoNullsubDelegate1(DeluxoNullsub1));
            Engine.CreateHook(deluxo_2, new DeluxoNullsubDelegate2(DeluxoNullsub2));
            //Engine.CreateHook(deluxo_3, new DeluxoNullsubDelegate3(DeluxoNullsub3));
            Engine.CreateHook(handling_GetSubHandling, new GetSubHandlingByTypeDelegate(GetSubHandlingByType_Hook));

            Engine.EnableHooks();
        }

        public static void Abort()
        {
            Engine.DisableHooks();
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x10)]
        public struct Vec3V
        {
            public float x, y, z, w;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0xC0)]
        public struct CSpecialFlightHandlingData
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] pad0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] pad1;
            public Vec3V vecAngularDamping;
            public Vec3V vecAngularDampingMin;
            public Vec3V vecLinearDamping;
            public Vec3V vecLinearDampingMin;
            public float fLiftCoefficient;
            public float fCriticalLiftAngle;
            public float fInitialLiftAngle;
            public float fMaxLiftAngle;
            public float fDragCoefficient;
            public float fBrakingDrag;
            public float fMaxLiftVelocity;
            public float fMinLiftVelocity;
            public float fRollTorqueScale;
            public float fMaxTorqueVelocity;
            public float fMinTorqueVelocity;
            public float fYawTorqueScale;
            public float fSelfLevelingPitchTorqueScale;
            public float fInitalOverheadAssist;
            public float fMaxPitchTorque;
            public float fMaxSteeringRollTorque;
            public float fPitchTorqueScale;
            public float fSteeringTorqueScale;
            public float fMaxThrust;
            public float fTransitionDuration;
            public float fHoverVelocityScale;
            public float fStabilityAssist;
            public float fMinSpeedForThrustFalloff;
            public float fBrakingThrustScale;
            public int mode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] strFlags; // TODO: Actual AtFinalHashString
        }
    }
}
