using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;

namespace FusionLibrary
{
    public unsafe class VehicleBone
    {
        private readonly phArchetypeDamp* archetype;

        public Vehicle Vehicle { get; }
        public int Index { get; }
        public Matrix Matrix
        {
            get => archetype->skeleton->desiredBonesMatricesArray[Index];
            set => archetype->skeleton->desiredBonesMatricesArray[Index] = value;
        }

        public Vector3 OriginalTranslation { get; }
        public Quaternion OriginalRotation { get; }

        private VehicleBone(Vehicle vehicle, int index)
        {
            this.Vehicle = vehicle;
            this.Index = index;

            CVehicle* veh = ((CVehicle*)vehicle.MemoryAddress);
            archetype = veh->inst->archetype;

            OriginalTranslation = vehicle.GetBoneOriginalTranslation(index);
            OriginalRotation = vehicle.GetBoneOriginalRotation(index);
        }

        public void RotateAxis(Vector3 axis, float degrees)
        {
            NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[Index]);
            Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.RotationAxis(axis, degrees.ToRad()) * (*matrix);
            *matrix = newMatrix;
        }

        public void Translate(Vector3 translation)
        {
            NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[Index]);
            Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(translation) * (*matrix);
            *matrix = newMatrix;
        }

        public void SetRotation(Quaternion rotation)
        {
            NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[Index]);
            Utils.Decompose(*matrix, out Vector3 scale, out _, out Vector3 translation);
            Matrix newMatrix = Matrix.Scaling(scale) * Matrix.RotationQuaternion(rotation) * Matrix.Translation(translation);
            *matrix = newMatrix;
        }

        public void SetTranslation(Vector3 translation)
        {
            NativeMatrix4x4* matrix = &(archetype->skeleton->desiredBonesMatricesArray[Index]);
            matrix->M41 = translation.X;
            matrix->M42 = translation.Y;
            matrix->M43 = translation.Z;
        }

        public void ResetRotation() => SetRotation(OriginalRotation);
        public void ResetTranslation() => SetTranslation(OriginalTranslation);


        public static bool TryGetForVehicle(Vehicle vehicle, string boneName, out VehicleBone bone)
        {
            int boneIndex = vehicle.GetBoneIndex(boneName);
            if (boneIndex == -1)
            {
                bone = null;
                return false;
            }

            bone = new VehicleBone(vehicle, boneIndex);
            return true;
        }
    }
}