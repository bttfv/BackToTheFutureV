using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
//using IrrKlang;
using BackToTheFutureV.Memory;
using GTA.UI;
//using NativeUI;
using System.Drawing;
using BackToTheFutureV.Vehicles;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace BackToTheFutureV.Utility
{   
    public enum LightsMode
    {
        Default,
        Disabled,
        AlwaysOn
    }

    public enum MapArea
    {
        County = 2072609373,
        City = -289320599
    }
    public enum WheelId
    {
        FrontLeft = 0,
        FrontRight = 1,
        RearLeft = 4,
        RearRight = 5
    }

    public enum Coordinate
    {
        X,
        Y,
        Z
    }

    public enum MissionType
    {
        None,
        Escape,
        Train
    }

    [Flags]
    public enum SpawnFlags 
    {
        Default = 0,
        WarpPlayer = 1,
        ForcePosition = 2,
        ResetValues = 4,
        Broken = 8,
        ForceReentry = 16,
        CheckExists = 32,
        NoOccupants = 64,
        NoVelocity = 128,        
    }

    public enum CameraType
    {
        DestinationDate,
        DriverSeat,
        DigitalSpeedo,
        AnalogSpeedo,
        FrontPassengerWheelLookAtRear,
        TrainApproaching
    }

    public class BTTFImportantDates
    {
        public readonly static List<DateTime> Dates = new List<DateTime>() { new DateTime(1985, 10, 26, 1, 21, 0), new DateTime(1885, 1, 1, 0, 0, 0), new DateTime(1955, 11, 5, 6, 15, 0), new DateTime(1985, 10, 26, 1, 24, 0), new DateTime(1985, 10, 26, 1, 24, 0), new DateTime(2015, 10, 21, 16, 29, 0), new DateTime(1955, 11, 12, 13, 40, 0), new DateTime(1985, 10, 26, 21, 0, 0), new DateTime(1955, 11, 12, 6, 0, 0), new DateTime(1885, 9, 2, 8, 0, 0), new DateTime(1985, 10, 27, 11, 0, 0) };

        public static DateTime GetRandom()
        {
            return Dates[Utils.Random.Next(0, Dates.Count - 1)];
        }
    }

    public class Utils
    {
        static readonly float radToDeg = (float)(180.0 / Math.PI);
        static readonly float degToRad = (float)(Math.PI / 180.0);

        public static Random Random = new Random(DateTime.Now.Millisecond);

        public static readonly string[] WheelNames = new string[4]
        {
            "wheel_lf",
            "wheel_lr",
            "wheel_rr",
            "wheel_rf"
        };

        private static readonly Weather[] validWeatherTypes = new Weather[]
        {
            Weather.Clear,
            Weather.Clearing,
            Weather.Clouds,
            Weather.ExtraSunny,
            Weather.Foggy,
            Weather.Overcast,
            Weather.Raining,
            Weather.ThunderStorm
        };

        //private static readonly Dictionary<string, Vector3> wheelNames = new Dictionary<string, Vector3>()
        //{
        //    {
        //        "wheel_lf",
        //        new Vector3(-0.7996449f, 1.271523f, 0.124941f)
        //    },
        //    {
        //        "wheel_lr",
        //        new Vector3(-0.805218f, -1.201979f, 0.1590068f)
        //    },
        //    {
        //        "wheel_rr",
        //        new Vector3(0.7984101f, -1.201924f, 0.1596001f)
        //    },
        //    {
        //        "wheel_rf",
        //        new Vector3(0.8009894f, 1.275887f, 0.1288272f)
        //    }
        //};

        private static readonly VehicleHash[] cityVehicles =
        {
            VehicleHash.Mule,
            VehicleHash.Mule2,
            VehicleHash.Mule3,
            VehicleHash.Blista,
            VehicleHash.Blista2,
            VehicleHash.Brioso,
            VehicleHash.Dilettante,
            VehicleHash.Issi2,
            VehicleHash.Prairie,
            VehicleHash.Rhapsody,
            VehicleHash.Exemplar,
            VehicleHash.Felon,
            VehicleHash.Sentinel,
            VehicleHash.Sentinel2,
            VehicleHash.Zion,
            VehicleHash.Zion2,
            VehicleHash.Ambulance,
            VehicleHash.Police2,
            VehicleHash.Police3,
            VehicleHash.Blade,
            VehicleHash.Dominator,
            VehicleHash.Ruiner,
            VehicleHash.Coach,
            VehicleHash.Airbus,
            VehicleHash.Bus,
            VehicleHash.Tailgater,
            VehicleHash.Warrener,
            VehicleHash.Washington,
            VehicleHash.Stratum,
            VehicleHash.Romero,
        };

        private static readonly VehicleHash[] countryVehicles =
        {
            VehicleHash.Rebel,
            VehicleHash.Rebel2,
            VehicleHash.Mesa,
            VehicleHash.Dune,
            VehicleHash.BfInjection,
            VehicleHash.Police,
            VehicleHash.Police4,
            VehicleHash.Habanero,
            VehicleHash.Rocoto,
            VehicleHash.Asterope,
            VehicleHash.Emperor2,
            VehicleHash.Glendale,
            VehicleHash.Regina,
            VehicleHash.Peyote,
            VehicleHash.Tornado,
            VehicleHash.Tornado3,
            VehicleHash.Tornado4,
            VehicleHash.TowTruck2,
            VehicleHash.Scrap,
            VehicleHash.Surfer2,
            VehicleHash.Surfer,
        };

        //public static UIMenuItem AttachSubmenu(UIMenu menu, UIMenu menuToBind, string buttonName, string buttonDescription)
        //{
        //    UIMenuItem item = new UIMenuItem(buttonName, buttonDescription);
        //    menu.AddItem(item);
        //    menu.BindMenuToItem(menuToBind, item);

        //    return item;
        //}

        public static void LiftUpWheel(Vehicle vehicle, WheelId id, float height)
        {
            //_SET_HYDRAULIC_STATE
            Function.Call((Hash)0x84EA99C62CB3EF0C, vehicle, id, height);
        }

        public static DateTime? ParseFromRawString(string raw, DateTime currentTime)
        {
            try
            {
                if (raw.Length == 12)
                {
                    var month = raw.Substring(0, 2);
                    var day = raw.Substring(2, 2);
                    var year = raw.Substring(4, 4);
                    var hour = raw.Substring(8, 2);
                    var minute = raw.Substring(10, 2);

                    return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), 0);
                }
                else if(raw.Length == 8)
                {
                    var month = raw.Substring(0, 2);
                    var day = raw.Substring(2, 2);
                    var year = raw.Substring(4, 4);

                    return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), currentTime.Hour, currentTime.Minute, 0);
                }
                else if(raw.Length == 4)
                {
                    var hour = raw.Substring(0, 2);
                    var minute = raw.Substring(2, 2);

                    return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, int.Parse(hour), int.Parse(minute), 0);
                }

                return null;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public static Model GetRandomVehicleHash(Model model, Vector3 position)
        {
            var areaType = (MapArea)Function.Call<int>(Hash.GET_HASH_OF_MAP_AREA_AT_COORDS, position.X, position.Y, position.Z);
            switch (areaType)
            {
                case MapArea.City:
                    return GetRandomVehicle(cityVehicles, model);
                case MapArea.County:
                    return GetRandomVehicle(countryVehicles, model);
            }
            return VehicleHash.Premier;
        }

        private static Model GetRandomVehicle(VehicleHash[] array, Model model, float maxDiff = 0.6f)
        {
            var validVehicles = array.Where(x => Difference(x, model) < 0.6f).ToArray();

            if(validVehicles == null || validVehicles.Length <= 0)
            {
                return VehicleHash.Premier;
            }

            return validVehicles[Random.Next(validVehicles.Length)];
        }

        public static float Difference(Model model, Model otherModel)
        {
            return Math.Abs(GetRadiusOfModel(model) - GetRadiusOfModel(otherModel)); 
        }

        public static float GetRadiusOfModel(Model model)
        {
            // Get the dimensions
            (Vector3 min, Vector3 max) = model.Dimensions;

            // Return the output
            return (max - min).Length() / 2;
        }

        public static Vehicle ReplaceVehicle(Vehicle vehicle, Model newHash, bool markAsNoLongerNeeded = true)
        {
            // Get the ped out of the vehicle
            Ped driver = vehicle.Driver;
            driver?.Task.WarpOutOfVehicle(vehicle);

            // Get the info for the original vehicle
            VehicleInfo info = new VehicleInfo(vehicle);
            info.Model = newHash;

            // Delete original vehicle
            vehicle.DeleteCompletely();

            // Spawn the new vehicle
            Vehicle spawnedVehicle = SpawnFromVehicleInfo(info);
            
            // This driving style means: (https://gtaforums.com/topic/822314-guide-driving-styles/)
            // 1 = Stop Before Vehicles
            // 2 = Stop Before Peds
            // 8 = Avoid Empty Vehicles
            // 16 = Avoid Peds
            // 32 = Avoid Objects
            // 128 = Stop at Traffic Lights
            // 256 = Use blinkers
            int drivingStyle = 1 + 2 + 8 + 16 + 32 + 128 + 256;

            // Set the ped inside the vehicle
            driver?.SetIntoVehicle(spawnedVehicle, VehicleSeat.Driver);
            driver?.Task.CruiseWithVehicle(spawnedVehicle, 20, (DrivingStyle)drivingStyle);

            // Mark the vehicle as no longer needed to save memory
            if(markAsNoLongerNeeded)
            {
                spawnedVehicle.MarkAsNoLongerNeeded();
                spawnedVehicle.Model.MarkAsNoLongerNeeded();
            }

            // Return the spawned vehicle
            return spawnedVehicle;
        }

        public static Vehicle SpawnFromVehicleInfo(VehicleInfo vehicleInfo)
        {
            ModelHandler.RequestModel(vehicleInfo.Model);

            Vehicle vehicle = World.CreateVehicle(vehicleInfo.Model, vehicleInfo.Position);

            vehicleInfo.ApplyTo(vehicle);

            return vehicle;
        }

        public static void DrawLine(Vector3 from, Vector3 to, Color col)
        {
            Function.Call(Hash.DRAW_LINE, from.X, from.Y, from.Z, to.X, to.Y, to.Z, col.R, col.G, col.B, col.A);
        }

        public static Vector3 GetPositionOnGround(Vector3 position, float verticalOffset)
        {
            float result = -1;

            position.Z += 2.5f;
            unsafe
            {
                Function.Call(Hash.GET_GROUND_Z_FOR_3D_COORD, position.X, position.Y, position.Z, &result, false);
            }
            position.Z = result + verticalOffset;

            return position;
        }

        public static WheelType GetVariantWheelType(WheelType wheelType)
        {
            switch (wheelType)
            {
                case WheelType.Stock:
                    return WheelType.StockInvisible;
                case WheelType.StockInvisible:
                    return WheelType.Stock;
                case WheelType.Red:
                    return WheelType.RedInvisible;
                case WheelType.RedInvisible:
                    return WheelType.Red;
                default:
                    return WheelType.Stock;
            }
        }

        public static bool IsNight()
        {
            return Main.CurrentTime.Hour >= 20 || (Main.CurrentTime.Hour >= 0 && Main.CurrentTime.Hour <= 5);
        }

        public static Weather GetRandomWeather()
        {
            var num = Random.Next(0, validWeatherTypes.Length);

            return validWeatherTypes[num];
        }

        public static bool IsCameraValid(Camera cam)
        {
            return cam != null && cam.Position != Vector3.Zero;
        }

        public static DateTime GetWorldTime()
        {
            try
            {
                int month = Function.Call<int>(Hash.GET_CLOCK_MONTH) + 1;
                int year = Function.Call<int>(Hash.GET_CLOCK_YEAR);
                int day = Function.Call<int>(Hash.GET_CLOCK_DAY_OF_MONTH);
                int hour = Function.Call<int>(Hash.GET_CLOCK_HOURS);
                int minute = Function.Call<int>(Hash.GET_CLOCK_MINUTES);
                int second = Function.Call<int>(Hash.GET_CLOCK_SECONDS);

                return new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception)
            {
                Function.Call(Hash.SET_CLOCK_DATE, 1984, 2, 4);
            }

            return DateTime.MinValue;
        }

        public static void SetWorldTime(DateTime time)
        {
            Function.Call(Hash.SET_CLOCK_DATE, time.Day, time.Month - 1, time.Year);
            Function.Call(Hash.SET_CLOCK_TIME, time.Hour, time.Minute, time.Second);
        }

        public static void DisplayHelpText(string text)
        {
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, 0, 1, -1);
        }

        public static Vector3 DirectionToRotation(Vector3 target, Vector3 current, float roll)
        {
            Vector3 dir = (current - target).Normalized;
            Vector3 rotval = Vector3.Zero;
            rotval.Z = -(radToDeg * (float)Math.Atan2(dir.X, dir.Y));
            Vector3 rotpos = Vector3.Normalize(new Vector3(dir.Z, new Vector3(dir.X, dir.Y, 0.0f).Length(), 0.0f));
            rotval.X = radToDeg * ((float)Math.Atan2(rotpos.X, rotpos.Y));
            rotval.Y = roll;
            return rotval;
        }

        public static Ped ClonePed(Ped toCloneFrom)
        {
            var newPed = World.CreatePed(toCloneFrom.Model, toCloneFrom.Position, toCloneFrom.Heading);
            // CLONE_PED_TO_TARGET
            Function.Call((Hash)0xE952D6431689AD9A, toCloneFrom.Handle, newPed.Handle);

            return newPed;
        }

        public static Vector3 GetWaypointPosition()
        {
            if (!Game.IsWaypointActive)
                return Vector3.Zero;

            bool blipFound = false;
            Vector3 position = Vector3.Zero;

            int it = Function.Call<int>(Hash._GET_BLIP_INFO_ID_ITERATOR);
            for (int i = Function.Call<int>(Hash.GET_FIRST_BLIP_INFO_ID, it); Function.Call<bool>(Hash.DOES_BLIP_EXIST, i); i = Function.Call<int>(Hash.GET_NEXT_BLIP_INFO_ID, it))
            {
                if (Function.Call<int>(Hash.GET_BLIP_INFO_ID_TYPE, i) == 4)
                {
                    position = Function.Call<Vector3>(Hash.GET_BLIP_INFO_ID_COORD, i);
                    blipFound = true;
                    break;
                }
            }

            if (blipFound)
            {
                do
                {                    
                    position.RequestCollision();
                    Script.Yield();
                    position.Z = World.GetGroundHeight(new Vector2(position.X, position.Y));
                } while (position.Z == 0);                
            }

            return position;
        }
        
        public static Vector3 GetClosestVehicleNode(Vector3 position)
        {
            Vector3 ret = Vector3.Zero;

            unsafe
            {
                Function.Call(Hash.GET_CLOSEST_VEHICLE_NODE, position.X, position.Y, position.Z, &ret, 1, 0f, 0f);
            }

            return ret;
        }

        public static Dictionary<string, Vector3> GetWheelPositions(Vehicle vehicle)
        {            
            Dictionary<string, Vector3> ret = new Dictionary<string, Vector3>();

            if (vehicle.Bones["wheel_lf"].Index > 0)
                ret.Add("wheel_lf", vehicle.Bones["wheel_lf"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_rf"].Index > 0)
                ret.Add("wheel_rf", vehicle.Bones["wheel_rf"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_lr"].Index > 0)
                ret.Add("wheel_lr", vehicle.Bones["wheel_lr"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_rr"].Index > 0)
                ret.Add("wheel_rr", vehicle.Bones["wheel_rr"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_lm1"].Index > 0)
                ret.Add("wheel_lm1", vehicle.Bones["wheel_lm1"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_rm1"].Index > 0)
                ret.Add("wheel_rm1", vehicle.Bones["wheel_rm1"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_lm2"].Index > 0)
                ret.Add("wheel_lm2", vehicle.Bones["wheel_lm1"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_rm2"].Index > 0)
                ret.Add("wheel_rm2", vehicle.Bones["wheel_rm2"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            return ret;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value > max)
                return max;
            else if (value < min)
                return min;

            return value;
        }

        public static float CalculateVolume(Entity ent)
        {
            var distance = Vector3.Distance(Main.PlayerPed.Position, ent.Position);

            var volume = 25f / distance;

            return Clamp(volume, 0, 1.5f);
        }

        public static void SetDecorator<T>(Entity entity, string propertyName, T value)
        {
            Type type = typeof(T);

            if (type == typeof(int))
                Function.Call(Hash.DECOR_SET_INT, entity.Handle, propertyName, (int)Convert.ChangeType(value, typeof(int)));
            else if (type == typeof(float))
                Function.Call(Hash.DECOR_SET_FLOAT, entity.Handle, propertyName, (float)Convert.ChangeType(value, typeof(float)));
            else if (type == typeof(bool))
                Function.Call(Hash.DECOR_SET_BOOL, entity.Handle, propertyName, (bool)Convert.ChangeType(value, typeof(bool)));
            else
                throw new Exception("SetDecorator provided with invalid type: " + type);
        }

        public static T GetDecorator<T>(Entity entity, string propertyName)
        {
            Type type = typeof(T);

            if (type == typeof(int))
                return (T)Convert.ChangeType(Function.Call<int>(Hash.DECOR_GET_INT, entity.Handle, propertyName), type);
            else if (type == typeof(float))
                return (T)Convert.ChangeType(Function.Call<float>(Hash.DECOR_GET_FLOAT, entity.Handle, propertyName), type);
            else if (type == typeof(bool))
                return (T)Convert.ChangeType(Function.Call<bool>(Hash.DECOR_GET_BOOL, entity.Handle, propertyName), type);
            else
                throw new Exception("GetDecorator provided with invalid type: " + type);

        }

        public static float MphToMs(float mph)
        {
            return mph * 0.44704f;
        }

        public static float MsToMph(float ms)
        {
            return ms * 2.23694f;
        }

        public static float DegreesToRadians(float val)
        {
            return ((float)Math.PI / 180) * val;
        }

        public static float RadiansToDegrees(float val)
        {
            return radToDeg * val;
        }

        public static unsafe int GetBoneIndex(Vehicle vehicle, string boneName)
        {
            if (vehicle == null)
                return -1;

            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            crSkeletonData* skelData = veh->inst->archetype->skeleton->skeletonData;
            uint boneCount = skelData->bonesCount;

            for (uint i = 0; i < boneCount; i++)
            {
                if (skelData->GetBoneNameForIndex(i) == boneName)
                    return unchecked((int)i);
            }

            return -1;
        }

        public static Prop SpawnAttachedProp(Vehicle vehicle, string modelName, string bone, Vector3 rotation)
        {
            Model model = new Model(modelName);
            model.Request();

            if (!model.IsInCdImage || !model.IsValid)
            {
                return null;
            }

            while (!model.IsLoaded)
            {
                Script.Yield();
            }
            
            if(!model.IsLoaded)
            {
                return null;
            }

            Prop prop = World.CreateProp(model, Vector3.Zero, false, false);
            Vector3 offset = vehicle.GetPositionOffset(vehicle.Bones[bone].Position);

            if (bone == "")
                offset = Vector3.Zero;

            prop.AttachTo(vehicle, offset, Vector3.Zero);

            return prop;
        }

        public static unsafe Vector3 GetBoneOriginalTranslation(Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            NativeVector3 v = veh->inst->archetype->skeleton->skeletonData->bones[index].translation;
            return v;
        }

        public static unsafe Quaternion GetBoneOriginalRotation(Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            NativeVector4 v = veh->inst->archetype->skeleton->skeletonData->bones[index].rotation;
            return v;
        }

        // https://code.google.com/archive/p/slimmath/
        public static bool Decompose(Matrix matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            const float ZeroTolerance = 1e-6f;

            rotation = Quaternion.Identity;
            translation = Vector3.Zero;
            scale = Vector3.Zero;

            //Source: Unknown
            //References: http://www.gamedev.net/community/forums/topic.asp?topic_id=441695

            //Get the translation.
            translation.X = matrix.M41;
            translation.Y = matrix.M42;
            translation.Z = matrix.M43;

            //Scaling is the length of the rows.
            scale.X = (float)Math.Sqrt((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12) + (matrix.M13 * matrix.M13));
            scale.Y = (float)Math.Sqrt((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22) + (matrix.M23 * matrix.M23));
            scale.Z = (float)Math.Sqrt((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32) + (matrix.M33 * matrix.M33));

            //If any of the scaling factors are zero, than the rotation matrix can not exist.
            if (Math.Abs(scale.X) < ZeroTolerance ||
                Math.Abs(scale.Y) < ZeroTolerance ||
                Math.Abs(scale.Z) < ZeroTolerance)
            {
                rotation = Quaternion.Identity;
                return false;
            }

            //The rotation is the left over matrix after dividing out the scaling.
            Matrix rotationmatrix = new Matrix();
            rotationmatrix.M11 = matrix.M11 / scale.X;
            rotationmatrix.M12 = matrix.M12 / scale.X;
            rotationmatrix.M13 = matrix.M13 / scale.X;

            rotationmatrix.M21 = matrix.M21 / scale.Y;
            rotationmatrix.M22 = matrix.M22 / scale.Y;
            rotationmatrix.M23 = matrix.M23 / scale.Y;

            rotationmatrix.M31 = matrix.M31 / scale.Z;
            rotationmatrix.M32 = matrix.M32 / scale.Z;
            rotationmatrix.M33 = matrix.M33 / scale.Z;

            rotationmatrix.M44 = 1f;

            rotation = Quaternion.RotationMatrix(rotationmatrix);
            return true;
        }


        public static bool SerializeObject<T>(T obj, string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                TextWriter stream = new StreamWriter(path);

                serializer.Serialize(stream, obj);
                stream.Close();

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public static T DeserializeObject<T>(string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                FileStream fs = new FileStream(path, FileMode.Open);

                return (T)serializer.Deserialize(fs);
            }
            catch(Exception)
            {
                return default(T);
            }
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = Math.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            float change = current - target;
            float originalTo = target;

            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;
            change = Clamp(change, -maxChange, maxChange);
            target = current - change;

            float temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float output = target + (change + temp) * exp;

            // Prevent overshooting
            if (originalTo - current > 0.0F == output > originalTo)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }

            return output;
        }

        public static Vector3 ToEulerRad(Quaternion q1)
        {
            float sqw = q1.W * q1.W;
            float sqx = q1.X * q1.X;
            float sqy = q1.Y * q1.Y;
            float sqz = q1.Z * q1.Z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = q1.X * q1.W - q1.Y * q1.Z;
            Vector3 v = new Vector3();

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.Y = 2f * (float)Math.Atan2(q1.Y, q1.X);
                v.X = (float)Math.PI / 2;
                v.Z = 0;
                return NormalizeAngles(v * (float)(360 / (Math.PI * 2)));
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.Y = -2f * (float)Math.Atan2(q1.Y, q1.X);
                v.X = -(float)Math.PI / 2f;
                v.Z = 0;
                return NormalizeAngles(v * (float)(360 / (Math.PI * 2)));
            }
            Quaternion q = new Quaternion(q1.W, q1.Z, q1.X, q1.Y);
            v.Y = (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W));     // Yaw
            v.X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y));                             // Pitch
            v.Z = (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.Y, 1 - 2f * (q.Y * q.Y + q.Z * q.Z));      // Roll
            return NormalizeAngles(v * (float)(360 / (Math.PI * 2))) * radToDeg;
        }

        private static Quaternion FromEuler(Vector3 euler)
        {
            var yaw = euler.X;
            var pitch = euler.Y;
            var roll = euler.Z;
            float rollOver2 = roll * 0.5f;
            float sinRollOver2 = (float)System.Math.Sin((float)rollOver2);
            float cosRollOver2 = (float)System.Math.Cos((float)rollOver2);
            float pitchOver2 = pitch * 0.5f;
            float sinPitchOver2 = (float)Math.Sin((float)pitchOver2);
            float cosPitchOver2 = (float)System.Math.Cos((float)pitchOver2);
            float yawOver2 = yaw * 0.5f;
            float sinYawOver2 = (float)System.Math.Sin((float)yawOver2);
            float cosYawOver2 = (float)System.Math.Cos((float)yawOver2);
            Quaternion result;
            result.X = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
            result.Y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
            result.Z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
            result.W = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
            return result * degToRad;

        }

        static Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.X = NormalizeAngle(angles.X);
            angles.Y = NormalizeAngle(angles.Y);
            angles.Z = NormalizeAngle(angles.Z);
            return angles;
        }

        static float NormalizeAngle(float angle)
        {
            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;
            return angle;
        }

        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            smoothTime = Math.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            Vector3 change = current - target;
            Vector3 originalTo = target;

            float maxChange = maxSpeed * smoothTime;
            change = ClampMagnitude(change, maxChange);
            target = current - change;

            Vector3 temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            Vector3 output = target + (change + temp) * exp;

            if (Vector3.Dot(originalTo - current, output - originalTo) > 0)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }

            return output;
        }

        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        {
            if (vector.LengthSquared() > maxLength * maxLength)
                return vector.Normalized * maxLength;
            return vector;
        }

        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat + (secondFloat - firstFloat) * by;
        }

        public static int Lerp(int firstFloat, int secondFloat, float by)
        {
            return (int)((float)firstFloat + ((float)secondFloat - (float)firstFloat) * by);
        }

        public static bool IsAnyOfFrontDoorsOpen(Vehicle vehicle)
        {
            var doorOpen = false;
            foreach (var door in vehicle.Doors)
            {
                if (door.IsOpen)
                    doorOpen = true;
            }

            return doorOpen;
        }

        public static bool IsPlayerUseFirstPerson()
        {
            return Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE) == 4 && !GameplayCamera.IsLookingBehind && !Function.Call<bool>((Hash)0xF5F1E89A970B7796);
        }

        public static void DeleteNearbyVehPed()
        {
            var allVehicles = World.GetAllVehicles();

            allVehicles.Where(x => !x.IsTimeMachine() && Main.PlayerVehicle != x && x.Model != ModelHandler.DMCDebugModel).ToList()
                .ForEach(x => x.Delete());

            var allPeds = World.GetAllPeds();

            allPeds.Where(x => x != Main.PlayerPed && !Main.PlayerVehicle.Passengers.Contains(x)).ToList()
                .ForEach(x => x.Delete());

            Function.Call(Hash.DELETE_ALL_TRAINS);
        }

        public static bool IsAllTiresBurst(Vehicle vehicle)
        {
            if (!Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, (int) WheelId.FrontLeft, true))
                return false;

            if (!Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, (int)WheelId.FrontRight, true))
                return false;

            if (!Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, (int)WheelId.RearRight, true))
                return false;

            if (!Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, (int) WheelId.RearLeft, true))
                return false;

            return true;
        }

        public static bool IsAnyTireBurst(Vehicle vehicle)
        {
            if (Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, (int)WheelId.FrontLeft, true))
                return true;

            if (Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, (int)WheelId.FrontRight, true))
                return true;

            if (Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, (int)WheelId.RearRight, true))
                return true;

            if (Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, (int)WheelId.RearLeft, true))
                return true;

            return false;
        }

        public static void SetTiresBurst(Vehicle vehicle, bool toggle)
        {
            if (toggle)
            {
                vehicle.Wheels[(int)WheelId.FrontLeft].Burst();
                vehicle.Wheels[(int)WheelId.FrontRight].Burst();
                vehicle.Wheels[(int)WheelId.RearRight].Burst();
                vehicle.Wheels[(int)WheelId.RearLeft].Burst();
            }
            else
            {
                vehicle.Wheels[(int) WheelId.FrontLeft].Fix();
                vehicle.Wheels[(int) WheelId.FrontRight].Fix();
                vehicle.Wheels[(int) WheelId.RearRight].Fix();
                vehicle.Wheels[(int) WheelId.RearLeft].Fix();
            }
        }

        public static Vector3 EntitySpeedVector(Vehicle vehicle)
        {
            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, vehicle, true);
        }

        public static float Magnitude(Vector3 vector3)
        {
            return Function.Call<float>(Hash.VMAG2, vector3.X, vector3.Z, vector3.Y);
        }

        public static List<Vector3> GetWheelsPositions(Vehicle vehicle)
        {
            return new List<Vector3>
                    {
                        vehicle.Bones["wheel_lf"].Position,
                        vehicle.Bones["wheel_rf"].Position,
                        vehicle.Bones["wheel_rr"].Position,
                        vehicle.Bones["wheel_lr"].Position
                    };
        }

        public static bool IsVehicleOnTracks(Vehicle vehicle)
        {
            return GetWheelsPositions(vehicle).TrueForAll(x => IsWheelOnTracks(x, vehicle));
        }

        public static bool IsWheelOnTracks(Vector3 pos, Vehicle vehicle)
        {
            // What it basicly does is drawing circle around that "pos" so we can
            //  detect if wheel position is near tracks position

            //                                 **    **
            //                            **              **
            //                                 ||    ||
            //                        **       ||    ||       **
            //                                 ||    ||
            //                      **         ||    ||         **
            //                                 ||    || 
            //                      **         ||    ||         **
            //                                 ||    ||  
            //                       **        ||    ||        **
            //                                 ||    || 
            //                          **                  **
            //                                 **    ** 

            const float r = 0.15f;
            for (float i = 0; i <= 360; i += 30)
            {
                var angleRad = i * (float) Math.PI / 180;

                var x = r * Math.Cos(angleRad);
                var y = r * Math.Sin(angleRad);

                var circlePos = pos;
                circlePos.X += (float) x;
                circlePos.Y += (float) y;

                //DrawLine(pos, circlePos, Color.Aqua);

                // Then we check for every pos if it hits tracks material
                var surface = World.Raycast(circlePos, circlePos + new Vector3(0, 0, -10),
                    IntersectFlags.Everything, vehicle);

                // Tracks materials
                var allowedSurfaces = new List<MaterialHash>
                {
                    MaterialHash.MetalSolidRoadSurface,
                    MaterialHash.MetalSolidSmall,
                    MaterialHash.MetalSolidMedium
                };

                if (allowedSurfaces.Contains(surface.MaterialHash))
                    return true;
            }
            return false;
        }

        public static DateTime RandomDate()
        {
            var rand = new Random();

            var second = rand.Next(0, 59);
            var minute = rand.Next(0, 59);
            var hour = rand.Next(0, 23);
            var month = rand.Next(1, 12);
            var year = rand.Next(1, 9999);
            var day = rand.Next(1, DateTime.DaysInMonth(year, month));

            return new DateTime(year, month, day, hour, minute, second);
        }
    }

    public static class DrawingUtils
    {
    }
}
