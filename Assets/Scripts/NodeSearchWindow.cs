using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogueGraphView _graphView;

    //Gets mouse position in graph.
    private EditorWindow _window;

    private Texture2D _indenttationIcon;

    //Test
    private DialogueGraphViewTest _graphViewTest;

    public void Init(EditorWindow window, DialogueGraphViewTest graphView)
    {
        _graphViewTest = graphView;
        _window = window;

        //Fixes indentation in list.
        _indenttationIcon = new Texture2D(1, 1);
        _indenttationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
        _indenttationIcon.Apply();
    }
    
    //TEST
    public void Init(EditorWindow window, DialogueGraphView graphView)
    {
        _graphView = graphView;
        _window = window;

        //Fixes indentation in list.
        _indenttationIcon = new Texture2D(1, 1);
        _indenttationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
        _indenttationIcon.Apply();
    }

    //Search list of nodes
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Select Node Type"), 0),
            
            //Creates Test Node
            new SearchTreeEntry(new GUIContent("Dialogue Node", _indenttationIcon))
            {
                userData = new DialogueNodeTest(), level = 1
            },  
            
            //Creates Speaker Node
            new SearchTreeEntry(new GUIContent("Speaker Node", _indenttationIcon))
            {
                userData = new SpeakerNode(), level = 1
            },
            
            //Creates Response Node
            new SearchTreeEntry(new GUIContent("Response Node", _indenttationIcon))
            {
                userData = new ResponseNode(), level = 1
            }

            //Creates Ghost node
        };

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        //Creates nodes on mouse position in graph.
        var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, context.screenMousePosition - _window.position.position);
        var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);

        //Search Menu options.
        switch (SearchTreeEntry.userData)
        {
            //Test Node
            case DialogueNodeTest dialogueNode:
                _graphView.CreateNode("Dialogue Node", localMousePosition);
                return true;

            //Speaker Node
            case SpeakerNode speakerNode:
                _graphView.CreateNode("Speaker Node", localMousePosition);
                return true;

            //Response Node
            case ResponseNode responseNode:
                _graphView.CreateNode("Response Node", localMousePosition);
                return true;

            default:
                return false;
        }
    }
  
}
