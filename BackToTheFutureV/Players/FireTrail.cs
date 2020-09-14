using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;

namespace BackToTheFutureV.Players
{
    public class FireTrail
    {
        private List<PtfxPlayer> _fireTrailPtfxs = new List<PtfxPlayer>();

        private int _nextSpawn;
        private int _currentSpawnIndex;
        private int _appearTime;

        private float _currentStrength;
        private float _currentFadeIn;
        private float _disappearAt = -1;
        private float _disappearTime;
        private float _fireSize;

        private bool _is99;
        private bool _hasSpawned;
        private bool _useBlueFadein;

        public FireTrail(Vehicle vehicle, bool is99, float disappearTime = 45, int appearTime = 15, bool useBlueFadein = true, int maxLength = 50)
        {
            _is99 = is99;
            _disappearTime = disappearTime;
            _appearTime = appearTime;
            _useBlueFadein = useBlueFadein;

            _fireSize = 1.2f;

            if (!is99)
            {
                // World positions
                Vector3 leftWheelOffset = vehicle.GetPositionOffset(vehicle.Bones["wheel_lf"].Position);
                Vector3 rightWheelOffset = vehicle.GetPositionOffset(vehicle.Bones["wheel_rf"].Position);

                float baseOffset = 0.3f;
                for (int i = 0; i < maxLength; i++)
                {
                    // Define fire offset on left and right wheels
                    Vector3 leftPosOffset = leftWheelOffset + new Vector3(0, i * baseOffset, -0.2f);
                    Vector3 rightPosOffset = rightWheelOffset + new Vector3(0, i * baseOffset, -0.2f);

                    // Place fire offset on ground if car is not flying
                    if(vehicle.IsInAir)
                    {
                        leftPosOffset = vehicle.GetOffsetPosition(leftPosOffset);
                        rightPosOffset = vehicle.GetOffsetPosition(rightPosOffset);
                    }
                    else
                    {
                        leftPosOffset = Utils.GetPositionOnGround(vehicle.GetOffsetPosition(leftPosOffset), 0.125f);
                        rightPosOffset = Utils.GetPositionOnGround(vehicle.GetOffsetPosition(rightPosOffset), 0.125f);
                    }

                    // Create and configure fire particle
                    PtfxPlayer leftWheelPtfx = new PtfxPlayer("core", "fire_petrol_one", leftPosOffset, vehicle.Rotation, _fireSize, true, false);
                    PtfxPlayer rightWheelPtfx = new PtfxPlayer("core", "fire_petrol_one", rightPosOffset, vehicle.Rotation, _fireSize, true, false);

                    _currentStrength = 1f;
                    _currentFadeIn = useBlueFadein ? 0.15f : 0f;

                    _fireTrailPtfxs.Add(leftWheelPtfx);
                    _fireTrailPtfxs.Add(rightWheelPtfx);
                    foreach (var ptfx in _fireTrailPtfxs)
                    {
                        ptfx.SetEvolutionParam("strength", 1f);
                        ptfx.SetEvolutionParam("dist", 0f);
                        ptfx.SetEvolutionParam("fadein", _currentFadeIn);
                    }
                }
            }
            else
            {
                // Create fire particles out of pre-defines offsets
                foreach(var offset in Constants.FireTrails99Offsets)
                {
                    // Create start position on left and right wheels
                    var leftPosOffset = offset;
                    var rightPosOffset = new Vector3(Math.Abs(leftPosOffset.X), leftPosOffset.Y, leftPosOffset.Z);

                    leftPosOffset = vehicle.GetOffsetPosition(leftPosOffset);
                    rightPosOffset = vehicle.GetOffsetPosition(rightPosOffset);

                    PtfxPlayer leftWheelPtfx = new PtfxPlayer("core", "fire_petrol_one", leftPosOffset, vehicle.Rotation, _fireSize, true, false);
                    PtfxPlayer rightWheelPtfx = new PtfxPlayer("core", "fire_petrol_one", rightPosOffset, vehicle.Rotation, _fireSize, true, false);

                    _currentStrength = 1f;

                    _fireTrailPtfxs.Add(leftWheelPtfx);
                    _fireTrailPtfxs.Add(rightWheelPtfx);
                    foreach (var ptfx in _fireTrailPtfxs)
                    {
                        ptfx.SetEvolutionParam("strength", 1);
                        ptfx.SetEvolutionParam("dist", 0f);
                        ptfx.SetEvolutionParam("fadein", 0f);
                    }
                }
            }

            if (_appearTime == -1)
            {
                _hasSpawned = true;
                _currentSpawnIndex = 0;
                _fireTrailPtfxs.ForEach(x => x.Play());
            }
            else
            {
                _hasSpawned = false;
                _currentSpawnIndex = 0;
            }
        }

        public void Process()
        {
            if (_fireTrailPtfxs.Count > 0)
            {
                if(!_hasSpawned && Game.GameTime > _nextSpawn)
                {
                    if (_currentSpawnIndex > (_fireTrailPtfxs.Count - 1))
                    {
                        _hasSpawned = true;
                        _currentSpawnIndex = 0;
                    }
                    else
                    {
                        _fireTrailPtfxs[_currentSpawnIndex].Play();

                        _currentSpawnIndex++;
                        _nextSpawn = Game.GameTime + _appearTime;
                    }
                    return;
                }

                var amountToSub = (1f * Game.LastFrameTime) / (_disappearTime * 0.75f);
                _currentStrength -= amountToSub;

                if (_currentStrength < 0)
                    _currentStrength = 0;

                _fireTrailPtfxs.ForEach(x => x.SetEvolutionParam("strength", _currentStrength));

                if(_useBlueFadein)
                {
                    amountToSub = (0.15f * Game.LastFrameTime) / _disappearTime;
                    _currentFadeIn -= amountToSub;

                    if (_currentFadeIn < 0)
                        _currentFadeIn = 0;

                    _fireTrailPtfxs.ForEach(x => x.SetEvolutionParam("fadein", _currentFadeIn));
                }

                if (_currentStrength <= 0 && _disappearAt == -1)
                    _disappearAt = Game.GameTime + 5000;
                else if(_currentStrength <= 0 && Game.GameTime > _disappearAt)
                    Stop();
            }
        }

        public void Stop()
        {
            _fireTrailPtfxs.ForEach(x => x.RemovePtfx(x.ParticleId));
            _fireTrailPtfxs.ForEach(x => x.Dispose());
            _fireTrailPtfxs.Clear();
        }
    }
}