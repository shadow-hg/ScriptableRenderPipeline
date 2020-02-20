using System.Collections.Generic;

using UnityEditor;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEditor.Rendering.HighDefinition
{
    internal class SerializedCompositionLayer
    {
        public SerializedProperty LayerName;
        public SerializedProperty Show;
        public SerializedProperty ResolutionScale;
        public SerializedProperty ExpandLayer;
        public SerializedProperty OutTarget;
        public SerializedProperty OutputRenderer;
        public SerializedProperty ClearDepth;
        public SerializedProperty ClearAlpha;
        public SerializedProperty InputLayerType;
        public SerializedProperty InputCamera;
        public SerializedProperty InputVideo;
        public SerializedProperty InputTexture;
        public SerializedProperty FitType;
        public SerializedProperty ColorFormat;
        public SerializedProperty OverrideAA;
        public SerializedProperty AAMode;
        public SerializedProperty OverrideClearMode;
        public SerializedProperty ClearMode;
        public SerializedProperty OverrideCulling;
        public SerializedProperty CullingMaskProperty;
        public SerializedProperty OverrideVolume;
        public SerializedProperty VolumeMask;
        public SerializedProperty AOVBitmask;
        public SerializedProperty InputFilters;

        public List<SerializedCompositionFilter> FilterList = new List<SerializedCompositionFilter>();

        public SerializedCompositionLayer(SerializedProperty root)
        {
            LayerName = root.FindPropertyRelative("m_LayerName");
            Show = root.FindPropertyRelative("m_Show");
            ResolutionScale = root.FindPropertyRelative("m_ResolutionScale");
            ExpandLayer = root.FindPropertyRelative("m_ExpandLayer");
            OutTarget = root.FindPropertyRelative("m_OutputTarget");
            ClearDepth = root.FindPropertyRelative("m_ClearDepth");
            ClearAlpha = root.FindPropertyRelative("m_ClearAlpha");
            OutputRenderer = root.FindPropertyRelative("m_OutputRenderer");
            InputLayerType = root.FindPropertyRelative("m_Type");
            InputCamera = root.FindPropertyRelative("m_Camera");
            InputVideo = root.FindPropertyRelative("m_InputVideo");
            InputTexture = root.FindPropertyRelative("m_InputTexture");
            FitType = root.FindPropertyRelative("m_BackgroundFit");
            ColorFormat = root.FindPropertyRelative("m_ColorBufferFormat");
            OverrideClearMode = root.FindPropertyRelative("m_OverrideClearMode");
            ClearMode = root.FindPropertyRelative("m_ClearMode");
            OverrideAA = root.FindPropertyRelative("m_OverrideAntialiasing");
            AAMode = root.FindPropertyRelative("m_Antialiasing");
            OverrideCulling = root.FindPropertyRelative("m_OverrideCullingMask");
            CullingMaskProperty = root.FindPropertyRelative("m_CullingMask");
            OverrideVolume = root.FindPropertyRelative("m_OverrideVolumeMask");
            VolumeMask = root.FindPropertyRelative("m_VolumeMask");
            AOVBitmask = root.FindPropertyRelative("m_AOVBitmask");
            InputFilters = root.FindPropertyRelative("m_InputFilters");

            for (int index = 0; index < InputFilters.arraySize; index++)
            {
                var serializedFilter = InputFilters.GetArrayElementAtIndex(index);
                FilterList.Add(new SerializedCompositionFilter(serializedFilter));
            }
        }

        public float GetHeight(bool detailed = false)
        {
            if (detailed)
            {
                //TODO: update this one when the UI is final
                return EditorGUI.GetPropertyHeight(LayerName, null) +
                    EditorGUI.GetPropertyHeight(Show, null) +
                    EditorGUI.GetPropertyHeight(OutTarget, null) +
                    EditorGUI.GetPropertyHeight(ClearDepth, null) +
                    EditorGUI.GetPropertyHeight(InputLayerType, null) +
                    EditorGUI.GetPropertyHeight(InputVideo, null) +
                    EditorGUI.GetPropertyHeight(OutputRenderer, null) +
                    EditorGUI.GetPropertyHeight(ColorFormat, null) +
                    EditorGUI.GetPropertyHeight(OverrideAA, null) +
                    EditorGUI.GetPropertyHeight(AAMode, null) +
                    EditorGUI.GetPropertyHeight(OverrideCulling, null) +
                    EditorGUI.GetPropertyHeight(CullingMaskProperty, null) +
                    EditorGUI.GetPropertyHeight(OverrideVolume, null) +
                    EditorGUI.GetPropertyHeight(VolumeMask, null);
            }
            else
            {
                int pading = 10;
                if (OutTarget.intValue != (int)CompositorLayer.OutputTarget.CameraStack)
                {
                    return CompositorStyle.k_ThumbnailSize + pading;
                }
                else
                {
                    return EditorGUIUtility.singleLineHeight + pading;
                }
            }
        }
    }
}
