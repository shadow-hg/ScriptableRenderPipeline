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

        /// <summary>Selected camera volume stack.</summary>
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

        /// <summary>Selected camera volume layer mask.</summary>
        public LayerMask selectedCameraLayerMask
        {
            get
            {
                if (selectedCamera <= 0 || selectedCamera > cameras.Count)
                    return (LayerMask)0;
                return cameras[selectedCamera - 1].volumeLayerMask;
            }
        }

        /// <summary>Selected camera volume position.</summary>
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

        /// <summary>List of Volume component types.</summary>
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

        /// <summary>List of HD Additional Camera data.</summary>
        static public List<HDAdditionalCameraData> cameras {get; private set; } = new List<HDAdditionalCameraData>();

        /// <summary>Register HDAdditionalCameraData for DebugMenu</summary>
        /// <param name="camera">The camera to register.</param>
        public static void RegisterCamera(HDAdditionalCameraData camera)
        {
            if (!cameras.Contains(camera))
                cameras.Add(camera);
        }

        /// <summary>Unregister HDAdditionalCameraData for DebugMenu</summary>
        /// <param name="camera">The camera to unregister.</param>
        public static void UnRegisterCamera(HDAdditionalCameraData camera)
        {
            if (cameras.Contains(camera))
                cameras.Remove(camera);
        }


        /// <summary>Get a VolumeParameter from a VolumeComponent</summary>
        /// <param name="component">The component to get the parameter from.</param>
        /// <param name="field">The field info of the parameter.</param>
        /// <returns>The volume parameter.</returns>
        public VolumeParameter GetParameter(VolumeComponent component, FieldInfo field)
        {
            return (VolumeParameter)field.GetValue(component);
        }

        /// <summary>Get a VolumeParameter from a VolumeComponent on the <see cref="selectedCameraVolumeStack"/></summary>
        /// <param name="field">The field info of the parameter.</param>
        /// <returns>The volume parameter.</returns>
        public VolumeParameter GetParameter(FieldInfo field)
        {
            VolumeStack stack = selectedCameraVolumeStack;
            return GetParameter(stack.GetComponent(selectedComponentType), field);
        }

        /// <summary>Get a VolumeParameter from a component of a volume</summary>
        /// <param name="volume">The volume to get the component from.</param>
        /// <param name="field">The field info of the parameter.</param>
        /// <returns>The volume parameter.</returns>
        public VolumeParameter GetParameter(Volume volume, FieldInfo field)
        {
            if (!volume.profileRef.TryGet(selectedComponentType, out VolumeComponent component))
                return null;
            var param = GetParameter(component, field);
            if (!param.overrideState)
                return null;
            return param;
        }

        float[] weights = null;
        float ComputeWeight(Volume volume)
        {
            if (!volume.gameObject.activeInHierarchy) return 0;
            if (!volume.enabled || volume.profileRef == null || volume.weight <= 0f) return 0;
            if (!volume.profileRef.TryGet(selectedComponentType, out VolumeComponent component)) return 0;
            if (!component.active) return 0;

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
            return weight;
        }

        Volume[] volumes = null;

        /// <summary>Get an array of volumes on the <see cref="selectedCameraLayerMask"/></summary>
        /// <returns>An array of volumes sorted by influence.</returns>
        public Volume[] GetVolumes()
        {
            return VolumeManager.instance.GetVolumes(selectedCameraLayerMask).Reverse().ToArray();
        }

        /// <summary>Updates the list of volumes and recomputes volume weights</summary>
        /// <param name="newVolumes">The new list of volumes.</param>
        /// <returns>True if the volume list have been updated.</returns>
        public bool RefreshVolumes(Volume[] newVolumes)
        {
            bool ret = false;
            if (volumes == null || !newVolumes.SequenceEqual(volumes))
            {
                volumes = (Volume[])newVolumes.Clone();
                weights = null;
                ret = true;
            }

            float total = 0f;
            weights = new float[volumes.Length];
            for (int i = 0; i < volumes.Length; i++)
            {
                float weight = ComputeWeight(volumes[i]);
                if (i != 0)
                    weight *= 1f - total;
                weights[i] = Mathf.Clamp01(weight);
                total += weight;
            }

            return ret;
        }

        /// <summary>Get the weight of a volume computed form the <see cref="selectedCameraPosition"/></summary>
        /// <param name="volume">The volume to compute weight for.</param>
        /// <returns>The weight of the volume.</returns>
        public float GetVolumeWeight(Volume volume)
        {
            if (weights == null)
                return 0;

            int index = Array.IndexOf(volumes, volume);
            if (index == -1)
                return 0;

            return weights[index];
        }
    }
}
