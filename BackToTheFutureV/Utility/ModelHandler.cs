using FusionLibrary;
using System.Collections.Generic;

namespace BackToTheFutureV
{
    internal class ModelHandler : CustomModelHandler
    {
        //Common DMC12 CustomModels
        public static CustomModel RPMNeedle = new CustomModel("dmc12_rpm_needle");
        public static CustomModel SpeedNeedle = new CustomModel("dmc12_speed_needle");
        public static CustomModel FuelNeedle = new CustomModel("dmc12_fuel_needle");
        public static CustomModel TemperatureNeedle = new CustomModel("dmc12_temperature_needle");
        public static CustomModel OilNeedle = new CustomModel("dmc12_oil_needle");
        public static CustomModel VoltageNeedle = new CustomModel("dmc12_voltage_needle");
        public static CustomModel DoorIndicator = new CustomModel("dmc12_door_indicator");
        public static CustomModel RadiatorFan = new CustomModel("dmc12_radiator_fan");
        public static CustomModel SuspensionFront = new CustomModel("suspension_lf_prop");
        public static CustomModel SuspensionRear = new CustomModel("suspension_lr_prop");
        public static CustomModel SuspensionRightFront = new CustomModel("suspension_rf_prop");
        public static CustomModel SuspensionRightRear = new CustomModel("suspension_rr_prop");

        // Common BTTF CustomModels
        public static CustomModel DMC12 = new CustomModel("dmc12");
        public static CustomModel BTTFDecals = new CustomModel("bttf_decals");
        public static CustomModel BTTFMrFusion = new CustomModel("bttf_mrfusion");
        public static CustomModel BTTFMrFusionHandle = new CustomModel("bttf_mrfusion_handle");
        public static CustomModel BTTFSpeedo = new CustomModel("bttf_speedo");
        public static CustomModel Empty = new CustomModel("bttf_empty");
        public static CustomModel EmptyOff = new CustomModel("bttf_empty_off");
        public static CustomModel CoilsGlowing = new CustomModel("bttf_coils_glowing");
        public static CustomModel CoilsGlowingNight = new CustomModel("bttf_coils_glowing_night");
        public static CustomModel TFCOn = new CustomModel("bttf_tfc_on");
        public static CustomModel TFCOff = new CustomModel("bttf_tfc_off");
        public static CustomModel TFCHandle = new CustomModel("bttf_handle");
        public static CustomModel LicensePlate = new CustomModel("bttf_outatime_plate");
        public static CustomModel TickingDiodes = new CustomModel("bttf_diodes_ticking");
        public static CustomModel TickingDiodesOff = new CustomModel("bttf_diodes_ticking_off");
        public static CustomModel DiodesOff = new CustomModel("bttf_diodes_off");
        public static CustomModel SparkModel = new CustomModel("bttf_spark_blue");
        public static CustomModel SparkNightModel = new CustomModel("bttf_spark_blue_night");
        public static CustomModel WormholeViolet = new CustomModel("bttf_wormhole_violet");
        public static CustomModel WormholeVioletNight = new CustomModel("bttf_wormhole_violet_night");
        public static CustomModel FluxModel = new CustomModel("bttf_flux");
        public static CustomModel FluxBlueModel = new CustomModel("bttf_flux_blue");
        public static CustomModel FluxOrangeModel = new CustomModel("bttf_flux_orange");
        public static CustomModel GaugeGlow = new CustomModel("bttf_gauges_glow");
        public static List<CustomModel> GaugeModels = new List<CustomModel>();
        public static Dictionary<string, CustomModel> TCDRTModels = new Dictionary<string, CustomModel>();
        public static Dictionary<string, CustomModel> TCDAMModels = new Dictionary<string, CustomModel>();
        public static Dictionary<string, CustomModel> TCDPMModels = new Dictionary<string, CustomModel>();
        public static CustomModel WhiteSphere = new CustomModel("tm_flash");
        public static CustomModel Compass = new CustomModel("bttf_compass");
        public static CustomModel CoilsIndicatorLeft = new CustomModel("indicator_left");
        public static CustomModel CoilsIndicatorRight = new CustomModel("indicator_right");
        public static CustomModel InvisibleProp = new CustomModel("prop_dummy");
        public static List<CustomModel> Lightnings = new List<CustomModel>() { new CustomModel("bolt_m0"), new CustomModel("bolt_m1"), new CustomModel("bolt_m2") };
        public static List<CustomModel> LightningsOnCar = new List<CustomModel>();
        public static CustomModel SID = new CustomModel("bttf_sid");
        public static CustomModel SpeedoCover = new CustomModel("bttf_interior_speedo_cover");

        // BTTF1 CustomModels
        public static CustomModel BTTFReactorCap = new CustomModel("bttf_reactorcap");
        public static CustomModel BulovaClockHour = new CustomModel("clock_hour");
        public static CustomModel BulovaClockMinute = new CustomModel("clock_minute");
        public static CustomModel BulovaClockRing = new CustomModel("clock_ring");

        // BTTF2 CustomModels
        public static CustomModel Strut = new CustomModel("bttf2_strut");
        public static CustomModel Piston = new CustomModel("bttf2_piston");
        public static CustomModel Disk = new CustomModel("bttf2_disc");
        public static CustomModel HoverGlowing = new CustomModel("bttf_hover_glowing");
        public static CustomModel WheelGlowing = new CustomModel("bttf_wheel_glowing");
        public static CustomModel VentGlowing = new CustomModel("bttf_vents_glowing");
        public static CustomModel VentGlowingNight = new CustomModel("bttf_vents_glowing_night");
        public static CustomModel RearWheelGlowing = new CustomModel("bttf_rearwheel_glowing");
        public static CustomModel WheelProp = new CustomModel("bttf_wheelprop");
        public static CustomModel RearWheelProp = new CustomModel("bttf_rearwheelprop");
        public static CustomModel WormholeBlue = new CustomModel("bttf_wormhole_blue");
        public static CustomModel WormholeBlueNight = new CustomModel("bttf_wormhole_blue_night");
        public static List<CustomModel> UnderbodyLights = new List<CustomModel>();

        // BTTF3 Props
        public static List<CustomModel> CoilSeparated = new List<CustomModel>();
        public static CustomModel RedWheelProp = new CustomModel("bttf3_redwheel_prop");
        public static CustomModel RRWheelProp = new CustomModel("wheel_rr_prop");
        public static CustomModel SparkRedModel = new CustomModel("bttf_spark_red");
        public static CustomModel SparkRedNightModel = new CustomModel("bttf_spark_red_night");
        public static CustomModel WormholeRed = new CustomModel("bttf_wormhole_red");
        public static CustomModel WormholeRedNight = new CustomModel("bttf_wormhole_red_night");
        public static CustomModel HoodboxLights = new CustomModel("bttf3_hoodbox_light");
        public static CustomModel HoodboxLightsNight = new CustomModel("bttf3_hoodbox_light_night");

        public static CustomModel DMCDebugModel = new CustomModel("dmc_debug");
        public static CustomModel FreightModel = new CustomModel("freight");
        public static CustomModel FreightCarModel = new CustomModel("freightcar");
        public static CustomModel TankerCarModel = new CustomModel("tankercar");

        public static CustomModel Deluxo = new CustomModel("deluxo");

        private static readonly string[] tcdTypes = new string[3]
        {
            "red",
            "green",
            "yellow"
        };

        public static bool RequestModels()
        {
            for (int i = 0; i <= 10; i++)
            {
                LightningsOnCar.Add(new CustomModel($"bolt_s_{i}"));
            }

            for (int i = 1; i < 6; i++)
            {
                UnderbodyLights.Add(new CustomModel($"bttf_light_{i}"));
            }

            foreach (string strModel in tcdTypes)
            {
                TCDRTModels.Add(strModel, new CustomModel($"bttf_3d_row_{strModel}"));
                TCDAMModels.Add(strModel, new CustomModel($"bttf_{strModel}_am"));
                TCDPMModels.Add(strModel, new CustomModel($"bttf_{strModel}_pm"));
            }

            for (int i = 1; i <= 3; i++)
            {
                GaugeModels.Add(new CustomModel($"bttf_needle{i}"));
            }

            for (int i = 1; i <= 11; i++)
            {
                CoilSeparated.Add(new CustomModel($"bttf3_coils_glowing_{i}"));
            }

            GetAllModels(typeof(ModelHandler)).ForEach(x => x.Request());

            return true;
        }
    }
}
