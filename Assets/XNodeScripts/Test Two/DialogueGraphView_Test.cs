using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

public class DialogueGraphViewTest : GraphView
{
  /*  //Sets sizing of nodes
    public readonly Vector2 defaultNodeSize = new Vector2(200, 200);

    //List of exposed blackboard properties.
    public List<ExposeProperty> ExposeProperties = new List<ExposeProperty>();
    public Blackboard Blackboard;

    private NodeSearchWindow _searchWindow;

    //Creates a graph tab and allows users to drag and drop nodes.
    public DialogueGraphViewTest(EditorWindow editorWindow)
    {
        //Gets style sheet for grid background.
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphUSS"));
        
        //Grid background
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        //Allows users to zoom in and out.
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        //this.AddManipulator(new RectangleSelection());
        //TODO: Add auto snap here.

        AddElement(GenerateEntryPointNode());
        AddSearchWindow(editorWindow);
    }

    private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); //Pass variable on exit
    }

    public DialogueNodeTest GenerateEntryPointNode()
    {
        var node = new DialogueNodeTest
        {
            title = "START",
            guid = Guid.NewGuid().ToString(),
            dialogueText = "ENTRYPOINT",
            entryPoint = true
        };

        //Jumps to the next connected node.
        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        //Makes Start node unmoveable and undeletable.
        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        //Start node's visual appearance.
        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }

    public void CreateNode(string nodeName, Vector2 position)
    {
        AddElement(CreateDialogueNode(nodeName, position));
    }

    public DialogueNodeTest CreateDialogueNode(string nodeName, Vector2 position)
    {
        var dialogueNode = new DialogueNodeTest()
        {
            title = nodeName,
            dialogueText = nodeName,
            guid = Guid.NewGuid().ToString()
        };

        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi); //Allows multiple branching nodes.
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        Button button = new Button(() =>
        {
            AddChoicePort(dialogueNode);
        });

        dialogueNode.titleContainer.Add(button);
        button.text = "New Choice";

        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt =>
        {
            //Dialogue Text
            dialogueNode.dialogueText = evt.newValue;
            //Node Title
            dialogueNode.title = evt.newValue;
        });

        textField.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField);
        dialogueNode.mainContainer.Add(textField);

        //Updates node's visual appearance.
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position, defaultNodeSize));

        return dialogueNode;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    public void AddChoicePort(DialogueNodeTest dialogueText, string overridenPortName = "")
    {
        var generatedPort = GeneratePort(dialogueText, Direction.Output);

        //Query UI Element
        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);

        var outputPortCount = dialogueText.outputContainer.Query("connector").ToList().Count;

        var choicePortName = string.IsNullOrEmpty(overridenPortName) ? $"Choice {outputPortCount + 1}" : overridenPortName;

        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };

        //Creates a delete button to remove choice ports.
        textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.contentContainer.Add(textField);
        var deleteButton = new Button(() => RemovePort(dialogueText, generatedPort))
        {
            text = "X"
        };

        generatedPort.contentContainer.Add(deleteButton);
        generatedPort.portName = choicePortName;

        dialogueText.outputContainer.Add(generatedPort);
        dialogueText.RefreshPorts();
        dialogueText.RefreshExpandedState();
    }

    private void RemovePort(DialogueNodeTest dialogueText, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if(targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }

        dialogueText.outputContainer.Remove(generatedPort);
        dialogueText.RefreshExpandedState();
        dialogueText.RefreshPorts();
    }

    //Search Window for Blackboard
    private void AddSearchWindow(EditorWindow editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(editorWindow, this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    //Add properties into Blackboard
    public void AddBlackBoardProperty(ExposeProperty exposedProperty)
    {
        var localPropertyName = exposedProperty.propertyName; 
        var localPropertyValue = exposedProperty.propertyValue; 

        while(ExposeProperties.Any(x => x.propertyName == localPropertyName))
        {
            localPropertyName = $"{localPropertyName}(1)";//Adds number tag when multiple of same variable name is created.
        }

        var property = new ExposeProperty();
        property.propertyName = localPropertyName;
        property.propertyValue = localPropertyValue;

        //Store data
        ExposeProperties.Add(property);

        var container = new VisualElement();
        var blackboardField = new BlackboardField { text = property.propertyName, typeText = "string property" };
        container.Add(blackboardField);

        var propertyValueTextField = new TextField("Value: ")
        {
            value = localPropertyValue
        };

        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var changingPropertyIndex = ExposeProperties.FindIndex(x => x.propertyName == property.propertyName);
            ExposeProperties[changingPropertyIndex].propertyValue = evt.newValue;
        });

        var blackBoardValueRow = new BlackboardRow(blackboardField, propertyValueTextField);
        container.Add(blackBoardValueRow);

        Blackboard.Add(container);
    }

    //Clears existing blackboard when loading a saved file blackboard in another file.
    public void ClearBlackBoardExposedProperties()
    {
        ExposeProperties.Clear();
        Blackboard.Clear();
    }*/
}
