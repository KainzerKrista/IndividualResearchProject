using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ResponseNode : Node
{
    public string guid;
    public string name;
    public string response;
    public bool entryPoint = false;
}
