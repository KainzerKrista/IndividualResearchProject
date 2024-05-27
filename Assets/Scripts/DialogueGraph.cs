using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.U2D.Animation;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;

    private string _fileName = "New Narrative";

    //Creates editor window option
    [MenuItem("Dialogue/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    //Creates graph
    private void ConstructGraphView()
    {
        //Gets name of graph window.
        _graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        //Stretches the window
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new UnityEditor.UIElements.Toolbar();

        //Save file
        var fileNameTextField = new TextField("File Name:");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Data" });
        toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Data" });

        //Button for node creation
        Vector2 position = new Vector2(100, 200);
        toolbar.Add(new Button(() => _graphView.CreateNode("Speaker Node", position)) { text = "Create Speaker Node" });
        toolbar.Add(new Button(() => _graphView.CreateNode("Response Node", position)) { text = "Create Response Node" });

        rootVisualElement.Add(toolbar);
    }

    //Checks if file name exists when users click save or load graph data
    private void RequestDataOperation(bool save)
    {
        if (!string.IsNullOrEmpty(_fileName))
        {
            //New instance
            var saveUtility = GraphSaveUtilty.GetInstance(_graphView);
            if (save)
            {
                saveUtility.SaveGraph(_fileName);
            }
            else
            {
                saveUtility.LoadGraph(_fileName);
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Invalid file name.", "Please enter a valid file name.", "Ok");
        } 
    }

    private void GenerateMiniMap()
    {
        //Stays anchored
        var miniMap = new MiniMap
        {
            anchored = true
        };

        //Offsets window tabs by 10px from the left
        var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 4000, 30));
        miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
        _graphView.Add(miniMap);
    }

    private void GenerateBlackBoard()
    {
        var blackboard = new Blackboard(_graphView);
        var tabContainer = new VisualElement();
        var conContainer = new VisualElement();
        var varContainer = new VisualElement();
        var expoContainer = new VisualElement();
        var nodeVarContainer = new VisualElement();
        var seperator = new VisualElement();
        tabContainer.style.flexDirection = FlexDirection.Row;

        var varCol = new Label("Variable");
        var conCol = new Label("Node Properties");
       
        blackboard.addItemRequested = _blackboard =>
        {
            _graphView.AddBlackBoardProperty(new ExposeProperty(), expoContainer);
            varContainer.Add(expoContainer);
        };

        //Add number tag to blackboard values with same name.
        blackboard.editTextRequested = (blackboard1, element, newValue) =>
        {
            var oldPropertyName = ((BlackboardField)element).text;

            if (_graphView.ExposeProperties.Any(x => x.propertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "Property name already exists!", "Ok");
                return;
            }

            var propertyIndex = _graphView.ExposeProperties.FindIndex(x => x.propertyName == oldPropertyName);
            _graphView.ExposeProperties[propertyIndex].propertyName = newValue;
            ((BlackboardField)element).text = newValue;
        };

        //Gets style sheet to define visual elements of the blackbaord
        blackboard.styleSheets.Add(Resources.Load<StyleSheet>("DialogueNodeStyleSheet"));
        blackboard.AddToClassList("blackboard");
        varCol.AddToClassList("bb-var");
        conContainer.AddToClassList("bb-con");
        seperator.AddToClassList("bb-container");
        conContainer.style.width = 200;
        varCol.style.width = 200;

        //TODO: Add node properties content here
        var nodeName = new Label("Node Name:    " + _graphView.nodeName);
        var nodeVarTitle = new Label("Available Variables:   ");

        //Adds Variable and Conditions to blackboard section
        nodeVarContainer.Add(nodeVarTitle);
        varContainer.Add(varCol);
        conContainer.Add(conCol);
        conContainer.Add(seperator);
        conContainer.Add(nodeName);
        conContainer.Add(nodeVarContainer);


        //Adds blackboard to graph view
        varContainer.Add(expoContainer);
        tabContainer.Add(varContainer);
        tabContainer.Add(conContainer);
        blackboard.Add(tabContainer);
        _graphView.Add(blackboard);
        _graphView.Blackboard = blackboard;
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMiniMap();
        GenerateBlackBoard();
    }

    //Avoids overlapping windows.
    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
}
