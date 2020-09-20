using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
//using RogersSierra;

namespace BackToTheFutureV.Utility
{
    public delegate void OnVehicleDestroyed();
    public delegate void OnVehicleAttached();
    public delegate void OnTrainDeleted();

    public class CustomTrain
    {
        public event OnVehicleDestroyed OnVehicleDestroyed;
        public event OnVehicleAttached OnVehicleAttached;
        public event OnTrainDeleted OnTrainDeleted;

        public bool IsRogersSierra { get; private set; }
        //public cRogersSierra RogersSierra { get; private set; }

        private Vector3 _deloreanOffset = new Vector3(0, 7.5f, -1.0f);

        private float _deloreanWheelieRotX;
        private float _deloreanWheeliePosZ = -0.35f;

        public bool DoWheelie { get; set; }
        public bool WheelieUp { get; set; }

        public Vehicle Train;
        public bool Direction { get; set; }
        public Vector3 Position { get { return Train.Position; } set { Function.Call(Hash.SET_MISSION_TRAIN_COORDS, Train, value.X, value.Y, value.Z); } }
        public int CarriageCount { get; }

        private int _variation;
        private float _cruiseSpeed;
        private bool _setSpeed;
        private float _speed;
        private float _distance;
        public bool Exists { get; private set; } = true;
        public bool IsAutomaticBrakeOn = true;
        public bool IsAccelerationOn = false;

        public float CruiseSpeed { get { return _cruiseSpeed; } set { _cruiseSpeed = value; _setSpeed = false; IsAutomaticBrakeOn = false; Function.Call(Hash.SET_TRAIN_CRUISE_SPEED, Train, value); } }
        public float CruiseSpeedMPH { get { return Utils.MsToMph(CruiseSpeed); } set { CruiseSpeed = Utils.MphToMs(value); } }
        public float Speed { get { return _speed; } set { _speed = value; _setSpeed = true; } }
        public float SpeedMPH { get { return Utils.MsToMph(Speed); } set { Speed = Utils.MphToMs(value); } }

        public bool ToDestroy { get; private set; }
        public Vehicle TargetVehicle;
        public float DestroyCounter;
        public bool TargetExploded;

        public bool IsReadyToAttach { get; private set; }
        public bool AttachedToTarget => TargetVehicle.IsAttachedTo(AttachVehicle);
        public Vector3 AttachOffset;
        public int CarriageIndexForAttach { get; private set; }
        public int CarriageIndexForRotation { get; private set; }

        private Vehicle AttachVehicle => CarriageIndexForAttach == 0 ? Train : Carriage(CarriageIndexForAttach);
        private Vehicle RotationVehicle => CarriageIndexForRotation == 0 ? Train : Carriage(CarriageIndexForRotation);

        public CustomTrain(Vector3 position, bool direction, int variation, int carriageCount)
        {
            Direction = direction;
            Train = Function.Call<Vehicle>(Hash.CREATE_MISSION_TRAIN, variation, position.X, position.Y, position.Z, direction);

            _variation = variation;

            CruiseSpeed = 0;
            Speed = 0;
            CarriageCount = carriageCount;

            Train.IsPersistent = true;

            for (int i = 0; i <= CarriageCount; i++)
                Carriage(i).IsPersistent = true;

            ToDestroy = false;
        }

        public void SetPosition(Vector3 position)
        {
            Function.Call(Hash.SET_MISSION_TRAIN_COORDS, Train, position.X, position.Y, position.Z);
        }

        public void SetVisible(bool state)
        {
            Train.IsVisible = state;

            if (CarriageCount == 0)
                return;

            for (int i = 1; i <= CarriageCount; i++)
                Carriage(i).IsVisible = state;
        }

        public void SetHorn(bool state)
        {
            Function.Call(Hash.SET_HORN_ENABLED, Train, state);

            if (CarriageCount == 0)
                return;

            for (int i = 1; i <= CarriageCount; i++)
                Function.Call(Hash.SET_HORN_ENABLED, Carriage(i), state);
        }

        public void SetCollision(bool state)
        {
            Train.IsCollisionEnabled = state;

            if (CarriageCount == 0)
                return;

            for (int i = 1; i <= CarriageCount; i++)
                Carriage(i).IsCollisionEnabled = state;
        }

        public Vehicle Carriage(int index)
        {
            return Function.Call<Vehicle>(Hash.GET_TRAIN_CARRIAGE, Train, index);
        }

        private void Brake()
        {
            if (IsAccelerationOn && (Game.IsControlPressed(Control.VehicleAccelerate) | Game.IsControlPressed(Control.VehicleBrake)))
                return;

            if (_speed > 0f)
            {
                _speed -= 2 * Game.LastFrameTime;

                if (_speed < 0f)
                    _speed = 0f;
            }
            else if (_speed < 0f)
            {
                _speed += 2 * Game.LastFrameTime;

                if (_speed > 0f)
                    _speed = 0f;
            }

            if (!_setSpeed)
            {
                CruiseSpeedMPH = 0;
                _setSpeed = true;
            }
        }

        private void Acceleration()
        {
            if (Game.IsControlPressed(Control.VehicleHandbrake))
            {
                if (_speed < 0)
                {
                    _speed += 3 * Game.LastFrameTime;

                    if (_speed > 0)
                        _speed = 0;
                }
                else if (_speed > 0)
                {
                    _speed -= 3 * Game.LastFrameTime;

                    if (_speed < 0)
                        _speed = 0;
                }

                return;
            }

            if (Game.IsControlPressed(Control.VehicleAccelerate))
            {
                if (_speed < 0)
                    _speed += 3 * Game.LastFrameTime;
                else
                    _speed += (float)Math.Pow(Game.GetControlValueNormalized(Control.VehicleAccelerate) / 10, 1 / 3) * Game.LastFrameTime * 1.5f;
            }
            else if (Game.IsControlPressed(Control.VehicleBrake))
            {
                if (_speed > 0)
                    _speed -= 3 * Game.LastFrameTime;
                else
                    _speed -= (float)Math.Pow(Game.GetControlValueNormalized(Control.VehicleBrake) / 10, 1 / 3) * Game.LastFrameTime * 2;
            }

        }

        public void Process()
        {
            if (!IsRogersSierra)
            {
                if (IsAccelerationOn)
                    Acceleration();

                if (IsAutomaticBrakeOn)
                    Brake();

                if (_setSpeed)
                {
                    if (SpeedMPH > 90)
                        SpeedMPH = 90;

                    if (SpeedMPH < -25)
                        SpeedMPH = -25;

                    Function.Call(Hash.SET_TRAIN_SPEED, Train, Speed);
                }
            }
            else
            {
                if (DoWheelie)
                {
                    switch (WheelieUp)
                    {
                        case true:
                            _deloreanWheelieRotX += 15 * Game.LastFrameTime;
                            _deloreanWheeliePosZ += 0.35f * Game.LastFrameTime;

                            if (_deloreanWheelieRotX >= 15 && _deloreanWheeliePosZ >= 0)
                            {
                                _deloreanWheelieRotX = 15;
                                _deloreanWheeliePosZ = 0;
                                DoWheelie = false;
                            }

                            break;
                        case false:
                            _deloreanWheelieRotX -= 15 * Game.LastFrameTime;
                            _deloreanWheeliePosZ -= 0.35f * Game.LastFrameTime;

                            if (_deloreanWheelieRotX <= 0 && _deloreanWheeliePosZ <= -0.35f)
                            {
                                _deloreanWheelieRotX = 0;
                                _deloreanWheeliePosZ = -0.35f;
                                DoWheelie = false;
                            }

                            break;
                    }
                }
            }

            if (ToDestroy)
            {
                DestroyCounter -= Game.LastFrameTime;

                if (TargetVehicle != null && !TargetExploded && TargetVehicle.IsTouching(Train))
                {
                    PrepareTargetVehicle(false);

                    OnVehicleDestroyed?.Invoke();

                    TargetVehicle.Explode();
                    TargetExploded = true;
                }

                if (DestroyCounter <= 0)
                    DeleteTrain();
            }

            if (IsReadyToAttach)
                if (CheckForNearbyTargetVehicle())
                    AttachTargetVehicle();

            if (AttachedToTarget)
            {
                //if (!IsRogersSierra && Main.RogersSierra.Count > 0)
                //    SwitchToRogersSierra(CheckForRogersSierra());

                AttachTargetVehicle();
            }
        }

        public bool CheckForNearbyTargetVehicle()
        {
            //float _tempDistance = TargetVehicle.Position.DistanceToSquared(AttachVehicle.GetOffsetPosition(AttachOffset));

            //if (_tempDistance > _distance && _distance != 0)
            //{
            //    DeleteTrain();
            //    return false;
            //}

            //_distance = _tempDistance;

            //return _distance <= 0.1f * 0.1f;

            return TargetVehicle.Position.DistanceTo((CarriageIndexForAttach == 0 ? Train : Carriage(CarriageIndexForAttach)).GetOffsetPosition(AttachOffset)) <= 2.0f;
        }

        //public cRogersSierra CheckForRogersSierra()
        //{            
        //    foreach (var train in Main.RogersSierra)
        //    {
        //        if (Utils.EntitySpeedVector(train.Locomotive).Y >= 0 && World.GetClosestVehicle(train.Locomotive.GetOffsetPosition(_deloreanOffset.GetSingleOffset(Coordinate.Z, _deloreanWheeliePosZ).GetSingleOffset(Coordinate.Y, -0.1f)), 0.1f) == TargetVehicle)
        //            return train;
        //    }

        //    return null;
        //}

        public void SetToAttach(Vehicle targetVehicle, Vector3 attachOffset, int carriageIndexForAttach, int carriageIndexForRotation)
        {
            TargetVehicle = targetVehicle;
            AttachOffset = attachOffset;
            CarriageIndexForAttach = carriageIndexForAttach;
            CarriageIndexForRotation = carriageIndexForRotation;

            PrepareTargetVehicle(true);

            IsReadyToAttach = true;
        }

        public void PrepareTargetVehicle(bool state)
        {
            TargetVehicle.IsInvincible = state;
            TargetVehicle.CanBeVisiblyDamaged = !state;
            TargetVehicle.IsCollisionProof = state;
            TargetVehicle.IsRecordingCollisions = !state;
        }

        public void AttachTargetVehicle()
        {
            if (!IsRogersSierra)
            {
                TargetVehicle.AttachToPhysically(AttachVehicle, AttachOffset, Vector3.Zero);
                TargetVehicle.Rotation = RotationVehicle.Rotation;
            }
            else
            {
                TargetVehicle.AttachToPhysically(AttachVehicle, _deloreanOffset.GetSingleOffset(Coordinate.Z, _deloreanWheeliePosZ), Vector3.Zero);
                TargetVehicle.Rotation = RotationVehicle.Rotation.GetSingleOffset(Coordinate.X, _deloreanWheelieRotX);
            }

            if (IsReadyToAttach)
            {
                PrepareTargetVehicle(false);

                _distance = 0;
                OnVehicleAttached?.Invoke();
                IsReadyToAttach = false;
            }
        }

        public void DetachTargetVehicle()
        {
            Function.Call(Hash.DETACH_ENTITY, TargetVehicle, false, false);

            PrepareTargetVehicle(false);

            IsReadyToAttach = true;
        }

        public void SetToDestroy(Vehicle targetVehicle, float destroyCounter)
        {
            DestroyCounter = destroyCounter;
            TargetVehicle = targetVehicle;
            TargetExploded = false;
            ToDestroy = true;
        }

        public void SetToDestroy(float destroyCounter)
        {
            DestroyCounter = destroyCounter;
            ToDestroy = true;
        }

        public void DisableToDestroy()
        {            
            DestroyCounter = 0;
            ToDestroy = false;
        }

        //public void SwitchToRogersSierra(cRogersSierra rogersSierra)
        //{
        //    if (rogersSierra == null)
        //        return;

        //    Function.Call(Hash.DETACH_ENTITY, TargetVehicle, false, false);

        //    int handle = Train.Handle;
        //    unsafe
        //    {
        //        Function.Call(Hash.DELETE_MISSION_TRAIN, &handle);
        //    }

        //    IsRogersSierra = true;

        //    RogersSierra = rogersSierra;

        //    Train = RogersSierra.ColDeLorean;

        //    RogersSierra.VisibleLocomotive.ToggleExtra(1, true);

        //    RogersSierra.AttachedVehicle = TargetVehicle;

        //    IsAccelerationOn = false;
        //}

        //public void SwitchToRegular()
        //{
        //    DetachTargetVehicle();

        //    IsRogersSierra = false;

        //    RogersSierra.AttachedVehicle = null;

        //    RogersSierra = null;

        //    PrepareTargetVehicle(true);

        //    Vector3 position = TargetVehicle.GetOffsetPosition(new Vector3(0, -10, 0));            

        //    Train = Function.Call<Vehicle>(Hash.CREATE_MISSION_TRAIN, _variation, position.X, position.Y, position.Z, Direction);

        //    SetCollision(false);

        //    SetVisible(false);

        //    //CruiseSpeedMPH = 1;

        //    SetPosition(TargetVehicle.Position);            
        //}

        public void StartWheelie(bool goUp)
        {
            if (IsRogersSierra)
            {
                DoWheelie = true;
                WheelieUp = goUp;
            }            
        }

        public void DeleteTrain()
        {
            int handle = Train.Handle;
            unsafe
            {
                Function.Call(Hash.DELETE_MISSION_TRAIN, &handle);
            }

            Exists = false;

            if (IsReadyToAttach)
                DetachTargetVehicle();

            OnTrainDeleted?.Invoke();
        }
    }
}
