using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.Graphing.Util;
using UnityEditor.Rendering;
using UnityEditor.ShaderGraph.Internal;
using Edge = UnityEditor.Graphing.Edge;

namespace UnityEditor.ShaderGraph
{
    sealed partial class GraphData : ISerializationCallbackReceiver
    {
        public static class GraphConcretization
        {
            public static void ReplacePropertyNodeWithConcreteNode(GraphData graph, PropertyNode propertyNode)
            {
                ReplacePropertyNodeWithConcreteNodeNoValidate(graph,propertyNode);
                graph.ValidateGraph();
            }

            private static void ReplacePropertyNodeWithConcreteNodeNoValidate(GraphData graph, PropertyNode propertyNode)
            {
                var property = graph.properties.FirstOrDefault(x => x.guid == propertyNode.propertyGuid);
                if (property == null)
                    return;

                var node = property.ToConcreteNode() as AbstractMaterialNode;
                if (node == null)
                    return;

                var slot = propertyNode.FindOutputSlot<MaterialSlot>(PropertyNode.OutputSlotId);
                var newSlot = node.GetOutputSlots<MaterialSlot>().FirstOrDefault(s => s.valueType == slot.valueType);
                if (newSlot == null)
                    return;

                node.drawState = propertyNode.drawState;
                node.groupGuid = propertyNode.groupGuid;
                graph.AddNodeNoValidate(node);

                foreach (var edge in graph.GetEdges(slot.slotReference))
                    graph.ConnectNoValidate(newSlot.slotReference, edge.inputSlot);

                graph.RemoveNodeNoValidate(propertyNode);
            }

            public static void ConcretizeNode(AbstractMaterialNode node)
            {
                node.Concretize();
            }

            public static void ConcretizeMatchedProperties(GraphData graph, Func<PropertyNode, bool> matchFunction)
            {
                IEnumerable<PropertyNode> propertyNodes = graph.GetNodes<PropertyNode>().Where(matchFunction);
                foreach (PropertyNode pNode in propertyNodes)
                    ReplacePropertyNodeWithConcreteNodeNoValidate(graph, pNode);
            }

            public static void ConcretizeUnmanagedProperties(GraphData graph)
            {
                IEnumerable<PropertyNode> propertyNodes = graph.GetNodes<PropertyNode>().Where(n => !graph.m_Properties.Any(p => p.guid == n.propertyGuid));
                foreach (PropertyNode pNode in propertyNodes)
                    ReplacePropertyNodeWithConcreteNodeNoValidate(graph, pNode);
            }

            public static void ConcretizeGraph(GraphData graph)
            {
                GraphUtils.ApplyActionLeafFirst(graph, ConcretizeNode);
            }
        }
    }
}
