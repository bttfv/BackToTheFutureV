﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class FireTrail
    {
        // Fire trail in shape of "99" used when a time machine gets strike by lighting
        public static readonly Vector3[] FireTrails99Offsets = new Vector3[39]
        {
            new Vector3(-0.755061f, 1.651605f, -0.02032154f),
            new Vector3(-0.755047f, 1.765048f, -0.0212798f),
            new Vector3(-0.7550347f, 1.878489f, -0.02215795f),
            new Vector3(-0.7550243f, 1.991928f, -0.02288545f),
            new Vector3(-0.7550176f, 2.105363f, -0.02338029f),
            new Vector3(-0.7550131f, 2.218791f, -0.02355667f),
            new Vector3(-0.7550124f, 2.33221f, -0.02332497f),
            new Vector3(-0.7550139f, 2.445619f, -0.02259558f),
            new Vector3(-0.7550696f, 2.558903f, -0.02126825f),
            new Vector3(-0.7550794f, 2.672282f, -0.01921738f),
            new Vector3(-0.7550399f, 2.785753f, -0.0163248f),
            new Vector3(-0.7550461f, 2.899086f, -0.01244536f),
            new Vector3(-0.7549998f, 3.012501f, -0.007422969f),
            new Vector3(-0.7550375f, 3.125654f, -0.00105048f),
            new Vector3(-0.755052f, 3.238759f, 0.006885365f),
            new Vector3(-0.7550315f, 3.351803f, 0.01665901f),
            new Vector3(-0.7550484f, 3.464545f, 0.02859843f),
            new Vector3(-0.7550284f, 3.577074f, 0.04311036f),
            new Vector3(-0.7550181f, 3.689133f, 0.06073608f),
            new Vector3(-0.7550426f, 3.800449f, 0.08212345f),
            new Vector3(-0.7550373f, 3.910842f, 0.1081744f),
            new Vector3(-0.7549922f, 4.019742f, 0.1401516f),
            new Vector3(-0.7550288f, 4.124271f, 0.183698f),
            new Vector3(-0.7550554f, 4.21863f, 0.2460442f),
            new Vector3(-0.7550278f, 4.289472f, 0.3337793f),
            new Vector3(-0.755022f, 4.316851f, 0.4428475f),
            new Vector3(-0.7550405f, 4.3048f, 0.5551998f),
            new Vector3(-0.7550387f, 4.256822f, 0.6573231f),
            new Vector3(-0.7550518f, 4.175659f, 0.7356399f),
            new Vector3(-0.7550058f, 4.071905f, 0.7797211f),
            new Vector3(-0.7550583f, 3.95907f, 0.7891212f),
            new Vector3(-0.7550382f, 3.847078f, 0.7736835f),
            new Vector3(-0.7550091f, 3.747692f, 0.7211241f),
            new Vector3(-0.7550296f, 3.684013f, 0.6285456f),
            new Vector3(-0.7550266f, 3.660522f, 0.5180218f),
            new Vector3(-0.7550222f, 3.664883f, 0.4049845f),
            new Vector3(-0.7550736f, 3.708662f, 0.3014302f),
            new Vector3(-0.7550143f, 3.792399f, 0.2258026f),
            new Vector3(-0.7550426f, 3.894026f, 0.1759465f)
        };

        private readonly ParticlePlayerHandler _fireTrailPtfxs = new ParticlePlayerHandler();

        private readonly int _appearTime;
        private readonly int _disappearTime;

        private float _currentStrength;
        private readonly float baseOffset;

        private bool _fadeAway;

        public FireTrail(Vehicle vehicle, bool is99, int disappearTime, int appearTime, int maxLength)
        {
            _disappearTime = disappearTime;
            _appearTime = appearTime;

            if (!is99)
            {
                // World positions
                Vector3 leftWheelOffset = vehicle.GetPositionOffset(vehicle.Bones["wheel_lr"].Position);
                Vector3 rightWheelOffset = vehicle.GetPositionOffset(vehicle.Bones["wheel_rr"].Position);

                if (vehicle.RunningDirection() == RunningDirection.Forward || vehicle.RunningDirection() == RunningDirection.Stop)
                {
                    baseOffset = 0.3f;
                }
                else
                {
                    baseOffset = -0.3f;
                }

                for (int i = 0; i < maxLength; i++)
                {
                    // Define fire offset on left and right wheels
                    Vector3 leftPosOffset = vehicle.GetOffsetPosition(leftWheelOffset + new Vector3(0, i * baseOffset, -0.2f));
                    Vector3 rightPosOffset = vehicle.GetOffsetPosition(rightWheelOffset + new Vector3(0, i * baseOffset, -0.2f));

                    // Place fire offset on ground if car is not flying
                    if (!vehicle.IsInAir)
                    {
                        World.GetGroundHeight(leftPosOffset, out float leftTemp);
                        leftPosOffset.Z = leftTemp + 0.125f;
                        World.GetGroundHeight(rightPosOffset, out float rightTemp);
                        rightPosOffset.Z = rightTemp + 0.125f;
                    }

                    // Create and configure fire particle
                    _fireTrailPtfxs.Add("core", "fire_petrol_one", ParticleType.Looped, leftPosOffset, vehicle.Rotation, 1.2f);
                    _fireTrailPtfxs.Add("core", "fire_petrol_one", ParticleType.Looped, rightPosOffset, vehicle.Rotation, 1.2f);
                }
            }
            else
            {
                // Create fire particles out of pre-defines offsets
                foreach (Vector3 offset in FireTrails99Offsets)
                {
                    // Create start position on left and right wheels
                    Vector3 leftPosOffset = offset;
                    Vector3 rightPosOffset = new Vector3(Math.Abs(leftPosOffset.X), leftPosOffset.Y, leftPosOffset.Z);

                    leftPosOffset = vehicle.GetOffsetPosition(leftPosOffset);
                    rightPosOffset = vehicle.GetOffsetPosition(rightPosOffset);

                    _fireTrailPtfxs.Add("core", "fire_petrol_one", ParticleType.Looped, leftPosOffset, vehicle.Rotation, 1.2f);
                    _fireTrailPtfxs.Add("core", "fire_petrol_one", ParticleType.Looped, rightPosOffset, vehicle.Rotation, 1.2f);
                }
            }

            _currentStrength = 1f;

            _fireTrailPtfxs.SetEvolutionParam("strength", _currentStrength);
            _fireTrailPtfxs.SetEvolutionParam("dist", 0f);
            _fireTrailPtfxs.SetEvolutionParam("fadein", 0f);

            _fireTrailPtfxs.SequenceInterval = _appearTime;

            _fireTrailPtfxs.OnParticleSequenceCompleted += FireTrailPtfxs_OnParticleSequenceCompleted;

            _fireTrailPtfxs.Play();

            if (_appearTime == 0)
            {
                _fireTrailPtfxs.SequenceInterval = 1;
                _fireTrailPtfxs.StopInSequence();
                _fadeAway = true;
            }
        }

        private void FireTrailPtfxs_OnParticleSequenceCompleted(bool isStop)
        {
            if (!isStop)
            {
                _fadeAway = true;
            }
        }

        public void Tick()
        {
            if (!_fireTrailPtfxs.IsPlaying || !_fadeAway)
            {
                return;
            }

            if (_currentStrength > 0)
            {
                _currentStrength -= Game.LastFrameTime * (1f / _disappearTime);

                if (_currentStrength < 0)
                {
                    _currentStrength = 0;
                }

                _fireTrailPtfxs.SetEvolutionParam("strength", _currentStrength);
            }

            if (_currentStrength == 0)
            {
                Stop(false);
            }
        }

        public void Stop(bool instant = true)
        {
            _fireTrailPtfxs.Stop(instant);
        }
    }
}