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
    public class PTFX
    {
        private string assetName;
        private string ptfxName;

        public int Handle { get; private set; } = 0;

        public PTFX(int handle)
        {
            Handle = handle;
        }

        public PTFX(string assetName, string ptfxName, bool request = true)
        {
            this.assetName = assetName;
            this.ptfxName = ptfxName;

            if (request)
                RequestAsset();
        }

        public PTFX(string[] ptfx, bool request = true)
        {
            assetName = ptfx[0];
            ptfxName = ptfx[1];

            if (request)
                RequestAsset();
        }

        private void RequestAsset()
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, assetName);

            while (IsAssetLoaded() == false)
                Script.Yield();
        }

        public bool IsAssetLoaded()
        {
            return Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, assetName);
        }

        public int CreateLooped(Vector3 pos, Vector3 rot = default, float scale = 1.0F)
        {
            RequestAsset();

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, assetName);

            if (rot == default)
                Handle = Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_AT_COORD, ptfxName, pos.X, pos.Y, pos.Z, 0.0F, 0.0F, 0.0F, scale, false, false, false, false);
            else
                Handle = Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_AT_COORD, ptfxName, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, scale, false, false, false, false);

            return Handle;
        }

        public int CreateLooped(Entity entity, Vector3 offset, Vector3 rot = default, float scale = 1.0F)
        {
            RequestAsset();

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, assetName);

            if (rot == default)
                Handle = Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_ON_ENTITY, ptfxName, entity, offset.X, offset.Y, offset.Z, 0.0F, 0.0F, 0.0F, scale, false, false, false);
            else
                Handle = Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_ON_ENTITY, ptfxName, entity, offset.X, offset.Y, offset.Z, rot.X, rot.Y, rot.Z, scale, false, false, false);

            return Handle;
        }

        public int CreateLoopedOnEntityBone(Entity entity, string boneName, Vector3 offset, Vector3 rot = default, float scale = 1.0F)
        {
            RequestAsset();

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, assetName);

            if (rot == default)
                Handle = Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE, ptfxName, entity.Handle, offset.X, offset.Y, offset.Z, 0.0F, 0.0F, 0.0F, entity.Bones[boneName].Index, scale, false, false, false);
            else
                Handle = Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_ON_ENTITY_BONE, ptfxName, entity.Handle, offset.X, offset.Y, offset.Z, rot.X, rot.Y, rot.Z, entity.Bones[boneName].Index, scale, false, false, false);

            return Handle;
        }

        public void SetLoopedEvolution(string prop, float value)
        {
            Function.Call(Hash.SET_PARTICLE_FX_LOOPED_EVOLUTION, Handle, prop, value, 0);
        }

        public void SetLoopedScale(float scale)
        {
            Function.Call(Hash.SET_PARTICLE_FX_LOOPED_SCALE, Handle, scale);
        }

        public void ColorLooped(float r, float g, float b)
        {
            Function.Call(Hash.SET_PARTICLE_FX_LOOPED_COLOUR, Handle, r, g, b, 0);
        }

        public void AlphaLooped(float pAlpha)
        {
            Function.Call(Hash.SET_PARTICLE_FX_LOOPED_ALPHA, Handle, pAlpha);
        }

        public void OffsetsLooped(Vector3 oPosition, Vector3 oRotation)
        {
            Function.Call(Hash.SET_PARTICLE_FX_LOOPED_OFFSETS, Handle, oPosition.X, oPosition.Y, oPosition.Z, oRotation.X, oRotation.Y, oRotation.Z);
        }

        public void Delete()
        {
            Function.Call(Hash.REMOVE_PARTICLE_FX, Handle, 1);
            Handle = 0;
        }

        public void Stop()
        {
            Function.Call(Hash.STOP_PARTICLE_FX_LOOPED, Handle, 0);
            Handle = 0;
        }


        public bool Create(Vector3 pos, Vector3 rot = default, float scale = 1.0F)
        {
            RequestAsset();

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, assetName);

            bool ret;

            if (rot == default)
                ret = Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD, ptfxName, pos.X, pos.Y, pos.Z, 0.0F, 0.0F, 0.0F, scale, false, false, false);
            else
                ret = Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD, ptfxName, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, scale, false, false, false);

            return ret;
        }

        public bool Create(Entity entity, Vector3 offset, Vector3 rot = default, float scale = 1.0F)
        {
            RequestAsset();

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, assetName);

            bool ret;

            if (rot == default)
                ret = Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, ptfxName, entity, offset.X, offset.Y, offset.Z, 0.0F, 0.0F, 0.0F, scale, false, false, false);
            else
                ret = Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, ptfxName, entity, offset.X, offset.Y, offset.Z, rot.X, rot.Y, rot.Z, scale, false, false, false);

            return ret;
        }

        public bool CreateOnEntityBone(Entity entity, string boneName, Vector3 offset, Vector3 rot = default, float scale = 1.0F)
        {
            RequestAsset();

            Function.Call(Hash.USE_PARTICLE_FX_ASSET, assetName);

            offset = entity.Bones[boneName].GetRelativeOffsetPosition(offset);

            bool ret;

            if (rot == default)
                ret = Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, ptfxName, entity.Handle, offset.X, offset.Y, offset.Z, 0.0F, 0.0F, 0.0F, scale, false, false, false);
            else
                ret = Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, ptfxName, entity.Handle, offset.X, offset.Y, offset.Z, rot.X, rot.Y, rot.Z, scale, false, false, false);

            return ret;
        }

        public void Color(float r, float g, float b)
        {
            Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_COLOUR, r, g, b);
        }
    }
}
