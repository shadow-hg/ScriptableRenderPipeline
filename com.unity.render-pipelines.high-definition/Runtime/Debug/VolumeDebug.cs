using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.Rendering.HighDefinition
{
    /// <summary>
    /// Volume debug settings.
    /// </summary>
    public class VolumeDebugSettings
    {
        /// <summary>Current camera to debug.</summary>
        public int      selectedCamera = 0;
        /// <summary>Current volume component to debug.</summary>
        public int      selectedComponent = 0;

        /// <summary>Current HD camera selected.</summary>
        public HDCamera selectedHDCamera
        {
            get
            {
                if (selectedCamera <= 0 || selectedCamera > cameras.Count)
                    return null;
                return HDCamera.GetOrCreate(cameras[selectedCamera - 1].GetComponent<Camera>());
            }
        }

        /// <summary>Type of the current component to debug.</summary>
        public Type     selectedComponentType
        {
            get
            {
                int value = selectedComponent;
                foreach (var t in VolumeManager.instance.baseComponentTypes)
                {
                    if (--value == 0)
                        return t;
                }
                return null;
            }
            set
            {
                int i = 0;
                foreach (var t in VolumeManager.instance.baseComponentTypes)
                {
                    if (t == value)
                    {
                        selectedComponent = i;
                        selectedComponentType = t;
                        return;
                    }
                    i++;
                }
            }
        }

        static public List<HDAdditionalCameraData> cameras {get; private set; } = new List<HDAdditionalCameraData>();

        public static void RegisterCamera(HDAdditionalCameraData camera)
        {
            if (!cameras.Contains(camera))
                cameras.Add(camera);
        }

        public static void UnRegisterCamera(HDAdditionalCameraData camera)
        {
            if (cameras.Contains(camera))
                cameras.Remove(camera);
        }


        public Volume[] GetVolumes()
        {
            return VolumeManager.instance.GetActiveVolumes(selectedComponentType, selectedHDCamera.volumeLayerMask);
        }

        public VolumeParameter GetParameter(VolumeComponent component, FieldInfo field)
        {
            return (VolumeParameter)field.GetValue(component);
        }

        public VolumeParameter GetParameter(Type type, FieldInfo field)
        {
            VolumeStack stack = VolumeManager.instance.stack;

            var camera = selectedHDCamera;
            if (camera != null)
                stack = camera.volumeStack;
            if (stack == null)
                return null;

            return GetParameter(stack.GetComponent(type), field);
        }

        public VolumeParameter GetParameter(Volume volume, Type type, FieldInfo field)
        {
            if (!volume.profileRef.TryGet(type, out VolumeComponent component))
                return null;
            var param = GetParameter(component, field);
            if (!param.overrideState)
                return null;
            return param;
        }

        public string GetVolumeInfo(Volume volume, Type type)
        {
            if (volume.isGlobal)
                return "Global (" + Mathf.Clamp01(volume.weight) + ")";

            var triggerPos = selectedHDCamera.volumeAnchor.position;
            var colliders = volume.GetComponents<Collider>();

            // Find closest distance to volume, 0 means it's inside it
            float closestDistanceSqr = float.PositiveInfinity;
            foreach (var collider in colliders)
            {
                if (!collider.enabled)
                    continue;

                var closestPoint = collider.ClosestPoint(triggerPos);
                var d = (closestPoint - triggerPos).sqrMagnitude;

                if (d < closestDistanceSqr)
                    closestDistanceSqr = d;
            }
            float blendDistSqr = volume.blendDistance * volume.blendDistance;
            if (closestDistanceSqr > blendDistSqr)
                return "Local (0)";

            float interpFactor = 1f;
            if (blendDistSqr > 0f)
                interpFactor = 1f - (closestDistanceSqr / blendDistSqr);
            return "Local (" + (interpFactor * Mathf.Clamp01(volume.weight)) + ")";
        }
    }
}
