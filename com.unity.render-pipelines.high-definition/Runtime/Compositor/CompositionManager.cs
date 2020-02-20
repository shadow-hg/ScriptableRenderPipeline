using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.HighDefinition.Attributes;
using UnityEngine.Video;

using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.HighDefinition
{
    // The main entry point for the compositing operations. Manages the list of layers, output displays, etc.
    [ExecuteAlways]
    internal class CompositionManager : MonoBehaviour
    {
        public enum OutputDisplay
        {
            Display1 = 0,
            Display2,
            Display3,
            Display4,
            Display5,
            Display6,
            Display7,
            Display8
        }

        [SerializeField]
        Shader m_Shader = null;

        [HideInInspector, SerializeField]
        Material m_Material;

        public Camera m_OutputCamera = null;

        [SerializeField]
        public OutputDisplay m_OutputDisplay = OutputDisplay.Display1;

        [HideInInspector]
        public List<CompositorLayer> m_InputLayers = new List<CompositorLayer>();

        [HideInInspector]
        public CompositionProfile m_CompositionProfile;

        internal Matrix4x4 m_ViewProjMatrix;
        internal Matrix4x4 m_ViewProjMatrixFlipped;
        internal GameObject m_CompositorGameObject;

        internal bool m_IsCompositorDirty;

        public bool isDirty
        {
            get => m_IsCompositorDirty;
        }

        internal class SGShaderIDs
        {
            public static readonly int _ViewProjMatrix = Shader.PropertyToID("_ViewProjMatrix");
            public static readonly int _WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
            public static readonly int _ProjectionParams = Shader.PropertyToID("_ProjectionParams");
        }
        public bool enableOutput
        {
            get
            {
                if (m_OutputCamera)
                {
                    return m_OutputCamera.enabled;
                }
                return false;
            }
            set
            {
                if (m_OutputCamera)
                {
                    m_OutputCamera.enabled = value;
                }
            }
        }

        public int numLayers
        {
            get
            {
                if (m_CompositionProfile)
                {
                    return m_CompositionProfile.m_InputLayers.Count;
                }
                return 0;
            }
        }
        public Shader shader
        {
            get => m_Shader;
            set
            {
                m_Shader = value;
            }
        }

        public CompositionProfile profile
        {
            get => m_CompositionProfile;
            set
            {
                m_CompositionProfile = value;
                ValidateProfile();
            }
        }

        public Camera outputCamera
        {
            get => m_OutputCamera;
            set
            {
                m_OutputCamera = value;
            }
        }

        public float aspectRatio
        {
            get
            {
                if (m_CompositionProfile)
                {
                    return m_CompositionProfile.aspectRatio;
                }
                return 1.0f;
            }
        }

        #region Validation

        void ValidatePipeline()
        {
            var hdPipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
            if (hdPipeline != null)
            {
                if (hdPipeline.asset.currentPlatformRenderPipelineSettings.colorBufferFormat == RenderPipelineSettings.ColorBufferFormat.R11G11B10)
                {
                    Debug.LogWarning("The rendering pipeline was not configured to output an alpha channel. It is recommended to set the color buffer format for rendering and post-processing to a format that supports an alpha channel.");
                }
                else if (hdPipeline.asset.currentPlatformRenderPipelineSettings.postProcessSettings.bufferFormat == PostProcessBufferFormat.R11G11B10)
                {
                    Debug.LogWarning("The post processing system is not configured to process the alpha channel. It is recommended to set the color buffer format for rendering and post-processing to a format that supports an alpha channel.");
                }

                int indx = hdPipeline.asset.beforePostProcessCustomPostProcesses.FindIndex(x => x == typeof(ChromaKeying).AssemblyQualifiedName);
                if (indx < 0)
                {
                    Debug.Log("Registering chroma keying pass for the HDRP pipeline");
                    hdPipeline.asset.beforePostProcessCustomPostProcesses.Add(typeof(ChromaKeying).AssemblyQualifiedName);
                }

                indx = hdPipeline.asset.beforePostProcessCustomPostProcesses.FindIndex(x => x == typeof(AlphaInjection).AssemblyQualifiedName);
                if (indx < 0)
                {
                    Debug.Log("Registering alpha injection pass for the HDRP pipeline");
                    hdPipeline.asset.beforePostProcessCustomPostProcesses.Add(typeof(AlphaInjection).AssemblyQualifiedName);
                }

            }
        }
        bool ValidateCompositionShader()
        {
            if (m_Shader == null)
            {
                return false;
            }

            if (m_CompositionProfile == null)
            {
                Debug.Log("A composition profile was not found. Set the composition graph from the Compositor window to create one.");
                return false;
            }

            return true;
        }

        bool ValidateProfile()
        {
            if (m_CompositionProfile)
            {
                UpdateLayerList();
                return true;
            }
            else
            {
                Debug.LogError("No composition profile was found! Use the compositor tool to create one.");
                return false;
            }
        }

        bool ValidateMainCompositorCamera()
        {
            if (m_OutputCamera == null)
            {
                return false;
            }
            UpdateDisplayNumber();

            // Setup custom rendering (we don't want HDRP to compute anything in this camera)
            var cameraData = m_OutputCamera.GetComponent<HDAdditionalCameraData>();
            if (cameraData)
            {
                cameraData.customRender += CustomRender;
            }
            else
            {
                Debug.LogError("The compositor should be used with an HDRP camera");
                return false;
            }
            return true;
        }

        bool ValidateAndFixRuntime()
        {
            if (m_OutputCamera == null)
            {
                Debug.Log("No camera was found");
                return false;
            }

            if (m_Shader == null)
            {
                Debug.Log("The composition shader graph must be set");
                return false;
            }

            if (m_CompositionProfile == null)
            {
                Debug.Log("The composition profile was not set at runtime");
                return false;
            }

            if (m_Material == null)
            {
                Debug.Log("Null Material!");
                SetupCompositionMaterial();
            }

            var cameraData = m_OutputCamera.GetComponent<HDAdditionalCameraData>();
            if (cameraData && !cameraData.hasCustomRender)
            {
                Debug.Log("Validation error: the camera did not had a custom render callback! Registering a new callback.");
                cameraData.customRender += CustomRender;
            }

            return true;
        }
        #endregion

        // This is called when we change camera, to remove the custom draw callback from the old camera before we set the new one
        public void DropCompositorCamera()
        {
            if (m_OutputCamera)
            {
                var cameraData = m_OutputCamera.GetComponent<HDAdditionalCameraData>();
                if (cameraData && cameraData.hasCustomRender)
                {
                    cameraData.customRender -= CustomRender;
                }
            }
        }

        public void Init()
        {
            if ( ValidateCompositionShader() || ValidateProfile() || ValidateMainCompositorCamera())
            {
                SetupCompositionMaterial();

                SetupCompositorLayers();

                SetupGlobalCompositorVolume();

                SetupCompositorConstants();

                SetupLayerPriorities();
            }
            else
            {
                Debug.LogError("The compositor was disabled due to a validation error in the configuration.");
                enabled = false;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        public void UpdateLayerList()
        {
            if (m_CompositionProfile)
            {
                m_CompositionProfile.m_InputLayers = m_InputLayers; 
            }
        }

        void OnValidate()
        {
            m_IsCompositorDirty = true;
            UpdateLayerList();
        }

        public void OnEnable()
        {
            enableOutput = true;
#if UNITY_EDITOR
            //This is a work-around, to make edit and continue work when editing source code
            UnityEditor.AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
#endif
        }

        public void DeleteLayerRTs()
        {
            // delete the layer from last to first, in order to release first the camera and then the associated RT
            for (int i = m_CompositionProfile.m_InputLayers.Count - 1; i >= 0; --i)
            {
                m_CompositionProfile.m_InputLayers[i].DestroyRT();
            }
        }

        public bool IsOutputLayer(int layerID)
        {
            if (layerID >= 0 && layerID < m_CompositionProfile.m_InputLayers.Count)
            {
                if (m_CompositionProfile.m_InputLayers[layerID].GetOutputTarget() == CompositorLayer.OutputTarget.CameraStack)
                {
                    return false;
                }
            }
            return true;
        }

        public void UpdateDisplayNumber()
        {
            if (m_OutputCamera)
            {
                m_OutputCamera.targetDisplay = (int)m_OutputDisplay;
            }
        }

        void SetupCompositorLayers()
        {
            if (m_CompositionProfile)
            {
                m_CompositionProfile.Init();
            }
        }

        public void SetNewCompositionShader()
        {
            // When we load a new shader, we need to clear the serialized material. 
            m_Material = null;
            //m_CompositionProfile.ClearShaderProperties();
            SetupCompositionMaterial();
        }

        public void SetupCompositionMaterial()
        {
            // Create the composition material
            if (m_Shader)
            {
                if (m_Material == null)
                {
                    m_Material = new Material(m_Shader);
                }

                m_CompositionProfile.AddPropertiesFromShaderAndMaterial(m_Shader, m_Material);

                //TODO: this is mostly a hack, the compositor profile was not serialized properly, and for this reason we also keep a reference of this list outside of the profile.
                m_InputLayers = m_CompositionProfile.m_InputLayers;
            }
            else
            {
                Debug.LogError("Cannot find the default composition graph. Was the installation folder corrupted?");
                m_Material = null;
            }
        }

        public void SetupLayerPriorities()
        {
            if (m_CompositionProfile)
            {
                m_CompositionProfile.SetDrawOrder();
            }
        }

        public void OnAfterAssemblyReload()
        {
            // Bug? : After assembly reload, the customRender callback is dropped, so set it again
            var cameraData = m_OutputCamera.GetComponent<HDAdditionalCameraData>();
            if (cameraData && !cameraData.hasCustomRender)
            {
                cameraData.customRender += CustomRender;
            }
        }

        public void OnDisable()
        {
            enableOutput = false;
        }

        // Setup a global volume used for chroma keying, alpha injection etc
        void SetupGlobalCompositorVolume()
        {
            var compositorVolumes = Resources.FindObjectsOfTypeAll(typeof(CustomPassVolume));
            foreach (CustomPassVolume volume in compositorVolumes)
            {
                if(volume.isGlobal && volume.injectionPoint == CustomPassInjectionPoint.BeforeRendering)
                {
                    Debug.LogWarning($"A custom volume pass with name ${volume.name} was already registered on the BeforeRendering injection point.");
                }
            }

            // Instead of using one volume per layer/camera, we setup one global volume and we store the data in the camera
            // This way the compositor has to use only one layer/volume for N cameras (instead of N).
            m_CompositorGameObject = new GameObject("Global Composition Volume") { hideFlags = HideFlags.HideAndDontSave };
            Volume globalPPVolume = m_CompositorGameObject.AddComponent<Volume>();
            globalPPVolume.gameObject.layer = 31;
            AlphaInjection injectAlphaNode = globalPPVolume.profile.Add<AlphaInjection>();
            ChromaKeying chromaKeyingPass = globalPPVolume.profile.Add<ChromaKeying>();
            chromaKeyingPass.activate.Override(true);

            // Custom pass for "Clear to Texture"
            CustomPassVolume globalCustomPassVolume = m_CompositorGameObject.AddComponent<CustomPassVolume>();
            globalCustomPassVolume.injectionPoint = CustomPassInjectionPoint.BeforeRendering;
            globalCustomPassVolume.AddPassOfType(typeof(CustomClear));
        }

        void SetupCompositorConstants()
        {
            m_ViewProjMatrix = Matrix4x4.Scale(new Vector3(2.0f, 2.0f, 0.0f)) * Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0.0f));
            m_ViewProjMatrixFlipped = Matrix4x4.Scale(new Vector3(2.0f, -2.0f, 0.0f)) * Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0.0f));
        }

        bool ShaderPropertiesWereChanged()
        {
            if (shader == null)
            {
                return false;
            }

            int propCount = m_Shader.GetPropertyCount();
            if (propCount != m_CompositionProfile.m_ShaderProperties.Count)
            {
                return true;
            }

            for (int i = 0; i < propCount; i++)
            {
                if (m_Shader.GetPropertyName(i) != m_CompositionProfile.m_ShaderProperties[i].m_PropertyName)
                {
                    return true;
                }
                else if (m_Shader.GetPropertyType(i) != m_CompositionProfile.m_ShaderProperties[i].m_Type)
                {
                    return true;
                }
                else if (m_CompositionProfile.m_ShaderProperties[i].m_Type == ShaderPropertyType.Range &&
                    m_CompositionProfile.m_ShaderProperties[i].m_RangeLimits != m_Shader.GetPropertyRangeLimits(i))
                {
                    return true;
                }
            }

            return false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!ValidateAndFixRuntime())
            {
                return;
            }

            ValidatePipeline();

            UpdateDisplayNumber();

            if (m_IsCompositorDirty)
            {
                SetupCompositorLayers();

                SetupLayerPriorities();

                m_IsCompositorDirty = false;
            }
#if UNITY_EDITOR
            if (ShaderPropertiesWereChanged())
            {
                SetNewCompositionShader();
                m_IsCompositorDirty = true;
            }
#endif
            if (m_CompositionProfile)
            {
                m_CompositionProfile.UpdateLayers(Application.IsPlaying(gameObject));
            }
        }

        void OnDestroy()
        {
            if (m_CompositionProfile)
            {
                m_CompositionProfile.OnDestroy();
            }

            if (m_CompositorGameObject != null)
            {
                CoreUtils.Destroy(m_CompositorGameObject);
                m_CompositorGameObject = null;
            }

            var compositorVolumes = Resources.FindObjectsOfTypeAll(typeof(CustomPassVolume));
            foreach (CustomPassVolume volume in compositorVolumes)
            {
                if (volume.name == "Global Composition Volume" && volume.injectionPoint == CustomPassInjectionPoint.BeforeRendering)
                {
                    CoreUtils.Destroy(volume);
                }
            }
        }

        public void AddInputFilterAtLayer(CompositionFilter filter, int index)
        {
            m_CompositionProfile.m_InputLayers[index].AddInputFilter(filter);
        }

        public void AddNewLayer(int index, CompositorLayer.LayerType type = CompositorLayer.LayerType.CG_Element)
        {
            var newLayer = CompositorLayer.CreateStackLayer(type, "New Layer");
            m_CompositionProfile.AddNewLayerAtIndex(newLayer, index);
            m_IsCompositorDirty = true;
        }

        public void RemoveLayerAtIndex(int indx)
        {
            m_CompositionProfile.RemoveLayerAtIndex(indx);
            m_IsCompositorDirty = true;
        }

        public RenderTexture GetRenderTarget(int indx)
        {
            if (indx >= 0 && indx < m_CompositionProfile.m_InputLayers.Count)
            {
                return m_CompositionProfile.m_InputLayers[indx].GetRenderTarget();
            }
            return null;
        }

        void CustomRender(ScriptableRenderContext context, HDCamera camera)
        {
            if (camera == null || camera.camera == null || m_Material == null)
                return;

            // set shader uniforms

            foreach (var prop in m_CompositionProfile.m_ShaderProperties)
            {
                if (prop.m_Type == ShaderPropertyType.Float)
                {
                    m_Material.SetFloat(prop.m_PropertyName, prop.m_Value.x);
                }
                else if (prop.m_Type == ShaderPropertyType.Vector)
                {
                    m_Material.SetVector(prop.m_PropertyName, prop.m_Value);
                }
                else if (prop.m_Type == ShaderPropertyType.Range)
                {
                    m_Material.SetFloat(prop.m_PropertyName, prop.m_Value.x);
                }
                else if (prop.m_Type == ShaderPropertyType.Color)
                {
                    m_Material.SetColor(prop.m_PropertyName, prop.m_Value);
                }
            }

            int layerIndex = 0;
            foreach (var layer in m_CompositionProfile.m_InputLayers)
            {
                if (layer.GetOutputTarget() != CompositorLayer.OutputTarget.CameraStack)  // stacked cameras are not exposed as compositor layers 
                {
                    m_Material.SetTexture(layer.m_LayerName, layer.GetRenderTarget(), RenderTextureSubElement.Color);
                }
                layerIndex++;
            }

            // Blit command
            var cmd = CommandBufferPool.Get("Compositor Blit");
            {
                cmd.SetGlobalVector(SGShaderIDs._WorldSpaceCameraPos, new Vector3(0.0f, 0.0f, 0.0f));
                cmd.SetViewport(new Rect(0, 0, camera.camera.pixelWidth, camera.camera.pixelHeight));
            }

            if (camera.camera.targetTexture)
            {
                cmd.SetGlobalMatrix(SGShaderIDs._ViewProjMatrix, m_ViewProjMatrixFlipped);
                cmd.Blit(null, BuiltinRenderTextureType.CurrentActive, m_Material, m_Material.FindPass("ForwardOnly"));
                cmd.Blit(BuiltinRenderTextureType.CurrentActive, camera.camera.targetTexture);
            }
            else
            {
                cmd.SetGlobalMatrix(SGShaderIDs._ViewProjMatrix, m_ViewProjMatrix);
                cmd.Blit(null, BuiltinRenderTextureType.CurrentActive, m_Material, m_Material.FindPass("ForwardOnly"));
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        static public Camera GetSceceCamera()
        {
            if (Camera.main != null)
            {
                return Camera.main;
            }
            foreach (var camera in Camera.allCameras)
            {
                if (camera.name != "MainCompositorCamera")
                {
                    return camera;
                }
            }
            Debug.LogWarning("Camera not found");
            return null;
        }
    }
}
