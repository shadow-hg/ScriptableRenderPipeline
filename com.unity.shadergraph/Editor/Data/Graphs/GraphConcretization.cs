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
            public static void ConcretizeNode(AbstractMaterialNode node)
            {
                node.Concretize();
            }
            public static void ConcretizeProperties(GraphData graph)
            {
                var propertyNodes = graph.GetNodes<PropertyNode>().Where(n => !graph.m_Properties.Any(p => p.guid == n.propertyGuid)).ToArray();
                foreach (var pNode in propertyNodes)
                    graph.ReplacePropertyNodeWithConcreteNodeNoValidate(pNode);
            }
            public static void ConcretizeGraph(GraphData graph)
            {
                ConcretizeProperties(graph);
                GraphUtils.ApplyActionLeafFirst(graph, ConcretizeNode);
            }
        }
    }
}
