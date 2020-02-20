using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

using UnityEditor;

namespace UnityEditor.Rendering.HighDefinition
{
    internal class CompositionUtils
    {
        static public void LoadDefaultCompositionGraph(CompositionManager compositor)
        {
            compositor.shader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(HDUtils.GetHDRenderPipelinePath() + "Runtime/Compositor/ShaderGraphs/DefaultCompositionGraph.shadergraph");
        }

        static public void SetDefaultCamera(CompositionManager compositor)
        {
            compositor.shader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(HDUtils.GetHDRenderPipelinePath() + "Runtime/Compositor/ShaderGraphs/DefaultCompositionGraph.shadergraph");
            var camera = CompositionManager.GetSceceCamera();
            if (camera != null)
            {
                var outputCamera = Object.Instantiate(camera);
                outputCamera.name = "MainCompositorCamera";
                outputCamera.cullingMask = 0; // we don't want to render any 3D objects on the compositor camera
                compositor.outputCamera = outputCamera;
            }
        }

        static public void LoadOrCreateCompositionProfileAsset(CompositionManager compositor)
        {
            var shader = compositor.shader;
            var fullpath = AssetDatabase.GetAssetPath(shader);
            var path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fullpath), System.IO.Path.GetFileNameWithoutExtension(shader.name)) + ".asset";

            CompositionProfile newProfile = AssetDatabase.LoadAssetAtPath<CompositionProfile>(path);

            if (newProfile == null)
            {
                Debug.Log($"Creating new composition profile asset at path: {path}");

                newProfile = ScriptableObject.CreateInstance<CompositionProfile>();

                // path = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(newProfile, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log($"Loading composition profile from {path}");
            }
            compositor.profile = newProfile;
        }

        public void CloneCompositionProfile()
        {
            /*
            // First find the path we are going to clone to
            Scene scene = SceneManager.GetActiveScene();
            var scenePath = System.IO.Path.GetDirectoryName(scene.path);
            var extPath = scene.name;
            var profilePath = scenePath + "/" + extPath;

            if (!AssetDatabase.IsValidFolder(profilePath))
                AssetDatabase.CreateFolder(scenePath, extPath);

            var basePath = profilePath + "/";

            // First clone the shader graph 
            Shader shaderClone = Object.Instantiate(m_compositionManager.shader);
            shaderClone.name = "CompositionGraphClone";
            var targetShanderPath = basePath + m_compositionManager.shader.name + "Clone.shadergraph";
            {
                var shaderDir = System.IO.Path.GetDirectoryName(targetShanderPath);
                var newDirName = System.IO.Path.GetDirectoryName(m_compositionManager.shader.name);
                if (!AssetDatabase.IsValidFolder(shaderDir))
                    AssetDatabase.CreateFolder(profilePath, newDirName);
            }
            targetShanderPath = AssetDatabase.GenerateUniqueAssetPath(targetShanderPath);

            // Now clone the profile (.asset file)
            CompositionProfile profileClone = Object.Instantiate(m_compositionManager.profile);
            var targetProfilePath = basePath + m_compositionManager.shader.name + "Clone.asset";
            targetProfilePath = AssetDatabase.GenerateUniqueAssetPath(targetProfilePath);

            // Set the compositor to use the clones
            m_compositionManager.shader = shaderClone;
            m_compositionManager.profile = profileClone;

            // Make new assets with the clones
            AssetDatabase.CreateAsset(shaderClone, targetShanderPath);
            AssetDatabase.CreateAsset(profileClone, targetProfilePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            m_compositionManager.SetNewCompositionShader();
            m_IsEditorDirty = true;
            */
        }
    }
}
