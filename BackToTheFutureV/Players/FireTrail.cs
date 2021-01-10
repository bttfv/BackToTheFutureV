using FusionLibrary;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;

namespace BackToTheFutureV.Players
{
    public class FireTrail
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
                foreach(var offset in FireTrails99Offsets)
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
            _fireTrailPtfxs.ForEach(x => x.RemovePtfx(x.Handle));
            _fireTrailPtfxs.ForEach(x => x.Dispose());
            _fireTrailPtfxs.Clear();
        }
    }
}