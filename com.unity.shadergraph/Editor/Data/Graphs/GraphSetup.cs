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
       public static class GraphUtils
       {
            public static void ApplyActionLeafFirst(GraphData graph, Action<AbstractMaterialNode> action)
            {
                var temporaryMarks = IndexSetPool.Get();
                var permanentMarks = IndexSetPool.Get();
                var slots = ListPool<MaterialSlot>.Get();

                // Make sure we process a node's children before the node itself.
                var stack = StackPool<AbstractMaterialNode>.Get();
                foreach (var node in graph.GetNodes<AbstractMaterialNode>())
                {
                    stack.Push(node);
                }
                while (stack.Count > 0)
                {
                    var node = stack.Pop();
                    if (permanentMarks.Contains(node.tempId.index))
                    {
                        continue;
                    }

                    if (temporaryMarks.Contains(node.tempId.index))
                    {
                        action.Invoke(node);
                        permanentMarks.Add(node.tempId.index);
                    }
                    else
                    {
                        temporaryMarks.Add(node.tempId.index);
                        stack.Push(node);
                        node.GetInputSlots(slots);
                        foreach (var inputSlot in slots)
                        {
                            var nodeEdges = graph.GetEdges(inputSlot.slotReference);
                            foreach (var edge in nodeEdges)
                            {
                                var fromSocketRef = edge.outputSlot;
                                var childNode = graph.GetNodeFromGuid(fromSocketRef.nodeGuid);
                                if (childNode != null)
                                {
                                    stack.Push(childNode);
                                }
                            }
                        }
                        slots.Clear();
                    }
                }

                StackPool<AbstractMaterialNode>.Release(stack);
                ListPool<MaterialSlot>.Release(slots);
                IndexSetPool.Release(temporaryMarks);
                IndexSetPool.Release(permanentMarks);
            }
       }
       public static class GraphSetup
       {
            //Does anything else need to be setup? Should nodes track if they have been setup already, or should this class keep track?
            public static void SetupNode(AbstractMaterialNode node)
            {
                node.Setup();
            }

            public static void SetupGraph(GraphData graph)
            {
                GraphUtils.ApplyActionLeafFirst(graph, SetupNode);
            }
       }
    }
}
