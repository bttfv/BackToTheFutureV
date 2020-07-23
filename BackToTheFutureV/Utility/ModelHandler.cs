using System;
using System.Collections.Generic;
using System.Linq;
using BackToTheFutureV.Delorean.Handlers;
using GTA;
using GTA.Native;
using GTA.UI;
using NativeUI;

namespace BackToTheFutureV.Utility
{
    public class ModelHandler
    {
        private static Dictionary<Model, int> modelsToBeFreed = new Dictionary<Model, int>();
        private static Dictionary<Model, string> modelsToStrings = new Dictionary<Model, string>();

        // Common BTTF Models
        public static Model DMC12 = new Model("dmc12");
        public static Model BTTFDecals = new Model("bttf_decals");
        public static Model BTTFMrFusion = new Model("bttf_mrfusion");
        public static Model BTTFMrFusionHandle = new Model("bttf_mrfusion_handle");
        public static Model BTTFSpeedo = new Model("bttf_speedo");
        public static Model Empty = new Model("bttf_empty");
        public static Model EmptyOff = new Model("bttf_empty_off");
        public static Model CoilsGlowing = new Model("bttf_coils_glowing");
        public static Model CoilsGlowingNight = new Model("bttf_coils_glowing_night");
        public static Model TFCOn = new Model("bttf_tfc_on");
        public static Model TFCOff = new Model("bttf_tfc_off");
        public static Model TFCHandle = new Model("bttf_handle");
        public static Model LicensePlate = new Model("bttf_licenseplate");
        public static Model TickingDiodes = new Model("bttf_diodes_ticking");
        public static Model TickingDiodesOff = new Model("bttf_diodes_ticking_off");
        public static Model DiodesOff = new Model("bttf_diodes_off");
        public static Model SparkModel = new Model("bttf_spark_blue");
        public static Model SparkNightModel = new Model("bttf_spark_blue_night");
        public static Model WormholeViolet = new Model("bttf_wormhole_violet");
        public static Model WormholeVioletNight = new Model("bttf_wormhole_violet_night");
        public static Model FluxModel = new Model("bttf_flux");
        public static Model GaugeGlow = new Model("bttf_gauges_glow");
        public static Dictionary<int, Model> GaugeModels = new Dictionary<int, Model>();
        public static Dictionary<string, Model> TCDRTModels = new Dictionary<string, Model>();
        public static Model WhiteSphere = new Model("tm_flash");
        public static Model Compass = new Model("bttf_compass");
        public static Model RadiatorFan = new Model("dmc12_radiator_fan");
        public static Model SuspensionFront = new Model("suspension_lf_prop");
        public static Model SuspensionRear = new Model("suspension_lr_prop");
        public static Model SuspensionRightFront = new Model("suspension_rf_prop");
        public static Model SuspensionRightRear = new Model("suspension_rr_prop");
        public static Model CoilsIndicatorLeft = new Model("indicator_left");
        public static Model CoilsIndicatorRight = new Model("indicator_right");
        public static Model InvisibleProp = new Model("prop_dummy");

        // BTTF1 Models
        public static Model BTTFReactorCap = new Model("bttf_reactorcap");

        // BTTF2 Models
        public static Model Strut = new Model("bttf2_strut");
        public static Model Piston = new Model("bttf2_piston");
        public static Model Disk = new Model("bttf2_disc");
        public static Model HoverGlowing = new Model("bttf_hover_glowing");
        public static Model WheelGlowing = new Model("bttf_wheel_glowing");
        public static Model VentGlowing = new Model("bttf_vents_glowing");
        public static Model RearWheelGlowing = new Model("bttf_rearwheel_glowing");
        public static Model WheelProp = new Model("bttf_wheelprop");
        public static Model RearWheelProp = new Model("bttf_rearwheelprop");
        public static Model WormholeBlue = new Model("bttf_wormhole_blue");
        public static Model WormholeBlueNight = new Model("bttf_wormhole_blue_night");
        public static Dictionary<int, Model> UnderbodyLights = new Dictionary<int, Model>();

        // BTTF3 Props
        public static Dictionary<int, Model> CoilSeparated = new Dictionary<int, Model>();
        public static Model RedWheelProp = new Model("bttf3_redwheel_prop");
        public static Model RRWheelProp = new Model("wheel_rr_prop");
        public static Model GreenPrestoLogProp = new Model("presto_log1");
        public static Model YellowPrestoLogProp = new Model("presto_log3");
        public static Model RedPrestoLogProp = new Model("presto_log2");
        public static Model SparkRedModel = new Model("bttf_spark_red");
        public static Model SparkRedNightModel = new Model("bttf_spark_red_night");
        public static Model WormholeRed = new Model("bttf_wormhole_red");
        public static Model WormholeRedNight = new Model("bttf_wormhole_red_night");
        public static Model HoodboxLights = new Model("bttf3_hoodbox_light");
        public static Model HoodboxLightsNight = new Model("bttf3_hoodbox_light_night");

        public static Model DMCDebugModel = new Model("dmc_debug");
        public static Model FreightModel = new Model("freight");
        public static Model FreightCarModel = new Model("freightcar");
        public static Model TankerCarModel = new Model("tankercar");

        public static Model SierraModel = new Model("sierra");
        public static Model SierraTenderModel = new Model("sierratender");
        public static Model SierraDebugModel = new Model("sierra_debug");

        private static string[] tcdTypes = new string[3]
        {
            "red",
            "green",
            "yellow"
        };

        public static Model PreloadModel(Model model)
        {
            LoadingPrompt.Show("Loading: " + GetName(model));

            RequestModel(model);

            return model;
        }

        public static Model RequestModel(Model model)
        {
            if(!model.IsLoaded)
            {
                if (!model.IsInCdImage || !model.IsValid) throw new Exception(GetName(model) + " not present!");

                model.Request();

                while (!model.IsLoaded)
                {
                    Script.Yield();
                }
            }

            return model;
        }

        public static void RequestModels()
        {
            PreloadModel(BTTFDecals);
            PreloadModel(BTTFReactorCap);
            PreloadModel(BTTFMrFusion);
            PreloadModel(BTTFMrFusionHandle);
            PreloadModel(BTTFSpeedo);
            PreloadModel(Empty);
            PreloadModel(EmptyOff);
            PreloadModel(CoilsGlowing);
            PreloadModel(CoilsGlowingNight);
            PreloadModel(HoverGlowing);
            PreloadModel(WheelGlowing);
            PreloadModel(VentGlowing);
            PreloadModel(RearWheelGlowing);
            PreloadModel(WheelProp);
            PreloadModel(RearWheelProp);
            PreloadModel(TFCOn);
            PreloadModel(TFCOff);
            PreloadModel(TFCHandle);
            PreloadModel(Strut);
            PreloadModel(Piston);
            PreloadModel(Disk);
            PreloadModel(SparkModel);
            PreloadModel(WormholeViolet);
            PreloadModel(WormholeVioletNight);
            PreloadModel(FluxModel);
            PreloadModel(TickingDiodes);
            PreloadModel(TickingDiodesOff);
            PreloadModel(DiodesOff);
            PreloadModel(GaugeGlow);
            PreloadModel(WhiteSphere);
            PreloadModel(Compass);
            PreloadModel(RadiatorFan);
            PreloadModel(SuspensionFront);
            PreloadModel(SuspensionRear);
            PreloadModel(SuspensionRightFront);
            PreloadModel(SuspensionRightRear);
            PreloadModel(CoilsIndicatorLeft);
            PreloadModel(CoilsIndicatorRight);
            PreloadModel(HoodboxLights);
            PreloadModel(HoodboxLightsNight);
            PreloadModel(RedWheelProp);
            PreloadModel(RRWheelProp);
            PreloadModel(InvisibleProp);

            PreloadModel(GreenPrestoLogProp);
            PreloadModel(YellowPrestoLogProp);
            PreloadModel(RedPrestoLogProp);
            PreloadModel(DMCDebugModel);
            PreloadModel(FreightModel);
            PreloadModel(FreightCarModel);
            PreloadModel(TankerCarModel);

            PreloadModel(WormholeBlue);
            PreloadModel(WormholeBlueNight);
            PreloadModel(WormholeRed);
            PreloadModel(WormholeRedNight);

            for (int i = 1; i < 6; i++)
            {
                var str = "bttf_light_" + i.ToString();

                var model = new Model(str);

                modelsToStrings.Add(model, str);
                PreloadModel(model);
                UnderbodyLights.Add(i, model);
            }

            foreach(var strModel in tcdTypes)
            {
                var str = "bttf_3d_row_" + strModel;

                var slotModel = new Model(str);
                modelsToStrings.Add(slotModel, str);
                PreloadModel(slotModel);
                TCDRTModels.Add(strModel, slotModel);

                var amModelStr = $"bttf_{strModel}_am";
                var pmModelStr = $"bttf_{strModel}_pm";
                var amModel = new Model(amModelStr);
                var pmModel = new Model(pmModelStr);
                modelsToStrings.Add(amModel, amModelStr);
                modelsToStrings.Add(pmModel, pmModelStr);
                PreloadModel(amModel);
                PreloadModel(pmModel);
            }

            for (int i = 1; i <= 3; i++)
            {
                var modelString = $"bttf_needle{i}";
                var model = new Model(modelString);
                PreloadModel(model);
                modelsToStrings.Add(model, modelString);
                GaugeModels.Add(i, model);
            }

            for (int i = 1; i <= 11; i++)
            {
                var modelStr = $"bttf3_coils_glowing_{i}";
                var model = new Model(modelStr);
                PreloadModel(model);
                modelsToStrings.Add(model, modelStr);
                CoilSeparated.Add(i, model);
            }

            Function.Call(Hash.BUSYSPINNER_OFF);
        }

        public static List<Model> GetAllModels()
        {
            var fields = typeof(ModelHandler).GetFields();
            var models = new List<Model>();

            foreach (var field in fields)
            {
                var obj = field.GetValue(null);
                if (obj.GetType() == typeof(Model))
                {
                    var modelObj = (Model)obj;
                    models.Add(modelObj);
                }
                else if (obj.GetType() == typeof(Dictionary<int, Model>))
                {
                    var dict = (Dictionary<int, Model>)obj;
                    models.AddRange(dict.Values);
                }
            }

            return models;
        }

        public static string GetName(Model model)
        {
            var fields = typeof(ModelHandler).GetFields();

            foreach(var field in fields)
            {
                var obj = field.GetValue(null);
                if(obj.GetType() == typeof(Model))
                {
                    var modelObj = (Model)obj;
                    if (modelObj.Hash == model.Hash)
                        return field.Name;
                }
                else if(obj.GetType() == typeof(Dictionary<int, Model>))
                {
                    if(modelsToStrings.ContainsKey(model))
                        return modelsToStrings[model];
                }
            }

            return "";
        }
    }
}
