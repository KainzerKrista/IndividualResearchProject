using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class StartNode : Node
{
    public string guid;
    public Vector2 position;
    public bool entryPoint = false;
}
