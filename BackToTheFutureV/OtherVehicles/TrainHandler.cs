using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;

namespace BackToTheFutureV.Utility
{
    public delegate void OnVehicleAttached();
    public delegate void OnTrainDeleted();

    public class TrainHandler
    {
        public event OnVehicleAttached OnVehicleAttached;
        public event OnTrainDeleted OnTrainDeleted;

        public Vehicle Train;        
        public bool Direction;
        public Vector3 Position { get { return Train.Position; } set { Function.Call(Hash.SET_MISSION_TRAIN_COORDS, Train, value.X, value.Y, value.Z); } }

        private float _cruiseSpeed;
        private bool _setSpeed;
        private float _speed;
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
        public bool AttachedToTarget => TargetVehicle.IsAttachedTo(Carriage(CarriageIndexForAttach));
        public Vector3 AttachOffset;
        public int CarriageIndexForAttach { get; private set; }
        public int CarriageIndexForRotation { get; private set; }

        public TrainHandler(Vector3 position, bool direction, int variation)
        {
            Direction = direction;
            Train = Function.Call<Vehicle>(Hash.CREATE_MISSION_TRAIN, variation, position.X, position.Y, position.Z, direction);

            CruiseSpeed = 0;
            Speed = 0;

            ToDestroy = false;
        }

        public void SetPosition(Vector3 position)
        {
            Function.Call(Hash.SET_MISSION_TRAIN_COORDS, Train, position.X, position.Y, position.Z);
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
        
        public bool Process()
        {
            if (!Train.Exists())
                return true;

            if (IsAccelerationOn)
                Acceleration();

            if (IsAutomaticBrakeOn)
                Brake();

            if (_setSpeed)
            {
                if (SpeedMPH > 90)
                    SpeedMPH = 90;

                if (SpeedMPH < -20)
                    SpeedMPH = -20;

                Function.Call(Hash.SET_TRAIN_SPEED, Train, Speed);
            }
                
            if (ToDestroy)
            {
                DestroyCounter -= Game.LastFrameTime;

                if (TargetVehicle != null && !TargetExploded && TargetVehicle.HasBeenDamagedBy(Train))
                {
                    TargetVehicle.Explode();
                    TargetExploded = true;
                }

                if (DestroyCounter <= 0)
                {
                    DeleteTrain();
                    return true;
                }
            }

            if (IsReadyToAttach)
                if (CheckForNearbyTargetVehicle())
                    AttachTargetVehicle();

            if (AttachedToTarget)
                AttachTargetVehicle();

            return false;
        }

        public bool CheckForNearbyTargetVehicle()
        {
            return TargetVehicle.Position.DistanceTo((CarriageIndexForAttach == 0 ? Train : Carriage(CarriageIndexForAttach)).GetOffsetPosition(AttachOffset)) <= 2.0f;
        }

        public void SetToAttach(Vehicle targetVehicle, Vector3 attachOffset, int carriageIndexForAttach, int carriageIndexForRotation)
        {
            TargetVehicle = targetVehicle;
            AttachOffset = attachOffset;
            CarriageIndexForAttach = carriageIndexForAttach;
            CarriageIndexForRotation = carriageIndexForRotation;

            TargetVehicle.IsInvincible = true;
            TargetVehicle.CanBeVisiblyDamaged = false;
            TargetVehicle.IsCollisionProof = true;

            IsReadyToAttach = true;
        }

        public void AttachTargetVehicle()
        {
            if (IsReadyToAttach)
            {               
                OnVehicleAttached?.Invoke();
                IsReadyToAttach = false;
            }

            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY, TargetVehicle, CarriageIndexForAttach == 0 ? Train : Carriage(CarriageIndexForAttach), 0, 0, AttachOffset.X, AttachOffset.Y, AttachOffset.Z, 0, 0, 0, 0, 0, 0, 1000000.0f, true, true, false, false, 2);
            TargetVehicle.Rotation = CarriageIndexForRotation == 0 ? Train.Rotation : Carriage(CarriageIndexForRotation).Rotation;
        }

        public void DetachTargetVehicle()
        {
            Function.Call(Hash.DETACH_ENTITY, TargetVehicle, false, false);
            TargetVehicle.IsInvincible = false;
            TargetVehicle.CanBeVisiblyDamaged = true;
            TargetVehicle.IsCollisionProof = false;

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

        public void DeleteTrain()
        {
            int handle = Train.Handle;
            unsafe
            {
                Function.Call(Hash.DELETE_MISSION_TRAIN, &handle);
            }

            Exists = false;

            OnTrainDeleted?.Invoke();
        }
    }
}
