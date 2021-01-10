using FusionLibrary;
using System.Collections.Generic;

namespace BackToTheFutureV.Utility
{
    public class ModelHandler : CustomModelHandler
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
        //public static CustomModel LicensePlate = new CustomModel("bttf_licenseplate");
        public static CustomModel TickingDiodes = new CustomModel("bttf_diodes_ticking");
        public static CustomModel TickingDiodesOff = new CustomModel("bttf_diodes_ticking_off");
        public static CustomModel DiodesOff = new CustomModel("bttf_diodes_off");
        public static CustomModel SparkModel = new CustomModel("bttf_spark_blue");
        public static CustomModel SparkNightModel = new CustomModel("bttf_spark_blue_night");
        public static CustomModel WormholeViolet = new CustomModel("bttf_wormhole_violet");
        public static CustomModel WormholeVioletNight = new CustomModel("bttf_wormhole_violet_night");
        public static CustomModel FluxModel = new CustomModel("bttf_flux");
        public static CustomModel GaugeGlow = new CustomModel("bttf_gauges_glow");
        public static Dictionary<int, CustomModel> GaugeModels = new Dictionary<int, CustomModel>();
        public static Dictionary<string, CustomModel> TCDRTModels = new Dictionary<string, CustomModel>();
        public static CustomModel WhiteSphere = new CustomModel("tm_flash");
        public static CustomModel Compass = new CustomModel("bttf_compass");        
        public static CustomModel CoilsIndicatorLeft = new CustomModel("indicator_left");
        public static CustomModel CoilsIndicatorRight = new CustomModel("indicator_right");
        public static CustomModel InvisibleProp = new CustomModel("prop_dummy");
        public static List<CustomModel> Lightnings = new List<CustomModel>() { new CustomModel("ls_1"), new CustomModel("ls_2"), new CustomModel("ls_3"), new CustomModel("ls_4") };

        // BTTF1 CustomModels
        public static CustomModel BTTFReactorCap = new CustomModel("bttf_reactorcap");

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
        public static Dictionary<int, CustomModel> UnderbodyLights = new Dictionary<int, CustomModel>();

        // BTTF3 Props
        public static Dictionary<int, CustomModel> CoilSeparated = new Dictionary<int, CustomModel>();
        public static CustomModel RedWheelProp = new CustomModel("bttf3_redwheel_prop");
        public static CustomModel RRWheelProp = new CustomModel("wheel_rr_prop");
        public static CustomModel GreenPrestoLogProp = new CustomModel("presto_log1");
        public static CustomModel YellowPrestoLogProp = new CustomModel("presto_log3");
        public static CustomModel RedPrestoLogProp = new CustomModel("presto_log2");
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

        public static CustomModel SierraModel = new CustomModel("sierra");
        public static CustomModel SierraTenderModel = new CustomModel("sierratender");
        public static CustomModel SierraDebugModel = new CustomModel("sierra_debug");

        public static CustomModel DeluxoModel = new CustomModel("DELUXO");

        private static string[] tcdTypes = new string[3]
        {
            "red",
            "green",
            "yellow"
        };

        public static void RequestModels()
        {
            GetAllModels(typeof(ModelHandler)).ForEach(x => PreloadModel(x));

            foreach (var x in Lightnings)
                PreloadModel(x);

            for (int i = 1; i < 6; i++)
            {
                var str = "bttf_light_" + i.ToString();

                var model = new CustomModel(str);

                PreloadModel(model);
                UnderbodyLights.Add(i, model);
            }

            foreach(var strModel in tcdTypes)
            {
                var str = "bttf_3d_row_" + strModel;

                var slotModel = new CustomModel(str);
                
                PreloadModel(slotModel);
                TCDRTModels.Add(strModel, slotModel);

                var amModelStr = $"bttf_{strModel}_am";
                var pmModelStr = $"bttf_{strModel}_pm";
                var amModel = new CustomModel(amModelStr);
                var pmModel = new CustomModel(pmModelStr);

                PreloadModel(amModel);
                PreloadModel(pmModel);
            }

            for (int i = 1; i <= 3; i++)
            {
                var modelString = $"bttf_needle{i}";
                var model = new CustomModel(modelString);
                PreloadModel(model);
                
                GaugeModels.Add(i, model);
            }

            for (int i = 1; i <= 11; i++)
            {
                var modelStr = $"bttf3_coils_glowing_{i}";
                var model = new CustomModel(modelStr);
                PreloadModel(model);
                
                CoilSeparated.Add(i, model);
            }
        }
    }
}
