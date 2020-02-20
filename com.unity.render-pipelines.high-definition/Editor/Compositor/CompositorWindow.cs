using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition.Compositor;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

namespace UnityEditor.Rendering.HighDefinition.Compositor
{
    internal class CompositorWindow : EditorWindow
    {
        static partial class TextUI
        {
            static public readonly GUIContent EnableCompositor = EditorGUIUtility.TrTextContent("Enable Compositor", "Enabled the compositor and creates a default composition profile.");
            static public readonly GUIContent RemoveCompositor = EditorGUIUtility.TrTextContent("Remove compositor from scene", "Removes the compositor and any composition settings from the scene.");
        }

        static CompositorWindow s_Window;
        CompositionManagerEditor m_Editor;
        Vector2 m_ScrollPosition = Vector2.zero;

        [MenuItem("Window/Render Pipeline/HD Render Pipeline Compositor", false, 10400)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            s_Window = (CompositorWindow)EditorWindow.GetWindow(typeof(CompositorWindow));
            s_Window.titleContent = new GUIContent("Compositor (Preview)");
            s_Window.Show();
        }

        void Update()
        {
            // This ensures that layer thumbnails are updated every frame (for video layers)
            Repaint();
        }

        void OnGUI()
        {
            CompositionManager compositor = GameObject.FindObjectOfType(typeof(CompositionManager), true) as CompositionManager;
            bool enableCompositor = false;
            if (compositor)
            {
                enableCompositor = compositor.enabled;
            }
            enableCompositor = EditorGUILayout.Toggle(TextUI.EnableCompositor, enableCompositor);

            if (compositor == null && enableCompositor)
            {
                Debug.Log("The scene does not have a compositor. Creating a new one with the default configuration.");
                GameObject go = new GameObject("HDRP Compositor") { hideFlags = HideFlags.HideInHierarchy };
                compositor = go.AddComponent<CompositionManager>();

                // Now add the default configuration
                CompositionUtils.LoadDefaultCompositionGraph(compositor);
                CompositionUtils.LoadOrCreateCompositionProfileAsset(compositor);
                CompositionUtils.SetDefaultCamera(compositor);
            }

            if (compositor)
            {
                compositor.enabled = enableCompositor;
            }
            else
            {
                return;
            }

            if (compositor && compositor.enabled == false)
            {
                if (GUILayout.Button(new GUIContent("Remove compositor from scene")))
                {
                    CoreUtils.Destroy(compositor);
                    return;
                }
            }

            if (compositor.profile == null)
            {
                // The compositor was loaded, but there was no profile (someone deleted the asset from disk?), so create a new one
                CompositionUtils.LoadOrCreateCompositionProfileAsset(compositor);
                compositor.SetupCompositionMaterial();
                return;
            }

            if (m_Editor == null || m_Editor.target == null || m_Editor.isDirty || compositor.redraw)
            {
                m_Editor = (CompositionManagerEditor)Editor.CreateEditor(compositor);
                compositor.redraw = false;
            }

            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            using (new EditorGUI.DisabledScope(compositor.enabled == false))
            {
                if (m_Editor)
                {
                    m_Editor.OnInspectorGUI();
                }
            }
            GUILayout.EndScrollView();
        }
    }
}
#endif
