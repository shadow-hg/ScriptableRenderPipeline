using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace UnityEditor.Rendering.HighDefinition
{
    static internal class CompositorStyle
    {
        internal static readonly int k_ThumbnailSize = 32;
        internal static readonly int k_IconSize = 28;
        internal static readonly int k_ListItemPading = 4;
        internal static readonly int k_ListItemStackPading = 20;
        internal static readonly float k_SingleLineHeight = EditorGUIUtility.singleLineHeight;
        internal static readonly float k_Spacing = k_SingleLineHeight * 1.1f;

        internal static readonly Texture2D videoIcon = UnityEditor.AssetDatabase.LoadAssetAtPath <Texture2D>(HDUtils.GetHDRenderPipelinePath() + "Editor/Compositor/Icons/Video.png");
        internal static readonly Texture2D imageIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(HDUtils.GetHDRenderPipelinePath() + "Editor/Compositor/Icons/Layer.png");
        internal static readonly Texture2D cameraIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(HDUtils.GetHDRenderPipelinePath() + "Editor/Compositor/Icons/Camera.png");
    }
}
