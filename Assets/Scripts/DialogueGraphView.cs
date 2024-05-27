using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.U2D.Animation;
using UnityEngine;

public class DialogueGraphView : GraphView
{
    //Sets sizing of nodes
    public readonly Vector2 defaultNodeSize = new Vector2(200, 400);

    //List of exposed blackboard properties.
    public List<ExposeProperty> ExposeProperties = new List<ExposeProperty>();
    public Blackboard Blackboard;
    public ObjectField spriteLibraryField;
    public string selectedNode;
    public string nodeName;

    
    private Dictionary<string, Dictionary<string, Sprite>> spriteDictionary;
    private NodeSearchWindow _searchWindow;
    private DialogueGraph _graph;
    private EditorWindow _window;
    private Image previewSprite;
    private bool isVarDrag = false;

    //Creates a graph tab and allows users to drag and drop nodes.
    public DialogueGraphView(EditorWindow editorWindow)
    {
        spriteDictionary = new Dictionary<string, Dictionary<string, Sprite>>();

        //Gets style sheet for grid background.
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphUSS"));

        //Grid background
        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        //Allows users to zoom in and out.
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        //TODO: Add auto snap here.

        //Blackboard events
        this.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        this.RegisterCallback<DragEnterEvent>(OnDragEnter);
        this.RegisterCallback<DragPerformEvent>(OnDragPerform);

        AddElement(GenerateEntryPointNode());
        AddSearchWindow(editorWindow);
    }

    //Output port generation
    private Port GeneratePort(Node node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Multi)
    {
        var port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); //Pass variable on exit
        
        //If output edge is released outside of node, create node option window to create new node
        var edgeConnectorListener = new CustomEdgeConnectorListener(this, _window);
        var edgeConnector = new EdgeConnector<Edge>(edgeConnectorListener);
        port.AddManipulator(edgeConnector);

        return port;
    }

    //Defines and creates start node
    public StartNode GenerateEntryPointNode()
    {
        var node = new StartNode
        {
            title = "START",
            guid = Guid.NewGuid().ToString(),
            entryPoint = true
        };

        //Creates output port to the next connected node.
        Port generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        //Makes Start node unmoveable and undeletable.
        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        //Gets style sheet
        styleSheets.Add(Resources.Load<StyleSheet>("StartNodeStyleSheet"));

        //Start node's visual appearance.
        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }

    //  ==========================================================================================
    //  SECTION: Response and Speaker Node Creation
    //  ==========================================================================================

    //Checks which type of node is being created
    public void CreateNode(string nodeName, Vector2 position)
    {
        switch (nodeName)
        {

            case "Speaker Node":
                AddElement(CreateSpeakerNode(nodeName, position));
                Debug.Log("Speaker");
                break;

            case "Response Node":
                AddElement(CreateResponseNode(nodeName, position));
                Debug.Log("Response");
                break;
        }
    }

    //Speaker Node
    public DialogueNode CreateSpeakerNode(string nodeName, Vector2 position)
    {
        //Contents of speaker node
        DialogueNode speakerNode = new DialogueNode()
        {
            title = nodeName,
            dialogueText = "Speaker dialogue goes here.",
            guid = Guid.NewGuid().ToString(),
        };

        //Defines output Port
        Port generatedPort = GeneratePort(speakerNode, Direction.Output);
        speakerNode.outputContainer.Add(generatedPort);

        //Defines input Node
        Port inputPort = GeneratePort(speakerNode, Direction.Input, Port.Capacity.Multi); //Allows multiple branching nodes.
        inputPort.portName = "Input";
        speakerNode.inputContainer.Add(inputPort);

        //Creates button to create outputs to connecting nodes
        AddChoicePort(speakerNode);

        var foldout = new Foldout { text = "Show/Hide Content" };

        //Sprite Library Content
        spriteLibraryField = new ObjectField("Sprite Library Editor")
        {
            objectType = typeof(SpriteLibraryAsset),
            allowSceneObjects = false
        };

        SpriteLibraryAsset spriteLib = new SpriteLibraryAsset();
        spriteLibraryField.RegisterValueChangedCallback(evt =>
        {
            var newValue = evt.newValue as SpriteLibraryAsset;
            if (newValue == null)
            {
                Debug.Log("new Value error");
            }
            spriteLib = SelectedSpriteLibrary(newValue);
            DisplaySpriteLibrary(speakerNode, spriteLib);

        });

        //Preview sprite information
        var previewSprite = new Image
        {
            name = "Preview Sprite"
        };

        //Set preview text for dialogue
        var previewDialogue = new TextField("")
        {
            value = "This is preview dialogue",
            isReadOnly = true
        };

        previewDialogue.style.display = DisplayStyle.None;

        //Defines content of output
        var speakerField = new TextField(string.Empty);
        speakerField.AddToClassList("dialogue-field");
        speakerField.RegisterValueChangedCallback(evt =>
        {
            speakerNode.dialogueText = evt.newValue;
            UpdatePreviewNode(speakerField, previewDialogue, spriteLibraryField, previewSprite);

        });

        //Register and trigger node summary depending on foldout's state
        foldout.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            if (evt.newValue)
            {
                //Open Foldout
                previewDialogue.style.display = DisplayStyle.None;
                previewSprite.style.display = DisplayStyle.None;
                speakerField.style.display = DisplayStyle.Flex;
                spriteLibraryField.style.display = DisplayStyle.Flex;
            }
            else
            {
                //Close foldout
                UpdatePreviewNode(speakerField, previewDialogue, spriteLibraryField, previewSprite);
                previewSprite.style.display = DisplayStyle.Flex;
                previewDialogue.style.display = DisplayStyle.Flex;
                speakerField.style.display = DisplayStyle.None;
                spriteLibraryField.style.display = DisplayStyle.None;
            }
        });

        speakerField.SetValueWithoutNotify(speakerNode.dialogueText);

        Label node = new Label(nodeName);

        //Defines the layout of the main container
        foldout.Add(spriteLibraryField);
        foldout.Add(speakerField);
        speakerNode.mainContainer.Add(foldout);

        speakerNode.mainContainer.Add(previewDialogue);

        //Updates visual appearance of nodes
        ColorUtility.TryParseHtmlString("#245A98", out Color color);
        speakerNode.titleContainer.style.backgroundColor = color; 
        RefreshNodeAppearance(speakerNode, position);

        //Attach selectable event to get name and list of variables of a node
        speakerNode.RegisterCallback<MouseDownEvent>(evt => OnNodeMouseDown(evt, nodeName));

        return speakerNode;
    }

    //Creates Response Node
    public DialogueNode CreateResponseNode(string nodeName, Vector2 position)
    {
        DialogueNode responseNode = new DialogueNode()
        {
            title = nodeName,
            dialogueText = "Speaker dialogue goes here.",
            guid = Guid.NewGuid().ToString(),
        };

        Port generatedPort = GeneratePort(responseNode, Direction.Output);
        responseNode.outputContainer.Add(generatedPort);

        var inputPort = GeneratePort(responseNode, Direction.Input, Port.Capacity.Multi); //Multiple branching nodes
        inputPort.portName = "Input";
        responseNode.inputContainer.Add(inputPort);

        var foldout = new Foldout { text = "Show/Hide Content" };

        //Defines style of nodes using USS
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueNodeStyleSheet"));

        //Defines button to allow users to create multiple output ports
        AddChoicePort(responseNode);

        //Sprite Library Content
        spriteLibraryField = new ObjectField("Sprite Library Editor")
        {
            objectType = typeof(SpriteLibraryAsset),
            allowSceneObjects = false
        };

        SpriteLibraryAsset spriteLib = new SpriteLibraryAsset();
        spriteLibraryField.RegisterValueChangedCallback(evt =>
        {
            var newValue = evt.newValue as SpriteLibraryAsset;
            if (newValue == null)
            {
                Debug.Log("new Value error");
            }
            spriteLib = SelectedSpriteLibrary(newValue);
            DisplaySpriteLibrary(responseNode, spriteLib);

        });

        //Preview sprite information
        var previewSprite = new Image
        {
            name = "Preview Sprite"
        };

        //Set preview text for dialogue
        var previewDialogue = new TextField("")
        {
            value = "This is preview dialogue",
            isReadOnly = true
        };

        previewDialogue.style.display = DisplayStyle.None;

        //Renames name of choice text field to whatever users set
        var dialogueField = new TextField(string.Empty);
        dialogueField.AddToClassList("dialogue-field");
        dialogueField.RegisterValueChangedCallback(evt =>
        {
            responseNode.response = evt.newValue;
            UpdatePreviewNode(dialogueField, previewDialogue, spriteLibraryField, previewSprite);
        });

        //Register and trigger node summary depending on foldout's state
        foldout.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            if(evt.newValue)
            {
                //Open Foldout
                previewDialogue.style.display = DisplayStyle.None;
                previewSprite.style.display = DisplayStyle.None;
                dialogueField.style.display = DisplayStyle.Flex;
                spriteLibraryField.style.display = DisplayStyle.Flex;
            }
            else
            {
                //Close foldout
                UpdatePreviewNode(dialogueField, previewDialogue, spriteLibraryField, previewSprite);
                previewSprite.style.display = DisplayStyle.Flex;
                previewDialogue.style.display = DisplayStyle.Flex;
                dialogueField.style.display = DisplayStyle.None;
                spriteLibraryField.style.display = DisplayStyle.None;
            }
        });

        dialogueField.SetValueWithoutNotify(responseNode.response);

        //Defines the layout of the main container
        foldout.Add(spriteLibraryField);
        foldout.Add(dialogueField);
        responseNode.mainContainer.Add(foldout);
        responseNode.mainContainer.Add(previewDialogue);


        //Updates visual appearance of nodes
        ColorUtility.TryParseHtmlString("#1B8B8C", out Color color);
        responseNode.titleContainer.style.backgroundColor = color; 
        responseNode.SetPosition(new Rect(position, defaultNodeSize));

        //Attach selectable event to get name and list of variables of a node
        responseNode.RegisterCallback<MouseDownEvent>(evt => OnNodeMouseDown(evt, nodeName));


        return responseNode;
    }

    private void UpdatePreviewNode(TextField dialogueContent, TextField previewContent, ObjectField spriteField, Image previewSprite)
    {
        Sprite sprite = spriteField.value as Sprite;
        previewSprite.image = sprite?.texture;

        //If text inside dialogue text is more than 20 characters, it will add "..." to the end in the summary
        previewContent.value = dialogueContent.value.Length > 20 ? dialogueContent.value.Substring(0, 20) + "..." : dialogueContent.value;
    }

    //Shows node information in blackboard properties window
    private void OnNodeMouseDown(MouseDownEvent evt, string nodeName)
    {
        string title = nodeName;
        selectedNode = nodeName;
        Debug.Log("Node Selected:   " + selectedNode);
        //TODO: Show list of available properties here
        //Reflect these into a function for blackboard
    }

    //Search Window for Node Types
    public void AddSearchWindow(EditorWindow editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(editorWindow, this);

        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    //  ==========================================================================================
    //  SECTION: Sprite Library
    //  ==========================================================================================

    //Select Sprite Library
    public SpriteLibraryAsset SelectedSpriteLibrary(SpriteLibraryAsset spriteLiberary)
    {
        return spriteLiberary;
    }

    private void DisplaySpriteLibrary(Node node, SpriteLibraryAsset spriteLibraryAsset)
    {
        VisualElement nodeContainer = new VisualElement();
        VisualElement leftContainer = new VisualElement();
        VisualElement rightContainer = new VisualElement();
        if(spriteLibraryAsset == null)
        {
            Debug.Log("Sprite library is null");
        }

        
        ObjectField spriteField = new ObjectField("Select Sprite")
        {
            objectType = typeof(Sprite),
            allowSceneObjects = false
        };

        //Declare dropdown list field of sprite category, label, and sprite
        var categoryList = new DropdownField("Category");
        var labelList = new DropdownField("Label");

        //Set preview image for sprite field
        previewSprite = new Image();
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueNodeStyleSheet"));
        previewSprite.AddToClassList("sprite-field-open");
        nodeContainer.AddToClassList("sprite-information");
        leftContainer.AddToClassList("left-container");

        GetCategory(spriteLibraryAsset, categoryList, labelList, spriteField);

        leftContainer.Add(previewSprite);
        rightContainer.Add(spriteField);
        rightContainer.Add(categoryList);
        rightContainer.Add(labelList);
        nodeContainer.Add(leftContainer);
        nodeContainer.Add(rightContainer);
        node.mainContainer.Add(nodeContainer);
    }

    //Gets category information from sprite library
    private void GetCategory(SpriteLibraryAsset spriteLibraryAsset, DropdownField categoryList, DropdownField labelList, ObjectField spriteField)
    {
        List<string> categories = new List<string>(spriteLibraryAsset.GetCategoryNames());
        categoryList.choices = categories;
        categoryList.value = categories[0];

        foreach(var category in categories)
        {
            List<string> labels = new List<string>(spriteLibraryAsset.GetCategoryLabelNames(category.ToString()));
            labelList.choices = labels;
            labelList.value = labels[0];

            categoryList.RegisterValueChangedCallback(evt =>
            {
                categoryList.value = evt.newValue;
                string selectedCategory = evt.newValue;
                List<string> labels = new List<string>(spriteLibraryAsset.GetCategoryLabelNames(selectedCategory));

                labelList.choices = labels;
                labelList.value = labels[0];
            });

            foreach (var label in labels)
            {

                Sprite sprite = spriteLibraryAsset.GetSprite(category, label);
                spriteField.RegisterValueChangedCallback(evt => OnSpriteChanged(evt.newValue as Sprite));

            }
        }
    }

    //Gets preview image to display when sprite is selected from sprite library
    private void OnSpriteChanged(Sprite selectedSprite)
    {
        //Checks if selected sprite exists
        if (selectedSprite != null)
        {
            //If sprite exists, converts sprite texture to image type
            previewSprite.image = selectedSprite.texture;
        }
        else
        {
            previewSprite.image = null;
        }

    }

    //  ==========================================================================================
    //  SECTION: Port 
    //  ==========================================================================================

    private void RefreshNodeAppearance(Node nodeType, Vector2 position)
    {
        //Updates node's visual appearance.
        nodeType.RefreshExpandedState();
        nodeType.RefreshPorts();
        nodeType.SetPosition(new Rect(position, defaultNodeSize));
    }

    //Creates choices for output
    public void AddChoicePort(Node nodeText, string overridenPortName = "")
    {
        //Query UI Elements for choice list
        var generatedPort = GeneratePort(nodeText, Direction.Output);
        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);
        var outputPortCount = nodeText.outputContainer.Query("connector").ToList().Count;

        nodeText.RefreshPorts();
        nodeText.RefreshExpandedState();
    }

    //Defines port compatibility
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        var startPointView = startPort;

        ports.ForEach((port) =>
        {
            var portView = port;
            if (startPointView != portView && startPort.node != portView.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    //Deletes Port
    private void RemovePort(Node speakerText, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }

        speakerText.outputContainer.Remove(generatedPort);
        speakerText.RefreshExpandedState();
        speakerText.RefreshPorts();
    }

    //  ==========================================================================================
    //  SECTION: Blackboard Properties
    //  ==========================================================================================

    //Add properties into Blackboard
    public void AddBlackBoardProperty(ExposeProperty exposedProperty, VisualElement varContainer)
    {
        var localPropertyName = exposedProperty.propertyName;
        var localPropertyValue = exposedProperty.propertyValue;

        while (ExposeProperties.Any(x => x.propertyName == localPropertyName))
        {
            localPropertyName = $"{localPropertyName}(1)";//Adds number tag when multiple of same variable name is created.
        }

        var property = new ExposeProperty();
        property.propertyName = localPropertyName;
        property.propertyValue = localPropertyValue;

        //Store data
        ExposeProperties.Add(property);

        //Defines dropdown list and text field of variables users can set
        var dropdown = new DropdownField(new List<string> { "int", "float", "string", "bool" }, 0);
        var blackboardField = new BlackboardField { text = property.propertyName};

        var propertyValueTextField = new TextField()
        {
            value = localPropertyValue
        };

        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var changingPropertyIndex = ExposeProperties.FindIndex(x => x.propertyName == property.propertyName);
            ExposeProperties[changingPropertyIndex].propertyValue = evt.newValue;
        });

        //Defines the content inside blackboard. This includes variable dropdown list, names, type, and value.
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.width = 250;

        blackboardField.RegisterCallback<MouseDownEvent>(evt =>
        {
            Vector2 mousePosition = Input.mousePosition;
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                Debug.Log("Button Pressed");
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData("BlackboardField", blackboardField);
                DragAndDrop.StartDrag(blackboardField.ToString());
                evt.StopPropagation();
            }
        });

        var seperator = new VisualElement();
        seperator.AddToClassList("blackboard-seperator");

        //Creates a row of containers holding variable data
        container.Add(blackboardField);
        container.Add(dropdown);
        container.Add(propertyValueTextField);
        varContainer.Add(seperator);
        varContainer.Add(container);

        //Define visual elements of content inside container
        Blackboard.Add(varContainer);
    }

    //Fires when item is dragged over an element
    private void OnDragUpdated(DragUpdatedEvent evt)
    {
        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
    }
    
    //Fires when item is dragged over an element
    private void OnDragEnter(DragEnterEvent evt)
    {
        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
    }

    //What happens to the blackboard variable once its dropped onto an element
    private void OnDragPerform(DragPerformEvent evt)
    {
        DragAndDrop.AcceptDrag();
        Debug.Log("Drag is performed");
        if (DragAndDrop.GetGenericData("BlackboardField") is BlackboardField blackboardField)
        {
            var propertyName = blackboardField.text;
            var propertyValue = "Default Value";
            Debug.Log("Var accepted");

            AddVariableToNode(propertyName, propertyValue);
        }

        //Add node to list of triggers
        //Shows list of variables available in that node
    }

    //Shows node name on property list on blackboard
    private void AddVariableToNode(string propertyName, string propertyValue)
    {
        var selectedNode = selection.FirstOrDefault() as Node;
        if(selectedNode != null)
        {
            var textField = new TextField(propertyName) { value = propertyName };
            selectedNode.extensionContainer.Add(textField);
            selectedNode.RefreshExpandedState();
            selectedNode.RefreshPorts();
        }
    }

    //Clears existing blackboard when loading a saved file blackboard in another file.
    public void ClearBlackBoardExposedProperties()
    {
        ExposeProperties.Clear();
        Blackboard.Clear();
    }
}
