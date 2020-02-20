using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

namespace UnityEditor.Rendering.HighDefinition
{
    internal class CompositorWindow : EditorWindow
    {
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
            enableCompositor = EditorGUILayout.Toggle("Enable Compositor", enableCompositor);

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

            if(compositor.profile == null)
            {
                // The compositor was loaded, but there was no profile (someone deleted the asset from disk?), so create a new one
                CompositionUtils.LoadOrCreateCompositionProfileAsset(compositor);
                compositor.SetupCompositionMaterial();
            }
            
            //if (m_Editor == null || m_Editor.target == null || m_Editor.isDirty || compositor.isDirty)
            if (m_Editor == null || m_Editor.target == null || m_Editor.isDirty)
            {
                m_Editor = (CompositionManagerEditor)Editor.CreateEditor(compositor);
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
