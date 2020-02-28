using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Graphing;
using UnityEditor.Graphing.Util;
//using UnityEditor.Searcher;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace UnityEditor.ShaderGraph.Drawing
{
    //class AutomatedTestingSearcherProvider : SearcherProvider
    //{
    //    public override void GenerateNodeEntries()
    //    {
    //        // First build up temporary data structure containing group & title as an array of strings (the last one is the actual title) and associated node type.
    //        List<NodeEntry> nodeEntries = new List<NodeEntry>();
    //        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    //        {
    //            foreach (var type in assembly.GetTypesOrNothing())
    //            {
    //                if (type.IsClass && !type.IsAbstract && (type.IsSubclassOf(typeof(AbstractMaterialNode)))
    //                    && type != typeof(PropertyNode)
    //                    && type != typeof(KeywordNode)
    //                    && type != typeof(SubGraphNode))
    //                {
    //                    var attrs = type.GetCustomAttributes(typeof(TitleAttribute), false) as TitleAttribute[];
    //                    if (attrs != null && attrs.Length > 0)
    //                    {
    //                        var node = (AbstractMaterialNode)Activator.CreateInstance(type);
    //                        AddEntries(node, attrs[0].title, nodeEntries);
    //                    }
    //                }
    //            }
    //        }

    //        foreach (var property in m_Graph.properties)
    //        {
    //            var node = new PropertyNode();
    //            node.owner = m_Graph;
    //            node.propertyGuid = property.guid;
    //            node.owner = null;
    //            AddEntries(node, new[] { "Properties", "Property: " + property.displayName }, nodeEntries);
    //        }
    //        foreach (var keyword in m_Graph.keywords)
    //        {
    //            var node = new KeywordNode();
    //            node.owner = m_Graph;
    //            node.keywordGuid = keyword.guid;
    //            node.owner = null;
    //            AddEntries(node, new[] { "Keywords", "Keyword: " + keyword.displayName }, nodeEntries);
    //        }

    //        // Sort the entries lexicographically by group then title with the requirement that items always comes before sub-groups in the same group.
    //        // Example result:
    //        // - Art/BlendMode
    //        // - Art/Adjustments/ColorBalance
    //        // - Art/Adjustments/Contrast
    //        nodeEntries.Sort((entry1, entry2) =>
    //        {
    //            for (var i = 0; i < entry1.title.Length; i++)
    //            {
    //                if (i >= entry2.title.Length)
    //                    return 1;
    //                var value = entry1.title[i].CompareTo(entry2.title[i]);
    //                if (value != 0)
    //                {
    //                        // Make sure that leaves go before nodes
    //                        if (entry1.title.Length != entry2.title.Length && (i == entry1.title.Length - 1 || i == entry2.title.Length - 1))
    //                    {
    //                            //once nodes are sorted, sort slot entries by slot order instead of alphebetically 
    //                            var alphaOrder = entry1.title.Length < entry2.title.Length ? -1 : 1;
    //                        var slotOrder = entry1.compatibleSlotId.CompareTo(entry2.compatibleSlotId);
    //                        return alphaOrder.CompareTo(slotOrder);
    //                    }

    //                    return value;
    //                }
    //            }
    //            return 0;
    //        });


    //        currentNodeEntries = nodeEntries;
    //    }
    //}
    //class AutomatedTestingGraphEditorView : GraphEditorView
    //{
    //    public Searcher.Searcher searcher;
    //    public AutomatedTestingGraphEditorView(EditorWindow editorWindow, GraphData graph, MessageManager messageManager) : base(editorWindow, graph, messageManager) { }

    //    protected override void InitializeSearchWindowProvider(EditorWindow editorWindow)
    //    {
    //        m_SearchWindowProvider = ScriptableObject.CreateInstance<SearcherProvider>();
    //        m_SearchWindowProvider.Initialize(editorWindow, m_Graph, m_GraphView);
    //        m_GraphView.nodeCreationRequest = (c) =>
    //        {
    //            m_SearchWindowProvider.connectedPort = null;
    //            searcher = (m_SearchWindowProvider as SearcherProvider).LoadSearchWindow();
    //            SearcherWindow.Show(editorWindow, searcher,
    //                item => (m_SearchWindowProvider as SearcherProvider).OnSearcherSelectEntry(item, c.screenMousePosition - editorWindow.position.position),
    //                c.screenMousePosition - editorWindow.position.position, null);
    //        };
    //    }


    //}

    //class AutomatedTestingGraphEditWindow : MaterialGraphEditWindow
    //{
    //    protected override void SetupGraphEditorView(GraphData materialGraph)
    //    {
    //        messageManager.ClearAll();
    //        materialGraph.messageManager = messageManager;
    //        var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(selectedGuid));
    //        graphEditorView = new AutomatedTestingGraphEditorView(this, materialGraph, messageManager)
    //        {
    //            viewDataKey = selectedGuid,
    //            assetName = asset.name.Split('/').Last()
    //        };
    //        m_ColorSpace = PlayerSettings.colorSpace;
    //        m_RenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
    //        graphObject.Validate();
    //    }

    //    public override void Initialize(string assetGuid)
    //    {
    //        try
    //        {
    //            m_ColorSpace = PlayerSettings.colorSpace;
    //            m_RenderPipelineAsset = GraphicsSettings.renderPipelineAsset;

    //            var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(assetGuid));
    //            if (asset == null)
    //                return;

    //            if (!EditorUtility.IsPersistent(asset))
    //                return;

    //            if (selectedGuid == assetGuid)
    //                return;

    //            var path = AssetDatabase.GetAssetPath(asset);
    //            var extension = Path.GetExtension(path);
    //            if (extension == null)
    //                return;
    //            // Path.GetExtension returns the extension prefixed with ".", so we remove it. We force lower case such that
    //            // the comparison will be case-insensitive.
    //            extension = extension.Substring(1).ToLowerInvariant();
    //            bool isSubGraph;
    //            switch (extension)
    //            {
    //                case ShaderGraphImporter.Extension:
    //                    isSubGraph = false;
    //                    break;
    //                case ShaderSubGraphImporter.Extension:
    //                    isSubGraph = true;
    //                    break;
    //                default:
    //                    return;
    //            }

    //            selectedGuid = assetGuid;

    //            var textGraph = File.ReadAllText(path, Encoding.UTF8);
    //            graphObject = CreateInstance<GraphObject>();
    //            graphObject.hideFlags = HideFlags.HideAndDontSave;
    //            graphObject.graph = JsonUtility.FromJson<GraphData>(textGraph);
    //            graphObject.graph.assetGuid = assetGuid;
    //            graphObject.graph.isSubGraph = isSubGraph;
    //            graphObject.graph.messageManager = messageManager;
    //            graphObject.graph.OnEnable();
    //            graphObject.graph.ValidateGraph();

    //            graphEditorView = new AutomatedTestingGraphEditorView(this, m_GraphObject.graph, messageManager)
    //            {
    //                viewDataKey = selectedGuid,
    //                assetName = asset.name.Split('/').Last()
    //            };

    //            Texture2D icon = GetThemeIcon(graphObject.graph);

    //            // This is adding the icon at the front of the tab
    //            titleContent = EditorGUIUtility.TrTextContentWithIcon(selectedGuid, icon);
    //            UpdateTitle();

    //            Repaint();
    //        }
    //        catch (Exception)
    //        {
    //            m_HasError = true;
    //            m_GraphEditorView = null;
    //            graphObject = null;
    //            throw;
    //        }
    //    }
    //}
}

    namespace UnityEditor.ShaderGraph.UnitTests
{


    internal class GraphCreationUtils
    {

        private static readonly string testGraphLocation = "Assets/Testing/CreatedTestGraphs/";
        private static readonly string testPrefix = "_Test_";

        public static void CloseAllOpenShaderGraphWindows()
        {
            foreach (MaterialGraphEditWindow graphWindow in Resources.FindObjectsOfTypeAll<MaterialGraphEditWindow>())
            {
                graphWindow.Close();
            }
        }

        public static MaterialGraphEditWindow OpenShaderGraphWindowForAsset(string assetPath)
        {
            var window = EditorWindow.CreateWindow<MaterialGraphEditWindow>(typeof(MaterialGraphEditWindow), typeof(SceneView));
            window.Initialize(AssetDatabase.AssetPathToGUID(assetPath));
            return window;
        }

        public static void CreateEmptyTestGraph(string basedOnGraph)
        {
            var window = OpenShaderGraphWindowForAsset(basedOnGraph);
            GraphObject graphObject = window.GetPrivateProperty<GraphObject>("graphObject");
            GraphData graphToCopy = graphObject.graph;

            GraphData graphData = new GraphData();
            var rootNode = Activator.CreateInstance(graphToCopy.outputNode.GetType()) as AbstractMaterialNode;
            rootNode.drawState = new DrawState
            {
                position = graphToCopy.outputNode.drawState.position,
                expanded = true
            };
            graphData.AddNode(rootNode);
            graphData.path = "Shader Graphs";
            string outputPath = testGraphLocation + testPrefix + Path.GetFileNameWithoutExtension(basedOnGraph) + '.' + ShaderGraphImporter.Extension;
            FileUtilities.WriteShaderGraphToDisk(outputPath, graphData);
            AssetDatabase.Refresh();
            window.UpdateAsset();
            window.Close();
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Shader>(outputPath));
        }

        private static void TrySaveWindows(MaterialGraphEditWindow copyWindow, MaterialGraphEditWindow testGraphWindow)
        {

                if (copyWindow != null)
                {
                    copyWindow.UpdateAsset();
                    copyWindow.Close();
                }
                if (testGraphWindow != null)
                {
                    testGraphWindow.UpdateAsset();
                    testGraphWindow.Close();
                }
            
        }

        private const float userTime = 0.5f;
        public static IEnumerator UserlikeGraphCreation(string assetPath)
        {
            var nodeLookup = new Dictionary<AbstractMaterialNode, AbstractMaterialNode>();
            var temporaryMarks = ListPool<(AbstractMaterialNode, SlotReference?, SlotReference?)>.Get();
            var permanentMarks = ListPool<(AbstractMaterialNode, SlotReference?, SlotReference?)>.Get();
            var slots = ListPool<MaterialSlot>.Get();
            var stack = StackPool<(AbstractMaterialNode, SlotReference?, SlotReference?)>.Get();
            MaterialGraphEditWindow copyWindow = null;
            MaterialGraphEditWindow testGraphWindow = null;
            GraphObject graphObjectToCopy = null;
            GraphObject testGraphObject = null;

            try
            {
                copyWindow = OpenShaderGraphWindowForAsset(assetPath);
                graphObjectToCopy = copyWindow.GetPrivateProperty<GraphObject>("graphObject");
                string testGraphPath = testGraphLocation + testPrefix + Path.GetFileNameWithoutExtension(assetPath) + '.' + ShaderGraphImporter.Extension;
                testGraphWindow = OpenShaderGraphWindowForAsset(testGraphPath);
                testGraphWindow.Focus();
                testGraphObject = testGraphWindow.GetPrivateProperty<GraphObject>("graphObject");
            }
            catch (Exception cantOpenWindowsOrAccessPrivatePropertyException)
            {
                try
                {
                    TrySaveWindows(copyWindow, testGraphWindow);
                }
                catch (Exception cantSaveOrCloseWindowsException)
                {
                    Debug.LogError(cantSaveOrCloseWindowsException);
                }
                throw cantOpenWindowsOrAccessPrivatePropertyException;
            }

            yield return new WaitForSecondsRealtime(userTime);



            nodeLookup.Add(graphObjectToCopy.graph.outputNode, testGraphObject.graph.outputNode);


            foreach (var node in graphObjectToCopy.graph.GetNodes<AbstractMaterialNode>())
            {
                stack.Push((node, null, null));
            }
            while (stack.Count > 0)
            {
                (AbstractMaterialNode node, SlotReference? to, SlotReference? from) = stack.Pop();
                if (permanentMarks.Contains((node, to, from)))
                {
                    continue;
                }

                if (temporaryMarks.Contains((node, to, from)))
                {
                    if (!nodeLookup.ContainsKey(node))
                    {                      
                        yield return UserlikeAddNode(node, testGraphObject, nodeLookup, testGraphWindow);
                    }

                    if (to.HasValue)
                    {
                        AbstractMaterialNode toNode = graphObjectToCopy.graph.GetNodeFromGuid(to.Value.nodeGuid);
                        if (toNode != null && !nodeLookup.ContainsKey(toNode))
                        {

                            yield return UserlikeAddNode(toNode, testGraphObject, nodeLookup, testGraphWindow);
                            permanentMarks.Add((toNode, null, null));
                        }
                        Assert.IsTrue(from.HasValue);
                        try
                        {
                            UserlikeAddEdge(to.Value, from.Value, ref graphObjectToCopy, ref testGraphObject, ref nodeLookup);
                        }
                        catch (Exception cantAddEdgeException)
                        {
                            StackPool<(AbstractMaterialNode, SlotReference?, SlotReference?)>.Release(stack);
                            ListPool<MaterialSlot>.Release(slots);
                            ListPool<(AbstractMaterialNode, SlotReference?, SlotReference?)>.Release(temporaryMarks);
                            ListPool<(AbstractMaterialNode, SlotReference?, SlotReference?)>.Release(permanentMarks);
                            try
                            {
                                TrySaveWindows(copyWindow, testGraphWindow);
                            }
                            catch (Exception cantSaveOrCloseWindowsException)
                            {
                                Debug.LogError(cantSaveOrCloseWindowsException);
                            }
                            throw cantAddEdgeException;
                        }
                        
                        yield return new WaitForSecondsRealtime(userTime);
                    }
                    permanentMarks.Add((node, to, from));
                }
                else
                {
                    temporaryMarks.Add((node, to, from));
                    stack.Push((node, to, from));
                    node.GetInputSlots(slots);
                    foreach (MaterialSlot inputSlot in slots)
                    {
                        var nodeEdges = graphObjectToCopy.graph.GetEdges(inputSlot.slotReference);
                        foreach (IEdge edge in nodeEdges)
                        {
                            var fromSocketRef = edge.outputSlot;
                            var childNode = graphObjectToCopy.graph.GetNodeFromGuid(fromSocketRef.nodeGuid);
                            if (childNode != null)
                            {
                                stack.Push((childNode, inputSlot.slotReference, fromSocketRef));
                            }
                        }
                    }
                    slots.Clear();
                }
            }

            try
            {
                if (copyWindow != null)
                {
                    copyWindow.UpdateAsset();
                    copyWindow.Close();
                }
                if (testGraphWindow != null)
                {
                    testGraphWindow.UpdateAsset();
                    testGraphWindow.Close();
                }
            }
            catch (Exception cantSaveAndCloseException)
            {
                throw cantSaveAndCloseException;
            }
            finally
            {
                StackPool<(AbstractMaterialNode, SlotReference?, SlotReference?)>.Release(stack);
                ListPool<MaterialSlot>.Release(slots);
                ListPool<(AbstractMaterialNode, SlotReference?, SlotReference?)>.Release(temporaryMarks);
                ListPool<(AbstractMaterialNode, SlotReference?, SlotReference?)>.Release(permanentMarks);
            }
        }

        public static void UserlikeAddEdge(SlotReference originalTo, SlotReference originalFrom, ref GraphObject originalGraph, ref GraphObject copyGraph, ref Dictionary<AbstractMaterialNode, AbstractMaterialNode> nodeLookup)
        {
            AbstractMaterialNode toNodeOriginal = originalGraph.graph.GetNodeFromGuid(originalTo.nodeGuid);
            AbstractMaterialNode fromNodeOriginal = originalGraph.graph.GetNodeFromGuid(originalFrom.nodeGuid);

            Assert.IsNotNull(toNodeOriginal);
            Assert.IsNotNull(fromNodeOriginal);

            Assert.IsTrue(nodeLookup.ContainsKey(toNodeOriginal));
            Assert.IsTrue(nodeLookup.ContainsKey(fromNodeOriginal));

            AbstractMaterialNode toNodeCopy = nodeLookup[toNodeOriginal];
            AbstractMaterialNode fromNodeCopy = nodeLookup[fromNodeOriginal];



            SlotReference copyTo = new SlotReference(toNodeCopy.guid, originalTo.slotId);
            SlotReference copyFrom = new SlotReference(fromNodeCopy.guid, originalFrom.slotId);

            copyGraph.graph.Connect(copyFrom, copyTo);
        }

        public static IEnumerator UserlikeAddNode(AbstractMaterialNode node, GraphObject graphObject, Dictionary<AbstractMaterialNode, AbstractMaterialNode> nodeLookup, MaterialGraphEditWindow testGraphWindow)
        {
            AbstractMaterialNode newNode;
            //ECL: Concretize property, will need to add full property later
            if (node is PropertyNode propertyNode)
            {
                AbstractShaderProperty property = propertyNode.owner.properties.FirstOrDefault(x => x.guid == propertyNode.propertyGuid);
                Assert.IsNotNull(property);

                newNode = property.ToConcreteNode() as AbstractMaterialNode;
            }
            else
            {
                newNode = Activator.CreateInstance(node.GetType()) as AbstractMaterialNode;
                var inputs = node.GetInputsWithNoConnection();
                foreach (var input in inputs)
                {
                    MaterialSlot materialSlot = input as MaterialSlot;
                    MaterialSlot newSlot = newNode.FindInputSlot<MaterialSlot>(materialSlot.id);
                    newSlot.CopyValuesFrom(materialSlot);
                }

                foreach (var property in node.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy))
                {
                    var enumControlsCheck = property.GetCustomAttributes(typeof(IControlAttribute), true);
                    if (enumControlsCheck.Length > 0)
                    {
                        property.SetValue(newNode, property.GetValue(node));
                    }
                }
            }
            /* ECL: Searcher integration I am currently working on
            GraphEditorView graphEditorView = testGraphWindow.GetPrivateProperty<GraphEditorView>("graphEditorView");
            //MaterialGraphView materialGraphView = graphEditorView.graphView;
            SearchWindowProvider searchWindowProvider = graphEditorView.GetPrivateField<SearchWindowProvider>("m_SearchWindowProvider");
            searchWindowProvider.connectedPort = null;
            Searcher.Searcher sercher = (searchWindowProvider as SearcherProvider).LoadSearchWindow();
            SearcherWindow.Show(testGraphWindow, sercher,
                item => (searchWindowProvider as SearcherProvider).OnSearcherSelectEntry(item, testGraphWindow.position.center),
                testGraphWindow.position.center, null);
            //foreach (SearcherWindow searcherWindow in Resources.FindObjectsOfTypeAll<SearcherWindow>())
            //{
            //    var name = searcherWindow.name;
            //    var searcherControlField = searcherWindow.GetType().GetField("m_SearcherControl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //    name = null;
            //}
            yield return new WaitForSecondsRealtime(userTime);
            var results = sercher.Search("add");
            ((SearcherProvider)searchWindowProvider).OnSearcherSelectEntry(results.First(), node.drawState.position.center);
            yield return new WaitForSecondsRealtime(userTime);
            */
            newNode.drawState = new DrawState
            {
                position = node.drawState.position,
                expanded = true
            };

            graphObject.graph.AddNode(newNode);
            nodeLookup.Add(node, newNode);
            yield return new WaitForSecondsRealtime(userTime);
        }

    }

    [TestFixture]
    public class GraphCreationTests
    {
        class SmokeTestGraphCases
        {
            private static string[] graphLocation = { "Assets/CommonAssets/Graphs/BuildGraphTests" };
            public static IEnumerator TestCases
            {
                get
                {
                    string[] guids = AssetDatabase.FindAssets("t:shader", graphLocation);
                    return guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)) //Get Paths
                                .Where(assetPath => Path.GetExtension(assetPath) == "." + ShaderGraphImporter.Extension) //Only Shadergraphs
                                .Select(assetPath => new TestCaseData(new object[] { assetPath }).Returns(null)) //Setup data as expected by TestCaseSource
                                .GetEnumerator();
                }
            }
        }

        [OneTimeSetUp]
        public void Setup()
        {
            GraphCreationUtils.CloseAllOpenShaderGraphWindows();
        }


        [UnityTest, TestCaseSource(typeof(SmokeTestGraphCases), "TestCases")]
        public IEnumerator SmokeTests(string assetPath)
        {
            GraphCreationUtils.CreateEmptyTestGraph(assetPath);
            return GraphCreationUtils.UserlikeGraphCreation(assetPath);
        }
    }
}
