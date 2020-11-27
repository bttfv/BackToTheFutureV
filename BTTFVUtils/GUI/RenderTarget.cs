using GTA;
using GTA.Math;
using GTA.Native;

namespace FusionLibrary
{
    public delegate void OnRenderTargetDraw();

    public class RenderTarget
    {
        public RenderTarget(Model propModel, string renderTargetName)
        {
            Function.Call(Hash.RELEASE_NAMED_RENDERTARGET, renderTargetName);

            if (!Function.Call<bool>(Hash.IS_NAMED_RENDERTARGET_REGISTERED, renderTargetName))
                Function.Call(Hash.REGISTER_NAMED_RENDERTARGET, renderTargetName, 0);

            if (!Function.Call<bool>(Hash.IS_NAMED_RENDERTARGET_LINKED, propModel.Hash))
                Function.Call(Hash.LINK_NAMED_RENDERTARGET, propModel.Hash);

            RenderTargetId = Function.Call<int>(Hash.GET_NAMED_RENDERTARGET_RENDER_ID, renderTargetName);
        }

        public RenderTarget(Model propModel, string renderTargetName, Entity attachTo, string bone) : this(propModel, renderTargetName)
        {
            Prop = new AnimateProp(attachTo, propModel, bone);
        }

        public RenderTarget(Model propModel, string renderTargetName, Entity attachTo, Vector3 offset) : this(propModel, renderTargetName)
        {
            Prop = new AnimateProp(attachTo, propModel, offset, Vector3.Zero);
        }

        public RenderTarget(Model propModel, string renderTargetName, Entity attachTo, Vector3 offset, Vector3 rotation) : this(propModel, renderTargetName)
        {
            Prop = new AnimateProp(attachTo, propModel, offset, rotation);
        }

        public int RenderTargetId { get; private set; }
        public AnimateProp Prop { get; private set; }
        public OnRenderTargetDraw OnRenderTargetDraw { get; set; }

        public void CreateProp()
        {
            Prop.SpawnProp();
        }

        public void DeleteProp()
        {
            Prop.DeleteProp();
        }

        public void Dispose()
        {
            Prop?.Dispose();
        }

        public void Draw()
        {
            Function.Call(Hash.SET_TEXT_RENDER_ID, RenderTargetId);

            OnRenderTargetDraw.Invoke();

            Function.Call(Hash.SET_TEXT_RENDER_ID, Function.Call<int>(Hash.GET_DEFAULT_SCRIPT_RENDERTARGET_RENDER_ID));
        }
    }
}
