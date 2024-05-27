using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//This script manages functions when an edge link is dropped outside a node.
public class CustomEdgeConnectorListener : IEdgeConnectorListener
{
    private DialogueGraphView _graphView;
    private DialogueGraph editorWindow;
    private NodeSearchWindow _searchWindow;
    private SearchWindowContext context;
    private EditorWindow _window;

    public CustomEdgeConnectorListener(DialogueGraphView graphView, EditorWindow window)
    {
        _graphView = graphView;
        _window = window;
        
    }

    public void OnDrop(GraphView graphView, Edge edge)
    {
        //Do something here
    }

    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
        //Creates node option window when output port is dragged out
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(_window, _graphView);

        SearchWindow.Open(new SearchWindowContext(position), _searchWindow);
    }
}
