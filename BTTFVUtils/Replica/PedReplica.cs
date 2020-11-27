using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionLibrary
{
    [Serializable]
    public class PedReplica
    {
        public PedReplica(Ped ped)
        {
            Model = ped.Model;
            Type = Function.Call<int>(Hash.GET_PED_TYPE, ped);

            Position = ped.Position;
            Rotation = ped.Rotation;
            Heading = ped.Heading;

            Seat = ped.SeatIndex;

            Weapons = new List<WeaponReplica>();

            foreach (WeaponHash x in Enum.GetValues(typeof(WeaponHash)))
                if (ped.Weapons.HasWeapon(x))
                    Weapons.Add(new WeaponReplica(ped, ped.Weapons[x]));
        }

        public int Model { get; }
        public int Type { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        public float Heading { get; }
        public VehicleSeat Seat { get; }
        public List<WeaponReplica> Weapons { get; }

        public Ped Spawn()
        {
            Ped ped = Function.Call<Ped>(Hash.CREATE_PED, Type, Model, Position.X, Position.Y, Position.Z, Heading, false, false);

            ped.Rotation = Rotation;

            foreach (var x in Weapons)
                x.Give(ped);

            return ped;
        }

        public Ped Spawn(Vector3 position, float heading)
        {
            Ped ped = Function.Call<Ped>(Hash.CREATE_PED, Type, Model, position.X, position.Y, position.Z, heading, false, false);

            foreach (var x in Weapons)
                x.Give(ped);

            return ped;
        }

        public Ped Spawn(Vehicle vehicle)
        {
            Ped ped = Function.Call<Ped>(Hash.CREATE_PED_INSIDE_VEHICLE, vehicle, Type, Model, Seat != VehicleSeat.None ? Seat : VehicleSeat.Any, false, false);

            foreach (var x in Weapons)
                x.Give(ped);

            return ped;
        }

        public Ped Spawn(Vehicle vehicle, VehicleSeat vehicleSeat)
        {
            Ped ped = Function.Call<Ped>(Hash.CREATE_PED_INSIDE_VEHICLE, vehicle, Type, Model, vehicleSeat, false, false);

            foreach (var x in Weapons)
                x.Give(ped);

            return ped;
        }
    }
}
