using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using RogersSierraRailway;
using System;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal delegate void OnVehicleDestroyed();
    internal delegate void OnVehicleAttached(bool toRogersSierra = false);
    internal delegate void OnVehicleDetached(bool fromRogersSierra = false);
    internal delegate void OnTrainDeleted();

    internal class CustomTrain
    {
        public event OnVehicleDestroyed OnVehicleDestroyed;
        public event OnVehicleAttached OnVehicleAttached;
        public event OnVehicleDetached OnVehicleDetached;
        public event OnTrainDeleted OnTrainDeleted;

        public SetWheelie SetWheelie;

        public bool IsRogersSierra => Train.IsRogersSierra();
        public RogersSierra RogersSierra => TrainManager.GetRogersSierraFromVehicle(Train);

        private float _wheelieRotX;
        private float _wheeliePosZ = -0.35f;

        public bool DoWheelie { get; set; }
        public bool WheelieUp { get; set; }

        public Vehicle Train;
        public bool Direction { get; set; }
        public Vector3 Position { get => Train.Position; set => Function.Call(Hash.SET_MISSION_TRAIN_COORDS, Train, value.X, value.Y, value.Z); }
        public int CarriageCount { get; }

        private Vector3 _checkOffset;
        private int _variation;
        private float _cruiseSpeed;
        private bool _setSpeed;
        private float _speed;

        public bool Exists { get; private set; } = true;
        public bool IsAutomaticBrakeOn { get; set; } = true;
        public bool IsAccelerationOn { get; set; } = false;

        public float CruiseSpeed { get => _cruiseSpeed; set { _cruiseSpeed = value; _setSpeed = false; IsAutomaticBrakeOn = false; Function.Call(Hash.SET_TRAIN_CRUISE_SPEED, Train, value); } }
        public float CruiseSpeedMPH { get => CruiseSpeed.ToMPH(); set => CruiseSpeed = value.ToMS(); }
        public float Speed { get => _speed; set { _speed = value; _setSpeed = true; } }
        public float SpeedMPH { get => Speed.ToMPH(); set => Speed = value.ToMS(); }

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

            SetWheelie += StartWheelie;
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

        public void Tick()
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
                            _wheelieRotX += 15 * Game.LastFrameTime;
                            _wheeliePosZ += 0.35f * Game.LastFrameTime;

                            if (_wheelieRotX >= 10 && _wheeliePosZ >= 0)
                            {
                                //_wheelieRotX = 10;
                                //_wheeliePosZ = 0;
                                DoWheelie = false;
                            }

                            break;
                        case false:
                            _wheelieRotX -= 15 * Game.LastFrameTime;
                            _wheeliePosZ -= 0.35f * Game.LastFrameTime;

                            if (_wheelieRotX <= 0 && _wheeliePosZ <= -0.35f)
                            {
                                _wheelieRotX = 0;
                                _wheeliePosZ = -0.35f;
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
                if (TargetVehicle.DistanceToSquared2D((CarriageIndexForAttach == 0 ? Train : Carriage(CarriageIndexForAttach)).GetOffsetPosition(AttachOffset), 2))
                    AttachTargetVehicle();

            if (AttachedToTarget)
                AttachTargetVehicle();
        }

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
            TrySwitchToRogersSierra();

            if (!IsRogersSierra)
            {
                TargetVehicle.AttachToPhysically(AttachVehicle, AttachOffset, Vector3.Zero);
                TargetVehicle.Rotation = RotationVehicle.Rotation;
            }
            else
            {
                TargetVehicle.AttachToPhysically(AttachVehicle, AttachOffset.GetSingleOffset(Coordinate.Z, _wheeliePosZ), Vector3.Zero);
                TargetVehicle.Rotation = RotationVehicle.Rotation.GetSingleOffset(Coordinate.X, _wheelieRotX);
            }

            if (IsReadyToAttach)
            {
                PrepareTargetVehicle(false);

                OnVehicleAttached?.Invoke();
                IsReadyToAttach = false;
            }
        }

        public void DetachTargetVehicle()
        {
            Function.Call(Hash.DETACH_ENTITY, TargetVehicle, false, false);

            PrepareTargetVehicle(false);

            IsReadyToAttach = true;

            OnVehicleDetached?.Invoke();
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

        public bool CheckForClosestRogersSierra()
        {
            _checkOffset = Vector3.Zero;

            _checkOffset.Z = TrainManager.ClosestRogersSierra.Locomotive.GetPositionOffset(TargetVehicle.Position).Z;
            _checkOffset.Y = 5.13f - TargetVehicle.Model.Dimensions.rearBottomLeft.Y;

            return TrainManager.ClosestRogersSierra.Locomotive.RelativeVelocity().Y >= 0 && TargetVehicle.SameDirection(TrainManager.ClosestRogersSierra) && World.GetClosestVehicle(TrainManager.ClosestRogersSierra.Locomotive.GetOffsetPosition(_checkOffset), 0.1f) == TargetVehicle;
        }

        public void TrySwitchToRogersSierra()
        {
            if (IsRogersSierra)
                return;

            if (TrainManager.ClosestRogersSierra is null || TrainManager.ClosestRogersSierra.IsExploded || TrainManager.ClosestRogersSierra.RejectAttach || !CheckForClosestRogersSierra())
                return;

            Function.Call(Hash.DETACH_ENTITY, TargetVehicle, false, false);

            int handle = Train.Handle;
            unsafe
            {
                Function.Call(Hash.DELETE_MISSION_TRAIN, &handle);
            }

            AttachOffset = _checkOffset;

            AttachOffset.Y += 0.075f;
            AttachOffset.Z -= _wheeliePosZ;

            Train = TrainManager.ClosestRogersSierra.ColDeLorean;

            TrainManager.ClosestRogersSierra.AttachedVehicle = TargetVehicle;

            IsAccelerationOn = false;

            if (TargetVehicle.IsTimeMachine())
                MissionHandler.TrainMission.OnVehicleAttachedToRogersSierra?.Invoke(TimeMachineHandler.GetTimeMachineFromVehicle(TargetVehicle));
        }

        public void SwitchToRegular()
        {
            RogersSierra.SetRejectDelay(2000);
            RogersSierra.AttachedVehicle = null;

            DeleteTrain();
        }

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
            if (!IsRogersSierra)
            {
                int handle = Train.Handle;
                unsafe
                {
                    Function.Call(Hash.DELETE_MISSION_TRAIN, &handle);
                }
            }

            Exists = false;

            if (IsReadyToAttach)
                DetachTargetVehicle();

            OnTrainDeleted?.Invoke();
        }
    }
}
