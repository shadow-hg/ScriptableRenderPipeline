using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphing;
using UnityEngine;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace UnityEditor.ShaderGraph
{
    // As soon as traversal can skip RedirectNodes, make this NOT a CodeFunctionNode
    class RedirectNodeData : AbstractMaterialNode, IGeneratesBodyCode
    {
        public Edge m_Edge;
        public const int InputSlotId = 0;
        public const int OutputSlotId = 1;
        public RedirectNodeData()
        {
            name = "Redirect Node";
            UpdateNodeAfterDeserialization();
        }

        RedirectNodeView m_nodeView;
        public RedirectNodeView nodeView
        {
            get { return m_nodeView; }
            set
            {
                if (value != m_nodeView)
                    m_nodeView = value;
            }
        }

        // Center the node's position?
        public void SetPosition(Vector2 pos)
        {
            var temp = drawState;
            temp.position = new Rect(pos, Vector2.zero);
            drawState = temp;
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            //AddSlot(new DynamicVectorMaterialSlot(InputSlotId, kInputSlotName, kInputSlotName, SlotType.Input, Vector4.zero));
            //AddSlot(new DynamicVectorMaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output, Vector4.zero));
        }

        public void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            using (var outputSlots = PooledList<MaterialSlot>.Get())
            {
                GetOutputSlots(outputSlots);
                var inputValue = GetSlotValue(InputSlotId, generationMode);
                var outputSlot = outputSlots[0];
                sb.AppendLine($"{outputSlot.concreteValueType.ToShaderString()} {GetVariableNameForSlot(OutputSlotId)} = {inputValue};");
                if (outputSlot.concreteValueType == ConcreteSlotValueType.Texture2D ||
                    outputSlot.concreteValueType == ConcreteSlotValueType.Texture3D ||
                    outputSlot.concreteValueType == ConcreteSlotValueType.Texture2DArray)
                {
                    sb.AppendLine("#if !defined(SHADER_API_GLES)");
                    sb.AppendLine($"{ConcreteSlotValueType.SamplerState.ToShaderString()} sampler{GetVariableNameForSlot(OutputSlotId)} = sampler{inputValue};");
                    sb.AppendLine("#endif");
                }
            }

        }
    }
//     class RedirectNodeData : CodeFunctionNode
//     {
//         public Edge m_Edge;
//
//         public RedirectNodeData() : base()
//         {
//             name = "Redirect Node";
//         }
//
//         protected override MethodInfo GetFunctionToConvert()
//         {
//             return GetType().GetMethod("Unity_Redirect", BindingFlags.Static | BindingFlags.NonPublic);
//         }
//
//         RedirectNodeView m_nodeView;
//         public RedirectNodeView nodeView
//         {
//             get { return m_nodeView; }
//             set
//             {
//                 if (value != m_nodeView)
//                     m_nodeView = value;
//             }
//         }
//
//         // Center the node's position?
//         public void SetPosition(Vector2 pos)
//         {
//             var temp = drawState;
//             temp.position = new Rect(pos, Vector2.zero);
//             drawState = temp;
//         }
//
//         static string Unity_Redirect(
//             [Slot(0, Binding.None)] DynamicDimensionMatrix In,
//             [Slot(1, Binding.None)] out DynamicDimensionMatrix Out)
//         {
//             return
//                 @"
// {
//     Out = In;
// }
// ";
//         }
//     }
}
