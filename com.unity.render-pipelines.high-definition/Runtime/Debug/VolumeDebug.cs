using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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
        public VolumeStack selectedCameraVolumeStack
        {
            get
            {
                if (selectedCamera <= 0 || selectedCamera > cameras.Count)
                    return null;
                Camera cam = cameras[selectedCamera - 1].GetComponent<Camera>();
                var stack = HDCamera.GetOrCreate(cam).volumeStack;
                if (stack != null)
                    return stack;
                return VolumeManager.instance.stack;
            }
        }

        public LayerMask selectedCameraLayerMask
        {
            get
            {
                if (selectedCamera <= 0 || selectedCamera > cameras.Count)
                    return (LayerMask)0;
                return cameras[selectedCamera - 1].volumeLayerMask;
            }
        }

        public Vector3 selectedCameraPosition
        {
            get
            {
                if (selectedCamera <= 0 || selectedCamera > cameras.Count)
                    return Vector3.zero;
                Camera cam = cameras[selectedCamera - 1].GetComponent<Camera>();

                var anchor = HDCamera.GetOrCreate(cam).volumeAnchor;
                if (anchor == null) // means the hdcamera has not been initialized
                {
                    // So we have to update the stack manually
                    anchor = cameras[selectedCamera - 1].volumeAnchorOverride;
                    if (anchor == null) anchor = cam.transform;
                    VolumeManager.instance.Update(selectedCameraVolumeStack, anchor, selectedCameraLayerMask);
                }
                return anchor.position;
            }
        }

        /// <summary>Type of the current component to debug.</summary>
        public Type     selectedComponentType
        {
            get { return componentTypes[selectedComponent - 1]; }
            set
            {
                var index = componentTypes.FindIndex(t => t == value);
                if (index != -1)
                    selectedComponent = index + 1;
            }
        }

        static public List<Type> componentTypes
        {
            get
            {
                return VolumeManager.instance.baseComponentTypes
                    .Where(t => !t.IsDefined(typeof(VolumeComponentDeprecated), false))
                    .OrderBy(t => t.Name)
                    .ToList();
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
            var componentType = selectedComponentType;

            // Sort the cached volume list(s) for the given layer mask if needed and return it
            var volumes = VolumeManager.instance.GrabVolumes(selectedCameraLayerMask);
            var activeVolumes = new List<Volume>();

            // Traverse all volumes
            foreach (var volume in volumes)
            {
                // Skip disabled volumes and volumes without any data or weight
                if (!volume.enabled || volume.profileRef == null || volume.weight <= 0f)
                    continue;

                if (!volume.profileRef.Has(componentType))
                    continue;

                // Global volumes always have influence
                if (volume.isGlobal)
                {
                    activeVolumes.Add(volume);
                    continue;
                }

                // If volume isn't global and has no collider, skip it as it's useless
                if (volume.GetComponent<Collider>() == null)
                    continue;

                activeVolumes.Add(volume);
            }

            return activeVolumes.ToArray();
        }

        public VolumeParameter GetParameter(VolumeComponent component, FieldInfo field)
        {
            return (VolumeParameter)field.GetValue(component);
        }

        public VolumeParameter GetParameter(Type type, FieldInfo field)
        {
            VolumeStack stack = selectedCameraVolumeStack;
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
            if (!volume.profileRef.TryGet(type, out VolumeComponent component))
                return "Component Removed";

            var scope = volume.isGlobal ? "Global" : "Local";
            if (!component.active)
                return scope + " (Inactive)";

            float weight = Mathf.Clamp01(volume.weight);
            if (!volume.isGlobal)
            {
                var triggerPos = selectedCameraPosition;
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
                    weight = 0f;
                else if (blendDistSqr > 0f)
                    weight *= 1f - (closestDistanceSqr / blendDistSqr);
            }
            return scope + " (" + weight.ToString() + ")";
        }
    }
}
