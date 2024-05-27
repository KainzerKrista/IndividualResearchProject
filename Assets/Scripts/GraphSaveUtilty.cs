using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtilty
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;

    //Cache Edge List as variable
    private List<Edge> Edges => _targetGraphView.edges.ToList();

    //Caches and casts nodes.
    private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static GraphSaveUtilty GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtilty
        {
            _targetGraphView = targetGraphView
        };
    }

    //Saves Graph
    public void SaveGraph(string fileName)
    {
        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        
        //Saves nodes
        if (SaveNodes(fileName, dialogueContainer))
        {
            return;
        }

        SaveExposedProperties(dialogueContainer);

        //Creates new folder to save graph files if it doesn't exist yet. 
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        //Checks if graph with same name already exists
        UnityEngine.Object loadedAsset = AssetDatabase.LoadAssetAtPath($"Assets/Resources/{fileName}.asset", typeof(DialogueContainer));

        if(loadedAsset == null || !AssetDatabase.Contains(loadedAsset))
        {
            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
        }
        else
        {
            DialogueContainer container = loadedAsset as DialogueContainer;
            container.NodeLinkData = dialogueContainer.NodeLinkData;
            container.DialogueNodeData = dialogueContainer.DialogueNodeData;
            container.ExposeProperties = dialogueContainer.ExposeProperties;
            EditorUtility.SetDirty(container);
        }

        //Saves file in specific location in the project.
        AssetDatabase.SaveAssets();
    }

    //Loads in Graph
    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<DialogueContainer>(fileName);

        //checks if file exists.
        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exists!", "Ok");
            return;
        }

        ClearGraph();
        CreateNode();
        ConnectNodes();
        CreateExposedProperties();
    }

    //Clears current graph when loading up saved graph
    private void ClearGraph()
    {
        //Sets entry points from last save but discards exisiting guid.
        Nodes.Find(x => x.entryPoint).guid = _containerCache.NodeLinkData[0].baseNodeGuid;

        foreach(var node in Nodes)
        {
            if(node.entryPoint)
            {
                continue;
            }

            //Removes edges connected to every node.
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            //Removes node after connections are removed.
            _targetGraphView.RemoveElement(node);
        }
    }

    //Creates nodes saved from loaded graph
    private void CreateNode()
    {
        foreach (var nodeData in _containerCache.DialogueNodeData)
        {
            var tempNode = _targetGraphView.CreateSpeakerNode(nodeData.name, Vector2.zero);
            tempNode.guid = nodeData.guid;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.NodeLinkData.Where(x => x.baseNodeGuid == nodeData.guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }

    //Save nodes in current graph
    private bool SaveNodes(string fileName, DialogueContainer dialogueContainer)
    {
        //Checks if there are no connections to the nodes.
        if (!Edges.Any())
        {
            return false;
        }

        //Saves connections between nodes.
        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (var i = 0; i < connectedPorts.Count(); i++)
        {
            var outputNode = (connectedPorts[i].output.node as DialogueNode);
            var inputNode = (connectedPorts[i].input.node as DialogueNode);

            dialogueContainer.NodeLinkData.Add(new NodeLinkData
            {
                baseNodeGuid = outputNode.guid,
                portName = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.guid
            });
        }

        //Saves Nodes.
        foreach (var node in Nodes.Where(node => !node.entryPoint))
        {
            dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
            {
                guid = node.guid,
                name = node.name,
                dialogueText = node.dialogueText,
                position = node.GetPosition().position
            });
        }

        return true;
    }

    //Save blackboard values created in current graph
    private void SaveExposedProperties(DialogueContainer dialogueContainer)
    {
        dialogueContainer.ExposeProperties.Clear();
        dialogueContainer.ExposeProperties.AddRange(_targetGraphView.ExposeProperties);
    }

    //Shows variables created in current graph when loading up
    private void CreateExposedProperties()
    {
        //Clear existing properties.
        _targetGraphView.ClearBlackBoardExposedProperties();

        //Add saved properties from data
        foreach (var exposedProperty in _containerCache.ExposeProperties)
        {
            _targetGraphView.AddBlackBoardProperty(exposedProperty, new VisualElement());
        }
    }

    //Connects nodes
    private void ConnectNodes()
    {
        for (var i = 0; i < Nodes.Count; i++)
        {
            var connections = _containerCache.NodeLinkData.Where(x => x.baseNodeGuid == Nodes[i].guid).ToList();
            
            for(var c = 0; c < connections.Count; c++)
            {
                var targetNodeGuid = connections[c].targetNodeGuid;
                var targetNode = Nodes.First(x => x.guid == targetNodeGuid);
                LinkNodes(Nodes[i].outputContainer[c].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(_containerCache.DialogueNodeData.First(x => x.guid == targetNodeGuid).position, _targetGraphView.defaultNodeSize));
            }
        }
    }

    //Gets linkage of nodes
    private void LinkNodes(Port outputPort, Port inputPort)
    {
        var tempEdge = new Edge()
        {
            output = outputPort,
            input = inputPort
        };

        outputPort.Connect(tempEdge);
        inputPort.Connect(tempEdge);

        _targetGraphView.AddElement(tempEdge);
    }
}
