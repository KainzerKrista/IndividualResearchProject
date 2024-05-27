using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphTest : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _fileName = "New Narrative";

    [MenuItem("Graph/Dialogue Graph Test")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraphTest>();
        window.titleContent = new GUIContent("Dialogue Graph Test");
    }

    private void ConstructGraphView()
    {
        //Gets name of graph window.
        _graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph Test"
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
        fileNameTextField.SetValueWithoutNotify("New Narrative");
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => RequestDataOperation(true)) {text = "Save Data" });
        toolbar.Add(new Button(() => RequestDataOperation(false)) {text = "Load Data" });

        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
        if(string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name.", "Please enter a valid file name.", "Ok");
            return;
        }

        //New instance
        var saveUtility = GraphSaveUtilty.GetInstance(_graphView);
        if(save)
        {
            saveUtility.SaveGraph(_fileName);
        }
        else
        {
            saveUtility.LoadGraph(_fileName);
        }
    }

    private void GenerateMiniMap()
    {
        //Stays anchored
        var miniMap = new MiniMap
        {
            anchored = true
        };

        //Offsets window tabs by 10px from the left.
        var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 4000, 30));
        miniMap.SetPosition(new Rect (cords.x, cords.y, 200, 140));
        _graphView.Add(miniMap);
    }

    private void GenerateBlackBoard()
    {
        var blackboard = new Blackboard(_graphView);
        blackboard.Add(new BlackboardSection { title = "Exposed Variables" });
        blackboard.addItemRequested = _blackboard =>
        {
           // _graphView.AddBlackBoardProperty(new ExposeProperty());
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
        //Set blackboard position.
        blackboard.SetPosition(new Rect(10, 30, 200, 300));
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
