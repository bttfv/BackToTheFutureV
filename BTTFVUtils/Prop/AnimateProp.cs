using GTA;
using GTA.Math;
using GTA.UI;
using GTA.Native;
using System.Collections.Generic;

namespace FusionLibrary
{
    public class AnimateProp
    {
        public Prop Prop { get; protected set; }
        public Entity EntityAttachedTo { get; set; }
        public Model Model { get; set; }
        public Vector3 Offset { get; set; }
        public Vector3 Rotation { get; set; }
        public string Bone { get; set; } = "";
        public bool IsSpawned { get; protected set; }
        public Dictionary<string, dynamic> Properties { get; set; } = new Dictionary<string, dynamic>();

        public float Duration = 0;
       
        public AnimateProp(Entity attachTo, Model model, Vector3 offset, Vector3 rotation)
        {
            EntityAttachedTo = attachTo;
            Model = model;
            Offset = offset;
            Rotation = rotation;

            AnimatePropsHandler.AddAnimateProp(this);
        }

        public AnimateProp(Entity attachTo, Model model, string bone) 
            : this(attachTo, model, Vector3.Zero, Vector3.Zero)
        {
            Bone = bone;
        }

        protected Vector3 _secondOffset = Vector3.Zero;
        protected Vector3 _secondRotation = Vector3.Zero;
        protected float _currentTime = 0;

        public void SpawnProp(bool deletePreviousProp = true)
        {
            _secondOffset = Vector3.Zero;
            _secondRotation = Vector3.Zero;

            // Delete the old prop
            if(deletePreviousProp)
                DeleteProp();

            if(!Model.IsValid || !Model.IsInCdImage)
            {
                Screen.ShowSubtitle(Model.ToString() + " not valid!");
            }

            // Request the model and wait for it to load
            Model.Request();

            while (!Model.IsLoaded)
                Script.Yield();

            // Spawn the new prop
            if (deletePreviousProp || Prop == null || !Prop.Exists())
                Prop = World.CreateProp(Model, Vector3.Zero, false, false);

            if(Bone == "")
                Prop?.AttachTo(EntityAttachedTo, Offset, Rotation);
            else if(EntityAttachedTo.Bones.Contains(Bone))
            {
                EntityBone bone = EntityAttachedTo.Bones[Bone];
                Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, Prop.Handle, bone.Owner.Handle, bone.Index, 0f, 0f, 0f, 0f, 0f, 0f, 0, 0, 0, 0, 2, 1);
            }

            Prop.IsPersistent = true;
            IsSpawned = true;
        }

        public virtual void SpawnProp(Vector3 offset, Vector3 rotation, bool deletePreviousProp = true)
        {
            _secondOffset = offset;
            _secondRotation = rotation;

            // Delete the old prop
            if (deletePreviousProp)
                DeleteProp();

            if (!Model.IsValid || !Model.IsInCdImage)
            {
                Screen.ShowSubtitle(Model.ToString() + " not valid!");
            }

            // Request the model and wait for it to load
            Model.Request();

            while (!Model.IsLoaded)
                Script.Yield();

            // Spawn the new prop
            if (deletePreviousProp || Prop == null || !Prop.Exists())
                Prop = World.CreateProp(Model, Vector3.Zero, false, false);

            if (Bone == "")
                Prop?.AttachTo(EntityAttachedTo, Offset + offset, Rotation + rotation);
            else if(EntityAttachedTo.Bones.Contains(Bone))
            {
                EntityBone bone = EntityAttachedTo.Bones[Bone];
                Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, Prop.Handle, bone.Owner.Handle, bone.Index, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, 0, 0, 0, 0, 2, 1);
            }

            Prop.IsPersistent = true;
            IsSpawned = true;
        }

        public void ProcessPropExistance()
        {
            if (IsSpawned && (Prop == null || !Prop.Exists()))
            {
                SpawnProp(_secondOffset, _secondRotation);
            }
                
            if (IsSpawned && Prop.Exists() && Duration > 0)
            {
                _currentTime += Game.LastFrameTime;

                if (_currentTime > Duration)
                    DeleteProp();
            }
        }

        public void DeleteProp()
        {
            // Delete the prop if it exists
            IsSpawned = false;
            _currentTime = 0;
            Prop?.Delete();
            Prop = null;
        }

        public void Dispose()
        {
            DeleteProp();
            AnimatePropsHandler.RemoveAnimateProp(this);
        }

        public void ToggleState()
        {
            if (!IsSpawned)
                SpawnProp();
            else
                DeleteProp();
        }

        public void SetState(bool state)
        {
            if (state)
                SpawnProp();
            else
                DeleteProp();
        }
    }

    public class AnimatePropPhysically : AnimateProp
    {
        public AnimatePropPhysically(Entity attachTo, Model model, string bone) : base(attachTo, model, bone)
        {
        }

        public AnimatePropPhysically(Entity attachTo, Model model, Vector3 offset, Vector3 rotation) : base(attachTo, model, offset, rotation)
        {
        }

        public override void SpawnProp(Vector3 offset, Vector3 rotation, bool deletePreviousProp = true)
        {
            _secondOffset = offset;
            _secondRotation = rotation;

            // Delete the old prop
            if (deletePreviousProp)
                DeleteProp();

            if (!Model.IsValid || !Model.IsInCdImage)
            {
                Screen.ShowSubtitle(Model.ToString() + " not valid!");
            }

            // Request the model and wait for it to load
            Model.Request();

            while (!Model.IsLoaded)
                Script.Yield();

            // Spawn the new prop
            if (deletePreviousProp || Prop == null || !Prop.Exists())
                Prop = World.CreateProp(Model, Vector3.Zero, false, false);

            if (Bone == "")
                Prop?.AttachTo(EntityAttachedTo, Offset + offset, Rotation + rotation);
            else if (EntityAttachedTo.Bones.Contains(Bone))
            {
                EntityBone bone = EntityAttachedTo.Bones[Bone];
                Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY, Prop.Handle, bone.Owner.Handle, 0, bone.Index, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, 0, 0, 0, 0, 0, 0, 1000000.0f, true, true, false, false, 2);
            }

            Prop.IsPersistent = true;
            IsSpawned = true;
        }
    }
}
    