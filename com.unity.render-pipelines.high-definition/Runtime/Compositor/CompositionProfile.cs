using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;

namespace UnityEngine.Rendering.HighDefinition.Compositor
{
    // Holds a list of layers and layer/composition properties. This is serialized and can be shared between projects
    internal class CompositionProfile : ScriptableObject
    {
        public List<CompositorLayer> m_InputLayers = new List<CompositorLayer>();
        public List<ShaderProperty> m_ShaderProperties = new List<ShaderProperty>();

        public float aspectRatio
        {
            get
            {
                if (m_InputLayers.Count > 0)
                {
                    return m_InputLayers[0].aspectRatio;
                }
                return 1.0f;
            }
        }

        public void AddNewLayerAtIndex(CompositorLayer layer, int index)
        {
            if (index >= 0 && index < m_InputLayers.Count)
            {
                m_InputLayers.Insert(index, layer);
            }
            else
            {
                m_InputLayers.Add(layer);
            }
        }

        public void RemoveLayerAtIndex(int indx)
        {
            Debug.Assert(indx < m_InputLayers.Count);
            m_InputLayers[indx].Destroy();
            m_InputLayers.RemoveAt(indx);
        }

        public void OnDestroy()
        {
            // We need to destroy the layers from last to first, to avoid releasing a RT that is used by a camera
            for (int i = m_InputLayers.Count - 1; i >= 0; --i)
            {
                m_InputLayers[i].Destroy();
            }
        }

        public void ClearShaderProperties()
        {
            m_ShaderProperties.Clear();
            m_InputLayers.Clear();
        }

        public void AddPropertiesFromShaderAndMaterial (Shader shader, Material material)
        {
            // reflect the non-texture shader properties
            List<string> propertyNames = new List<string>();
            int propCount = shader.GetPropertyCount();
            for (int i = 0; i < propCount; i++)
            {
                ShaderProperty sp = ShaderProperty.Create(shader, material, i);
                AddShaderProperty(sp);
                propertyNames.Add(sp.m_PropertyName);
            }

            // remove any left-over properties that do not appear in the shader anymore
            for (int j = m_ShaderProperties.Count - 1; j >= 0; --j)
            {
                int indx = propertyNames.FindIndex(x => x == m_ShaderProperties[j].m_PropertyName);
                if (indx < 0)
                {
                    m_ShaderProperties.RemoveAt(j);
                }
            }

            // Now remove any left-over  layers that do not appear in the shader anymore
            for (int j = m_InputLayers.Count - 1; j >= 0; --j)
            {
                if (m_InputLayers[j].GetOutputTarget() != CompositorLayer.OutputTarget.CameraStack)
                {
                    int indx = propertyNames.FindIndex(x => x == m_InputLayers[j].m_LayerName);
                    if (indx < 0)
                    {
                        RemoveLayerAtIndex(j);
                    }
                }
            }
        }

        public void AddShaderProperty(ShaderProperty sp)
        {
            Assert.IsNotNull(sp);

            // Check if property already exists / do not add duplicates
            int indx = m_ShaderProperties.FindIndex(s => s.m_PropertyName == sp.m_PropertyName);
            if (indx < 0)
            {
                m_ShaderProperties.Add(sp);
            }

            bool hide = ((int)sp.m_Flags & (int)ShaderPropertyFlags.NonModifiableTextureData) != 0;
            // For textures, check if we already have this layer in the layer list. If not, add it.
            if (sp.m_Type == ShaderPropertyType.Texture)
            {
                indx = m_InputLayers.FindIndex(s => s.m_LayerName == sp.m_PropertyName);
                if (indx < 0 && !hide)
                {
                    Debug.Log($"Adding output layer from shader graph: {sp.m_PropertyName}");
                    var newLayer = CompositorLayer.CreateOutputLayer(sp.m_PropertyName);
                    m_InputLayers.Add(newLayer);
                }
                else if (indx >= 0 && hide)
                {
                    // if a layer already in the list is now hidden, remove it
                    RemoveLayerAtIndex(indx);
                }
            }
        }

        public void Init()
        {
            bool isFirstLayerInSTack = true;
            CompositorLayer lastLayer = null;
            for (int i = 0; i < m_InputLayers.Count; ++i)
            {
                m_InputLayers[i].Init($"Layer{i}");

                if (m_InputLayers[i].GetOutputTarget() != CompositorLayer.OutputTarget.CameraStack)
                {
                    lastLayer = m_InputLayers[i];
                }

                if (m_InputLayers[i].GetOutputTarget() == CompositorLayer.OutputTarget.CameraStack && i > 0)
                {
                    m_InputLayers[i].SetupLayerCamera(lastLayer, isFirstLayerInSTack);

                    // Corner case: If the first layer in a camera stack was disabled, then it should still clear the color buffer
                    if (m_InputLayers[i].enabled == false && isFirstLayerInSTack)
                    {
                        m_InputLayers[i].SetupClearColor();
                    }
                    isFirstLayerInSTack = false;
                }
                else
                {
                    isFirstLayerInSTack = true;
                }
            }
        }
        public void SetDrawOrder()
        {
            int count = 0;
            foreach (var layer in m_InputLayers)
            {
                // Set camera priority (camera's at the beginning of the list should be rendered first)
                layer.SetPriotiry(count * 1.0f);
                count++;
            }
        }

        public void UpdateLayers(bool isPLaying)
        {
            foreach (var layer in m_InputLayers)
            {
                layer.Update(isPLaying);
            }
        }
    }
}
