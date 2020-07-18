using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BackToTheFutureV.Players
{
    public class PtfxPlayer : Player
    {
        public int ParticleId { get; protected set; }
        public string AssetName { get; set; }
        public string EffectName { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public float Size { get; }

        public bool ShouldLoop { get; }
        public bool DoLoopHandling { get; }
        public float LoopTime { get; }
        public int RemoveTime { get; }

        protected List<int> currentPlayingParticles = new List<int>();

        protected Dictionary<string, float> evolutionParams = new Dictionary<string, float>();

        protected int nextRemove;

        public PtfxPlayer(string assetName, string effectName, Vector3 position, Vector3 rotation, float size = 1f, bool loop = false, bool doLoopHandling = false, int removeTime = 30)
        {
            AssetName = assetName;
            EffectName = effectName;
            Position = position;
            Rotation = rotation;
            Size = size;
            ShouldLoop = loop;
            DoLoopHandling = doLoopHandling;
            RemoveTime = removeTime;

            RequestPtfx();
        }

        public void RequestPtfx()
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, AssetName);

            while (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, AssetName))
            {
                Script.Yield();
            }
        }

        public override void Process()
        {
            if(IsPlaying && ShouldLoop && DoLoopHandling && Game.GameTime > nextRemove)
            {
                if (currentPlayingParticles.Count > 3)
                    RemovePtfx(currentPlayingParticles[0]);

                SpawnCopy();

                nextRemove = Game.GameTime + RemoveTime;
            }
        }

        public void SetEvolutionParam(string key, float value)
        {
            evolutionParams[key] = value;

            foreach(var entry in evolutionParams)
            {
                currentPlayingParticles.ForEach(x => Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, x, entry.Key, entry.Value, 0));
            }
        }

        public float GetEvolutionParam(string key)
        {
            if (evolutionParams.TryGetValue(key, out float value))
                return value;

            return -1f;
        }

        public override void Play()
        {
            IsPlaying = true;

            SpawnCopy();
        }

        public void StopNaturally()
        {
            IsPlaying = false;

            currentPlayingParticles.ForEach(x => Function.Call(Hash.STOP_PARTICLE_FX_LOOPED, x, 0));
            currentPlayingParticles.Clear();
        }

        public override void Stop()
        {
            IsPlaying = false;

            currentPlayingParticles.ForEach(x => RemovePtfx(x));
            currentPlayingParticles.Clear();
        }

        public override void Dispose()
        {
            Stop();
        }

        public virtual void SpawnCopy()
        {
            RequestPtfx();

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, AssetName);

            if (ShouldLoop)
            {
                int id = Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_AT_COORD, EffectName, Position.X, Position.Y, Position.Z, Rotation.X, Rotation.Y, Rotation.Z, Size, false, false, false);
                currentPlayingParticles.Add(id);
            }
            else
            {
                Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD, EffectName, Position.X, Position.Y, Position.Z, Rotation.X, Rotation.Y, Rotation.Z, Size, false, false, false);
            }

            if (!ShouldLoop) return;

            foreach(var entry in evolutionParams)
            {
                currentPlayingParticles.ForEach(x => Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, x, entry.Key, entry.Value, 0));
            }
        }

        public void RemovePtfx(int id)
        {
            Function.Call(Hash.REMOVE_PARTICLE_FX, id, 0);

            if (currentPlayingParticles.Contains(id))
                currentPlayingParticles.Remove(id);
        }
    }

    public class PtfxEntityPlayer : PtfxPlayer
    {
        public Entity Entity { get; set; }

        public PtfxEntityPlayer(string assetName, string effectName, Entity entity, Vector3 posOffset, Vector3 rot, float size = 1f, bool loop = false, bool doLoopHandling = false, int removeTime = 30) : base(assetName, effectName, posOffset, rot, size, loop, doLoopHandling, removeTime)
        {
            Entity = entity;
        }

        public override void SpawnCopy()
        {
            RequestPtfx();

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, AssetName);

            if (ShouldLoop)
            {
                ParticleId = Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_ON_ENTITY, EffectName, Entity.Handle, Position.X, Position.Y, Position.Z, Rotation.X, Rotation.Y, Rotation.Z, Size, false, false, false);
                currentPlayingParticles.Add(ParticleId);

                foreach (var entry in evolutionParams)
                {
                    currentPlayingParticles.ForEach(x => Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, x, entry.Key, entry.Value, 0));
                }
            }
            else
            {
                Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, EffectName, Entity.Handle, Position.X, Position.Y, Position.Z, Rotation.X, Rotation.Y, Rotation.Z, Size, false, false, false);
            }
        }

        public void StopNonLooped()
        {
            Entity.RemoveParticleEffects();

            Stop();
        }
    }

    public class PtfxEntityBonePlayer : PtfxPlayer
    {
        public Entity Entity { get; }
        public string BoneName;

        public PtfxEntityBonePlayer(string assetName, string effectName, Entity entity, string boneName, Vector3 posOffset, Vector3 rot, float size = 1f, bool loop = false, bool doLoopHandling = false, int removeTime = 30) : base(assetName, effectName, posOffset, rot, size, loop, doLoopHandling, removeTime)
        {
            Entity = entity;
            BoneName = boneName;
        }

        public override void SpawnCopy()
        {
            RequestPtfx();

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, AssetName);

            if (ShouldLoop)
            {
                ParticleId = Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE, EffectName, Entity.Handle, Position.X, Position.Y, Position.Z, Rotation.X, Rotation.Y, Rotation.Z, Entity.Bones[BoneName].Index, Size, false, false, false);
                currentPlayingParticles.Add(ParticleId);

                foreach (var entry in evolutionParams)
                {
                    currentPlayingParticles.ForEach(x => Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, x, entry.Key, entry.Value, 0));
                }
            }
            else
            {
                Vector3 tPosition = Entity.Bones[BoneName].GetRelativeOffsetPosition(Position);

                Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, EffectName, Entity.Handle, tPosition.X, tPosition.Y, tPosition.Z, Rotation.X, Rotation.Y, Rotation.Z, Size, false, false, false);
            }
        }
    }
}
