using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using static FusionLibrary.Enums;
using Control = GTA.Control;

namespace BackToTheFutureV
{
    internal class WaybackPedReplica
    {
        public DateTime Time { get; }
        public int Timestamp { get; }

        public Vector3 Position { get; }
        public float Heading { get; }
        public float Speed { get; }

        public bool PressControl { get; }
        public Control Control { get; }

        public Weapon Weapon { get; }

        public WaybackPedReplica(Ped ped, int startGameTime)
        {
            Time = Utils.CurrentTime;
            Timestamp = Game.GameTime - startGameTime;

            Position = ped.Position;
            Heading = ped.Heading;
            Speed = ped.Speed;

            Weapon = ped.Weapons.Current;

            if (Game.IsControlJustPressed(Control.Enter) && ped.IsEnteringVehicle().NotNullAndExists())
            {
                PressControl = true;
                Control = Control.Enter;
            }

            if (Game.IsControlJustPressed(Control.VehicleExit) && ped.IsInVehicle())
            {
                PressControl = true;
                Control = Control.VehicleExit;
            }

            if (Game.IsControlJustPressed(Control.Context))
            {
                PressControl = true;
                Control = Control.Context;
            }

            if (Game.IsControlJustPressed(Control.Jump))
            {
                PressControl = true;
                Control = Control.Jump;
            }

            if (Game.IsControlJustPressed(Control.Attack))
            {
                PressControl = true;
                Control = Control.Attack;
            }
        }

        public void Apply(Ped ped, int startPlayGameTime, WaybackPedReplica nextReplica)
        {
            if (ped.IsTaskActive(TaskType.Jump) || ped.IsTaskActive(TaskType.EnterVehicle) || ped.IsTaskActive(TaskType.ExitVehicle))
                return;

            if (!PressControl && !ped.IsInVehicle() && !ped.IsEnteringVehicle().NotNullAndExists())
            {
                float timeRatio = 0;

                if (Timestamp != nextReplica.Timestamp)
                    timeRatio = (Game.GameTime - startPlayGameTime - Timestamp) / (float)(nextReplica.Timestamp - Timestamp);

                Vector3 pos = Utils.Lerp(Position, nextReplica.Position, timeRatio);

                Function.Call(Hash.TASK_GO_STRAIGHT_TO_COORD, ped, pos.X, pos.Y, pos.Z, Utils.Lerp(Speed, nextReplica.Speed, timeRatio), 1, Utils.Lerp(Heading, nextReplica.Heading, timeRatio), 0);
            }

            if (Weapon != ped.Weapons.Current)
                ped.Weapons.Select(Weapon);

            if (!PressControl)
                return;

            switch (Control)
            {
                case Control.Enter:
                    ped.Task.EnterVehicle(ped.GetNearestVehicle());
                    break;
                case Control.VehicleExit:
                    ped.Task.LeaveVehicle();
                    break;
                case Control.Jump:
                    ped.Task.Jump();
                    break;
            }
        }
    }
}
